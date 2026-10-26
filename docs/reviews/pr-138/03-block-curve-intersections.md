# PR #138 模块专项评审报告：嵌套块内曲线求交

**评审模块**：嵌套块内实际曲线求交 Beta（`GetCurveIntersectionsBeta`）
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Blocks\BlockIntersectionExBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestBlockIntersectionsBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/TestShared/TestBlockIntersectionsBeta.cs)
**对应文档**：[`docs/guides/block-intersections-beta.md`](file:///D:/MakeX/Fs.Fox.CAD/docs/guides/block-intersections-beta.md)

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

## 3. 审查发现的问题与修复建议

---

### Finding 1 [P2 缺陷 / 建议合入前修复]: 块定义遍历未跳过已删除（Erased）图元，存在触发 `eWasErased` 原生崩溃风险

- **定位**：[`BlockIntersectionExBeta.cs#L82, L122`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs#L82)
- **触发条件**：
  当被查询的块定义在当前图形会话/事务中发生过实体删除（例如脚本调用过 `ent.Erase()` 或交互式删除了块内图元），且尚未执行数据库 Purge 或重新保存打开时；`definition.Cast<ObjectId>().ToArray()` 会抓取到已被标记为 Erased 的图元 ID。
- **实际行为**：
  遍历执行到 `var child = transaction.GetObject(frame.Children[frame.NextIndex++], OpenMode.ForRead);` 时，AutoCAD 原生抛出 `Autodesk.AutoCAD.Runtime.Exception: eWasErased`，导致求交查询失败。
- **预期行为**：
  跳过已被标记为删除的图元，继续正常求交剩余有效曲线。
- **推荐修复 Diff**：
  ```diff
  --- a/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  +++ b/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  @@ -80,7 +80,9 @@ public static class BlockIntersectionExBeta
                   continue;
               }

  -            var child = transaction.GetObject(frame.Children[frame.NextIndex++], OpenMode.ForRead);
  +            var childId = frame.Children[frame.NextIndex++];
  +            if (childId.IsErased) continue;
  +            var child = transaction.GetObject(childId, OpenMode.ForRead);
               if (child is BlockReference nested)
               {
                   PushDefinition(nested, frame.ToWorld);
  ```
- **验证办法**：
  在 `TestBlockIntersectionsBeta.cs` 中增加测试用例：向测试块定义中添加两条直线，将其中一条调用 `line.Erase()`，对引用执行 `GetCurveIntersectionsBeta`，断言能够正常返回另一条直线的交点而不抛出 `eWasErased`。

---

## 4. 验证证据与测试覆盖

- **宿主命令测试 (`TestBlockIntersectionsBeta.cs`)**：
  - 测试用例完整覆盖：两级嵌套块复合变换（旋转/缩放/位移）、同一定义在不同位置实例化求交、不可见圆弧/直线、轻量多段线 WCS 变换保持、非曲线子图元跳过、`ExtendArgument` 仅延伸目标曲线、三维高度差不投影、去重半径为 0 与正值比对、未入库目标曲线借用、异库事务/空间与非最上层事务拒绝、只读属性保持。
  - 状态：编译通过，真实 CAD 宿主执行状态依规保持为 `Not run`。

---

## 5. 评审结论

- **结论**：**建议在修复 Finding 1 后合入（Mergeable after addressing Finding 1）**。
- **评价**：除未防御 `childId.IsErased` 这一细微缺陷外，该模块在矩阵变换、DFS 环检测、事务顶层一致性和非托管资源释放方面均达到了极高的工业级标准。
