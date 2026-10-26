# PR #138 模块专项评审报告：多段线核心操作

> 状态：historical；针对 `4064025` 的专项静态评审快照。<br>
> 仅供追溯，不是现行技术契约；测试数字沿用原评审者记录，非本轮重跑。使用限制以[当前指南](../../guides/polyline-beta.md)为准，宿主验收 `Not run`。

**评审模块**：多段线几何操作 Beta（反向、清理、两两连接、整线圆角）
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Curves\Polylines\PolylineCoreBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineCoreBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Curves\Polylines\PolylineExBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineExBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\PolylineBeta.CoreTests\Program.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/PolylineBeta.CoreTests/Program.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestPolylineBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/TestShared/TestPolylineBeta.cs)
**对应文档**：[`docs/guides/polyline-beta.md`](file:///D:/MakeX/Fs.Fox.CAD/docs/guides/polyline-beta.md)

---

## 1. 算法与数学分析（第一性原理）

### 1.1 反向（`Reverse`）
- **数据结构与语义**：AutoCAD 轻量多段线中，顶点的 `Bulge`、`StartWidth`、`EndWidth` 描述的是**从该顶点出发、指向下一个顶点的出段（outgoing segment）**。
- **开线反向推导**：
  原顶点序列为 $V_0, V_1, \dots, V_{n-1}$。反向后新顶点序列为 $V'_{0}, \dots, V'_{n-1}$，其中 $V'_i$ 对应的原几何位置为 $V_{n-1-i}$。
  新起点 $V'_0$（原 $V_{n-1}$）的出段对应原先由 $V_{n-2}$ 到 $V_{n-1}$ 的入段。因此其凸度为原段凸度的相反数 $-Bulge_{n-2}$，起止宽度对调（原 $EndWidth_{n-2} \to StartWidth'_0$, 原 $StartWidth_{n-2} \to EndWidth'_0$）。
  针对开线末点未参与构段的元数据，代码在 `i == result.Length - 1` 时完整保留（`result[i] = new PolylineVertexDataBeta(point.X, point.Y, terminal.Bulge, terminal.StartWidth, terminal.EndWidth)`），使得连续两次反向严格满足对合性（Involution），即 `Reverse(Reverse(poly)) == poly`。
- **闭线反向推导**：
  闭合多段线在 CAD 中通常以首顶点作为参照基准。代码保持原首顶点 $V_0$ 索引不变，将其后继点逆序重排（$V_1$ 与 $V_{n-1}$ 互换等）。闭合段由原 $V_{n-1} \to V_0$ 变为 $V_0 \to V_{n-1}$，逻辑对称。

### 1.2 清理（`Clean`）
- **两阶段策略**：
  1. *精确零段剔除*：仅移除完全重合点之间的直线段（`SamePoint`），并将后继段属性正确传递给紧缩顶点；若圆弧两端精确重合直接拒绝（`Bulge != 0` 抛出 `InvalidOperationException`，防止零弦圆弧除零）。
  2. *半容差带直线合并*：
     传统逐点贪心合并会导致误差无界累加；而 Douglas-Peucker 算法需要递归且对回折线敏感。
     本实现采用**固定参考线半容差带（Half-tolerance band）**算法：
     以锚点段单位向量 $(u_x, u_y)$ 为基准，中间被替代顶点到基准线的法向距离控制在 $\le tolerance \times 0.5$；
     由于最终合并段端点与各中间点均在基准线两侧半容差带内，由三角不等式保证：任何被移除的原顶点到最终直线段的垂直距离严格 $\le tolerance$。
  3. *局部折返保护*：
     通过点积 `forwardTurn = previousDirectionX * directionX + previousDirectionY * directionY` 检查转向。当局部转向 $\ge 90^\circ$（`forwardTurn <= 0`）或投影距离不再单调递增（`projection <= previousProjection`）时强制截断合并，有效保护了在狭窄容差带内的斜向折返与回折拐角。

### 1.3 连接（`TryJoin`）
- **唯一匹配原则**：严格拒绝存在歧义的输入。仅当两开线的 4 组端点距离中恰有 1 组 $\le tolerance$ 时方允许连接。
- **圆弧端点守恒**：当端点存在非零微小间隙（$0 < dist \le tolerance$）时，**绝不移动或平移原圆弧端点**（否则会改变圆弧半径或圆心位置），而是显式插入一段直线过渡段，其宽度线性插值（从前段终宽平滑过渡到后段起宽）。

### 1.4 整线圆角（`Fillet`）
- **切点与回退量精确推导**：
  设两相邻线段夹角转角为 $\theta$，转向叉积为 $cross = d_1 \times d_2$，点积为 $dot = d_1 \cdot d_2$。
  角平分线回退距离公式为：
  $$retreat = R \cdot \tan\left(\frac{|\theta|}{2}\right)$$
  为防止当 $\theta \to 180^\circ$（折返角）时分母产生浮点下溢或抵消误差，算法采用条件半角恒等式：
  $$tangentHalf = \begin{cases} \frac{|cross|}{\sqrt{cross^2 + dot^2} + dot}, & dot \ge 0 \\ \frac{\sqrt{cross^2 + dot^2} - dot}{|cross|}, & dot < 0 \end{cases}$$
  凸度精确公式为 $Bulge = \tan(\theta / 4)$。
- **全线共享边预算统筹（Shared-edge budget）**：
  很多历史实现（包括原 Zfgk）采用逐角局部圆角，导致一条边两端的圆角相互重叠踩踏，或隐式缩小半径。
  本实现先完成全部拐角的规划，并严格执行全线原边长度校验：
  $$lengths[i] - retreats[i] - retreats[(i+1)\%n] > tolerance$$
  若两端切点恰好相接（$remaining = 0$）或重叠，坚决整体拒绝，绝不静默缩减半径。
- **全线闭合首末角处理**：
  闭合多段线的首末拐角（索引 0 处）被完全纳入统筹规划；首角圆角后以其入切点作为输出多段线的新索引 0 顶点。

---

## 2. 对象生命周期与内存安全性

1. **借用语义（Borrow semantics）**：
   `PolylineExBeta` 的所有方法（`CreateReversedBeta`、`CreateCleanedBeta`、`CreateFilletedBeta`、`TryCreateJoinedBeta`）均对输入 `source` 实体执行只读借用。不调用任何升级写方法，不启动事务，不直接修改输入。
2. **显式异常释放**：
   在输出几何创建逻辑 `CreateGeometry` 中：
   ```csharp
   var result = new Polyline(vertices.Count);
   try
   {
       result.Normal = source.Normal;
       result.Elevation = source.Elevation;
       result.Thickness = source.Thickness;
       // ... 填充顶点 ...
       result.Closed = closed;
       return result;
   }
   catch
   {
       result.Dispose();
       throw;
   }
   ```
   如果构造中途发生异常（例如顶点数据导致 CAD 内部报错），立即通过 `catch { result.Dispose(); throw; }` 释放半成品托管包装与底层非托管实体，防止内存泄漏。
3. **几何与非几何字段清晰划分**：
   明确复制且仅复制几何定义字段：`Normal`、`Elevation`、`Thickness`、`Closed`、`Point2d`、`Bulge`、`StartWidth`、`EndWidth`。不复制图层、颜色、扩展数据（XData）或句柄，避免伪克隆破坏数据库唯一性。

---

## 3. 数值边界与防御性机制复核

- **浮点下溢防护**：
  在圆角方向计算中，代码在归一化前先按分量最大值缩放：
  ```csharp
  var scale = Math.Max(Math.Abs(dx), Math.Abs(dy));
  directionsX[i] = dx / scale;
  directionsY[i] = dy / scale;
  if ((dx != 0 && directionsX[i] == 0) || (dy != 0 && directionsY[i] == 0))
      throw new InvalidOperationException("A nonzero segment direction component cannot be represented at this scale.");
  ```
  在微小尺度图形下，有效防止了非零分量下溢为 0 导致真实转角丢失。
- **半径精度保真重验**：
  计算出切点坐标后，代码逆向根据弦长和凸度重算实际圆弧半径：
  `actualRadius = (chord * 0.25 / absoluteBulge) * (1 + absoluteBulge * absoluteBulge)`，
  若由于坐标绝对值过大导致舍入误差使 `Math.Abs(actualRadius - radius) > FilletPrecision(radius, tolerance)`，立即抛出异常拒绝，确保“不承诺生成不准确半径”。

---

## 4. 验证证据与测试覆盖

- **生产核心测试 (`PolylineBeta.CoreTests`)**：
  - 运行命令：`dotnet run --project tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj --configuration Release`
  - 实际结果：**19 个测试场景、2929 项离散断言全部 PASS**。
  - 覆盖场景：随机生成的 100 次反向对合回归、开线两端宽度保留、闭线首点锚定、狭长容差带折返保留、四种连接端点组合、变宽直线桥接段、顺逆时针圆角、凹角圆角、首末角圆角、共享边预算超标、小尺度切点不可表示等。
- **宿主集成命令 (`TestPolylineBeta`)**：
  - 覆盖非默认 OCS 平面（`Normal = Vector3d.YAxis, Elevation = 7`）、只读已入库多段线借用验证。
  - 状态：编译通过，CAD 宿主实际运行依规保持为 `Not run`。

---

## 5. 评审结论与建议

- **结论**：**通过（Approved）**。
- **边界**：原评审在静态范围内未报告新增缺陷，核心回归记录覆盖反向、清理、连接和圆角约束；实际 CAD 对象行为仍为 `Not run`。
