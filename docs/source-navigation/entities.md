# 实体、曲线与填充定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-entities`<br>
> 基线与范围：[源码检索总入口](README.md)<br>
> 覆盖 `Cad/Database/Entities` 的 24 个共享编译文件；容器与事务另见[数据库分册](database.md)。

阅读入口：[对象归属](#ownership)、[块](#blocks)、[包围框](#bounds)、[曲线](#curves)、[多段线](#polylines)、[文字](#text)、[填充](#hatch)、[面域](#region)。

<a id="ownership"></a>

## 创建、落库与释放是不同步骤

| 返回或操作 | 典型入口 | 后续责任 |
| --- | --- | --- |
| 新的内存实体 | `CreateDBText`、`CreateMText`、`CreateArc`、`CreateCircle`、`CreatePolyline` | 决定目标空间并落库；不使用时释放。工厂名称不代表已写入图纸。 |
| 已追加的实体 ID | `BlockTableRecord.AddEntity`、`InsertBlock`、`HatchInfo.Build` | 原生事务需要正常完成；不要在已经完成的事务外继续使用打开对象。 |
| 打断段、几何对象或重建边界 | `CurveEx.GetSplitCurves`、`ToCurve3d`、`HatchEx.CreateBoundarys` | 检查集合中对象的所有权和异常路径，逐个决定落库或释放。 |
| 已有实体的修改 | `EntityEx.Move/Scale/Rotation/Mirror`、块属性修改 | 核对目标库、事务和写模式；坐标变换不等于复制。 |

最常见落库链：实体工厂 → `tr.CurrentSpace` / `tr.ModelSpace` 或指定块定义 → [`SymbolTableRecordEx.AddEntity`](../../src/CADShared/Cad/Database/SymbolTables/SymbolTableRecordEx.cs) → 原生事务提交。

<a id="blocks"></a>

## 块定义、块参照与动态可见性

| 文件 | 主要入口 | 职责与边界 |
| --- | --- | --- |
| [BlockReferenceEx.cs](../../src/CADShared/Cad/Database/Entities/Blocks/BlockReferenceEx.cs) | `XClip` | 对块参照建立裁剪边界，处理点集或两角点。 |
| 同上 | `TryGetBlockProperty`、`ChangeBlockProperty` | 读取/修改动态块属性；属性名、允许值和写入条件以宿主动态属性集合为准。 |
| 同上 | `ChangeBlockAttribute`、`GetAttributes` | 处理参照的属性文字，和动态属性不是一回事。 |
| 同上 | `GetBlockTableRecord`、`GetBlockName`、`GetNestedBlockPosition` | 块定义/名称及嵌套块插入位置。普通定义、动态原始定义和匿名定义需要按实际分支追踪。 |
| 同上 | `ForEach`、`NestedForEach` | 遍历块定义内容；嵌套版本累计 `BlockTransform`，回调收到实体和矩阵。它经 SymbolTable.Change 打开定义，可能升级写状态；严格只读求交使用下方 Beta。 |
| 同上 | `GetVisibilityInfo` | 支持块参照和块表记录，读取动态块扩展字典及可见性参数 DXF 数据。继续追踪 `Env.EntGet`，属于 native 相关路径。 |
| [BlockVisibilityInfo.cs](../../src/CADShared/Cad/Database/Entities/Blocks/BlockVisibilityInfo.cs) | `Has`、`PropertyName`、`AllowedValues` | 可见性参数的元数据结果。没有可见性参数时返回空信息，不是设置可见性的命令对象。 |

关联入口：

- 块定义创建/外部 DWG 导入在 [`SymbolTableEx`](../../src/CADShared/Cad/Database/SymbolTables/SymbolTableEx.cs) 的块表 `Add`、`GetBlockFrom`。
- 块参照插入与属性参照生成在 [`SymbolTableRecordEx.InsertBlock`](../../src/CADShared/Cad/Database/SymbolTables/SymbolTableRecordEx.cs)，不在 `BlockReferenceEx` 中。
- 动态可见性异常依次查块是否动态、`ACAD_ENHANCEDBLOCK` 扩展数据、参数类名、`Env.EntGet` 与宿主 native 分支。

已有调用：[TestBlock.cs](../../tests/TestShared/TestBlock.cs)。

<a id="bounds"></a>

## 通用变换与包围框

| 文件 | 入口 | 功能细节 |
| --- | --- | --- |
| [EntityEx.cs](../../src/CADShared/Cad/Database/Entities/EntityEx.cs) | `Move`、`Scale`、`Rotation`、`Mirror` | 对原实体应用平移、缩放、旋转和镜像矩阵，不生成副本。 |
| 同上 | `GetExtents`、`GetBoundingBoxEx`、`GetStretchPoints` | 合并实体范围，取得更适合当前实体类型的包围框及拉伸点。 |
| [BoundingBox9.cs](../../src/CADShared/Cad/Database/Entities/Bounds/BoundingBox9.cs) | 构造、九点属性、`Move` | 从 `Extents2d/3d` 表达矩形九点参考系统和最小/最大坐标，适合对齐定位；不是任意旋转实体的精确外轮廓。 |
| [EntityBoundingInfo.cs](../../src/CADShared/Cad/Database/Entities/Bounds/EntityBoundingInfo.cs) | `GetEntityBoxEx`、`GetMTextBoxCorners` 等 | 类型为 `internal`，处理文字、块等实体的范围细节。外部使用者从 `EntityEx.GetBoundingBoxEx` 进入，不能把内部静态方法列为可直接调用 API。 |

包围框只适合范围与候选判断。旋转文字、嵌套块矩阵、无有效 Bounds 和精确几何相交应分别处理。已有入口：[TestExtents.cs](../../tests/TestShared/TestExtents.cs)。

<a id="curves"></a>

## CurveEx 与圆弧/圆工厂

| 文件 | 方法组 | 查找内容 |
| --- | --- | --- |
| [CurveEx.cs](../../src/CADShared/Cad/Database/Entities/Curves/CurveEx.cs) | `GetCurveLength`、`GetPointAtDistanceFraction` | 曲线长度与沿曲线长度比例取点。后者检查有限参数/距离域、比例范围并拒绝 `Ray/Xline` 的无界总长场景。不要把长度比例等同于参数比例。 |
| 同上 | `GetMidpointChordDeviation`、`GetMidpointChordDeviationByDistance` | 参数区间或距离区间中点到端点弦中点的距离；不是整个区间的最大弦偏差。距离版本不支持没有距离起点的 Xline。 |
| 同上 | `GetSplitCurves`、`BreakCurve`、`BreakCurveOnZPlane` | 按参数/点切分及一组曲线互相打断。参数集合版本当前是 `CurveEx.GetSplitCurves(curve, ...)` 静态辅助，点集合版本为扩展方法，注意旧扩展实现已被注释。 |
| 同上 | `BreakCurve2dByQuadTree` | 利用几何转换与四叉树筛选候选后处理二维交点；在早期 GStarCAD 分支未启用。辅助 `BreakCurveInfo` 也定义在此文件。 |
| 同上 | `GetScaleCurve`、`ToCurve3d`、`ToCompositeCurve3d`、`ToNurbCurve3d` | 实体与 `Curve3d`、组合曲线、NURBS 的类型分派，支持多种实体重载；未知或不支持类型有 null 路径。 |
| 同上 | `ChamferAt` | 原地处理直线相邻的内部单个顶点（索引 1 至 N-2），不覆盖闭合首末角；批量圆角见下方 Beta。 |
| [ArcEx.cs](../../src/CADShared/Cad/Database/Entities/Curves/ArcEx.cs) | `CreateArcSCE`、`CreateArc`、`ToPolyline` | 起点/中心/终点、三点或角度构造圆弧，转换为含 bulge 的多段线。 |
| [CircleEx.cs](../../src/CADShared/Cad/Database/Entities/Curves/CircleEx.cs) | `CreateCircle` | 两点、三点或中心/半径/法向量构造圆。三点路径可能返回 null。 |

不要混淆这里的数据库 `Curve` 与[几何分册](geometry.md#curves)中的 `Curve2d/Curve3d`。它们有不同的创建、数据库归属与释放要求。

已有入口：[TestCurve.cs](../../tests/TestShared/TestCurve.cs)。

<a id="polylines"></a>

## PolylineEx：顶点、采样与构造

| 文件 | 入口 | 功能与精度边界 |
| --- | --- | --- |
| [PolylineEx.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineEx.cs) | `GetPoints` | 对轻量、二维、三维多段线取得顶点；后二者通过数据库顶层原生事务读取顶点对象，存在延迟枚举。 |
| 同上 | `GetVertexData` | 返回独立 `BulgeVertexWidth` 数据快照，修改列表或数据项不会直接修改原多段线。 |
| 同上 | `GetSegmentLength` | 按段索引取实际长度，包含闭合段的索引条件；错误索引抛异常。 |
| 同上 | `GetSamplePointsByDistance` | 每个原始子段独立细分，限制相邻采样点的沿线距离，保留所有原顶点；闭合路径末尾再次包含首点。 |
| 同上 | `GetSamplePointsByChordDeviation` | 直线保留端点，圆弧按弓高公式细分，保留顶点和闭合终点；这里的最大圆弧弦偏差与 `CurveEx` 的“中点偏差测量”不同。 |
| 同上 | `CreatePolyline`、`ToPolyline` | 从点、含 bulge/宽度的元组或范围构造轻量多段线。它是平面对象，不能将任意三维输入理解为无损三维曲线。 |
| [BulgeVertexWidth.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/BulgeVertexWidth.cs) | 构造、`ToBulgeVertex` | 顶点、bulge、起终宽度的数据载体；可从 Polyline 指定顶点读取。 |

两种采样都要求有限正阈值，并限制总采样点数不超过 1,000,000；超出表示能力或几何域无效时抛异常，不能把极小阈值当作无限精度开关。

原地圆角等能力先查 `CurveEx.ChamferAt`；搜到 `PolylineExtensions.FilletAll/FilletAt` 时要注意它们在[未编译 ToDo](geometry.md#todo)目录。

<a id="text"></a>

## DBText、MText 与字体

| 文件 | 入口 | 功能细节 |
| --- | --- | --- |
| [DBTextEx.cs](../../src/CADShared/Cad/Database/Entities/Text/DBTextEx.cs) | `CreateDBText` | 设置数据库默认值、内容、高度、插入点及对齐方式，必要时调整 AlignmentPoint，并调用 `AdjustAlignment`；返回内存 `DBText`。未显式传库时使用 `WorkingDatabase`。 |
| 同上 | `ValidateMirror` | 根据数据库的 `Mirrtext` 清理单行文字镜像标志；需要文字已经有适用的数据库上下文。 |
| [MTextEx.cs](../../src/CADShared/Cad/Database/Entities/Text/MTextEx.cs) | `CreateMText` | 设置数据库默认值、内容、高度和位置，返回内存 `MText`。未传库时从内部 `DBTrans.Top.Database` 获取，与 DBText 工厂的默认来源不同。 |
| 同上 | `ExplodeFragments`、`GetUnFormatString` | 通过 MText fragment 回调读取无格式片段并拼接，不是对原实体执行永久炸开。 |
| [FontTTF.cs](../../src/CADShared/Cad/Database/Entities/Text/FontTTF.cs) | `FontTTF` | 文字样式辅助使用的字体枚举；创建/修改文字样式仍到 `SymbolTableEx` 查。 |

已有入口：[TestText.cs](../../tests/TestShared/TestText.cs)。

<a id="hatch"></a>

## Hatch：三类职责不要混用

| 文件 / 类型 | 入口 | 主要用途 |
| --- | --- | --- |
| [HatchEx.cs](../../src/CADShared/Cad/Database/Entities/Hatch/HatchEx.cs) | `ForEach`、`GetAssociatedBoundaryIds`、`CreateBoundarys`、`ResetBoundarys` | 操作已有填充：遍历环、读关联边界、重建边界实体或替换边界并处理关联关系。 |
| [HatchConverter.cs](../../src/CADShared/Cad/Database/Entities/Hatch/HatchConverter.cs) | 构造、`GetBoundarysData`、`CreateBoundary`、`CreateBoundarysAndHatchToMsPs` | 收集旧填充的多段线/圆/样条边界数据，必要时重建边界与填充；通过 `OldHatchId`、`BoundaryIds` 暴露 ID 信息。内部辅助类型不单独对外使用。 |
| [HatchInfo.cs](../../src/CADShared/Cad/Database/Entities/Hatch/HatchInfo.cs) | `Mode1PreDefined`、`Mode2UserDefined`、`Mode3UserDefined`、`Mode4Gradient`、`Build` | 配置并创建新填充或渐变；`Build` 负责向指定块表记录追加 Hatch、设置模式/角度/关联性、加环并求值。 |
| 同上 | `Action`、`ClearBoundary`、`EraseBoundary`、`AppendLoop`、`GradientName` | 链式属性调整、边界集合管理及渐变类型；`EraseBoundary` 会删除边界实体，和仅清空列表不同。 |

边界输入先确认每个 ID 代表完整闭合环还是由多条线组合的环。`HatchInfo` 内部按单个 ID 追加环的路径不能直接套用于任意线段集合；读取到边界数据也不等于已把边界永久加入数据库。

已有入口：[TestHatchinfo.cs](../../tests/TestShared/TestHatchinfo.cs)。

<a id="region"></a>

## RegionEx：文件已编译，边界转换成员未启用

[RegionEx.cs](../../src/CADShared/Cad/Database/Entities/RegionEx.cs) 列入共享编译，但 `ToCurves` 被 `FSFOX_ENABLE_REGION_TO_CURVES` 保护，当前正式项目均未定义该符号。源码明确记录边界几何对象所有权、异常清理及 ZWCAD 2022 API 适配尚需处理。

因此搜索到 `RegionEx.ToCurves` 只能定位保留实现，不能将其写成正式可用的公共能力。`ToDo/RegionExtensions.Centroid` 也不在当前共享编译清单。现行点与区域判断应看 [GeometryEx.PointOnRegion](geometry.md#points)，两者不是同一功能。

## Beta 实体扩展能力

下列源码参与共享编译，支持范围及对象归属见 [Beta 总览](../guides/beta-capabilities.md)。构建测试命令不等于真实宿主已运行。

| 文件 | 主要入口与边界 |
| --- | --- |
| [PolylineExBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineExBeta.cs) | `CreateReversedBeta`、`CreateCleanedBeta`、`TryCreateJoinedBeta`、`CreateFilletedBeta`；借用输入，返回只含指定几何字段的新对象。 |
| [PolylineCoreBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineCoreBeta.cs) | 内部无 SDK 数值核心，处理凸度、段宽、保守整理、唯一端点连接及批量/闭合圆角的共享边预算；生产链接回归工程直接测试此文件。 |
| [PolylineContainmentCoreBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineContainmentCoreBeta.cs) | 内部解析点关系核心与 PolylinePointRelationBeta；直线/圆弧半开穿越、偶奇填充及边界距离，不把主弧裁成顶点范围。 |
| [PolylineRegionExBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineRegionExBeta.cs) | `GetPointRelationBeta`；闭合线 WCS 查询点转 OCS，分别判断平面距离与区域关系，输入借用。 |
| [BlockIntersectionExBeta.cs](../../src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs) | `GetCurveIntersectionsBeta`；显式事务 ForRead 遍历，当前路径防循环，父×子矩阵及自有临时曲线求交，返回只读快照。 |
| [CurveOrderingExBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/CurveOrderingExBeta.cs) | `GetOrderAlongCurveBeta`；有限曲线上的最近位置，稳定返回输入索引，不延长路径。 |
| [ExtentGroupingBeta.cs](../../src/CADShared/Cad/Database/Entities/Bounds/ExtentGroupingBeta.cs) | `GroupByProximity`、`ExtentGroupBeta`；原始 XY 框邻近连通分量，保留成员索引及完整 Z 范围。 |
| [TextMetricsBeta.cs](../../src/CADShared/Cad/Database/Entities/Text/TextMetricsBeta.cs) | `MeasureSingleLine`；显式数据库和样式；旧 ZWCAD/GStarCAD 绑定明确报告不支持。 |
| [PolyFaceMeshBuilderBeta.cs](../../src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs) | `Append`；显式目标库、空间和调用方事务，所有输入先验证，宿主写入失败需中止事务。 |
| [PolyFaceMeshDataBeta.cs](../../src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshDataBeta.cs) | 内部面索引预验证和独立快照；只接受三角面、四边面，不静默截断多边形。 |

验收入口：[TestPolylineBeta.cs](../../tests/TestShared/TestPolylineBeta.cs)、[TestBlockIntersectionsBeta.cs](../../tests/TestShared/TestBlockIntersectionsBeta.cs)、[TestOrderingBeta.cs](../../tests/TestShared/TestOrderingBeta.cs)、[TestTextMetricsBeta.cs](../../tests/TestShared/TestTextMetricsBeta.cs)、[TestGeometryConstructionBeta.cs](../../tests/TestShared/TestGeometryConstructionBeta.cs)。实际 CAD 执行均保持各能力指南记录的状态。

返回[源码检索总入口](README.md)或[数据库分册](database.md)。
