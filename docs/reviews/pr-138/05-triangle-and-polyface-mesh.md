# PR #138 模块专项评审报告：三维几何与多面网格构造

**评审模块**：三角形数学（坡度/坡向/重心）与多面网格构造 Beta
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Geometry\TriangleMathBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Geometry/TriangleMathBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Geometry\Triangle3dBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Geometry/Triangle3dBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Meshes\PolyFaceMeshDataBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshDataBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Meshes\PolyFaceMeshBuilderBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\GeometryBeta.CoreTests\Program.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/GeometryBeta.CoreTests/Program.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestGeometryConstructionBeta.cs`](file:///D:/MakeX/Fs.Fox.CAD/tests/TestShared/TestGeometryConstructionBeta.cs)
**对应文档**：[`docs/guides/geometry-construction-beta.md`](file:///D:/MakeX/Fs.Fox.CAD/docs/guides/geometry-construction-beta.md)

---

## 1. 算法与数学分析（第一性原理）

### 1.1 三角形重心：Neumaier 补偿求和与 Z 轴保真
- **历史缺陷（Zfgk 坏味道）**：
  原 `Fs.Zfgk.CAD` 中的 `GeTriangle` 在求重心时直接忽略了三维顶点的 Z 轴（将其强制投影为 0），或者在坐标绝对值很大且相对差很小时产生严重的浮点相消。
- **本实现重构方案**：
  采用 **Neumaier 补偿求和算法（Compensated Summation）**：
  在累加三个坐标分量时实时追踪尾数低位舍入误差（`error`），在处理大坐标小相对位移或坐标正负相消时，精度达到浮点双精度极限；严格保留完整的 Z 轴三维算术平均，退化或共线顶点依然可稳定计算重心。

### 1.2 三角形坡度（Slope）与坡向（Aspect）
在 GIS 地形分析与土方工程中，坡度与坡向定义具有严格行业规范：
- **向上法向量与单值地形面**：
  取三边向量 $u = B - A, v = C - A, w = C - B$。先除以三边最大绝对分量实现归一化缩放，防止大坐标相乘溢出。
  计算法向量 $\vec{N} = \vec{u} \times \vec{v}$。若 $N_z < 0$，取相反向量 $-\vec{N}$，确保法向朝上（Upward normal）。
- **退化与近垂直判定（无量纲相对容差）**：
  退化判定公式为：
  $$2 \times \text{Area} = \|\vec{N}\| \le \text{LongestEdge}^2 \times relativeTolerance$$
  该判定指标在顶点的全部 6 种置换排列下严格不变；近垂直平面判定：$|N_z| \le \|\vec{N}\| \times relativeTolerance$（垂直面无法构成单值地形面 $z(x,y)$，返回 false 并将坡度坡向置为 NaN）。
- **坡度计算与上界保护**：
  水平分量 $horizontal = \sqrt{N_x^2 + N_y^2}$。
  若 $horizontal \le \|\vec{N}\| \times relativeTolerance$，为严格水平面：坡度返回 $0^\circ$，坡向返回 `NaN`。
  否则 $slope = \text{atan2}(horizontal, N_z) \times 180 / \pi$。
  为防止由于浮点舍入使极陡峭法线产生 $90.00000000000001^\circ$ 的超界数值，代码设有严格防御：`if (slope >= 90) return false;`，保证成功返回的坡度严格位于 $[0, 90)^\circ$。
- **坡向（下坡方位角）与正北 360/0 归一**：
  由向上法向量的水平投影 $(N_x, N_y)$ 代表最陡下坡方向。
  以正北（+Y）为 $0^\circ$、顺时针向东（+X）为 $90^\circ$ 计量：
  $$aspect = \text{atan2}(N_x, N_y) \times 180 / \pi$$
  若为负则加 $360^\circ$。关键边界防护：若微小负角度加 $360$ 后因浮点舍入成为 $360.0^\circ$，或者为负零 $-0.0$，代码统一归一化为标准的 $0.0^\circ$。

### 1.3 多面网格（PolyFaceMesh）构造：完整预验证
- **惰性集合完整先期求值（Exhaust Lazy Inputs First）**：
  在向块表记录写入任何网格图元之前，代码先调用 `PolyFaceMeshDataBeta.ValidateAndCopyFaces` 将传入的 `IEnumerable<IEnumerable<int>> faces` 完整枚举并快照为内存数组。
  - **关键防范**：如果调用方传入的惰性序列在枚举到第 100 个面时抛出异常，此时数据库写事务**尚未打开，没有任何半成品图元写入**，事务保持纯净，绝不留下悬空网格。
- **AutoCAD 1-based 索引转换与三角面规则**：
  AutoCAD 原生 `FaceRecord` 采用 1-based 索引（顶点编号从 1 开始）；三角面的第四个索引在底层必须置 0。代码在写入前统一完成校验并置 0。
- **数量硬上限防护**：
  AutoCAD 经典 `PolyFaceMesh` 顶点与面数内部受 `short.MaxValue`（32767）限制，超出范围在预验证阶段即被拦截。

---

## 2. CAD 对象生命周期与事务安全性审查

### 2.1 顶层事务校验与所有权边界
- **事务归属**：
  `PolyFaceMeshBuilderBeta.Append` 严格校验 `database.TransactionManager.TopTransaction == transaction` 以及目标空间的数据库归属，拒绝子事务与跨库写入；
- **回滚权归属调用方**：
  代码不自行调用 `Commit()` 或 `Abort()`。网格实体及其所有的 `PolyFaceMeshVertex`、`FaceRecord` 子实体均通过 `AddNewlyCreatedDBObject(..., true)` 注册进调用方的同一事务中。若后续操作失败，由外层事务统一整体中止回滚。

### 2.2 审查发现与探针标记

---

### Finding 2 [P2 风险 / 待宿主确认项]: 网格构造器在登记事务后对网格及子图元保留 `using var` 显式释放模式

- **定位**：[`PolyFaceMeshBuilderBeta.cs#L56-L75`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs#L56-L75)
- **机制与分析**：
  代码对网格父实体和每个生成的顶点、面记录均使用了 `using var`：
  ```csharp
  using (target.ForWrite())
  using (var mesh = new PolyFaceMesh())
  {
      target.AppendEntity(mesh);
      transaction.AddNewlyCreatedDBObject(mesh, true);
      foreach (var point in points)
      {
          using var vertex = new PolyFaceMeshVertex(point);
          mesh.AppendVertex(vertex);
          transaction.AddNewlyCreatedDBObject(vertex, true);
      }
      foreach (var indices in faceData)
      {
          using var face = new FaceRecord(indices[0], indices[1], indices[2], indices[3]);
          mesh.AppendFaceRecord(face);
          transaction.AddNewlyCreatedDBObject(face, true);
      }
      return id;
  }
  ```
  在 AutoCAD 原生 .NET SDK 中，此写法符合官方 SDK 示例（`Dispose()` 仅释放托管包装器，底层非托管 C++ 对象的生命周期由 Transaction 托管）。
  但根据 CAD 协作规则与第一性原理，部分第三方 CAD 宿主（如 ZWCAD、GStarCAD）在底层桥接层处理已落库图元的显式 `Dispose` 时可能存在非托管句柄提前清空的风险。
