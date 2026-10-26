# 几何、坐标与空间索引定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-geometry`<br>
> 基线：[源码检索总入口](README.md)<br>
> 覆盖 `Cad/Geometry` 的 20 个共享编译文件，并单独标识 21 个未编译的 ToDo 文件。

按问题进入：[坐标](#coordinates)、[点与区域](#points)、[几何曲线](#curves)、[四叉树](#quadtree)、[未编译源码](#todo)。数据库实体曲线的采样和打断见[实体分册](entities.md#curves)。

<a id="coordinates"></a>

## 坐标、平面和向量

| 文件 | 类型 / 方法 | 定位细节 |
| --- | --- | --- |
| [CoordinateSystemCode.cs](../../src/CADShared/Cad/Geometry/CoordinateSystemCode.cs) | `Wcs`、`Ucs`、`MDcs`、`PDcs` | 现行坐标系枚举，和 ToDo 中的 `CoordSystem` 不是同一个类型。 |
| [GeometryEx.cs](../../src/CADShared/Cad/Geometry/GeometryEx.cs) | 点/向量的 `Ucs2Wcs`、`Wcs2Ucs`、`Trans`、`Wcs2Dcs` | 从活动 Editor 获取矩阵，继续追到 `EditorEx.GetMatrix` 及具体视口矩阵。不是脱离 CAD 上下文的静态常量转换。 |
| 同上 | `GetScaleMatrix`、`GetSize`、`Geometry.Draw` | 非等比缩放矩阵、范围尺寸及 GraphicsInterface 绘制；此文件混合纯计算与宿主相关方法。 |
| [PlaneEx.cs](../../src/CADShared/Cad/Geometry/PlaneEx.cs) | 静态 `X`、`Y`、`Z` 平面 | 以坐标轴为法线建立的基准平面。不要把 `PlaneEx.X` 当作 XOY 平面。 |
| [VectorEx.cs](../../src/CADShared/Cad/Geometry/VectorEx.cs) | `Convert2d/Convert3d`、`Cross2d`、`Z20`、`AngleOnPlane` | 维度转换、二维叉积、去 Z 和平面内角度。 |

坐标错误的阅读链：先确定点/向量目前处于哪个坐标系 → 查 `CoordinateSystemCode` → `GeometryEx.Trans` → [`EditorEx.GetMatrix`](../../src/CADShared/Cad/Editor/EditorEx.cs) → 模型/图纸视口状态。点和向量的矩阵作用不同，不能通过改变类型名互换。

已有入口：[TestPoint.cs](../../tests/TestShared/TestPoint.cs)、[TestEditor.cs](../../tests/TestShared/TestEditor.cs)。

<a id="points"></a>

## 点、区域、方向、面积与包围圆

| 文件 | 入口 | 细节 |
| --- | --- | --- |
| [PointEx.cs](../../src/CADShared/Cad/Geometry/PointEx.cs) | `GetAngle`、`GetMidPointTo`、`InterpolateTo`、`Polar`、`GetArcBulge` | 点间角度、中点、比例插值、极坐标与三点弧凸度。长度比例插值与实体曲线沿线比例不是同一个操作。 |
| 同上 | `Point2d/Point3d`、`Z20`、`OrthoProject`、`Distance2dTo` | 二维/三维转换、投影和忽略 Z 的距离。 |
| 同上 | `GetHashString`、`End2End`、`GetRecPoint2ds/GetRecPoint3ds` | 格式化坐标键、让点集合首尾闭合以及取矩形角点。闭合操作修改传入集合，不能当作只读查询。 |
| [GeometryEx.cs](../../src/CADShared/Cad/Geometry/GeometryEx.cs) | `PointOnRegion` | 二维/三维顶点集合与点的位置关系。先检查边界和包围框，再做射线交叉判断。输入按直线段连接，不携带多段线 bulge。 |
| 同上 | `GetArea`、`IsClockWise` | 有向面积和方向判断；三点、向量与点集重载分别处理。 |
| 同上 | `GetMinCircle`、`ConvexHull` | 两到四点及点集的包围圆、凸包。包围圆还使用 `LoopList<Point2d>` 维护支撑点。 |
| [PointOnRegionType.cs](../../src/CADShared/Cad/Geometry/PointOnRegionType.cs) | `Inside`、`On`、`Outside`、`Error` | 不要把 `On` 或 `Error` 简化成“内部/外部”两种布尔结果。 |
| [OrientationType.cs](../../src/CADShared/Cad/Geometry/OrientationType.cs) | `OrientationType` | 方向判定结果。 |

`PointOnRegion` 的三维重载不等于任意三维实体内部判断：边界和包围框检查使用三维几何，但后续交叉判断仍取 X/Y。对倾斜平面、不共面顶点、自交或退化多边形，必须核对实际输入和算法约束，不能由三维参数类型推断通用支持。

已有入口：[TestPointEx.cs](../../tests/TestShared/TestPointEx.cs)、[TestPointOnRegion.cs](../../tests/TestShared/TestPointOnRegion.cs)、[TestConvexHull.cs](../../tests/TestShared/TestConvexHull.cs)。

<a id="curves"></a>

## 几何 Curve2d/Curve3d 与数据库 Curve 的桥接

| 文件 | 方法组 | 职责 |
| --- | --- | --- |
| [Curve2dEx.cs](../../src/CADShared/Cad/Geometry/Curve2dEx.cs) | `ToCurve`、`ToCircle`、`ToArc`、`ToLineSegment2d`、圆弧 `IsIn` | 将二维圆弧、椭圆弧、线、线段、NURBS 等转换为实体，部分重载显式接收变换矩阵。 |
| [Curve3dEx.cs](../../src/CADShared/Cad/Geometry/Curve3dEx.cs) | `GetParamsAtIntersectionPoints`、`GetSubCurve`、组合曲线 `GetSplitCurves` | 求交参数、几何曲线截取与组合段分割。 |
| 同上 | `ToCurve`、`ToNurbCurve3d`、`ToPolyline`、`IsCircular` | 针对具体几何曲线类型分派，建立 Line、Arc、Circle、Ellipse、Spline 或 Polyline 等对象；不支持类型有 null 路径。 |

双向路线如下：

```text
数据库 Entity / Curve
  -> Cad/Database/Entities/Curves/CurveEx.ToCurve3d / ToCompositeCurve3d
  -> Curve3dEx：几何分割、参数、转换
  -> Curve3dEx.ToCurve：返回数据库实体类型的内存对象
  -> BlockTableRecord.AddEntity：需要永久保存时才落库
```

不要统一假定转换总是返回新对象。例如 `Curve3dEx.ToNurbCurve3d` 在输入已为 NURBS 时直接返回该输入；调用方必须按具体分支判断借用与所有权，避免重复释放或让对象逃出有效作用域。

<a id="quadtree"></a>

## 四叉树与 Rect

路径为 `Cad/Geometry/SpatialIndex/QuadTree`；类型仍在 `Fs.Fox.Cad`。

| 文件 | 主要类型 / 入口 | 职责 |
| --- | --- | --- |
| [Rect.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/Rect.cs) | `Rect`、`TolerancePoint2d`、`Contains`、`IntersectsWith`、`Expand`、`GetMinMax`、`XCollision` | 轴对齐矩形、容差比较、碰撞/排序和范围辅助；`ToPolyLine` 能生成 CAD 实体，不能把整个文件当作无 CAD 依赖的数据结构。 |
| [QuadEntity.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadEntity.cs) | `QuadEntity : Rect` | 供调用方继承并附加业务数据的范围载体，不自动保存或监听 ObjectId。 |
| [QuadTree.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadTree.cs) | `QuadTree<TEntity>`、`Insert`、`Query`、`Remove`、`FindNearEntity`、`ForEach` | 根控制器；超出根范围时扩张，矩形实体进入节点，点状对象单独放入 `SortedSet`。 |
| [QuadTreeNode.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadTreeNode.cs) | 同名节点、`Insert/Query/Remove`、邻近查找 | 四象限拆分、父子关系、内容集合、范围查询与搜索递归的实际实现。 |
| [QuadTreeEvn.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadTreeEvn.cs) | `QuadTreeEvn` | 静态 `MinArea`、`SelectMode`、最大深度和拆分数量阈值；保留 `Evn` 拼写。 |
| [QuadTreeSelectMode.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadTreeSelectMode.cs) | `IntersectsWith`、`Contains` | 查询范围相交或完全包含。 |
| [QuadTreeFindMode.cs](../../src/CADShared/Cad/Geometry/SpatialIndex/QuadTree/QuadTreeFindMode.cs) | `Top/Bottom/Left/Right` | 方向邻近搜索所用枚举。 |

高频使用流程是：从真实实体提取所需 XY 包围框 → 构造附加业务信息的 `QuadEntity` → 插入 → `Query` 得到候选 → 再进行真实几何或业务精确判断。节点保存的是创建时的范围，不会自动跟随 CAD 实体移动、删除或变形。

### 查结果数量、删除和“最近”时的注意点

- `Query` 根据矩形范围筛选，不等于曲线真实相交。
- `FindNeibor` 已标记过时，说明其依据附近节点，不能称为真实几何最近对象。
- `FindNearEntity` 逐步扩大候选范围，在候选中比较矩形中心距离；不是点到实体边界的距离 API。
- 点状对象有独立 `_points` 容器；`Count` 返回根节点子树数量，两个 `Remove` 主入口也委托根节点。查询、计数、删除对点和有面积矩形并非同一路径。
- 查询模式写在静态 `QuadTreeEvn.SelectMode` 中，不能将多个实例理解为完全隔离的并发索引。

已有入口：[TestQuadTree.cs](../../tests/TestShared/TestQuadTree.cs)。

## Beta 排序与三角形

| 文件 | 入口与边界 |
| --- | --- |
| [PointOrderingBeta.cs](../../src/CADShared/Cad/Geometry/Ordering/PointOrderingBeta.cs) | `GetRowOrder`、`GetColumnOrder` 返回原始索引；固定锚点分组，读取方向不改变组成员。 |
| [OrderingBetaCore.cs](../../src/CADShared/Cad/Geometry/Ordering/OrderingBetaCore.cs) | 无 SDK 内部核心；稳定排序及原始范围连通分量，由生产链接测试直接覆盖。 |
| [Triangle3dBeta.cs](../../src/CADShared/Cad/Geometry/Triangle3dBeta.cs) | 三维 `Centroid`、`TryGetSlopeAndAspect`；坡度为度，坡向从北顺时针计量，退化及水平面的输出有明确约定。 |
| [TriangleMathBeta.cs](../../src/CADShared/Cad/Geometry/TriangleMathBeta.cs) | 无 SDK 内部核心；补偿均值、最长边相对退化判断与有限数处理。 |

这些类型参与当前编译，和下方未编译 ToDo 不是同一组公共类型。具体约定见[排序指南](../guides/ordering-beta.md)、[三角形与构造指南](../guides/geometry-construction-beta.md)。

<a id="todo"></a>

## ToDo：仅供定位的未编译源码

[`ExtensionMethod/Geometry/ToDo`](../../src/CADShared/ExtensionMethod/Geometry/ToDo) 的 21 个文件未列入 `CADShared.projitems` 的 `Compile` 项，命名空间为 `GeometryExtensions`。目录名不表示已承诺实现，也不能通过主产品 using 直接使用下面的能力。

下表只说明“搜索命中来自哪里”，不是可用性、算法正确性或启用建议：

| 保留文件 | 类型 / 搜索关键词 | 内容线索 |
| --- | --- | --- |
| [CircularArc2dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/CircularArc2dExtensions.cs) | `SignedArea`、`Centroid`、`GetTangentsTo` | 二维圆弧面积、重心、切线。 |
| [CircularArc3dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/CircularArc3dExtensions.cs) | `GetTangentsTo` | 三维圆弧切线。 |
| [EditorExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/EditorExtensions.cs) | `UCS2WCS`、`DCS2PSDCS` 等 | 另一组 Editor 坐标矩阵；现行入口看 `EditorEx`。 |
| [EllipseExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/EllipseExtensions.cs) | `ToPolyline`、角度/参数换算 | 椭圆离散与参数转换。 |
| [GeomExt.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/GeomExt.cs) | `CoordSystem`、`TangentType`、`GeomExt` | 待整理扩展的坐标枚举、切线模式和投影辅助。 |
| [Point2dCollectionExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Point2dCollectionExtensions.cs) | `RemoveDuplicates`、`Contains`、`Point2dComparer` | 二维点去重与容差集合。 |
| [Point2dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Point2dExtensions.cs) | `Convert3d`、`Flatten`、`IsBetween`、`IsInside`、`Polar` | 二维点转换和位置判断。 |
| [Point3dCollectionExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Point3dCollectionExtensions.cs) | 去重、`ToExtents3d`、`Point3dComparer` | 三维点集合和范围。 |
| [Point3dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Point3dExtensions.cs) | `Convert2d`、`Flatten`、`IsBetween`、`Trans` | 三维点转换和坐标变换。 |
| [Polyline2dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Polyline2dExtensions.cs) | `GetVertices`、`GetArcSegmentAt`、`Centroid`、投影 | 旧式二维多段线顶点、段及重心。 |
| [Polyline3dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Polyline3dExtensions.cs) | `GetProjectedPolyline`、`GetOrthoProjectedPolyline` | 三维多段线投影。 |
| [PolylineExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/PolylineExtensions.cs) | `Centroid`、`FilletAll`、`FilletAt`、投影 | 轻量多段线重心、圆角和投影。 |
| [PolylineSegment.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/PolylineSegment.cs) | `PolylineSegment`、`Inverse`、`ToCurve2d` | 单个带 bulge 和宽度的二维段。 |
| [PolylineSegmentCollection.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/PolylineSegmentCollection.cs) | `Join`、`Reverse`、`ToPolyline` | 段集合连接和转换。 |
| [RegionExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/RegionExtensions.cs) | `Region.Centroid` | 面域重心。 |
| [SplineExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/SplineExtensions.cs) | `Spline.Centroid` | 样条重心。 |
| [Triangle.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Triangle.cs) | `Triangle<T>` | 三顶点抽象、逆序、设置和枚举。 |
| [Triangle2d.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Triangle2d.cs) | 转换、相交、`IsPointInside/On` | 二维三角形几何。 |
| [Triangle3d.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Triangle3d.cs) | 平面、三维变换、点内外 | 三维三角形几何。 |
| [Vector3dExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/Vector3dExtensions.cs) | `Flatten` | 向量压平。 |
| [ViewportExtensions.cs](../../src/CADShared/ExtensionMethod/Geometry/ToDo/ViewportExtensions.cs) | `DCS2WCS`、`DCS2PSDCS` 等 | 视口矩阵，注意与现行 `EditorEx` 路径区分。 |

返回[源码检索总入口](README.md)。
