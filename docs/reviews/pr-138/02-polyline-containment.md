# PR #138 模块专项评审报告：含弧闭合多段线点关系

> 状态：historical；针对 `4064025` 的专项静态评审快照，2026-10-26 更正。<br>
> 仅供追溯，不是现行技术契约；测试数字沿用原评审者记录，非本轮重跑。使用限制以[当前指南](../../guides/polyline-beta.md)为准，宿主验收 `Not run`。
> 后续完整审查与修正见[实现记录](../../beta-implementation-notes.md#完整核心审查与修正)。

**评审模块**：含弧闭合多段线点关系查询（`GetPointRelationBeta`）
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Curves\Polylines\PolylineContainmentCoreBeta.cs`](../../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineContainmentCoreBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Curves\Polylines\PolylineRegionExBeta.cs`](../../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineContainmentExtensionsBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\PolylineRegionBeta.CoreTests\Program.cs`](../../../tests/PolylineContainmentBeta.CoreTests/Program.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestPolylineBeta.cs`](../../../tests/TestShared/TestPolylineBeta.cs)
**对应文档**：[`docs/guides/polyline-beta.md`](../../../docs/guides/polyline-beta.md)

---

## 1. 算法与数学分析（第一性原理）

### 1.1 区域查询的几何与确定性约束
含弧闭合多段线的区域查询需要满足以下约束：
1. **保留圆弧语义**：按凸度解析计算圆弧，避免采样折线改变大半径浅弧附近的内外关系；
2. **确定性计数**：端点和切点使用统一的半开区间规则，不依赖随机射线重试；
3. **处理 OCS 平面**：先校验 Normal、Elevation 与容差，再区分平面外距离与平面内关系。

### 1.2 本实现解析几何数学推导

本实现基于**有限解析几何（Finite Analytic Crossings）**与**确定性射线穿透（Ray Casting）**，不依赖 AutoCAD Brep / Region 宿主组件：

#### (1) 弦相对坐标系与分离展开圆方程
设圆弧起点为 $A$，终点为 $B$，弦长为 $L = \|B - A\|$，弦方向单位向量为 $\vec{u}$。
以 $A$ 为原点建立弦坐标系 $(t, n)$，其中 $t$ 为沿弦切向投影，$n$ 为法向投影。
标准圆弧凸度为 $b$（对应圆心角包角 $\theta = 4 \arctan b$）。
经过严密代数推导，弦两端过 $(0,0)$ 与 $(1,0)$ 的圆方程展开为：
$$f(t, n) = t(t - 1) + n\left(n + \frac{b}{2}\right) - n \cdot \frac{1}{2b} = 0$$
- **分离式优势**：将 $b$ 与 $1/b$ 分离保留，避免了先求圆心 $(c_x, c_y)$ 与半径 $R$ 时造成的巨额消去误差（Catastrophic Cancellation）。
- **极值矢高有理化**：针对浅弧（$b \to 0$），矢高 $s \approx b L / 2$ 极小，若直接计算 $c_y - R$ 会使双精度尾数全部归零。算法对圆上下边界极值采用分子有理化公式：
  $$\text{Upper sagitta} = \frac{c_x^2}{R - c_y}, \quad \text{Lower sagitta} = \frac{-c_x^2}{R + c_y}$$
  该公式避开浅弧极值计算中的近值相减，结果仍受输入精度和浮点舍入影响。

#### (2) Y 单调划分与二次方程求根
- **单调弧划分**：横向射线与圆弧求交的关键是单值性。算法以圆极值点（切线水平处，即导数 $dY/dt = 0$ 的角度 $\pi/2$ 与 $3\pi/2$）为分界节点，将圆弧精确划分为最多 3 段 **Y 单调子弧**（`ArcPart`）。
- **半开 Y 区间（Half-open Y interval）**：
  判定条件 `(part.StartY > qy) == (part.EndY > qy)` 严格实现 $[StartY, EndY)$ 的半开计数。若射线恰好穿过圆弧端点或切线极值点，切向极值计数两次（或零次），奇偶性不反转；端点仅被属于其出段的子弧计数一次，彻底消除了交点重计与切点漂移问题。
- **高稳定二次根（Stable Quadratic Formula）**：
  射线方程在弦坐标下为 $qy = const$。代入圆方程解横坐标 $qx$：
  $$qx^2 - 2c_x qx + C = 0, \quad \text{其中 } C = qy(qy - u_y + \frac{b}{2}u_x) - qy \frac{1}{2b} u_x$$
  为防止根在原点附近时发生减法抵消，采用稳定根乘积公式：
  $$x_{far} = c_x \ge 0 ? (c_x + \sqrt{D}) : (c_x - \sqrt{D}), \quad x_{near} = \frac{C}{x_{far}}$$
  该形式降低近根的消去误差，不代表所有输入都能获得相同的绝对精度。