- **处理结论（已与用户对齐）**：
  **代码保持现状**。该项在本次评审中列为**待宿主确认项**；在后续实施真实 CAD 宿主验收时，将其作为重点探针项目核验，重点检查在国产 CAD 宿主下事务执行 `Commit()` 后的持久化稳定性。

---

## 3. 验证证据与测试覆盖

- **生产核心独立测试 (`GeometryBeta.CoreTests`)**：
  - 运行命令：`dotnet run --project tests/GeometryBeta.CoreTests/GeometryBeta.CoreTests.csproj --configuration Release`
  - 实际结果：**85 项离散断言全部 PASS**。
  - 覆盖场景：Neumaier 补偿重心求和、坡度坡向六种顶点排列不变性、接近垂直与退化判定、近正北 360/0 归一、面索引预验证（三角面第 4 索引置零、四边面、索引越界、面内重复索引、面数/顶点数超出 32767 上限等）。
- **宿主集成命令 (`TestGeometryConstructionBeta`)**：
  - 覆盖临时数据库、`TopTransaction` 身份核验、惰性面枚举失败不写入且事务保持、模型空间网格落库与子 FaceRecord 读取核对。
  - 状态：编译通过，真实 CAD 宿主执行状态依规保持为 `Not run`。

---

## 4. 评审结论

- **结论**：**通过（Approved）**。
- **评价**：数值算法在地形学与 GIS 坡度/坡向标准上极其严密，解决了历史遗留的 Z 轴丢失与浮点溢出；网格构造的前置校验机制有效杜绝了数据库半写污染。
