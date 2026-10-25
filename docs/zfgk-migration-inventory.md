# Zfgk 来源能力处置清单

> 状态：Active，2026-10-25；依据[整体路线图](zfgk-migration-roadmap.md)。<br>
> 来源：`FeiSiDev/Fs.Zfgk.CAD` @ `a882003436fb4cc31b24c90f6779177c13a60abe`，49 个跟踪的 C# 文件。<br>
> 五条工作线的 Beta 代码已集成；“迁入”仅表示代码能力进入 Fox，不表示真实宿主已验收。

## 全部来源的处置

来源目录保留原拼写。仅迁入选定能力，其他同类方法按表中说明复用或排除。来源 README 记录已获原作者张帆授权；新代码保留算法来源说明，不复制二进制、程序集信息或原库依赖体系。

| 来源文件 | 处置 | Fox 落点或理由 |
| --- | --- | --- |
| `AutoCADCommonApp.cs` | 排除 | 空初始化类型。 |
| `Geometry/AcadPlaneUtil.cs` | 复用 | 现有 Plane、坐标体系。 |
| `Geometry/CoordinateUtil.cs` | 复用 | 现有坐标转换。 |
| `Geometry/GePointUtil.cs` | 复用 | PointEx、GeometryEx；排序新增归 B，圆角复用现有入口。 |
| `Geometry/GeRectangleUtil.cs` | 复用 | Rect 的相交和包含。 |
| `Geometry/GeTriangle.cs` | 迁入 D | 重心、坡度和坡向；修正 Z 值和退化处理，与 ToDo 区分。 |
| `Geometry/MathUtil.cs` | 复用 | BCL 及 Fox 数值辅助；不复制整套数学包装。 |
| `ObjectARX/AcadDocumentUtil.cs` | 复用 | Env、文档集合和锁；不迁入忽略文件名的查询。 |
| `ObjectARX/ArxOthers/DwgScaleUtil.cs` | 排除 | 专用图纸比例记录协议。 |
| `ObjectARX/ArxOthers/ExtentUtil.cs` | 迁入 B，其他复用 | 邻近包围框组合；尺寸、范围获取复用 Fox。 |
| `ObjectARX/ArxOthers/GroupUtil.cs` | 复用 | DBDictionaryEx 与现有组能力。 |
| `ObjectARX/ArxOthers/GsPreviewUtil.cs` | 迁入 E | AutoCAD 中读取外部 DWG 并输出图片，处理所有图形资源。 |
| `ObjectARX/ArxOthers/SectionUtil.cs` | 排除 | 带业务假设和输入修改的断面计算。 |
| `ObjectARX/DictionaryUtil.cs` | 复用 | DBDictionaryEx、Xrecord。 |
| `ObjectARX/DwgDatabaseUtil.cs` | 复用 | DBTrans、符号表和落库接口。 |
| `ObjectARX/DwgTable/AcadDwgTable.cs` | 排除 | 用户明确跳过自绘表格业务。 |
| `ObjectARX/DwgTable/AcadSubDrawTable.cs` | 排除 | 自绘表格布局及专用模型。 |
| `ObjectARX/Entity/ArcUtil.cs` | 复用 | ArcEx 与 SDK 几何接口。 |
| `ObjectARX/Entity/BlockUtil.cs` | 迁入 B，其他复用 | 排序归 B；块插入、属性读写、名称及动态属性复用 Fox。 |
| `ObjectARX/Entity/CircleUtil.cs` | 复用 | CircleEx。 |
| `ObjectARX/Entity/CurveUtil.cs` | 复用 | CurveEx；不迁入忽略 ratio 的取点错误。 |
| `ObjectARX/Entity/DimensionUtil.cs` | 复用 | SDK 创建实体后使用现有落库；不引入固定 F2 文字覆盖测量值。 |
| `ObjectARX/Entity/EntityUtil.cs` | 复用 | EntityEx、对象访问、变换及范围能力。 |
| `ObjectARX/Entity/HatchUtil.cs` | 复用 | HatchEx、HatchInfo、HatchConverter。 |
| `ObjectARX/Entity/LineUtil.cs` | 复用 | SDK 求交、CurveEx；不另建重复直线工具类。 |
| `ObjectARX/Entity/PolyfaceMeshUtil.cs` | 迁入 D | 显式目标空间和事务，预先验证三角面/四边面及全部索引。 |
| `ObjectARX/Entity/PolylineUtil.cs` | 迁入 A，其他复用 | 整理、连接、反向副本；采样、段长、创建及现有圆角复用 Fox。 |
| `ObjectARX/Entity/RegionUtil.cs` | 排除 | 不随迁移扩大尚未统一的面域重建和宿主契约。 |
| `ObjectARX/Entity/TextUtil.cs` | 迁入 C，其他复用 | 按文字样式测量字符串；实体创建与文本提取复用 Fox。 |
| `ObjectARX/Entity/XDataUtil.cs` | 复用 | TypedValueList、XDataList、DBObjectEx。 |
| `ObjectARX/Interaction/GetInputUtil.cs` | 复用 | EditorEx 与现有输入能力。 |
| `ObjectARX/Interaction/GetPointUtil.cs` | 复用 | EditorEx、坐标转换。 |
| `ObjectARX/Interaction/ResultLocateUtil.cs` | 排除 | 植物结果窗口的数据和交互协议。 |
| `ObjectARX/Interaction/SelectUtil.cs` | 复用 | EditorEx、OpFilter。 |
| `ObjectARX/Interaction/SelectionSetUtil.cs` | 复用 | 现有选择与对象枚举。 |
| `ObjectARX/LayerUtil.cs` | 复用 | SymbolTableEx、LayerUnlockScope。 |
| `ObjectARX/LinetypeUtil.cs` | 复用 | 现有线型表和资源加载。 |
| `ObjectARX/Others/ConvertUtil.cs` | 复用 | 现有点和坐标转换，不引入 ZFGK 几何类型。 |
| `ObjectARX/Others/FormatUtil.cs` | 复用 | BCL 格式化。 |
| `ObjectARX/Others/ListUtil.cs` | 复用 | BCL 集合操作。 |
| `ObjectARX/Others/ViewUtil.cs` | 复用 | EditorEx 缩放与视图。 |
| `ObjectARX/SpacialIndex/DynamicPointSpatialIndex.cs` | 排除 | 有局部最近点与重置限制，不引入第二套索引。 |
| `ObjectARX/SpacialIndex/EntitySpatialIndex.cs` | 复用 | QuadTree；保留候选范围和精确几何的区别。 |
| `ObjectARX/TextStyleUtil.cs` | 复用 | 现有文字样式表能力。 |
| `Other/NumberUtil.cs` | 排除 | 通用字符串业务，不属于本次 CAD 能力补齐。 |
| `Others/ApplicationUtil.cs` | 复用 | Env 中的配置和支持路径。 |
| `Properties/AssemblyInfo.cs` | 排除 | 保留 Fox 自身包、版本及程序集身份。 |
| `UI/ResultLocateForm.Designer.cs` | 排除 | 植物业务窗口布局。 |
| `UI/ResultLocateForm.cs` | 排除 | 植物重叠检查及删除行为。 |

