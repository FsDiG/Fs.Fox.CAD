# Beta 能力维护与验收计划

> 状态：active；更新于 2026-10-26。<br>
> 稳定 ID：`plan.beta-maintenance`。<br>
> 使用入口见[能力总览](guides/beta-capabilities.md)，实施事实见[实现与验证记录](beta-implementation-notes.md)。

## 目标与边界

Beta 能力覆盖多段线、排序与范围、文字测量、三角形／网格、块曲线求交及 AutoCAD 预览。实现和自动检查已完成，后续以真实宿主验收、限制收敛及转正为主，不因编译通过自动宣布正式支持。

新增能力独立演进，沿用现有模块、`Fs.Fox.Cad` 命名空间、程序集及基础设施，不自动替换稳定入口。若发现 Fox 原有代码的问题，先单独报告和讨论，不在 Beta 维护中顺手修改稳定行为。

自绘表格及其专用模型、布局、合并和绘制业务保持排除；专用断面、植物结果窗口、图纸比例业务及重复空间索引不纳入本计划。简单 SDK 属性转发不单独增加公共 API。

## 并行推进与责任

按能力领域分别负责：曲线与多段线、排序与范围、文字、几何／构造／块查询、DWG 预览。每项能力只有一个主负责人，领域之间可以并行，不提前拆成互相冲突的方法级任务。

能力负责人在实施前明确输入输出、副作用、异常、支持宿主和验证场景。独立复核检查数值、所有权、兼容性及证据；统筹人维护公共约定、共享编译清单、基线和集成顺序。跨领域规则先收敛，再由单一责任人修改共享位置。

按能独立评审的能力滚动集成，无需等待其他领域全部结束。普通类型在名称中保留 Beta，扩展方法在调用入口体现 Beta；查询、计算、创建对象和修改数据库分别表达清楚。

## Beta 命名定稿：已实施