#### (3) 边界优先与偶奇填充（Even-Odd Rule）
- 边界判定优先于内外判定：先计算查询点到圆弧/线段的最短投影距离是否 $\le tolerance$；若在容差带内直接判定为 `Boundary`。
- 多重自交轮廓、孔洞嵌套、重复绕行采用拓扑标准的**偶奇填充规则（Even-Odd Rule）**，穿透次数为奇数判定为 `Inside`，偶数判定为 `Outside`。

---

## 2. 空间平面与 WCS/OCS 映射安全性

在 CAD 二次开发中，轻量多段线（`Polyline`）始终属于 2D ECS/OCS 几何：
1. **WCS 到 OCS 投影严格保序**：
   ```csharp
   var ocs = point.TransformBy(Matrix3d.WorldToPlane(normal));
   if (Math.Abs(ocs.Z - elevation) > tolerance)
       return PolylinePointRelationBeta.OffPlane;
   ```
   代码遵循 AutoCAD Arbitrary Axis Algorithm（任意轴算法），使用 `Matrix3d.WorldToPlane(normal)` 将 WCS 点投影到基准平面。
2. **容差解耦（平面距离 vs 平面内边界距离）**：
   点到多段线平面的法向距离与平面内的边界容差采用同一参数但在不同维度独立判定：当点到平面的法向距离 $> tolerance$ 时直接返回 `OffPlane`；未超出平面的点投影到 OCS 中计算平面内的边界距离。两者不混淆为单一的 3D 欧氏距离。
3. **源验证先于平面外过滤（Fail-fast 契约）**：
   在判断 `OffPlane` 之前，代码必须先完整解析源多段线的全部顶点、凸度并建立 Loop。如果源多段线存在非法凸度或退化几何，即使传入的点在数光年之外的平面外，依然保证抛出数据非法异常，防止因点位置偶然隐藏数据缺陷。

---

## 3. 数值边界复核与已知限制

- **非零绝对凸度范围**：
  显式限定为 **`[1e-8, 1e8]`**。
  - $|b| < 1e-8$：超出当前解析实现的支持范围，明确抛出 `NotSupportedException`，不以弦静默替代。bulge 无量纲，实际矢高还取决于弦长，不能单凭该阈值断言纳米级误差；
  - $|b| > 1e8$：接近整圆的包角超出当前实现的支持范围，明确拒绝；不能仅凭 bulge 推断实际半径或曲率大小；
  - 契约明确，不猜测、不静默改写。
- **大坐标系下的浮点溢出守卫**：
  `Segment.Query` 具有局部坐标有限性检查及其他数值守卫。它们能拒绝非有限值和显式不支持的数值状态，不能检测所有结果仍为有限值的精度损失，也不能保证在某一地理坐标量级必然抛出 `InvalidOperationException`。已有尺度／旋转回归只证明其覆盖的输入，不能替代任意尺度的误差界。

---

## 4. 验证证据与测试覆盖

- **生产核心独立测试 (`PolylineRegionBeta.CoreTests`)**：
  - 运行命令：`dotnet run --project tests/PolylineRegionBeta.CoreTests/PolylineRegionBeta.CoreTests.csproj --configuration Release`
  - 实际结果：**10 个测试场景、80,885 项离散断言全部 PASS**。
  - 关键测试亮点：
    - **独立半平面圆盘裁切比对（Oracle Comparison）**：测试工程引入了独立于生产代码的“解析圆盘半平面布尔求交”真值判定器，对任意旋转、平移、缩放、正负凸度的大批量随机点进行了密集真值断言（断言数达 8 万+）；
    - 覆盖双半圆构成的完整圆、主弧（Major Arc, $|b| > 1$）、浅弧（$b = 1e-8$）、水平切线穿越、极值点穿透、旋转与反向不变性等。
- **宿主集成命令 (`TestPolylineBeta.Test_PolylineRegionBeta`)**：
  - 覆盖非默认 OCS、标高与厚度实体、只读已入库多段线查询。
  - 状态：编译通过，真实 CAD 宿主执行状态依规保持为 `Not run`。

---

## 5. 评审结论与建议

- **结论**：**通过（Approved）**。
- **边界**：原评审记录了独立解析真值回归；其覆盖输入之外仍须考虑数值支持范围和误差，实际 OCS／数据库场景为 `Not run`。
