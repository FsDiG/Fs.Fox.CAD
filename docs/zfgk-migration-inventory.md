# Zfgk 来源能力处置清单

> 状态：Active，2026-10-25；依据[整体路线图](zfgk-migration-roadmap.md)。<br>
> 来源：`FeiSiDev/Fs.Zfgk.CAD` @ `a882003436fb4cc31b24c90f6779177c13a60abe`，49 个跟踪的 C# 文件。<br>
> 五条工作线的 Beta 代码已集成；“迁入”仅表示代码能力进入 Fox，不表示真实宿主已验收。

## 全部来源的处置

来源目录保留原拼写。仅迁入选定能力，其他同类方法按表中说明复用或排除。来源 README 记录已获原作者张帆授权；来源和版权记录集中保留在本清单及相关指南中，生产源码注释只描述算法、行为与对象归属，不复制二进制、程序集信息或原库依赖体系。

原源码注释中的来源信息按以下记录保留，不因清理源码用语而丢失：`PolylineUtil`、`TextUtil`、`GeTriangle` 记载作者为张帆（Zhang Fan）；`BlockUtil`、`ExtentUtil` 原文件头记载 `Author TianTeng`、`Copyright © 北京智帆高科科技有限公司 2015`。对应的排序与块求交实现原先引用同一来源声明。完整来源快照为上述 `a882003436fb4cc31b24c90f6779177c13a60abe`，具体能力和文件关系见下表及[块求交](guides/block-intersections-beta.md)、[排序](guides/ordering-beta.md)指南。

| 来源文件 | 处置 | Fox 落点或理由 |
| --- | --- | --- |
| `AutoCADCommonApp.cs` | 排除 | 空初始化类型。 |
| `Geometry/AcadPlaneUtil.cs` | 复用 | 现有 Plane、坐标体系。 |
| `Geometry/CoordinateUtil.cs` | 复用 | 现有坐标转换。 |
| `Geometry/GePointUtil.cs` | 复用，圆角归 A | PointEx、GeometryEx 与 SDK 组合；批量/闭合圆角补入 A，不迁入共享状态的极角排序。 |
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
| `ObjectARX/Entity/BlockUtil.cs` | 迁入 B/D，其他复用或排除 | 排序归 B，块内曲线求交归 D；插入及已有属性读写复用。缺失属性补建和整图导入语义分别见下表。 |
| `ObjectARX/Entity/CircleUtil.cs` | 复用 | CircleEx。 |
| `ObjectARX/Entity/CurveUtil.cs` | 复用或排除 | CurveEx、Curve3dEx 与 SDK 分割组合；排除忽略 ratio、少量采点判断完全包含及只取首条偏移结果的历史行为。 |
| `ObjectARX/Entity/DimensionUtil.cs` | 复用 | SDK 创建实体后使用现有落库；不引入固定 F2 文字覆盖测量值。 |
| `ObjectARX/Entity/EntityUtil.cs` | 复用 | EntityEx、对象访问、变换及范围能力；批量求交可组合 SDK，不新增薄包装。 |
| `ObjectARX/Entity/HatchUtil.cs` | 复用或排除 | HatchEx 等覆盖提取/构造；原外内边界分类未处理岛嵌套及坐标平面，排除该分类实现。 |
| `ObjectARX/Entity/LineUtil.cs` | 复用 | SDK 求交、CurveEx；不另建重复直线工具类。 |
| `ObjectARX/Entity/PolyfaceMeshUtil.cs` | 迁入 D | 显式目标空间和事务，预先验证三角面/四边面及全部索引。 |
| `ObjectARX/Entity/PolylineUtil.cs` | 迁入 A，其他复用或排除 | 整理、两两连接、反向、批量/闭合圆角及含弧点关系；批量链组装和历史采样规则分别见下表。 |
| `ObjectARX/Entity/RegionUtil.cs` | 构造复用，重建排除 | 构造使用 SDK；排除不能完整表达孔洞、离散边界及对象归属的原重建实现。 |
| `ObjectARX/Entity/TextUtil.cs` | 迁入 C，其他复用 | 按文字样式测量字符串；实体创建与文本提取复用 Fox。 |
| `ObjectARX/Entity/XDataUtil.cs` | 复用 | TypedValueList、XDataList、DBObjectEx。 |
| `ObjectARX/Interaction/GetInputUtil.cs` | 复用 | EditorEx 与现有输入能力。 |
| `ObjectARX/Interaction/GetPointUtil.cs` | 组合复用 | SDK 提示与 EditorEx 转换矩阵组合；来源的 WCS 输入/输出约定不能直接等同于 SDK 的 UCS 坐标。 |
| `ObjectARX/Interaction/ResultLocateUtil.cs` | 排除 | 植物结果窗口的数据和交互协议。 |
| `ObjectARX/Interaction/SelectUtil.cs` | 复用 | EditorEx、OpFilter；空间查询只保证 XY 包围框候选筛选。 |
| `ObjectARX/Interaction/SelectionSetUtil.cs` | 复用 | 现有选择与对象枚举。 |
| `ObjectARX/LayerUtil.cs` | 复用 | SymbolTableEx、LayerUnlockScope。 |
| `ObjectARX/LinetypeUtil.cs` | 复用 | 现有线型表和资源加载。 |
| `ObjectARX/Others/ConvertUtil.cs` | 复用 | 现有点和坐标转换，不引入 ZFGK 几何类型。 |
| `ObjectARX/Others/FormatUtil.cs` | 复用或排除 | BCL 格式化；排除依赖 ZFGK.Text 且舍弃不完整尾组的旧坐标文本协议。 |
| `ObjectARX/Others/ListUtil.cs` | 复用 | BCL 集合操作。 |
| `ObjectARX/Others/ViewUtil.cs` | 复用 | EditorEx 缩放与视图。 |
| `ObjectARX/SpacialIndex/DynamicPointSpatialIndex.cs` | 排除 | 有局部最近点与重置限制，不引入第二套索引。 |
| `ObjectARX/SpacialIndex/EntitySpatialIndex.cs` | 复用 | QuadTree；保留候选范围和精确几何的区别。 |
| `ObjectARX/TextStyleUtil.cs` | 组合复用 | 通用 SymbolTable 的 Add/AddOrChange 回调显式设置 FileName/BigFontFileName；不把 AddWithChange 的新建分支当作大字体已覆盖。 |
| `Other/NumberUtil.cs` | 排除 | 通用字符串业务，不属于本次 CAD 能力补齐。 |
| `Others/ApplicationUtil.cs` | 复用 | Env 中的配置和支持路径。 |
| `Properties/AssemblyInfo.cs` | 排除 | 保留 Fox 自身包、版本及程序集身份。 |
| `UI/ResultLocateForm.Designer.cs` | 排除 | 植物业务窗口布局。 |
| `UI/ResultLocateForm.cs` | 排除 | 植物重叠检查及删除行为。 |

