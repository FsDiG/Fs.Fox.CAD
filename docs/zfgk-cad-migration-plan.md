# Fs.Zfgk.CAD 有价值能力迁移计划

> 稳定 ID：`plan.zfgk-cad-migration`<br>
> 状态：最终收口中（Active）<br>
> 目标分支：`migration/zfgk-cad`<br>
> 来源快照：`FeiSiDev/Fs.Zfgk.CAD@c38ce320c75284536c907c1046e5458da4ae0468`<br>
> 最近 `main` 集成：`origin/main@6e6f94223be418481663833fc65386d8cf2839a4` 已由 `2e68759f710046416f4e27d2c2f87487e4160619` 合入<br>
> 实施跟踪：[Issue #110](https://github.com/FsDiG/Fs.Fox.CAD/issues/110)<br>
> 最近复核：2026-08-03<br>
> CAD 宿主验收：Not run

本文记录从 `Fs.Zfgk.CAD` 向 `Fs.Fox.CAD` 吸收通用 CAD 能力的取舍、目标组织、实施顺序和验收边界。它不把输入仓库的文件结构、类名或实现视为迁移规格；每项能力在落地前仍须重新核对实时源码、ObjectARX/ZRX/GRX API 和目标分支现状。

## 1. 结论

不应整体复制 `Fs.Zfgk.CAD`，也不应在 `Fs.Fox.CAD` 中建立第二套 `ZFGK.AutoCAD.*` 或 `*Util` API。来源快照包含 49 个 C# 文件、15,856 行、53 个公开类型和约 451 个公开方法，但其形态有以下限制：

- 43 个文件直接引用 Autodesk AutoCAD 类型，没有 ZWCAD 共源编译边界。
- 11 个文件依赖未随源码提供的 `ZFGK.*` 项目类型；9 个文件直接依赖 WinForms、`MessageBox` 或相近 UI 行为。
- 项目引用固定到 AutoCAD 2019 managed API 和本机相对路径，当前检出不能作为可重复构建基线。
- 大量能力已被 `DBTrans`、`SymbolTable`、`PointEx`、`GeometryEx`、`CurveEx`、`EditorEx`、`DBDictionaryEx`、`HatchEx`、`QuadTree` 等现行实现覆盖。
- 若干未覆盖算法仍有通用价值，但旧实现存在资源所有权、容差、异常、事务和边界条件问题，必须按目标库契约重写并验证。

迁移的合理目标是“吸收能力”，不是“搬运代码”。最终只吸收三组可证明的通用增量：

1. 曲线、点和折线的只读几何查询，包括距离比例取点、弦偏差、顶点数据、子段长度、插值和确定性采样；
2. 面向点去重、邻域和范围查询的稀疏 `PointGridIndex`，与现有四叉树形成明确分工；
3. 为以上能力配套的跨宿主编译、公共契约守卫和宿主内确定性测试命令。

其余来源能力最终决定为“复用现有”或“不迁移”，不再保留后续候选。以后出现相似需求时，应从 `Fs.Fox.CAD` 的实时契约重新设计，不继续以 `Fs.Zfgk.CAD` 为迁移输入。

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

- 正式共享代码至少通过 AC_2019、AC_2025、ZW_2022、ZW_2025、GC_2022、GC_2023、GC_2026 七目标编译和兼容性守卫。
- `Autodesk.AutoCAD.GraphicsSystem`、COM、native export 或仅 AutoCAD 存在的 API 默认不得进入共享实现。
- 真实宿主验证优先 ZWCAD 2022，其次 AutoCAD 2020/2026；没有 ZWCAD 2025 宿主不是迁移阻塞项，也不能把编译表述为宿主通过。
- 当前没有 GStarCAD 真实宿主验收约定；三个 GC 目标只记录 Build-only 结果，不能从编译推断宿主行为。
- 未经明确批准，不启动 CAD，不修改 profile、Trusted Paths、注册表或启动组。

## 3. 审查决策与可追溯性

维护者已于 2026-08-03 明确：本迁移不把源代码来源或许可核对作为能力审查和实施门槛。Issue #105 继续独立审计当前 `main` 的既有第三方代码，不阻塞本计划；迁移范围、取舍和验收统一由 [Issue #110](https://github.com/FsDiG/Fs.Fox.CAD/issues/110) 跟踪。

固定输入快照只用于保证“审查了什么”可以复核，不决定目标代码的目录、类型或 API。每个代码批次仍需记录：

- 对应的输入文件和能力，以便核对 49 文件清单是否遗漏；
- 目标库中的既有等价实现、实际增量和最终所有权位置；
- 已修正的旧实现缺陷，以及新增 API 的容差、资源所有权和多宿主边界；
- 自动验证和真实 CAD 宿主验证分别执行了什么，未执行项明确写为 `Not run`。

当前调用情况不作为通用类库价值判断依据。某项能力没有现成调用方时，仍按通用性和契约质量评估；没有增量价值或无法形成可靠契约时，也不为追求迁移数量而加入平行 API。

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
| 点邻域/去重网格 | `Cad/Geometry/SpatialIndex/Grid` | `PointGridIndex` | 有状态索引用名词类，不叫 `DynamicPointSpatialIndexUtil`。使用无固定 extent 的稀疏桶，避免旧实现的二维巨型数组。 |
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
| `Geometry/GePointUtil.cs` | 极坐标、中点、二维距离和方向已有等价实现；其点排序和折线圆角没有形成比现有几何 API 更可靠的契约。 | 复用现有 `PointEx`、`GeometryEx` 和 `PointGridIndex`；其余不迁移。 |
| `Geometry/GeRectangleUtil.cs` | 轴向矩形相交已由 `Rect.IntersectsWith` 覆盖。 | 复用现有实现。 |
| `Geometry/GeTriangle.cs` | 坡度/坡向偏向地形业务，旧实现还存在退化三角形、法向方向和重心 Z 值问题。 | 不迁移。 |
| `Geometry/MathUtil.cs` | 多数函数已有 `System.Math` 等价项；采样、反函数和容差方法的边界契约不完整。 | 不迁移整类；真实需求单独设计。 |

### 6.2 ObjectARX 基础能力

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/AcadDocumentUtil.cs` | 文档枚举可直接使用 `DocumentManager`；`GetOpenedDocument` 忽略文件名并返回第一个文档。 | 不迁移。 |
| `ObjectARX/ArxOthers/DwgScaleUtil.cs` | 使用特定字典键保存“标注/信息比例”，属于产品持久化协议。 | 不迁移。 |
| `ObjectARX/ArxOthers/ExtentUtil.cs` | 宽高、中心、扩展、范围关系和碰撞扫描已被 `Rect`、`BoundingBox9`、`PointEx`、`Rect.XCollision` 覆盖。 | 复用现有实现，不迁移事务辅助。 |
| `ObjectARX/ArxOthers/GroupUtil.cs` | 已由 `DBDictionaryEx.AddGroup`/`GetGroups` 覆盖。 | 复用现有实现。 |
| `ObjectARX/ArxOthers/GsPreviewUtil.cs` | DWG 到位图预览直接使用 AutoCAD GraphicsSystem、设备和视图生命周期，没有可验证的共享宿主契约。 | 不迁移。 |
| `ObjectARX/ArxOthers/SectionUtil.cs` | 道路/断面面积算法带明显业务语义和 WinForms 提示。 | 不进入通用基础库。 |
| `ObjectARX/DictionaryUtil.cs` | XDictionary/Xrecord 能力已由 `DBDictionaryEx`、`XRecordDataList` 和对象扩展覆盖。 | 复用现有实现。 |
| `ObjectARX/DwgDatabaseUtil.cs` | 入库、空间枚举、Handle 转 ObjectId 已由 `DBTrans`、`SymbolTableRecordEx`、`ObjectIdEx`、`EntityEx` 覆盖。 | 复用现有实现。 |

### 6.3 DWG 表格

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/DwgTable/AcadDwgTable.cs` | 自绘表格、合并单元格和布局依赖缺失的 `ZFGK.DwgTableBase`/`ZFGK.Utility`，且与原生 `Table` 的职责边界不清。 | 不迁移。 |
| `ObjectARX/DwgTable/AcadSubDrawTable.cs` | 只是缺失表格基础模型的 CAD 适配。 | 不迁移。 |

### 6.4 实体、曲线和面域

| 来源文件 | 价值与现状 | 决定 |
| --- | --- | --- |
| `ObjectARX/Entity/ArcUtil.cs` | 反转圆弧可由厂商曲线 API 表达；旧实现直接改 Normal/角度，不能可靠处理非 XY 平面。 | 复用厂商 API，不迁移。 |
| `ObjectARX/Entity/BlockUtil.cs` | 块导入、插入和属性已有等价实现；导出 DWG 可由厂商 Wblock API 与现有数据库能力组合，旧实现存在失败后提交和固定版本问题。 | 复用现有实现，不迁移。 |
| `ObjectARX/Entity/CircleUtil.cs` | 仅构造并入库，现有实体添加 API 足够。 | 不迁移。 |
| `ObjectARX/Entity/CurveUtil.cs` | 子曲线、归一化位置和弦偏差具有通用价值；目标库已有拆分、偏移和交点相关能力，旧实现对返回对象的释放和异常处理不完整。 | 已吸收长度比例取点和两种中点弦偏差；其余复用现有实现，不再迁移。 |
| `ObjectARX/Entity/DimensionUtil.cs` | 旋转标注构造简单，未形成独特抽象。 | 不迁移。 |
| `ObjectARX/Entity/EntityUtil.cs` | 擦除、变换、颜色、包围盒和克隆已有等价实现。 | 复用现有实现。 |
| `ObjectARX/Entity/HatchUtil.cs` | 已有 `HatchEx`、`HatchConverter` 和边界创建能力；来源还依赖外部工具。 | 不迁移平行 API。 |
| `ObjectARX/Entity/LineUtil.cs` | 方位、点线距离和插值有通用价值；目标库已有面积/方向和厂商相交能力，旧实现的相交状态与高程插值存在语义问题。 | 已吸收比例插值和高程唯一点查询；其余复用现有实现，不再迁移。 |
| `ObjectARX/Entity/PolyfaceMeshUtil.cs` | 仅创建网格并入库，现有实体添加机制可组合完成。 | 不迁移。 |
| `ObjectARX/Entity/PolylineUtil.cs` | 顶点数据、子段长度和只读采样具有明确通用价值；连接、圆角、去零段和去共线点的旧实现会混合事务、UI、对象释放和原位修改。 | 已吸收顶点快照、子段长度、按距离采样和按弦偏差采样；修改操作不迁移。 |
| `ObjectARX/Entity/RegionUtil.cs` | `Explode` 后转折线并连接的旧实现泄漏临时对象，且没有内外环、方向、顺序和部分失败契约；当前也缺少 ZWCAD 2022 真实宿主证据。 | 不迁移；`RegionEx.ToCurves()` 的条件编译历史实现保持禁用。 |
| `ObjectARX/Entity/TextUtil.cs` | 添加文字、文字样式和去格式化已有实现；文字边界度量存在宿主差异。 | 复用现有实现，不迁移度量包装。 |
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
| `ObjectARX/LinetypeUtil.cs` | 线型表访问已有统一符号表入口；系统线型文件加载属于宿主路径策略。 | 复用现有实现，不迁移。 |
| `ObjectARX/TextStyleUtil.cs` | 文字样式写入已有实现；SHX 枚举混合宿主路径、程序集目录、外部工具和 UI 回退。 | 复用现有文字样式与 `Env` 能力，不迁移。 |
| `ObjectARX/Others/ConvertUtil.cs` | 点/向量/集合和 UCS/WCS 转换已由 `PointEx`、`VectorEx`、`GeometryEx`、LINQ 覆盖；外部向量类型不属于目标库。 | 复用现有实现。 |
| `ObjectARX/Others/FormatUtil.cs` | 点/向量文本序列化缺少文化区、版本和失败契约，也不属于 CAD 对象核心能力。 | 不迁移。 |
| `ObjectARX/Others/ListUtil.cs` | 唯一有效方法为交换元素，其余为大段历史注释。 | 不迁移。 |
| `ObjectARX/Others/ViewUtil.cs` | 已由 `EditorEx.Zoom*` 覆盖。 | 复用现有实现。 |
| `ObjectARX/SpacialIndex/DynamicPointSpatialIndex.cs` | 点去重、邻域、最近点和范围查询有明确通用价值。旧实现存在无效状态、密集二维数组、UI 弹窗和边界错误。 | 已吸收为 `PointGridIndex`：保留稳定索引、近似去重、范围和最近点能力；不保留固定 extent、公开网格行列和两阶段初始化。 |
| `ObjectARX/SpacialIndex/EntitySpatialIndex.cs` | 实体范围查询已由 `QuadTree` 承担；旧实现把事务和网格构建耦合。 | 复用现有 `QuadTree`，不迁移。 |
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

### Phase 0：基线和去重

- [x] 从最新 `main` 建立 `migration/zfgk-cad`。
- [x] 固定来源/目标 commit，完成 49 个 C# 文件的首轮清单。
- [x] 记录现有等价 API、外部依赖和已知风险。
- [x] 明确来源核对不作为本迁移实施门槛；Issue #105 与本计划解耦。
- [x] 创建实施总跟踪 Issue #110。
- [x] 为第一批候选写具体 API 草案，确认没有与实时 `main` 重复。

Phase 0 的清单和结构决策已经完成。后续每个代码批次必须回写本文的最终结论，不能只在 PR 对话中记录取舍。

### Phase 1：只读几何查询

最终范围：

- 曲线按归一化距离取点、弦高/逼近误差计算；
- 折线顶点、bulge、段长和确定性采样；
- 直线/线段方位与高程插值的明确结果契约；
- Extents/Rect 的关系和相交簇合并。

约束：不写数据库、不依赖活动文档、不原位修改输入、不弹 UI。先增加测试，再增加公共 API；每个返回的新 CAD 对象都写明释放责任。

批次 1 的 API 决定如下；它们都只返回值或托管快照，不创建需要调用方释放的 CAD 对象：

| 目标类型 | API | 吸收的价值与契约修正 |
| --- | --- | --- |
| `CurveEx` | `GetPointAtDistanceFraction` | 按总长闭区间 `[0, 1]` 取点；修正旧实现忽略输入比例、固定取 `0.5` 的问题，并拒绝无限长度域。 |
| `CurveEx` | `GetMidpointChordDeviation` | 明确按参数中点计算三维弦偏差，不把结果表述为区间最大误差。 |
| `CurveEx` | `GetMidpointChordDeviationByDistance` | 明确按沿曲线距离中点计算，避免把距离端点转参数后误用参数中点；`Ray` 以基点为距离原点并支持有限非负区间，`Xline` 因没有曲线起点而明确失败。 |
| `PolylineEx` | `GetVertexData` | 一次取得顶点、bulge、起宽和终宽的独立托管快照，不暴露 `ref` 集合。 |
| `PolylineEx` | `GetSegmentLength` | 对开放/闭合折线验证真实子段索引；直线返回线长，圆弧返回弧长。 |
| `PointEx` | `InterpolateTo` | 使用闭区间 `[0, 1]` 的三维线性插值，非法比例抛出明确异常。 |
| `PointEx` | `TryInterpolateAtElevation` | 只在非水平线段内存在唯一高程点时成功；修正旧实现把结果 Z 固定为 `0` 的错误，不引入隐式容差。 |

ObjectARX、ZRX 和 GRX 的曲线契约均把 `Ray` 定义为起始参数 `0` 且没有终止参数，把 `Xline` 定义为没有起止参数或起止点。因此三种查询不能共用“必须具有有限总长”这一前置条件：长度比例取点明确拒绝两者；参数中点弦偏差接受有限的 `Ray`/`Xline` 参数区间，其中 `Ray` 参数不得小于 `0`；距离中点弦偏差接受从基点开始的有限非负 `Ray` 距离区间，但拒绝没有距离原点的 `Xline`。这一区分以 SDK 曲线契约为准。

批次 1 提供宿主内确定性验收命令 `Test_GeometryQuery`，覆盖直线、半圆、`Ray`/`Xline` 边界、开放/闭合折线和高程插值边界。AC_2019、AC_2025、ZW_2022、ZW_2025、GC_2022、GC_2023、GC_2026 的 Release 库和测试程序集均已通过直接构建，模块、兼容性和 TypeDef 顺序守卫通过；这只是 Build-only 证据，命令尚未在 CAD 中执行，真实宿主状态为 `Not run`。

最终批次只补充折线确定性采样，不混入折线修改或数据库行为：

| 目标类型 | API | 最终契约 |
| --- | --- | --- |
| `PolylineEx` | `GetSamplePointsByDistance` | 每个原始子段按沿折线的最大间距独立等距细分，保留所有原始顶点；闭合折线在结果末尾重复首顶点。 |
| `PolylineEx` | `GetSamplePointsByChordDeviation` | 直线段只保留端点；圆弧段按弓高公式细分，使每个采样子弧的弦偏差不超过给定有限正数；同样保留原始顶点和闭合点。 |

两个方法均返回独立的 `Point3d` 快照，不修改输入、不访问数据库、不创建需要释放的 CAD 对象。`Test_GeometryQuery` 同步覆盖直线/圆弧混合、开放/闭合、退化折线、非零 `Normal`/`Elevation`、非法阈值和不可表示的细分数量。点在线段左右关系由现有 `GeometryEx.GetArea`/`IsClockWise` 表达，范围关系和碰撞扫描由 `Rect`/`Rect.XCollision` 表达，不再新增平行 API。

最终批次的 AC_2019、AC_2025、ZW_2022、ZW_2025、GC_2022、GC_2023、GC_2026 Release 库和测试程序集均已通过直接构建。模块守卫保持 `142 / 42 / 17`，七目标兼容性基线仅增加 `PolylineEx` 的两个公开方法及对应 XML 文档，四目标 TypeDef 顺序未变化；HostAcceptance runner 自测通过。以上仍是 Build-only 和基础设施证据，`Test_GeometryQuery` 未在 CAD 中执行，真实宿主状态为 `Not run`。

### Phase 2：点网格索引

以来源的需求集合为输入，重新设计稀疏 `PointGridIndex`：

- 构造后始终有效，不保留需要先调用 `Create()` 的半初始化状态；
- 支持近似点去重、范围查询和受最大距离约束的最近点查询；
- XY 容差、Z 容差、格长和边界闭合规则显式；
- 不依赖 WinForms，不在失败时弹窗；
- 与 `QuadTree` 做职责对照和基准，不能建立两套等价实体索引。

Phase 2 的目标 API 与边界如下：

| API/成员 | 契约 |
| --- | --- |
| 构造函数、`CellSize` | 构造即有效；网格边长必须是有限正数，不设置固定 extent。 |
| `Add`、索引器、`Points`、`Count` | 无条件添加并返回稳定整数索引；点视图只读，`Clear` 前索引不变化。 |
| `TryAdd`、`TryFind` | XY 使用欧氏距离闭区间，Z 使用独立闭区间容差；多个候选按三维距离、再按添加顺序决定。 |
| `QueryIndices` | 查询 XY 矩形闭区间，结果按添加顺序稳定返回，不隐含 Z 过滤。 |
| `TryGetNearest` | 在有限最大三维距离内查询，并额外应用 Z 差闭区间过滤；失败结果为索引 -1、距离正无穷。 |
| `Clear` | 同时释放点和稀疏桶引用；之后索引重新从零开始。 |

旧实现的 `Create`、`GetBlockOfPoint`、`GetNeighborPoints`、`GetExtent` 和 `IsIn` 不成为公共 API：无界稀疏索引不需要固定范围，网格桶只是实现细节。`Test_PointGridIndex` 覆盖跨桶去重、Z 分层、负坐标、闭区间范围、最近点并列、退化参数和清空重用；当前仅参加测试程序集编译，CAD 宿主状态为 `Not run`。

PR #112 的评审收口规定：有限查询范围因坐标量级与 `CellSize` 组合而不能表示为 `long` 网格坐标时，索引退化为扫描全部已存点并继续执行精确过滤，不把实现范围限制暴露成查询异常；高频查询复用实例内候选缓冲区，仍遵守类型既有的非线程安全契约。查询结果是与索引内部状态分离的独立集合，因此不为阻止调用方修改其副本而增加只读包装分配。

Phase 2 的七目标公共契约基线均只新增 `PointGridIndex` 的 16 条记录。以 `migration/zfgk-cad@587e77f` 为基线重新构建并比较 AC_2019、AC_2025、ZW_2022、ZW_2025 实际程序集后，每个目标都只新增一个 TypeDef，所有既有 TypeDef 的相对顺序保持不变；由于 Roslyn 先发射 `Fs.Fox.Cad` 根类型再发射 `Fs.Fox.Cad.Assoc`，新类型位于现有根类型末尾并使其后的 token 顺延一位，这是新增公共根类型的已审查结果，不是既有类型重排。

AC_2019、AC_2025、ZW_2022、ZW_2025、GC_2022、GC_2023、GC_2026 的 Release 库和测试程序集均已通过直接构建，模块期望更新为 `142` 个编译项、`Cad.Geometry = 17`；真实 CAD 宿主状态仍为 `Not run`。

### Phase 3：曲线/折线修改与 Region（已关闭，不迁移）

- 目标库已有曲线拆分等基础能力，不从来源复制连接、圆角、去零段或去共线点实现。
- 来源的原位修改不能完整保留宽度、bulge、闭合状态、`Normal`、`Elevation` 和实体属性，最终不迁移。
- Region 边界提取缺少内外环、方向、顺序、部分失败、对象所有权和 ZWCAD 2022 宿主证据，最终不迁移。
- Issue #103/#107 中条件编译保留的 `RegionEx.ToCurves()` 继续只作为历史实现和风险清单，不启用、不扩展。

### Phase 4：数据库工作流（已关闭，不迁移）

- 块导入、插入、属性和数据库保存复用现有 `GetBlockFrom`、`InsertBlock`、`ChangeBlockAttribute`、`DatabaseEx` 及厂商 Wblock API。
- 来源的实体导出 DWG 实现固定旧版本、隐式依赖活动文档，并在失败路径提交事务，不作为公共契约迁移。
- SHX 搜索混合宿主支持路径、程序集目录和 UI 回退；本次不建立新的文件发现 API。

### Phase 5：宿主专属能力（已关闭，不迁移）

DWG 图像预览、复杂自绘表格、结果定位窗体和其他 WinForms/GraphicsSystem 能力均不进入共享程序集。它们缺少可重复依赖、共享宿主契约或通用基础库边界；最终清单不再保留候选状态，也不使用条件编译复制来源实现。

## 9. 分支和 PR 组织

`migration/zfgk-cad` 是本次迁移的长期集成分支。已经完成的代码批次均从该分支建立短分支，并将 PR base 指向该分支：

- `zfgk/geometry-query`
- `zfgk/point-grid`
- `zfgk/review-fixes`
- `zfgk/curve-domain`
- `zfgk/final-sampling`

最终采样 PR 合入后不再建立新的 `Fs.Zfgk.CAD` 迁移批次。`migration/zfgk-cad` 继续与 `main` 隔离；是否以及何时进入 `main` 属于独立集成决策，必须重新同步最新 `main`、审查完整差异并获得维护者明确批准，不能用各子 PR 已通过替代最终集成验证。

## 10. 每批验收

### 10.1 静态和构建

- `git diff --check`
- `pwsh -File Build/Verify-CADSharedModuleMap.ps1`
- `pwsh -File tools/verification/Test-CADSharedTypeDefSequence.ps1`
- `pwsh -File Build/Verify-CADSharedCompatibility.ps1`
- AC_2019、AC_2025、ZW_2022、ZW_2025、GC_2022、GC_2023、GC_2026 Release 构建
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

当前所有迁移批次均未运行 CAD，宿主状态统一为 `Not run`。七目标构建和静态守卫不代表真实宿主通过。

## 11. 完成定义

本迁移不是以“来源文件全部处理”或“公共方法数量接近 451”为完成标准。满足以下条件才可收口：

1. 每个来源文件都已有 `复用现有`、`已吸收` 或 `不迁移` 的最终结论，不再保留候选；
2. 所有迁移能力符合当前目录、命名、事务和对象所有权契约；
3. 没有引入来源二进制、AutoCAD-only using 泄漏或第二套平行 API；
4. 自动构建/守卫与要求的真实宿主场景分别记录，未执行项不被包装为通过；
5. 最终有效的公共行为写入 XML 注释，实施结果和未运行的宿主项写入本文；
6. 最终采样 PR 合入 `migration/zfgk-cad` 后关闭 Issue #110，并将本文转为 `historical`。

进入 `main` 不属于上述迁移完成定义。长期分支的完整集成审查和维护者批准仍是未来独立门槛。

## 12. 收口后状态

- 不再从 `Fs.Zfgk.CAD` 开展后续迁移，也不复用已删除的短期分支。
- 新的通用 CAD 需求从 `Fs.Fox.CAD` 实时结构、厂商 SDK 和独立 Issue 出发设计，不以来源方法为规格。
- `migration/zfgk-cad` 保留为尚未进入 `main` 的长期集成结果；在明确批准前不创建以 `main` 为 base 的迁移 PR。
- 真实 CAD 宿主状态保持 `Not run`；未来若决定集成到 `main`，再按当时差异确定宿主验收范围。
