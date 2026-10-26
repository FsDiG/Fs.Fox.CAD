# PR #138 模块专项评审报告：空间排序与邻近范围组合

**评审模块**：空间行列/曲线排序与邻近范围分量组合 Beta
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Geometry\Ordering\OrderingBetaCore.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Geometry/Ordering/OrderingBetaCore.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Geometry\Ordering\PointOrderingBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Geometry/Ordering/PointOrderingBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Curves\CurveOrderingExBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Curves/CurveOrderingExBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Bounds\ExtentGroupingBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Bounds/ExtentGroupingBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\OrderingBeta.CoreTests\Program.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/OrderingBeta.CoreTests/Program.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestOrderingBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/TestShared/TestOrderingBeta.cs)
**对应文档**：[`docs/guides/ordering-beta.md`](file:///D:/MakeX/Fs.Fox.CAD/docs/guides/ordering-beta.md)

---

## 1. 算法与数学分析（第一性原理）

### 1.1 行列排序：固定锚点与严格传递性比较
- **历史缺陷（Zfgk 坏味道）**：
  原 `Fs.Zfgk.CAD` 中的点排序比较器直接在 `IComparer` 中使用“容差内模糊相等”返回 0。这是极危险的数学违规：
  “在容差内相等”**不满足传递性**（$A \approx B$ 且 $B \approx C$ 并不蕴含 $A \approx C$）。交给 `Array.Sort` 或快速排序会导致比较器违反严格弱序（Strict Weak Ordering），在不同元素排列下产生乱序甚至抛出 `ArgumentException`。
- **本实现重构方案**：
  1. *两阶段解耦（分组成组 + 组内精排）*：
     先通过固定极值锚点将点划分为独立的行/列，再在行/列内部执行严格的单调精确比较。
  2. *固定锚点机制（Fixed Anchor，杜绝链式漂移）*：
     行分组始终从当前尚未分组的最高 Y 坐标建立锚点；凡与该锚点 Y 坐标差不超过 `rowTolerance` 的连续点归入同一行。
     **关键防范**：不通过中间点向外滚雪球式扩充行范围。例如 $Y$ 分别为 $1.5, 0.75, 0$，容差为 $1$：锚点 $1.5$ 仅吸纳 $0.75$；$0$ 与 $1.5$ 距离超过 $1$，因此 $0$ 独立成行，绝不通过 $0.75$ 将三者连成一行。
  3. *确定性 Tie-Breaker（保序索引回退）*：
     所有比较器的末位平局一律回退到原始索引 `a.CompareTo(b)`。对于完全重合的点，严格保证输入顺序不变，排序具备数学可重现性。

### 1.2 沿曲线排序：真实里程与确定性键
- **消除人为微扰（Zero Perturbation）**：
  原 Zfgk 为防止将点放入以距离为键的 `SortedDictionary` 时发生键冲突，竟采用在相等距离上人为减去微小偏移量（$dist - 0.0001$）的做法，严重破坏了几何真值。
  本实现直接使用 `(实际里程, 原始索引)` 的复合排序模型，计算几何起点到最近点的精确沿线弧长距离，相同里程的点完全保留并以原始索引打破平局，不人为引入任何几何噪声。
- **有限参数域严格守卫**：
  在调用 SDK 最近点前，严密校验曲线的 `StartParam`、`EndParam` 及由此确定的曲线正长度；明确拦截并拒绝无界曲线 `Ray` 与 `Xline`。

### 1.3 邻近范围组合：原始外框与并查集连通分量
- **消除合并框虚假滚雪球（Union Ballooning Bug）**：
  原 Zfgk 的范围合并逻辑在每合并两个包围框后，用合并生成的大包围框继续去寻找邻近对象。
  **第一性原理推导**：两个不相邻的物体 A 与 B 之间如果仅相差一点，合并成一个大外框后，该大框内部可能充满空白区域；如果继续用该外包框去吸纳位于空白内部的独立物体 C，就会将原本毫无几何关联的 C 强行合并进来（虚假桥接）。
  本实现彻底修正此逻辑：**邻接边只建立在原始范围之间**。
- **算法复杂度与数据结构**：
  1. 采用并查集（Disjoint-Set / Union-Find）维护连通分量，带路径压缩与按秩合并（Rank Optimization）；
  2. 成对判断两个原始轴对齐矩形的二维最短欧氏距离是否 $\le maximumGap$；
  3. 时间复杂度为 $O(n^2)$，额外空间为 $O(n)$，非常适合批次（几千以内）包围框的聚类分析。

---

## 2. 对象生命周期与内存安全性

1. **纯读索引映射**：
   - `PointOrderingBeta`、`CurveOrderingExBeta` 返回的均是纯整数索引快照 `IReadOnlyList<int>`；
   - 调用者依照返回的索引对其持有的业务对象进行重新排序，Fox 核心不持有、不克隆、不修改调用者的业务集合。
2. **曲线零副作用**：
   - `GetOrderAlongCurveBeta` 借用曲线执行 `GetClosestPointTo`，不修改曲线，不启动事务，不改变曲线内部参数。
3. **范围结果不可变快照**：
   - `ExtentGroupingBeta.GroupByProximity` 返回只读的 `ExtentGroupBeta` 列表，内部成员索引数组通过 `Array.AsReadOnly((int[])group.Indices.Clone())` 隔离，保证后续查询不受外界改动影响。

---

## 3. 数值边界复核与已知限制

- **大坐标平方溢出防护**：
  在 `WithinGap` 计算两矩形距离时：
  ```csharp
  var x = Math.Max(0, Math.Max(a.MinX - b.MaxX, b.MinX - a.MaxX));
  var y = Math.Max(0, Math.Max(a.MinY - b.MaxY, b.MinY - a.MaxY));
  if (x > gap || y > gap) return false;
  if (gap == 0) return true;
  var largest = Math.Max(x, y);
  if (largest == 0) return true;
  var ratio = Math.Min(x, y) / largest;
  return largest * Math.Sqrt(1 + ratio * ratio) <= gap;
  ```
  采用缩放斜边（Scaled Hypotenuse）计算 $\sqrt{x^2 + y^2}$，彻底避免了极大坐标或超大间距时直接计算 $x^2 + y^2$ 造成的双精度溢出。
- **闭合曲线与多解最近点已知边界**：
  当查询点位于闭合曲线（如正圆）几何中心时，到曲线上所有点的距离均精确相等。代码在指南与 XML 注释中诚实说明“多解沿用宿主 SDK 的选择结果，相等里程保留原输入顺序”。

---

## 4. 验证证据与测试覆盖

- **生产核心独立测试 (`OrderingBeta.CoreTests`)**：
  - 运行命令：`dotnet run --project tests/OrderingBeta.CoreTests/OrderingBeta.CoreTests.csproj --configuration Release`
  - 实际结果：**29 项测试检查全部 PASS**。
  - 覆盖场景：非传递性容差边界点（0, 0.75, 1.5）、输入点置换不变性、相等坐标原序保持、重复里程排序、合并外包框不虚假吸收第三者、非有限坐标/负容差拒绝、极大有限坐标防溢出等。
- **宿主集成命令 (`TestOrderingBeta`)**：
  - 覆盖有限直线端点投影、WCS 点、闭合轻量多段线接缝、`Ray` 拦截、SDK 范围结果比对。
  - 状态：编译通过，CAD 宿主实际运行依规保持为 `Not run`。

---

## 5. 评审结论

- **结论**：**通过（Approved）**。
- **评价**：空间排序与范围组合模块精准纠正了历史代码中违背严格弱序、人为扰动真实里程、外包框虚假膨胀等经典“反模式”，算法确定性与数学严谨性极高。