## 能力级复核与差额

文件级归类不表示同文件的全部行为均已有等价入口。复核逐一阅读了上述 49 个文件的方法体，并对照实际共享编译项；未使用未编译 ToDo 证明能力已经覆盖。下列结论细化了原先过宽的“其他复用”，本轮排除的实现也保留明确理由。

| 来源能力 | 实际差额与本轮处置 |
| --- | --- |
| 批量圆角、闭合首末角（PolylineUtil.Fillet、GePointUtil.Fillet） | **补迁 A：CreateFilletedBeta**。现行 CurveEx.ChamferAt 只处理内部单角，不覆盖闭合首末角。首版限全直线、全线同一恒定宽度，统一检查共享边的两端回退预算；半径不静默缩小，失败不生成部分结果。详见[多段线指南](guides/polyline-beta.md)。 |
| 含弧多段线的点内外关系（PtRelationToPoly） | **补迁 A：含弧闭合线的只读点关系查询**。GeometryEx.PointOnRegion 只处理直线点集；取顶点后调用会丢失 bulge。重写有限解析算法，明确 OCS、平面外点、边界容差与填充规则；不复制来源无界重试、固定容差及 Ray 释放缺口，不依赖尚未统一的 Region/Brep 组合。 |
| 块内实际几何求交（BlockUtil.IntersectWith） | **补迁 D：显式事务的嵌套块曲线求交**。来源仅展开一层且异常时副本释放不完整；NestedForEach 又通过 SymbolTable.Change 升级块定义，不能当作只读查询底座。新实现使用原生 ForRead，明确累计变换、去重及不支持的块/曲线范围，见[块求交指南](guides/block-intersections-beta.md)。 |
| 批量多段线链组装（JoinPolys） | **普通合并组合复用 SDK JoinEntities**；当前 Beta 只承诺两条开线的唯一端点连接。原实现会消费输入且闭合处理不完整；确定分链、分叉拒绝和来源索引不属于现有 Beta 契约，本轮不另建链组装 API。 |
| 曲线区间截取/删除、转多段线 | **组合复用** Curve3dEx.GetSubCurve、Curve.GetSplitCurves、Polyline.ConvertFrom、Spline.ToPolyline 等实际适用的入口；分割/转换生成的对象由调用方登记或释放。不同曲线类型不承诺统一无损转换，不迁入来源的泛化包装。 |
| 缺少属性引用时自动补建（SetBlockRefAttribute） | **本轮排除属性同步流程**。ChangeBlockAttribute 只更新已有引用，AddAttsToBlocks 修改的是定义，均不等价于补建引用；来源补建分支还写入定义默认值而非调用方值。后续需先确定定义与引用的同步范围。 |
| 填充外/内边界分类 | **提取复用，分类原算法排除**。HatchEx.CreateBoundarys 按环提取；来源把所有被包含环当作内环，不能表达岛的嵌套奇偶，且丢失平面/标高。SDK loop type 不自动等同于通用几何包含分类。 |
| 文字样式大字体设置 | **通用符号表回调组合复用**。现有 AddWithChange 仅在更新分支设置 BigFontFileName，新建分支忽略 bigFont；这是既有入口的差额，本轮不改变该稳定入口，也不以其证明大字体新建已覆盖。 |
| 外部 DWG 导成指定名称块 | **组合复用**。GetBlockFrom(file, blockName, over) 复制文件中的同名块定义；GetBlockFrom(file, over) 导入整图模型空间但使用文件名。任意新名称的整图导入使用原生 Database.Insert，不混淆两种语义。 |
| 输入坐标、空间候选与格式协议 | **组合复用并保留语义边界**。WCS 基点先转 UCS，成功输入再转回 WCS，保留 PromptStatus；QuadTree/Rect 只作包围框候选筛选，真实几何另精筛。原库固定容差、采样余段 20% 规则和文本序列化协议均不随薄包装自动保留。 |