## 工作线依赖与交接

五条线均依赖 Fox 的现有基础设施，互相不依赖新增公共 API，因此可在本清单提交后并行。A/B/D 的纯计算内核使用独立的、直接链接生产源文件的测试；CAD 对象适配和 C/E 的宿主行为由命令测试覆盖并单独记录是否执行。

| 工作线 | 新增能力 | 基础依赖 | 交接条件 |
| --- | --- | --- | --- |
| A | 多段线整理、连接和反向副本 | 现有顶点数据及 SDK Polyline | 输入不被消费；完整几何字段和错误边界可验证。 |
| B | 空间排序和范围组合 | 现有点、Curve、Extents3d | 不扰动排序键，重复项保留，范围组合规则确定。 |
| C | 字符串样式测量 | 各宿主 GraphicsInterface.TextStyle | 显式样式和目标数据库，释放测量资源；属性对照结论明确。 |
| D | 三角形计算、多面网格构造 | SDK 几何、目标空间和调用方事务 | 单位与退化策略确定，索引错误在写入前拒绝。 |
| E | AutoCAD DWG 预览 | 活动文档 GraphicsManager、外部只读数据库 | 所有权、空图与异常路径明确，不改当前图纸或全局配置。 |

集成人维护共享编译清单、模块元数据、基线和文档索引。各线使用独立工作树，提交可独立构建的变更；滚动合入时重算基线，不把新增类型当作纯文件移动，也不修改比较工具掩盖类型变化。

## 验证记录

