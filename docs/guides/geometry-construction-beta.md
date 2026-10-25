# 三角形与多面网格 Beta

> 状态：current；描述 Beta API 契约，不代表真实 CAD 宿主已验收。

## 三角形

[`Triangle3dBeta`](../../src/CADShared/Cad/Geometry/Triangle3dBeta.cs)保存三个有限坐标的只读顶点。`Centroid` 是包含 Z 的算术平均；重复或共线顶点仍可求重心。

`TryGetSlopeAndAspect` 返回坡度角（度），坡向按 +Y 为北、+X 为东、从北顺时针计量下坡方向。顶点绕序不影响结果。水平面成功返回坡度 0、坡向 NaN；共线、重复、近垂直或数值精度不足时返回 false，两个输出均为 NaN。容差必须为 (0,1) 中的有限数值：退化判断使用两倍面积与最长边平方的比值，近水平/垂直判断使用正弦阈值，均与首顶点选择无关。

该类不操作数据库。数值核心通过缩放后计算避免普通大、小尺度输入的溢出和下溢；不能从浮点数已经丢失的坐标差恢复精度。

## 多面网格

[`PolyFaceMeshBuilderBeta.Append`](../../src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs)接收显式数据库、该库当前最上层事务、目标块表记录 ID、WCS 顶点及从零开始的面索引。顶点和面各最多 32767 项；只接受三角面与四边面，各面索引必须有效且不重复。输入被完整验证和快照后才写入。

三角面的原生第四索引为零；全部顶点先于面记录追加。这遵循 Autodesk 的 [FaceRecord 构造规则](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_FaceRecord_FaceRecord_short_short_short_short.html)和 [AppendFaceRecord 生命周期](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_PolyFaceMesh_AppendFaceRecord_FaceRecord.html)。

调用方负责提交或中止事务。输入验证错误尚未写入；宿主在追加过程中报错时，调用方必须中止事务。构造器不提供事务级的局部回滚，不持有或释放调用方数据库/事务，不改变 WorkingDatabase。网格外观使用目标数据库默认值，全部边可见；不验证面的几何退化、自交或四边面共面性，不自动三角化。

旋转标注复用 SDK 和现有落库接口，本次不增加薄封装，也不迁入原库默认用固定 F2 文本覆盖实际测量值的行为。

## 验证

- [生产数值核心测试](../../tests/GeometryBeta.CoreTests/GeometryBeta.CoreTests.csproj)直接链接生产源码，覆盖坡度单位、坡向、绕序、退化、极端尺度、网格索引转换和完整预校验。
- [宿主命令](../../tests/TestShared/TestGeometryConstructionBeta.cs)：`Test_GeometryConstructionBeta`，使用临时数据库并中止事务，验证对象归属和失败不写入。
- 实际 CAD 宿主执行：`Not run`。构建矩阵与最终检查记录见[迁移清单](../zfgk-migration-inventory.md)。
