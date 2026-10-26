# PR #138 模块专项评审报告：嵌套块内曲线求交

> 状态：historical；针对 `4064025` 的专项静态评审快照，2026-10-26 更正。<br>
> 仅供追溯，不是现行技术契约；接口与支持范围以[当前指南](../../guides/block-intersections-beta.md)为准。宿主验收 `Not run`。
> 后续完整审查与修正见[实现记录](../../beta-implementation-notes.md#完整核心审查与修正)。

**评审模块**：嵌套块内实际曲线求交 Beta（`GetCurveIntersectionsBeta`）
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Blocks\BlockIntersectionExBeta.cs`](../../../src/CADShared/Cad/Database/Entities/Blocks/BlockReferenceIntersectionExtensionsBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestBlockIntersectionsBeta.cs`](../../../tests/TestShared/TestBlockIntersectionsBeta.cs)
**对应文档**：[`docs/guides/block-intersections-beta.md`](../../../docs/guides/block-intersections-beta.md)

---

## 1. 算法与数学分析（第一性原理）

### 1.1 嵌套块矩阵累计与坐标变换
- **几何变换累乘顺序**：
  在 AutoCAD 嵌套块引用中，定义内部的子块参照（`BlockReference`）具有其自身的局部变换矩阵 $M_{child} = \text{reference.BlockTransform}$。
  当该子块被父块包含，且父块在 WCS 下的累计变换矩阵为 $M_{parent}$ 时，子块内部图元到 WCS 的变换矩阵必须满足：
  $$M_{toWorld} = M_{parent} \times M_{child}$$
  代码在入栈遍历时采用：
  ```csharp
  stack.Push(new TraversalFrame(definitionId, parentToWorld * reference.BlockTransform, definition.Cast<ObjectId>().ToArray()));
  ```
  乘法顺序严密遵循 AutoCAD .NET 的行向量/列向量变换语义，保证了缩放、旋转和平移的多级精确传递。
- **真实三维求交（True 3D Intersection）**：
  不提供粗糙的二维 XY 投影近似。高度不同但在俯视图中交叉的曲线，在空间中无交点时严格返回空结果；只有在三维坐标相交时才产生交点。
- **延伸模式约束（`Intersect` 模式）**：
  方法仅允许 `Intersect.OnBothOperands`（双方均不延伸）或 `Intersect.ExtendArgument`（仅延伸目标曲线 `target`）。
  严格禁止延伸块内部图元（`ExtendThis` 或 `ExtendBoth`），防止块内部一条微小的局部线段被意外延伸为穿透整个图纸的无限射线。

### 1.2 遍历模型与环路检测
- **显式栈消除 CLR 调用栈溢出风险**：
  采用 `Stack<TraversalFrame>` 模拟深度优先搜索（DFS），替代传统的递归函数调用。在遭遇极深的合法块嵌套链条时，绝不会导致宿主进程 CLR 栈溢出崩溃。
- **路径环路检测（Path Cycle Detection）**：
  使用 `HashSet<ObjectId> path` 记录**当前祖先链**上的块表记录 ID：
  - 只有在向深层下探时，调用 `path.Add(definitionId)`；若已在祖先链中存在则抛出 `InvalidOperationException("检测到循环的块定义路径。")`；
  - 当当前帧的所有子图元处理完毕退栈时，执行 `path.Remove(frame.DefinitionId)`。
  - **关键收益**：支持同一定义在不同分支下的多次实例化（如块 A 中包含两个独立的块 B 实例），同时精准识别并阻止 A -> B -> A 的真正递归死循环。

### 1.3 去重算法与大坐标平方溢出防护
- **首次命中保留原则（First-hit Representative）**：
  每个原生求交返回的交点只与“已保留的代表点”进行三维欧氏距离比对；距离 $\le tolerance$ 则跳过。不将被合并的点加入比对集合，彻底杜绝了传统贪心去重产生的“链式漂移合并（Chained Aggregation）”。
- **归一化平方和防溢出**：
  在 `WithinDistance` 中，代码先筛三轴绝对差，再做除以容差的归一化：
  ```csharp
  var x = Math.Abs(first.X - second.X);
  var y = Math.Abs(first.Y - second.Y);
  var z = Math.Abs(first.Z - second.Z);
  if (x > tolerance || y > tolerance || z > tolerance) return false;
  x /= tolerance; y /= tolerance; z /= tolerance;
  return x * x + y * y + z * z <= 1;
  ```
  即使交点位于绝对坐标 $10^{14}$ 的天文级尺度，只要两点间距在容差内，归一化后的 $x, y, z \in [0, 1]$，其平方和绝不会发生浮点溢出（Overflow）。

---

## 2. CAD 对象生命周期、内存与事务安全审查

