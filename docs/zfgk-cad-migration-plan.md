# Fs.Zfgk.CAD 有价值能力迁移计划

> 稳定 ID：`plan.zfgk-cad-migration`<br>
> 状态：提案与执行准备（Proposal）<br>
> 目标分支：`migration/zfgk-cad`<br>
> 来源快照：`FeiSiDev/Fs.Zfgk.CAD@c38ce320c75284536c907c1046e5458da4ae0468`<br>
> 目标基线：`FsDiG/Fs.Fox.CAD@9faca0a4e420220bd3735de26c63f629564b6dc7`<br>
> 授权与再分发跟踪：[Issue #105](https://github.com/FsDiG/Fs.Fox.CAD/issues/105)<br>
> 最近复核：2026-08-03<br>
> CAD 宿主验收：Not run

本文记录从 `Fs.Zfgk.CAD` 向 `Fs.Fox.CAD` 吸收通用 CAD 能力的取舍、目标组织、实施顺序和验收边界。它不把来源仓库的文件结构、类名或实现视为迁移规格；每项代码在落地前仍须重新核对实时源码、ObjectARX/ZRX API、目标分支现状和授权范围。

## 1. 结论

不应整体复制 `Fs.Zfgk.CAD`，也不应在 `Fs.Fox.CAD` 中建立第二套 `ZFGK.AutoCAD.*` 或 `*Util` API。来源快照包含 49 个 C# 文件、15,856 行、53 个公开类型和约 451 个公开方法，但其形态有以下限制：

- 43 个文件直接引用 Autodesk AutoCAD 类型，没有 ZWCAD 共源编译边界。
- 11 个文件依赖未随源码提供的 `ZFGK.*` 项目类型；9 个文件直接依赖 WinForms、`MessageBox` 或相近 UI 行为。
- 项目引用固定到 AutoCAD 2019 managed API 和本机相对路径，当前检出不能作为可重复构建基线。
- 大量能力已被 `DBTrans`、`SymbolTable`、`PointEx`、`GeometryEx`、`CurveEx`、`EditorEx`、`DBDictionaryEx`、`HatchEx`、`QuadTree` 等现行实现覆盖。
- 若干未覆盖算法仍有通用价值，但旧实现存在资源所有权、容差、异常、事务和边界条件问题，必须按目标库契约重写并验证。

迁移的合理目标是“吸收能力”，不是“搬运代码”。优先级如下：

1. 曲线、折线和面域的只读几何算法，以及明确的对象所有权契约。
2. 面向点去重、邻域和范围查询的稀疏网格索引；它与现有四叉树互补，而不是替换四叉树。
3. 现有块导入/属性 API 没有覆盖的 DWG 导出工作流。
4. SHX 搜索与几何文本序列化等有明确跨产品需求的辅助能力。
5. 图形系统预览、DWG 表格和桌面 UI 只在出现真实通用消费者并能完成宿主验证后评估。

## 2. 不可突破的边界

### 2.1 产品与程序集

- `Fs.Fox.CAD` 仍是单产品、单套公共 API；迁移代码进入现有 `CADShared` 单程序集逻辑模块。
- 不改变 `Fs.Fox.Cad`、`Fs.Fox.Basal` 公共命名空间，不引入 `ZFGK.AutoCAD` 兼容命名空间。
- 不新增 `ZFGK.dll`、`ZFGK.AutoCAD.dll`、`ZFGK.WinForms.dll` 或其他来源二进制依赖。
- 不因为来源文件较大而建立新的 DLL、NuGet 包或“兼容层”。是否拆程序集仍遵循现行架构文档的消费者、依赖方向和宿主矩阵门槛。

### 2.2 API 与命名

- 无状态的宿主类型扩展进入现有 `*Ex` 类；拥有索引、缓存或生命周期的能力使用表达领域含义的名词类。
- 不沿用 `Acad*`、`Ge*`、`*Util`、`m_*`、`b*`、`p*` 等历史命名。平台差异通过现有全局别名、条件编译或平台实现表达。
- 新 API 不使用 `ref List<T>` 作为普通返回方式；查询优先返回值或 `IReadOnlyList<T>`，条件结果使用语义明确的 `Try*`。
- 容差参数必须说明度量单位、闭区间/开区间和零值语义，不能散落硬编码 `1e-4`、`1e-7`。
- 失败不能在几何或数据库层弹出 `MessageBox`、写当前 Editor 或静默提交事务；应返回可判断结果或抛出可记录的异常。

### 2.3 CAD 对象所有权

- `GetSplitCurves`、`Explode`、`Region.CreateFromCurves`、`GetOffsetCurves` 等返回的新 `DBObject`/`Curve` 必须逐项定义由谁释放。
- 方法不得释放调用方传入的 `Entity`、`Curve`、`Polyline`、`Database` 或 `Transaction`，除非 API 名称和 XML 注释明确转移所有权。
- 数据库写入必须接收或复用明确的事务/`DBTrans`；只读几何算法不隐式使用 `MdiActiveDocument` 或 `HostApplicationServices.WorkingDatabase`。
- 部分失败时应保证临时对象、目标数据库和事务被清理，不能通过 `catch` 后 `Commit()` 保留半完成状态。

### 2.4 多宿主

- 正式共享代码至少通过 AC_2019、AC_2025、ZW_2022、ZW_2025 四目标编译和兼容性守卫。
- `Autodesk.AutoCAD.GraphicsSystem`、COM、native export 或仅 AutoCAD 存在的 API 默认不得进入共享实现。
- 真实宿主验证优先 ZWCAD 2022，其次 AutoCAD 2020/2026；没有 ZWCAD 2025 宿主不是迁移阻塞项，也不能把编译表述为宿主通过。
- 未经明确批准，不启动 CAD，不修改 profile、Trusted Paths、注册表或启动组。

## 3. 授权与溯源门槛

来源仓库当前为 Private，GitHub 未识别到许可证；其 README 写明“基于智帆高科 CAD 类库封装，已经过张帆授权”，多份源码头同时标注“北京智帆高科科技有限公司版权所有”。目标仓库公开并采用 MIT License。因此，在复制源码或提交可识别的改写实现前，必须在 [Issue #105](https://github.com/FsDiG/Fs.Fox.CAD/issues/105) 确认现有授权明确覆盖：

1. 把相关实现并入公开的 `Fs.Fox.CAD`；
2. 修改、发布和分发这些实现；
3. 按目标仓库 MIT License 允许下游继续使用和再分发。

授权确认应在关联 Issue 中留下可追溯结论；敏感原件不必提交仓库。若授权只允许内部使用，则公开仓库只能保留功能需求和独立设计，不复制实现。若授权范围不能确定，则停止对应代码迁移，但不影响本清单继续用于去重和需求分析。

每个实际迁移 PR 需在正文中记录来源文件、来源提交和处理方式：

- `adapted`：确认授权后基于来源实现改写，并保留必要版权归属；
- `clean implementation`：仅采用公开需求/接口事实，由目标契约独立实现；
- `existing equivalent`：确认目标已有能力，不引入来源代码。

## 4. 评估方法

每项能力按以下维度复核，不按当前仓库是否存在调用方决定价值：

| 维度 | 需要回答的问题 |
| --- | --- |
| 通用性 | 是否属于 CAD 基础库，而非单一产品、图层命名或业务表格约定？ |
| 增量价值 | 目标库是否已经用更稳定的 API 提供相同能力？ |
| 正确性 | 边界、容差、异常、退化几何和部分失败是否可定义并验证？ |
| 所有权 | 临时 CAD 对象、事务、数据库和返回对象由谁释放？ |
| 多宿主 | 是否使用 AutoCAD 专属 API；ZWCAD 是否有等价行为？ |
| 依赖成本 | 是否需要未提供的 `ZFGK.*`、WinForms、COM 或 native 组件？ |
| 验证能力 | 能否先做确定性测试；哪些行为必须进真实 CAD 宿主？ |

只有同时具备通用价值、明确增量和可验证契约的能力才进入代码批次。来源实现有价值但短期不能验证时，保留在来源快照和本文中，不用条件编译把整份未经验证的旧类塞入生产项目。

## 5. 目标目录和类名

| 能力 | 目标位置 | 建议承载类型 | 说明 |
| --- | --- | --- | --- |
| 点、向量、坐标和普通几何 | `Cad/Geometry` | 现有 `PointEx`、`VectorEx`、`GeometryEx` | 只补确实缺失且可测试的方法。 |
| 点邻域/去重网格 | `Cad/Geometry/SpatialIndex/Grid` | `PointGridIndex` 或经评审后的同义名 | 有状态索引用名词类，不叫 `DynamicPointSpatialIndexUtil`。优先稀疏桶，避免旧实现的二维巨型数组。 |
| 曲线查询与拆分 | `Cad/Database/Entities/Curves` | 现有 `CurveEx`，必要时按能力拆文件 | 返回新曲线时必须写明释放责任。 |
| 折线查询、采样和编辑 | `Cad/Database/Entities/Curves/Polylines` | 现有 `PolylineEx` | 只读查询与原位修改分批，不在一个 PR 混合。 |
| 面域边界 | `Cad/Database/Entities` | 现有 `RegionEx` | 与 Issue #103/#107 记录的 `ToCurves()` 所有权风险一并设计。 |
| 块导入、导出和属性 | `Cad/Database/Entities/Blocks`、`Cad/Database/SymbolTables` | 现有 `BlockReferenceEx`、`SymbolTableEx`，或窄职责 `BlockExportEx` | 先复用 `GetBlockFrom`、`InsertBlock` 和属性 API，避免平行入口。 |
| 字体与文字样式 | `Cad/Database/Entities/Text`、`Cad/Database/SymbolTables` | 现有文字扩展和 `SymbolTableEx` | 搜索路径读取属于应用边界，不能藏在文字样式写事务中。 |
| Editor 输入与选择 | `Cad/Editor` | 现有 `EditorEx`、`PromptOptionsEx` | 原生 `Editor.Get*` 已足够时不再包一层 bool/out API。 |
| AutoCAD 专属图形系统 | 平台专属目录，待设计 | 不预设公共类型 | 只有存在真实消费者和 ZWCAD 边界决策时再建。 |

## 6. 逐文件迁移清单

### 6.1 根文件与 Geometry

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `AutoCADCommonApp.cs` | 空壳类型，无行为。 | 不迁移。 |
| `Properties/AssemblyInfo.cs` | 旧项目程序集元数据。 | 不迁移；目标项目自行生成/维护。 |
| `Geometry/AcadPlaneUtil.cs` | 仅返回 XY 平面；厂商 API 和现有 `PlaneEx` 足以表达。 | 不迁移。 |
| `Geometry/CoordinateUtil.cs` | UCS/WCS 转换已由 `GeometryEx`、`EditorEx` 提供；旧实现隐式取活动文档。 | 复用现有实现。 |
| `Geometry/GePointUtil.cs` | 极坐标、中点、二维距离和方向已有等价实现；点去重、排序和折线圆角概念仍可复核。 | 部分候选；只迁移经独立测试证明的缺失算法。 |
| `Geometry/GeRectangleUtil.cs` | 轴向矩形相交已由 `Rect.IntersectsWith` 覆盖。 | 复用现有实现。 |
| `Geometry/GeTriangle.cs` | 坡度/坡向和重心有领域价值，但旧实现存在退化三角形、法向方向和重心 Z 值问题。 | 条件候选；作为新的明确几何契约重写，不复制该类型。 |
| `Geometry/MathUtil.cs` | 多数函数已有 `System.Math` 等价项；采样、反函数和容差方法的边界契约不完整。 | 不迁移整类；真实需求单独设计。 |

### 6.2 ObjectARX 基础能力

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/AcadDocumentUtil.cs` | 文档枚举可直接使用 `DocumentManager`；`GetOpenedDocument` 忽略文件名并返回第一个文档。 | 不迁移。 |
| `ObjectARX/ArxOthers/DwgScaleUtil.cs` | 使用特定字典键保存“标注/信息比例”，更像产品数据协议。 | 默认不迁移；若多个产品依赖同一持久化格式，先建立独立契约。 |
| `ObjectARX/ArxOthers/ExtentUtil.cs` | 宽高、中心、扩展已被 `Rect`、`BoundingBox9`、`PointEx` 覆盖；范围关系和相交簇合并仍有价值。 | 部分候选；基于现有范围类型重写关系/聚类，不复制事务辅助。 |
| `ObjectARX/ArxOthers/GroupUtil.cs` | 已由 `DBDictionaryEx.AddGroup`/`GetGroups` 覆盖。 | 复用现有实现。 |
| `ObjectARX/ArxOthers/GsPreviewUtil.cs` | DWG 到位图预览有价值，但直接使用 AutoCAD GraphicsSystem、设备和视图生命周期。 | 高风险条件候选；独立平台设计和真实宿主测试前不迁移。 |
| `ObjectARX/ArxOthers/SectionUtil.cs` | 道路/断面面积算法带明显业务语义和 WinForms 提示。 | 不进入通用基础库。 |
| `ObjectARX/DictionaryUtil.cs` | XDictionary/Xrecord 能力已由 `DBDictionaryEx`、`XRecordDataList` 和对象扩展覆盖。 | 复用现有实现。 |
| `ObjectARX/DwgDatabaseUtil.cs` | 入库、空间枚举、Handle 转 ObjectId 已由 `DBTrans`、`SymbolTableRecordEx`、`ObjectIdEx`、`EntityEx` 覆盖。 | 复用现有实现。 |

### 6.3 DWG 表格

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/DwgTable/AcadDwgTable.cs` | 自绘表格、合并单元格和布局可能有业务价值，但依赖缺失的 `ZFGK.DwgTableBase`/`ZFGK.Utility`，实现约 1,500 行。 | 暂缓；先确认基础模型源码、真实消费者和与原生 `Table` 的差异。 |
| `ObjectARX/DwgTable/AcadSubDrawTable.cs` | 只是缺失表格基础模型的 CAD 适配。 | 随上项处理，不单独迁移。 |

### 6.4 实体、曲线和面域

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/Entity/ArcUtil.cs` | 反转圆弧可由厂商曲线 API表达；旧实现直接改 Normal/角度，需额外验证非 XY 平面。 | 不直接迁移。 |
| `ObjectARX/Entity/BlockUtil.cs` | 块导入、插入和属性大多已有等价实现；“选定实体导出 DWG”仍可能有增量价值。 | 部分候选；仅评估缺失的导出契约和属性度量。 |
| `ObjectARX/Entity/CircleUtil.cs` | 仅构造并入库，现有实体添加 API 足够。 | 不迁移。 |
| `ObjectARX/Entity/CurveUtil.cs` | 子曲线、弦高、归一化位置、偏移和交点查询具有通用价值；旧实现对返回对象的释放和异常处理不完整。 | 高优先候选；拆为只读计算、返回新对象、数据库修改三个批次。 |
| `ObjectARX/Entity/DimensionUtil.cs` | 旋转标注构造简单，未形成独特抽象。 | 低优先条件候选；有重复消费者时再加入窄扩展。 |
| `ObjectARX/Entity/EntityUtil.cs` | 擦除、变换、颜色、包围盒和克隆大多已有等价实现。 | 复用现有实现；块属性克隆随 Block 项评估。 |
| `ObjectARX/Entity/HatchUtil.cs` | 已有 `HatchEx`、`HatchConverter` 和边界创建能力；来源还依赖外部工具。 | 不迁移平行 API。 |
| `ObjectARX/Entity/LineUtil.cs` | 方位、点线距离和插值有通用价值；相交状态和高程插值旧实现存在语义/结果问题。 | 部分候选；采用明确枚举/结果类型和退化线契约重写。 |
| `ObjectARX/Entity/PolyfaceMeshUtil.cs` | 仅创建网格并入库，现有实体添加机制可组合完成。 | 不迁移。 |
| `ObjectARX/Entity/PolylineUtil.cs` | 连接、采样、分段、圆角、去零段和去共线点是本次最有价值的来源之一；旧实现同时操作事务、UI、对象释放和原位修改。 | 高优先候选；按“查询 -> 创建新对象 -> 原位修改 -> 批量数据库操作”四层重写。 |
| `ObjectARX/Entity/RegionUtil.cs` | `Explode` 后转折线并连接可为 `RegionEx.ToCurves()` 提供替代思路，但旧实现泄漏临时对象且没有内外环/方向契约。 | 高优先设计输入；与 Region 所有权和 ZWCAD BRep 路径统一处理。 |
| `ObjectARX/Entity/TextUtil.cs` | 添加文字、文字样式和去格式化大多已有实现；文字边界度量可能有宿主差异。 | 复用现有实现；度量需求单独验证。 |
| `ObjectARX/Entity/XDataUtil.cs` | 已由 `TypedValueList`、`XDataList`、`DBObjectEx` 和选择过滤体系覆盖。 | 复用现有实现。 |

### 6.5 Editor、符号表和辅助能力

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/Interaction/GetInputUtil.cs` | 对原生 PromptOptions 的 bool/out 包装会丢失 PromptStatus 和取消原因。 | 不迁移；增强现有 `EditorEx`/`PromptOptionsEx` 时保留原生结果。 |
| `ObjectARX/Interaction/GetPointUtil.cs` | 同上，且隐式依赖活动 Editor。 | 不迁移。 |
| `ObjectARX/Interaction/SelectUtil.cs` | 类型选择、窗口选择和按图层选择已可由 `EditorEx.SSGet` 和过滤器组合。 | 复用现有实现。 |
| `ObjectARX/Interaction/SelectionSetUtil.cs` | implied selection 是原生 Editor API，目标代码已有使用。 | 不新增平行包装。 |
| `ObjectARX/Interaction/ResultLocateUtil.cs` | 与专用 WinForms 结果定位窗口强耦合。 | 不进入基础库。 |
| `ObjectARX/LayerUtil.cs` | 已由 `SymbolTable<LayerTable,...>`、`SymbolTableEx` 和 Editor 选择能力覆盖。 | 复用现有实现。 |
| `ObjectARX/LinetypeUtil.cs` | 线型表访问已有统一符号表入口；系统线型文件加载需要单独的宿主路径契约。 | 默认复用现有实现；加载真实需求另立项。 |
| `ObjectARX/TextStyleUtil.cs` | 文字样式写入已有实现；支持路径中的 SHX 枚举/存在性检查仍可能有增量价值。 | 条件候选；分离“路径发现”和“样式写入”，禁止弹窗和静默替换。 |
| `ObjectARX/Others/ConvertUtil.cs` | 点/向量/集合和 UCS/WCS 转换已由 `PointEx`、`VectorEx`、`GeometryEx`、LINQ 覆盖；外部向量类型不属于目标库。 | 复用现有实现。 |
| `ObjectARX/Others/FormatUtil.cs` | 点/向量文本序列化可能有价值，但旧实现缺少文化区、版本和失败契约。 | 低优先条件候选；先定义 invariant/显示格式和 `TryParse`。 |
| `ObjectARX/Others/ListUtil.cs` | 唯一有效方法为交换元素，其余为大段历史注释。 | 不迁移。 |
| `ObjectARX/Others/ViewUtil.cs` | 已由 `EditorEx.Zoom*` 覆盖。 | 复用现有实现。 |
| `ObjectARX/SpacialIndex/DynamicPointSpatialIndex.cs` | 点去重、邻域、最近点和范围查询有明确通用价值。旧实现存在无效状态、密集二维数组、UI 弹窗和边界错误。 | 高优先候选；重写为稀疏网格索引并补确定性测试。 |
| `ObjectARX/SpacialIndex/EntitySpatialIndex.cs` | 实体范围查询有价值，但目标已有 `QuadTree`；旧实现把事务和网格构建耦合。 | 不迁移该类型；仅把可证明的批量建索引需求补到现有空间索引体系。 |
| `Other/NumberUtil.cs` | 只拆尾部数字；无数字时抛异常但始终声明返回 true。 | 不迁移；真实需求使用明确 `Try*` 契约。 |
| `Others/ApplicationUtil.cs` | 支持路径读取与现有 `Env` 边界重合。 | 复用现有实现。 |
| `UI/ResultLocateForm.cs`、`UI/ResultLocateForm.Designer.cs` | 专用结果定位窗口，包含删除/定位业务流程。 | 不进入通用基础库。 |

## 7. 已确认不能原样继承的问题

下列问题说明为何需要逐能力重写；它们不是对历史代码用途的否定：

| 来源位置 | 问题 | 迁移要求 |
| --- | --- | --- |
| `AcadDocumentUtil.GetOpenedDocument` | 参数没有参与匹配，循环第一次就返回。 | 文档查找必须定义路径比较、大小写和未保存文档语义。 |
| `ExtentUtil.GetExtentRelation` | 相等矩形和容差边界的包含判断不一致。 | 用表驱动测试覆盖相等、相切、包含、近似分离和退化范围。 |
| `DynamicPointSpatialIndex.GetBlockOfPoint` | 最大边界可能计算到数组长度之外；索引采用潜在百万格的密集二维数组。 | 使用经过边界夹取的稀疏桶；构造时拒绝无效范围/格长。 |
| `DynamicPointSpatialIndex.AddPoint` | 即使添加成功也固定返回 `false`。 | `TryAdd` 必须区分新索引、既有近似点和范围外失败。 |
| `PolylineUtil.Join*` | 内部释放第二条折线，外层随后仍可能擦除/访问同一对象。 | 方法不释放输入；结果和所有权由调用方显式处理。 |
| `CurveUtil.GetSubCurve`、`RegionUtil.CreatePolylineByRegion` | 只返回部分新对象，未完整释放其他 split/explode 结果。 | 对所有临时 `DBObject` 使用可审计清理路径，并定义返回对象所有权。 |
| `LineUtil.InterpolatePoint` | 按高程插值后返回点的 Z 使用默认值 0，边界分支也丢失目标高程。 | 结果必须保留请求高程并覆盖水平线、端点和范围外情况。 |
| `BlockUtil.CreateDwgBlockFile` | 异常路径仍提交源/目标事务，输出版本固定为 AC1800。 | 默认失败不提交；DWG 版本、覆盖和临时文件替换必须显式。 |
| `TextStyleUtil.Add` | `finally` 中无条件提交，字体不存在时弹窗并隐式替换。 | 事务成功才提交；字体回退是调用方策略，库层不弹窗。 |
| `GsPreviewUtil` | GraphicsSystem 设备、kernel、view 和 bitmap 生命周期为 AutoCAD 专属。 | 平台实现分别验证；禁止用 AC_2019 编译通过推断 ZWCAD 可用。 |

## 8. 分阶段实施

### Phase 0：基线、授权和去重

- [x] 从最新 `main` 建立 `migration/zfgk-cad`。
- [x] 固定来源/目标 commit，完成 49 个 C# 文件的首轮清单。
- [x] 记录现有等价 API、外部依赖和已知风险。
- [x] 将授权与再分发门槛关联到 Issue #105。
- [ ] 确认授权是否覆盖公开 MIT 再分发，并在关联 Issue 留下结论。
- [ ] 为第一批候选写具体 API 草案和测试样例，确认没有与实时 `main` 重复。

Phase 0 只允许文档和验证基础设施变更。授权未确认前，不直接复制或改写带来源版权的实现。

### Phase 1：只读几何查询

候选范围：

- 曲线按归一化距离取点、弦高/逼近误差计算；
- 折线顶点、bulge、段长和确定性采样；
- 直线/线段方位与高程插值的明确结果契约；
- Extents/Rect 的关系和相交簇合并。

约束：不写数据库、不依赖活动文档、不原位修改输入、不弹 UI。先增加测试，再增加公共 API；每个返回的新 CAD 对象都写明释放责任。

### Phase 2：点网格索引

以来源的需求集合为输入，重新设计稀疏 `PointGridIndex`：

- 构造后始终有效，不保留需要先调用 `Create()` 的半初始化状态；
- 支持近似点去重、范围查询和受最大距离约束的最近点查询；
- XY 容差、Z 容差、格长和边界闭合规则显式；
- 不依赖 WinForms，不在失败时弹窗；
- 与 `QuadTree` 做职责对照和基准，不能建立两套等价实体索引。

### Phase 3：曲线/折线修改与 Region

- 在只读算法稳定后，再实现返回新对象的拆分、连接和简化。
- 原位修改折线作为独立批次，保留宽度、bulge、闭合状态、Normal、Elevation 和属性。
- Region 边界提取同时评估 `Explode` 路径、AutoCAD BRep 路径和 ZWCAD 2022 能力；定义外环/内环、方向、顺序、部分失败和对象所有权。
- Issue #103/#107 中条件编译保留的 `RegionEx.ToCurves()` 只作为历史实现和风险清单，不因本迁移直接启用。

### Phase 4：数据库工作流

- 先证明现有 `GetBlockFrom`、`InsertBlock`、`ChangeBlockAttribute` 等 API 的缺口。
- 如仍有增量，再设计“选定实体导出 DWG”：目标版本、基点、重复记录、覆盖、临时文件、原子替换和失败回滚均为必填契约。
- SHX 搜索仅在调用方确有跨宿主需求时实现；路径枚举、去重、访问异常和文件名比较需可测试。
- 数据库写入与 UI 提示分离，不把当前 Document/Editor 隐式嵌入底层方法。

### Phase 5：宿主专属和暂缓能力

DWG 图像预览、复杂自绘表格、结果定位窗体等不能搭便车进入共享程序集。每项需具备：

1. 至少一个真实通用消费者；
2. 完整依赖源码或可接受的独立替代设计；
3. AutoCAD/ZWCAD 支持决策；
4. 可执行的真实宿主验收场景。

不满足时维持“暂缓/不迁移”，不使用大段 `#if false` 保存旧仓库副本。

## 9. 分支和 PR 组织

`migration/zfgk-cad` 是本次迁移的长期集成分支。具体代码使用从该分支建立的短分支，并将 PR base 指向该分支；建议批次名称保持窄小，例如：

- `zfgk/geometry-query`
- `zfgk/point-grid`
- `zfgk/polyline-edit`
- `zfgk/region-boundary`
- `zfgk/block-export`

每个批次只处理一个所有权边界。长期分支阶段性合入最新 `main`，但冲突解决后必须重新执行模块守卫、兼容性检查和受影响构建。最终进入 `main` 前重新审查整个差异，不能把“各子 PR 已通过”替代集成验证。

## 10. 每批验收

### 10.1 静态和构建

- `git diff --check`
- `pwsh -File Build/Verify-CADSharedModuleMap.ps1`
- `pwsh -File tools/verification/Test-CADSharedTypeDefSequence.ps1`
- `pwsh -File Build/Verify-CADSharedCompatibility.ps1`
- AC_2019、AC_2025、ZW_2022、ZW_2025 Release 构建
- 新 Markdown 相对链接和 GitHub Flavored Markdown 渲染检查

新增编译文件时，在 `CADShared.projitems` 中填写正确的 `FsFoxModule`/`FsFoxOrder`，并通过守卫提供的更新流程同步模块基线；不能手工绕过预期计数或把新边界债务静默加入白名单。

### 10.2 算法测试

至少覆盖：

- 空集合、单元素、重复点、零长度段和退化几何；
- 正/负坐标、非常大/小的数值、端点和恰好位于容差边界；
- 开放/闭合折线、顺/逆时针、直线段/圆弧段、非零 Normal/Elevation；
- 方法失败后的输入不变性、临时对象释放和重复调用；
- AutoCAD/ZWCAD API 返回集合在空、部分成功和异常时的所有权。

### 10.3 真实宿主

涉及数据库写入、Region/BRep、GraphicsSystem、Editor 或 UI 的批次，最终必须记录实际宿主：

| 优先级 | 宿主 | 用途 |
| --- | --- | --- |
| 1 | ZWCAD 2022 | 共享 API、Region/折线、数据库和 Editor 的首要验收。 |
| 2 | AutoCAD 2020（AC_2019 二进制基线） | 老 ObjectARX 行为与来源实现对照。 |
| 3 | AutoCAD 2026（AC_2025 二进制基线） | .NET 8/新宿主行为和兼容性。 |
| Not required | ZWCAD 2025 | 当前无真实宿主，不阻塞，但保持编译目标。 |

当前本文只完成静态盘点，没有运行 CAD，宿主状态统一为 `Not run`。

## 11. 完成定义

本迁移不是以“来源文件全部处理”或“公共方法数量接近 451”为完成标准。满足以下条件才可收口：

1. 每个来源文件都已有 `复用现有`、`已重写`、`暂缓` 或 `不迁移` 的明确结论；
2. 所有迁移能力符合当前目录、命名、事务和对象所有权契约；
3. 没有引入来源二进制、AutoCAD-only using 泄漏或第二套平行 API；
4. 自动构建/守卫与要求的真实宿主场景分别记录，未执行项不被包装为通过；
5. 最终有效的公共行为写入 XML 注释和 current 文档，本文随后转为 `historical`；
6. 长期分支相对最新 `main` 完成最终集成审查后，才建立合入 `main` 的 PR。

## 12. 下一步

当前最合理的下一批不是搬运 `PolylineUtil.cs`，而是：

1. 完成 MIT 再分发授权确认；
2. 从 `CurveUtil`、`PolylineUtil`、`RegionUtil` 提炼 5 至 8 个只读几何用例；
3. 对照实时 `CurveEx`、`PolylineEx`、`RegionEx` 去重，形成 Phase 1 API 草案；
4. 先补算法测试和所有权测试，再提交第一批代码。

若授权确认尚未完成，可以先推进完全独立的需求规格和测试向量，但不提交可识别的来源实现。