源码基线为同一提交中的 `Build/CADSharedModuleBaseline.json`：156 个共享编译文件，包含 13 个新增 Beta 源文件。原有 143 个共享源码的规范化 SHA256 与迁移前一致。模块校验通过，既有 42 个带债务标记的文件和 17 项边界发现未增加。来源清单与来源仓库 49 个跟踪 C# 文件逐项比对，无遗漏或多项。

| Fox 测试/类库目标 | Debug x64 | Release x64 | 实际宿主 |
| --- | --- | --- | --- |
| AutoCAD 2019 | 通过 | 通过 | `Not run` |
| AutoCAD 2025 | 通过 | 通过 | `Not run` |
| ZWCAD 2022 | 通过 | 通过 | `Not run` |
| ZWCAD 2025 | 通过 | 通过 | Build only，未扩大原验收矩阵 |
| GStarCAD 2022 | 通过 | 通过 | `Not run` |
| GStarCAD 2023 | 通过 | 通过 | `Not run` |
| GStarCAD 2026 | 通过 | 通过 | `Not run` |

七个目标共 14 次完整测试项目构建通过，同时编译对应生产类库；未部署、未启动 CAD。编译目标的年份不是实际宿主年份，后续执行仍按[宿主矩阵](../编译说明.md#编译目标与验收宿主)分别记录产品和完整版本。

首轮已完成正常依赖还原与全矩阵构建，最终源码检查复用了已还原的依赖。后续按[影响范围规则](zfgk-migration-roadmap.md#按影响范围选择验证)增量验证：纯核心修正跑相关回归，宿主适配选择受影响目标，文档变更只检查文档，不因每次修改重跑完整矩阵。

构建并非全零警告：部分 net48 测试项目报告 `MSB3277`（System.ValueTuple 与 System.Text.Json 的引用版本冲突），GStarCAD 2022 报告 `MSB3270`（SDK gmap/gmdb 的 x86 元数据与 x64 项目不匹配）。已在迁移前 `5729ef6` 的独立 Fox 工作树以 AutoCAD 2019 Debug、GStarCAD 2022 Release 重现这些类别；本次没有修改对应包、引用或压制警告。它们仍是实际宿主加载和依赖验收的限制，不能因构建通过而忽略。

| 直接链接的生产核心 | 实际结果 | 重点 |
| --- | --- | --- |
| `PolylineBeta.CoreTests` | 10 场景、2570 断言通过 | 反向弧宽度、闭合起点、保守简化误差、竖向/斜向折返保留、四种端点配对与输入归属。 |
| `OrderingBeta.CoreTests` | 29 项检查通过 | 固定锚点、重复键、方向独立和原始范围邻近连通分量。 |
| `GeometryBeta.CoreTests` | 79 项断言通过 | 坡度/坡向、六种顶点排列、相对退化量、补偿重心和面索引预验证。 |
| `DwgPreviewBeta.CoreTests` | 43 项检查通过 | 尺寸、拟合、空/极端范围、路径、ADS 拒绝及已有输出保护；未模拟 DWG 渲染。 |

CI 已登记这四组生产核心回归；TypeDef 比较工具的匹配、乱序、缺失和身份错误自测通过。本次有意新增类型，不要求新旧程序集完整 TypeDef 序列相同，也未削弱比较工具。独立复核覆盖各线；三角形首顶点相关的退化判断和重心抵消误差已通过生产核心反例修正，并增加六排列回归。多段线窄带内折返的丢失反例已修正；独立距离检查覆盖 100000 个原顶点，符合简化误差界。DWG 输出数据流路径已拒绝，字体未知加载状态不再当作成功。

### 实际能力限制

- `TextMetricsBeta`：AutoCAD 2019/2025、ZWCAD 2025、GStarCAD 2026 绑定提供实现；ZW_2022、GC_2022、GC_2023 缺少必要 API，明确抛出 `NotSupportedException`。字体加载成功位、失败/替代位按指南处理，实际字体行为仍需宿主验证。
- `DwgPreviewBeta`：公共入口仅在 AutoCAD 编译目标提供；资源生命周期已静态复核，实际渲染、驱动和资源泄漏验收为 `Not run`。
- CAD 命令覆盖样式/工作数据库恢复、实际曲线适配、网格归属、连续渲染与失败路径，但没有启动 CAD 执行。见 [Beta 能力总览](guides/zfgk-beta.md)。
- Zfgk 仓库保持只读且没有构建；表格业务和其他排除项始终未引入。