### 2.1 事务一致性（Transaction Alignment）
- **顶层事务校验**：
  代码通过 `ReferenceEquals(database.TransactionManager.TopTransaction, transaction)` 强制要求调用方传入该数据库当前最上层的有效事务。
  - 拒绝在存在活动子事务时传入已被挂起的父事务；
  - 拒绝跨数据库传入事务；
  - 绝不在内部调用 `Commit()` 或 `Abort()`，不释放调用方的事务，保持调用方的完整回滚边界。
- **纯读打开（ForRead）**：
  所有块表记录与子图元均使用 `OpenMode.ForRead` 读取，不升级写状态，不产生写锁冲突。

### 2.2 非托管对象释放（Disposable Wrapper）
- **变换副本与点集合严密释放**：
  ```csharp
  using var transformed = curve.GetTransformedCopy(frame.ToWorld);
  if (transformed is not Curve transformedCurve)
      throw new NotSupportedException("宿主未返回曲线变换副本，不能省略该叶节点的交点。");
  using var points = new Point3dCollection();
  transformedCurve.IntersectWith(target, mode, points, IntPtr.Zero, IntPtr.Zero);
  ```
  `GetTransformedCopy` 生成的托管包装对象与非托管 C++ 曲线实体，以及宿主 `IntersectWith` 填充的 `Point3dCollection` 均受 `using` 保护，确保在正常遍历及任何中间异常发生时立即销毁释放，杜绝 CAD 图形句柄泄漏。

### 2.3 安全曲线类型白名单（Conservative Whitelist）
- **类型过滤**：
  仅支持原生白名单曲线：`Line`、`Arc`、`Circle`、`Ellipse`、轻量 `Polyline`、`Spline`、`Ray`、`Xline`。
- **拒绝复杂复合多段线**：
  明确拒绝 `Polyline2d` 和 `Polyline3d`。依据 Autodesk 官方 API 警告，带顶点子实体的旧式复杂曲线在调用 `GetTransformedCopy` 时可能发生浅克隆并意外修改原始子图元，白名单策略有效捍卫了“输入绝不被篡改”的底线。
- **拒绝非几何图元与外部参照**：
  文字（`DBText`/`MText`）、属性定义（`AttributeDefinition`）、填充（`Hatch`）、三维网格跳过求交；动态块（`IsDynamicBlock`）、外部参照（`IsFromExternalReference`）、阵列块（`MInsertBlock`）直接保守抛错拒绝。

---

## 3. 原 Finding 1 复核：撤销 P2，不增加预过滤

原判断将“块定义包含未 Purge 的删除实体”直接等同于“默认枚举会返回该实体”，这个触发前提不成立。

- **本机静态证据**：AutoCAD 2019／2026 运行库中，`BlockTableRecord` 的普通构造与指针包装构造均令 `m_bSkipDeleted=true`；`GetEnumerator` 将其传给原生迭代器，枚举器的 `start` 和 `step` 继续使用同一值。
- **包含删除对象的独立入口**：`IncludingErased` 返回 `Copy()` 的包装副本，并将该副本的跳过标志设为 false。生产查询使用普通的 `transaction.GetObject` 与 `definition.Cast<ObjectId>()`，没有调用该入口。
- **官方依据**：[原生 newIterator](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-RefGuide/files/OREF-AcDbBlockTableRecord__newIterator_AcDbBlockTableRecordIterator___bool_bool_const.html)默认 `skipDeleted=true`，与本机运行库静态检查一致。
- **处理决定**：撤销原 P2 及弹栈／入栈两份 `ValidId` 过滤建议。没有证据表明现行普通枚举因删除成员必然抛出 `eWasErased`；托管异常本身也不能定性为原生进程崩溃。

这不代表所有宿主已通过验证。保留删除场景回归的建议：在独立临时数据库中使用不同交点的曲线，分别检查已提交删除、当前事务内删除、恢复后的交点及旧结果快照，比较查询前后的调用方对象和事务状态。新增场景实际宿主执行均为 `Not run`。

---

## 4. 验证证据与测试覆盖

- **宿主命令测试 (`TestBlockIntersectionsBeta.cs`)**：
  - 原有入口覆盖：两级嵌套块复合变换（旋转/缩放/位移）、同一定义重复实例、不可见圆弧/直线、轻量多段线 WCS 变换保持、非曲线叶节点、目标延伸、三维高度差、去重、借用语义、事务及空间身份校验。
  - 新增入口覆盖：已提交删除、当前事务内删除、恢复成员后交点恢复及旧快照独立，使用独立临时数据库隔离原有只读断言。
  - 原评审针对 `4064025` 记录了原有入口编译通过；本轮新增入口通过 `TestAcad2025` Debug x64 编译（0 警告、0 错误）。两者均不代表场景已运行，全部真实宿主场景均为 `Not run`。

---

## 5. 评审结论

- **结论**：原 P2 撤销，保留生产实现并补充删除／恢复回归。静态复核没有确认必须修改生产代码的缺陷；跨宿主行为仍须通过实际宿主验收确认。
