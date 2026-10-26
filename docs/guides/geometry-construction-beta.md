# 三角形与多面网格 Beta

> 状态：current；描述 Beta API 契约，不代表真实 CAD 宿主已验收。

## 三角形

[`Triangle3dBeta`](../../src/CADShared/Cad/Geometry/Triangle3dBeta.cs)保存三个有限坐标的只读顶点。`Centroid` 是包含 Z 的算术平均；重复或共线顶点仍可求重心。

`TryGetSlopeAndAspectDegrees` 返回坡度角（度），坡向按 +Y 为北、+X 为东、从北顺时针计量下坡方向。顶点绕序不影响结果。水平面成功返回坡度 0、坡向 NaN；共线、重复、近垂直或数值精度不足时返回 false，两个输出均为 NaN。容差必须为 (0,1) 中的有限数值：退化判断使用两倍面积与最长边平方的比值，近水平/垂直判断使用正弦阈值，均与首顶点选择无关。

坡度的成功范围严格为 [0,90)，坡向为 [0,360)。近北坡向加一周后若舍入为 360，归一为 0；非垂直法线若因精度不足使坡度舍入为 90，则返回 false 并保留两个 NaN。

该类不操作数据库。数值核心先对三条边使用共同缩放，再对法向量及其水平分量按最大分量缩放求模，避免小分量平方下溢使有效瘦三角形被误判退化、微小倾斜被误判水平。该处理不能从浮点数已经丢失的坐标差恢复精度。

## 多面网格

[`PolyFaceMeshFactoryBeta.CreateAndAppend`](../../src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshFactoryBeta.cs)接收显式数据库、该库当前最上层事务、目标块表记录 ID、顶点及从零开始的面索引。顶点按目标块表记录的存储坐标直接写入；普通块定义中的点是定义局部坐标，方法不执行 WCS 到定义坐标的转换。顶点和面各最多 32767 项；只接受三角面与四边面，各面索引必须有效且不重复。输入被完整验证和快照后才写入。

三角面的原生第四索引为零；全部顶点先于面记录追加。这遵循 Autodesk 的 [FaceRecord 构造规则](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_FaceRecord_FaceRecord_short_short_short_short.html)和 [AppendFaceRecord 生命周期](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_PolyFaceMesh_AppendFaceRecord_FaceRecord.html)。

调用方负责提交或中止事务。输入验证错误尚未写入；宿主在追加过程中报错时，调用方必须中止事务。构造器不提供事务级的局部回滚，不持有或释放调用方数据库/事务，不改变 WorkingDatabase。网格外观使用目标数据库默认值，全部边可见；不验证面的几何退化、自交或四边面共面性，不自动三角化。

旋转标注继续使用宿主 SDK 和现有落库接口。

## 验证

2026-10-26 生产数值核心实测 **121 项断言通过**；新增两类平方下溢反例均覆盖六种顶点排列。实际数据库写入与子对象生命周期仍须分别验收。

- [生产数值核心测试](../../tests/GeometryBeta.CoreTests/GeometryBeta.CoreTests.csproj)直接链接生产源码，覆盖坡度单位、坡向、绕序、退化、极端尺度、法向量／水平分量平方下溢、网格索引转换和完整预校验。
- [宿主命令](../../tests/TestShared/TestGeometryConstructionBeta.cs)：`Test_GeometryConstructionBeta`，使用临时数据库并中止事务，验证对象归属；异库事务/空间、非最上层事务和惰性 faces 中途抛错均应保持原事务且不写入，枚举异常原样传播。
- 实际 CAD 宿主执行：`Not run`。构建矩阵与最终检查记录见[实现与验证记录](../beta-implementation-notes.md)。