2026-10-26 已按本节定稿完成源码、测试及文档更名。下表保留更名前到现行名称的映射，当前 API 以源码和[能力总览](guides/beta-capabilities.md)为准。范围仅包括这批新增 Beta 能力及其测试、XML 注释、导航和编译清单；Fox 原有稳定类型和方法的命名优化另行处理。更名保持算法、默认值、异常、所有权和宿主支持范围不变，验证见[命名重构记录](beta-implementation-notes.md#命名重构与本地验证)。

类型使用职责明确的名词或名词短语，方法使用动作短语，参考 [.NET 类型命名](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces)和[成员命名](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members)。本批扩展类采用完整的 `ExtensionsBeta` 后缀；这是本次项目决策。Beta 始终置于类型名末尾，扩展方法同时保留方法级 Beta；普通静态方法和实例方法通过所属 Beta 类型表明状态。

`Point3d`、`Extents3d`、`PolyFaceMesh` 等 CAD SDK 名称及大小写保持原有含义。沿用 SDK 术语不要求沿用 Fox 现有的 `Ex` 缩写。

### 公开类型与文件

每个公开类型独立成文件，文件名与类型名一致。下表覆盖当前全部 12 个公开 Beta 类型。

| 更名前类型 | 现行类型 | 决策依据 |
| --- | --- | --- |
| `PolylineExBeta` | `PolylineGeometryExtensionsBeta` | 以几何新对象为输出，明确区别于完整数据库对象克隆。 |
| `PolylineRegionExBeta` | `PolylineContainmentExtensionsBeta` | 表达闭合多段线的点包含关系，避免与 CAD Region 实体混淆。 |
| `BlockIntersectionExBeta` | `BlockReferenceIntersectionExtensionsBeta` | 明确扩展对象是块参照，功能是内部曲线求交。 |
| `CurveOrderingExBeta` | `CurveOrderingExtensionsBeta` | 展开扩展类缩写，保留沿曲线排序的职责。 |
| `PointOrderingBeta` | 保留 | 静态排序入口职责明确。 |
| `ExtentGroupingBeta` | `ExtentsGroupingBeta` | 与输入 Extents3d 的术语统一。 |
| `ExtentGroupBeta` | `ExtentsGroupBeta` | 与分组入口一致，独立存放分组结果类型。 |
| `TextMetricsBeta` | `TextMeasurementBeta` | 这是执行测量的入口；Metrics 容易被理解为测量结果对象。 |
| `Triangle3dBeta` | 保留 | 准确表示三个三维顶点，沿用 CAD SDK 的 3d 大小写。 |
| `PolyFaceMeshBuilderBeta` | `PolyFaceMeshFactoryBeta` | 单次调用创建实体并追加，没有分步积累的 Builder 状态。 |
| `DwgPreviewBeta` | `DwgPreviewRendererBeta` | 明确执行图形渲染，区别于读取已有缩略图或预览数据。 |
| `PolylinePointRelationBeta` | 保留 | 四种分类含义准确；从内部计算文件移到独立文件。 |

`PolylinePointRelationBeta.cs` 从当前包含枚举的内部计算文件分离；`ExtentsGroupBeta.cs` 从分组工具文件分离。两处只改变声明位置，不增加新能力。枚举成员 `Outside`、`Inside`、`Boundary`、`OffPlane` 保持不变。

### 公开方法、属性与参数

下表覆盖当前 14 个公开操作方法，不含构造器和属性访问器。

| 更名前方法 | 现行方法 | 调用语义 |
| --- | --- | --- |
| `CreateReversedBeta` | 保留 | 返回新的反向几何对象。 |
| `CreateCleanedBeta` | `CreateSimplifiedBeta` | 删除冗余顶点并保守合并直段，不暗示完整图形修复。 |
| `CreateFilletedBeta` | 保留 | 返回新的圆角几何对象。 |
| `TryCreateJoinedBeta` | 保留 | 尝试生成新连接结果，继续保留既有 false／异常边界。 |
| `GetPointRelationBeta` | `ClassifyPointBeta` | 返回四类点关系，不收缩成布尔 Contains 查询。 |
| `GetCurveIntersectionsBeta` | `GetCurveIntersectionPointsBeta` | 返回交点集合。 |
| `GetRowOrder` | `GetRowOrderIndices` | 返回按行读取的原始索引。 |
| `GetColumnOrder` | `GetColumnOrderIndices` | 返回按列读取的原始索引。 |
| `GetOrderAlongCurveBeta` | `GetPointOrderIndicesBeta` | 在曲线接收者上返回点的排序索引。 |
| `GroupByProximity` | `GroupByXYProximity` | 只使用 XY 最短间距建立连通关系，结果仍保留 Z 范围。 |
| `MeasureSingleLine` | `MeasureSingleLineBounds` | 返回文字局部坐标的二维范围。 |
| `TryGetSlopeAndAspect` | `TryGetSlopeAndAspectDegrees` | 显式表达输出角度单位，保留水平面坡向为 NaN 的契约。 |
| `Append` | `CreateAndAppend` | 创建网格并写入调用方数据库事务，不自动提交。 |
| `SavePng` | `RenderModelSpaceToPng` | 从磁盘 DWG 渲染模型空间，不包含未保存的文档修改。 |

三角形属性 `First`／`Second`／`Third` 改为 `VertexA`／`VertexB`／`VertexC`，构造参数对应改为 `vertexA`／`vertexB`／`vertexC`；只重命名，不改变输入顺序或绕序语义。`Centroid`、分组结果的 `Bounds` 和 `SourceIndices` 保留。

参数同时明确其含义；这些更名需要同步命名参数调用：

- 曲线排序的 `reverse` 改为 `descending`，表示按里程降序，相同里程仍保持原始顺序。
- 块求交的 `tolerance` 改为 `duplicatePointTolerance`，`mode` 改为 `intersectionMode`，`target` 改为 `targetCurve`；去重容差仍不传给原生求交。
- 网格的 `targetSpaceId` 改为 `targetBlockTableRecordId`，覆盖模型空间、图纸空间及普通块定义。
- 文字测量的 `height` 改为 `textHeight`，`raw` 改为 `useRawText`，保持布尔极性和转义语义。
- 预览的 `width`／`height` 改为 `pixelWidth`／`pixelHeight`，`drawingPath` 改为 `sourceDwgPath`，`outputPngPath` 保留。

### 内部计算与实施边界

内部主类型与文件一并收敛；其余内部快照及局部辅助类型保留现有名称。

| 更名前内部主类型 | 现行类型 |
| --- | --- |
| `PolylineCoreBeta` | `PolylineGeometryCoreBeta` |
| `PolylineContainmentCoreBeta` | 保留 |
| `OrderingBetaCore` | `OrderingAndGroupingCoreBeta` |
| `TriangleMathBeta` | `TriangleMathCoreBeta` |
| `PolyFaceMeshDataBeta` | `PolyFaceMeshValidationCoreBeta` |
| `DwgPreviewCoreBeta` | 保留 |

网格内部 `ValidateAndCopyFaces` 改为 `ValidateAndConvertFaceIndices`，明确包含零基到原生一基索引的转换。对应计算操作与公共名称同步，有限性判断统一使用 `IsFinite` 语义，不改数值实现。

测试工程 `PolylineRegionBeta.CoreTests` 同步更名为 `PolylineContainmentBeta.CoreTests`，连同目录、项目文件及 CI 引用一起更新。当前接口指南、源码导航、XML 引用和共享编译清单同步新名称；历史评审继续保留其提交范围与旧名称，转向当前源码的链接必须有效。

按用户已确认的尚无上层引用前提，已核对工作区实际调用并一次性替换，未新增旧名兼容别名或转发包装。原有 143 个共享编译 C# 文件及 21 个未编译 ToDo 文件保持字节一致；编译项和基线只反映 Beta 更名、公开类型分文件及其实际哈希变化。共享编译项现为 161 个，其中 Beta 源码 18 个。

## 按影响范围选择验证

- 仅改文档：执行 `git diff --check`、链接检查和 GFM 实际渲染，本地不编译。
- 修改数值核心：运行直接链接该生产实现的相关核心回归，新增独立预期或反例；不在测试里维护另一份算法。
- 修改 SDK、条件编译或对象适配：选择受影响的宿主／框架代表目标，完成一次增量编译；新问题或兼容性证据才触发扩围。
- 修改共享编译入口、依赖或跨目标契约：检查模块归属、哈希基线、比较工具和适用的目标矩阵。
- 最终提交沿用仓库正常 CI，不因每次局部编辑重复本地全矩阵，也不通过压制告警掩盖问题。

正式目标与命令以[构建说明](../编译说明.md)和实时项目／工作流为准。源码哈希变化时同步模块基线并验证，不能用基线更新消除未经解释的差异。

## 真实宿主验收

宿主验收优先使用 [HostAcceptance](../tools/HostAcceptance/README.md)。分别记录编译目标、测试程序集、CAD 产品／完整版本、提交、场景与实际结果；SDK 年份不能替代实际宿主版本。未执行项保持 `Not run`。

除非任务明确安排，不启动 CAD。在 Issue #40 的隔离 profile、身份握手及命令协议完成前，不使用日常 profile 或已打开会话自动验收，也不修改 Trusted Paths、注册表、启动组或全局 CAD 配置。

重点场景包括：多段线弧宽／平面和退化边界，点关系半开计数，稳定排序与范围临界值，字体／样式和工作库恢复，三维数值及网格写入／回滚，块删除／恢复和原生变换，预览空图、连续渲染及异常清理。命令能够编译不表示这些场景已经运行。

## 转正与完成条件

代码、XML、指南和验证记录必须一致；支持范围内的真实宿主证据充分后，再决定转正及兼容过渡。转正不自动替换旧入口，具体安排根据实际调用方收敛。

未达到条件的能力继续保留 Beta，并明确剩余限制。阶段完成后更新实施记录和文档状态，避免把历史计划或审查快照当成当前技术契约。
