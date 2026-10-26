# PR #138 模块专项评审报告：三维几何与多面网格构造

> 状态：historical；针对 `4064025` 的专项静态评审快照，2026-10-26 更正。<br>
> 仅供追溯，不是现行技术契约；测试数字沿用原评审者记录，非本轮重跑。使用限制以[当前指南](../../guides/geometry-construction-beta.md)为准，宿主验收 `Not run`。
> 后续完整审查与修正见[实现记录](../../beta-implementation-notes.md#完整核心审查与修正)。

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
- **三维坐标与求和精度**：
  重心需要分别计算三个坐标分量的算术平均。大坐标叠加小偏移或正负分量相消时，直接求和可能丢失低位有效数字。
- **当前计算方式**：
  采用 **Neumaier 补偿求和算法（Compensated Summation）**：
  在累加时追踪低位舍入误差（`error`），降低有效数字丢失；计算保留完整的 XYZ 分量，退化或共线顶点仍可计算重心。结果仍受输入精度和浮点舍入影响。

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
  - **关键防范**：如果惰性面序列在枚举中途抛出异常，构造器尚未打开目标空间写入或追加网格。调用方事务已经存在，但本次调用尚未向它登记半成品图元。
- **AutoCAD 1-based 索引转换与三角面规则**：
  AutoCAD 原生 `FaceRecord` 采用 1-based 索引（顶点编号从 1 开始）；三角面的第四个索引在底层必须置 0。代码在写入前统一完成校验并置 0。
- **数量硬上限防护**：
  AutoCAD 经典 `PolyFaceMesh` 顶点与面数内部受 `short.MaxValue`（32767）限制，超出范围在预验证阶段即被拦截。

---

## 2. CAD 对象生命周期与事务安全性审查

### 2.1 顶层事务校验与所有权边界
- **事务归属**：
  `PolyFaceMeshBuilderBeta.Append` 校验 `database.TransactionManager.TopTransaction == transaction` 以及目标空间的数据库归属，拒绝非当前顶层事务及跨库写入；当前顶层本身可以是调用方启动的子事务；
- **回滚权归属调用方**：
  代码不自行调用 `Commit()` 或 `Abort()`。网格实体及其所有的 `PolyFaceMeshVertex`、`FaceRecord` 子实体均通过 `AddNewlyCreatedDBObject(..., true)` 注册进调用方的同一事务中。若后续操作失败，由外层事务统一整体中止回滚。

### 2.2 生产网格构造器子实体生命周期复核：保留现有 `using`

- **关注代码段**：[`PolyFaceMeshBuilderBeta.cs#L56-L75`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs#L56-L75)
- **实现模式**：
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
- **证据与边界**：[Autodesk 官方网格示例](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-DevGuide-Managed/files/GUID-6B49EE98-D513-46EB-9D7A-282F70A45B24.htm)在登记事务后仍对网格和 `FaceRecord` 使用 `using`；顶点循环并没有报告原先声称的 `using (PolyFaceMeshVertex ...)`。该示例支持保留显式释放模式，不能据此推导所有宿主的内部所有权实现完全相同。
- **撤销的推断**：没有证据支持原报告关于 `_autoDelete` 赋值及“Dispose 只释放包装器”的通用机制断言，也没有基准证明不使用 `using` 必然形成数万终结器并导致严重停顿，因此删除这些结论。
- **复核结论**：没有确认需要修改现有生产释放代码的缺陷。子对象登记、异常回滚和跨宿主生命周期仍以实际宿主回归为验收依据，当前 `Not run`。

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
- **边界**：核心回归覆盖已列出的几何和面索引输入；实际网格写入、回滚及跨宿主子对象生命周期仍为 `Not run`。