前三项是本次复核最有价值的差额，均已纳入本轮 Beta 补迁；以新契约与实际验证记录判断支持范围，不能再写成“已有接口完全覆盖”。自绘表格及专用模型继续排除。

## 迁入代码的静态审查

审查覆盖实际输入输出、副作用、数值边界、所有权、条件编译和测试是否调用生产实现。2026-10-26 以 `979b799` 为起点重新逐行检查下表全部 16 个生产文件及对应核心／宿主测试，并交叉复核；本次发现与实际验证见[完整核心审查与修正](#完整核心审查与修正)。注释说明推导、不变量、精度与对象生命周期，不逐句复述代码。

| 生产文件 | 关键审查结论 |
| --- | --- |
| PolylineCoreBeta.cs | 顶点元数据描述后继段；反向弧宽、固定半容差带、局部折返、四种端点配对和桥接归属均复核。批量圆角先规划再验证共享边预算。 |
| PolylineExBeta.cs | 只复制约定几何字段，输入借用、失败释放输出；精确比较 OCS 主动拒绝隐式平面变换。 |
| PolylineContainmentCoreBeta.cs | 直线和圆弧分别使用半开 Y 穿越，主弧不按端点框裁剪；稳定二次根、极值有理化和边界投影误差控制均有独立圆盘对照。 |
| PolylineRegionExBeta.cs | 完整验证源之后才判断 OffPlane；Normal/Elevation、WCS→OCS、宽度/厚度忽略及分别使用的容差与指南一致。 |
| BlockIntersectionExBeta.cs | 只读显式栈遍历当前定义路径，重复实例按各自矩阵处理；原生安全曲线白名单、球形去重及临时对象释放明确。 |
| OrderingBetaCore.cs | 使用固定锚点及传递的精确比较；原始索引保证相等键稳定。范围图只由原框建边，合并框不参与后续邻接。 |
| PointOrderingBeta.cs | 有限点、快照和方向语义与核心一致；XML 已明确重复项和原始索引。 |
| CurveOrderingExBeta.cs | 全量验证点后再查询 native 最近点，使用实际里程而非人为扰动键；闭合接缝及多解沿用 SDK。 |
| ExtentGroupingBeta.cs | 原框快照及独立只读成员索引保持追溯，Z 仅在输出范围保留，不宣称真实实体相交。 |
| TextMetricsBeta.cs | 样式身份、字体加载/替代和旧 SDK 分支明确；先释放 GI/读取事务，再恢复 WorkingDatabase。字体实际表现仍待宿主验收。 |
| TriangleMathBeta.cs | 修正近北坡向舍入为 360，以及极小合法容差下坡度舍入为 90 的反例；失败保持两个 NaN。补偿重心及三边共同缩放说明已补。 |
| Triangle3dBeta.cs | 不可变有限顶点、退化仍可查询重心、角度/轴/NaN 契约完整，不持有数据库对象。 |
| PolyFaceMeshDataBeta.cs | 完整快照和索引/数量预验证；三角第四索引为零，不把索引合法当作几何非退化。 |
| PolyFaceMeshBuilderBeta.cs | Top 事务身份及异库拒绝；惰性 faces 完整枚举先于写入，所有 native 子对象登记同一调用方事务。 |
| DwgPreviewCoreBeta.cs | 像素/路径/ADS、拟合和极端数值边界明确；文件输出仅清理本次成功创建的临时文件。 |
| DwgPreviewBeta.cs | 外部数据库/事务包住 GS 对象生命周期，借用 manager 与自有对象分开；修复 CreateNew 失败后误删既存临时路径的问题。实际 GS 清理仍待宿主验收。 |

## 完整核心审查与修正

2026-10-26 的完整审查重新检查实现和测试，不以先前分册的 Approved 结论代替源码复核。公共入口、Beta 标记、程序集及编译项不变；16 个文件的注释补充算法原因、坐标和所有权约定。源码中的迁移品牌／作者来源表述已清理，原始版权及能力来源保留在本清单和指南中。

| 确认的问题 | 修正与回归 |
| --- | --- |
| 直段端点往返计算使 `(0,1)→(1,0.3)` 的下界上移，三角形外点 `(-1,0.3)` 被误判内部 | 直段 AABB 直接使用输入端点；原向／反向、左右外点、顶点边界和内部点回归。圆弧保守范围保持原算法。 |
| 次正规端点的对角距离被舍入到轴向容差，连接错误接受超距间隙 | 在阈值谓词中对次正规差量和容差共同升尺度，不改变通用长度函数；测试拒绝斜距、接受轴距及普通／次正规勾股边界。 |
| 同类距离舍入使区域外点被当成端点容差内边界 | 点关系端点复用该阈值谓词，正反轮廓独立验证 Outside／Boundary。 |
| 清理中单位化后的点积舍入，使 `(0,0)→(1,12)→(-59,17)` 的精确 90° 折点被删除 | 局部转向符号改为最大分量缩放方向，固定参考带算法不变；正反方向保留折点回归。 |
| 次正规参考长度无法可靠归一化，零容差仍可能删除非共线点 | 保守保留次正规非零直段，只清除精确重复点；零容差、开闭状态及反向均有回归。 |
| 范围组合在 `x=y=maximumGap=double.Epsilon` 时误把斜距算进邻近阈值 | 次正规数共同升尺度后计算缩放斜距；保留正常／次正规 `5-12-13` 含边界语义。 |
| 法向量或水平分量约 `1e-200` 时，平方下溢导致有效三角形被当作退化／水平 | 按最大分量缩放求模，维持原容差量纲；两类反例各检查六种顶点排列。 |
| 块交点按容差归一化的平方和会把距离恰为 13 的 `(5,12,0)` 排除在阈值外 | 按最大轴差缩放求斜距，次正规数先升尺度；新增二维／三维含边界公共入口回归，宿主 `Not run`。 |
| 预览眼点和目标各自有限，但相减后的方向可能溢出 | 同时要求视线方向有限且为正；生产核心新增对应拒绝回归，避免传入无效 GS 相机。 |

另明确两处契约：网格顶点按目标块表记录的存储坐标追加，普通块定义使用局部坐标；预览只筛选顶层实体，复合实体使用整体范围，不保证输出像素非空白或无内部隐藏几何留白。文字测量、原生克隆／求交、网格事务与 GS 释放均完成静态审查，尚未运行的宿主行为仍为 `Not run`。

五组回归均直接调用生产核心。在仓库外，以 `979b799` 的 Fox 核心配合本轮测试进行对照，五个项目均按预期检出旧行为错误；没有回退工作区，也没有构建来源库。当前实现的实测结果如下：

| 生产核心 | 当前结果 |
| --- | --- |
| `PolylineBeta.CoreTests` | 21 场景／2979 断言通过 |
| `PolylineRegionBeta.CoreTests` | 12 场景／80911 断言通过 |
| `OrderingBeta.CoreTests` | 30 项通过 |
| `GeometryBeta.CoreTests` | 121 断言通过 |
| `DwgPreviewBeta.CoreTests` | 56 项通过 |

本地仅串行构建 AutoCAD 2025 Debug x64（net8，0 警告／0 错误）和 AutoCAD 2019 Release x64（net48，通过），覆盖修改后的生产库及命令入口。没有重复本地七目标全矩阵。模块检查为 159 项、42 个债务文件、17 项边界发现；更新仅涉及这 16 个文件的源码哈希，TypeDef 比较工具自测通过，不代表实际 CAD 宿主运行。正式 CI 沿用现有工作流集中验证最终提交。

## 工作线依赖与交接

五条线均依赖 Fox 的现有基础设施，互相不依赖新增公共 API，因此可在本清单提交后并行。A/B/D 的纯计算内核使用独立的、直接链接生产源文件的测试；CAD 对象适配和 C/E 的宿主行为由命令测试覆盖并单独记录是否执行。

| 工作线 | 新增能力 | 基础依赖 | 交接条件 |
| --- | --- | --- | --- |
| A | 多段线整理、连接、反向、批量/闭合圆角及含弧点关系 | 现有顶点数据及 SDK Polyline | 输入不被消费；完整几何字段、共享边预算、坐标平面和错误边界可验证。 |
| B | 空间排序和范围组合 | 现有点、Curve、Extents3d | 不扰动排序键，重复项保留，范围组合规则确定。 |
| C | 字符串样式测量 | 各宿主 GraphicsInterface.TextStyle | 显式样式和目标数据库，释放测量资源；属性对照结论明确。 |
| D | 三角形计算、多面网格构造、嵌套块曲线求交 | SDK 几何、目标空间和调用方事务 | 单位与退化策略确定，写入前验证；求交查询使用只读遍历和自有临时副本。 |
| E | AutoCAD DWG 预览 | 活动文档 GraphicsManager、外部只读数据库 | 所有权、空图与异常路径明确，不改当前图纸或全局配置。 |

集成人维护共享编译清单、模块元数据、基线和文档索引。各线使用独立工作树，提交可独立构建的变更；滚动合入时重算基线，不把新增类型当作纯文件移动，也不修改比较工具掩盖类型变化。

## 验证记录

源码基线为同一提交中的 `Build/CADSharedModuleBaseline.json`：159 个共享编译文件，包含 16 个新增 Beta 源文件。原有 143 个共享源码的规范化 SHA256 与迁移前一致。模块校验通过，既有 42 个带债务标记的文件和 17 项边界发现未增加。来源清单与来源仓库 49 个跟踪 C# 文件逐项比对，无遗漏或多项。

首次集成 `afb56f1` 的本地完整构建如下；后续改动另按影响范围补充记录，不能把历史构建表当作新提交所有目标的复验。

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
| `PolylineBeta.CoreTests` | 19 场景、2929 断言通过 | 反向弧宽度、保守清理与连接；整线/闭合圆角、锐钝转角、共享边预算、精确共线及方向下溢拒绝。 |
| `OrderingBeta.CoreTests` | 29 项检查通过 | 固定锚点、重复键、方向独立和原始范围邻近连通分量。 |
| `GeometryBeta.CoreTests` | 85 项断言通过 | 坡度/坡向、360/90 度舍入边界、六种顶点排列、相对退化量、补偿重心和面索引预验证。 |
| `DwgPreviewBeta.CoreTests` | 55 项检查通过 | 尺寸、拟合、空/极端范围、路径、ADS 拒绝；真实 IO 验证临时名冲突、编码异常及目标竞争，不执行 CAD 渲染。 |
| `PolylineRegionBeta.CoreTests` | 10 场景、80885 断言通过 | 主弧/负凸度、切线及半开端点、三段 Y 单调弧、范围端点旋转、平移/缩放/反向；密集对照独立圆盘切割判据。 |

CI 已登记五组生产核心回归，包括新增含弧点关系工程；TypeDef 比较工具的匹配、乱序、缺失和身份错误自测通过。本次有意新增类型，不要求新旧程序集完整 TypeDef 序列相同，也未削弱比较工具。独立复核覆盖各线；三角形首顶点相关的退化判断和重心抵消误差已通过生产核心反例修正，并增加六排列回归。多段线窄带内折返的丢失反例已修正；独立距离检查覆盖 100000 个原顶点，符合简化误差界。DWG 输出数据流路径已拒绝，字体未知加载状态不再当作成功。

### 本轮增量验证

本小节保留截至 `4064025` 的补迁增量记录；后续以 `979b799` 为起点的完整复核和新修正见[完整核心审查与修正](#完整核心审查与修正)，不能将两批结果混用。

本轮完成 49 个来源文件和全部 16 个迁入源文件的静态复核，并补迁批量/闭合圆角、含弧点关系及块内曲线求交。已修复角度输出边界、临时文件删除权、圆角方向下溢、远点边界溢出和旋转浅弧误差传播反例；相关测试直接调用生产实现。

按实际影响执行四个单配置代表目标，未重复本地七目标 Debug/Release 全矩阵：

| 测试/类库目标 | 配置 | 实际结果与覆盖 |
| --- | --- | --- |
| AutoCAD 2025 | Debug x64 | 通过；net8、AutoCAD 预览适配及新增查询命令。 |
| ZWCAD 2022 | Release x64 | 通过；net48、本地 ZRX 2022 绑定。 |
| GStarCAD 2022 | Release x64 | 通过；net48、GrxCAD 命名空间；保留既有 MSB3270 架构告警。 |
| GStarCAD 2026 | Debug x64 | 通过；net8、Gssoft.Gscad 命名空间。 |

构建复用已还原依赖并串行执行。所需坐标转换、变换副本、IntPtr 求交重载和事务成员还逐一核对了七组 SDK 元数据；这仅证明成员存在，不能代替宿主行为证据。

圆角、含弧点关系、三角形和预览分别运行受影响核心回归。排序只有原因注释改变，沿用原生产核心检查，不因注释重跑。文档执行受影响链接与 GFM 渲染检查，CI 增加第五组核心工程；正式 CI 仍按现行工作流进行一次集成检查。

### 实际能力限制

- `CreateFilletedBeta`：首版只支持全直线且全线同一恒定宽度；不检查全局自交，半径/共享边预算不满足时整体拒绝。
- `GetPointRelationBeta`：非零绝对 bulge 仅支持 `[1e-8,1e8]`，按偶奇规则查询中心线区域，分别判断平面与边界容差。
- `GetCurveIntersectionsBeta`：查询普通嵌套块的存储曲线；动态块、外参、阵列、旧式多段线和未知曲线类型拒绝。非均匀变换及重合求交保留宿主语义，不承诺屏幕可见几何。
- `TextMetricsBeta`：AutoCAD 2019/2025、ZWCAD 2025、GStarCAD 2026 绑定提供实现；ZW_2022、GC_2022、GC_2023 缺少必要 API，明确抛出 `NotSupportedException`。字体加载成功位、失败/替代位按指南处理，实际字体行为仍需宿主验证。
- `DwgPreviewBeta`：公共入口仅在 AutoCAD 编译目标提供；资源生命周期已静态复核，实际渲染、驱动和资源泄漏验收为 `Not run`。
- CAD 命令覆盖样式/工作数据库恢复、实际曲线适配、网格归属、连续渲染与失败路径，但没有启动 CAD 执行。见 [Beta 能力总览](guides/zfgk-beta.md)。
- Zfgk 仓库保持只读且没有构建；表格业务和其他排除项始终未引入。
