# src 源码检索总入口

> 状态：current；受众：maintainer<br>
> 稳定 ID：`reference.source-navigation`<br>
> 范围：仓库 `src/`；既有 API 核对起点：`main` @ `ea400b3409e154357c97a3013ff0f9b11f6515ac`；已同步本次 Zfgk Beta 扩展<br>
> 上级入口：[文档索引](../README.md)；整体设计：[架构说明](../关于IFoxCAD的架构说明.md)

本组文档回答“功能放在哪个文件、从哪个类型和方法进入、下一步追到哪里”。它不是第二份完整 API 参考，也不把设计提案或未编译代码当作已实现功能。所有源码链接指向仓库内文件，便于在 GitHub、编辑器和本地检索中使用。

## 如何使用

1. 已知现象或任务：先看本页的[按问题定位](#tasks)。
2. 已知类型或方法：在编辑器中搜索对应标识符，再查所属分册。一个文件可能包含多个类型，类型名不一定等于文件名。
3. 找不到方法：先核对[宿主和编译边界](#projects)，尤其是 `#if`、`GlobalUsings.cs` 和 `ToDo`。
4. 修改前沿调用链核对事务、数据库、对象所有权、坐标系和宿主分支；本组文档不替代实现及 XML 注释。

既有 API 的导航来自静态源码、项目与文档核对。新增 Beta 的构建、生产核心回归及复核结果见[迁移验证记录](../zfgk-migration-inventory.md#验证记录)。真实 CAD 宿主验收为 `Not run`；命令测试文件存在或编译通过不代表已运行。

<a id="modules"></a>

## 源码范围与模块地图

当前 `src` 有 198 个纳入源码管理的文件，其中 183 个是 C# 文件：159 个由共享项目显式列入编译、3 个是版本项目选用的宿主全局 using、21 个属于未编译的 `ToDo`。相对最初导航基线，增加了 `src/README.md` 和 16 个 Beta 源码文件。忽略本地 `obj`、`bin` 和其他可重建输出。

模块数量以 [`CADShared.projitems`](../../src/CADShared/CADShared.projitems) 的 `FsFoxModule` 元数据为准：

| 模块 / 源码位置 | 共享编译项 | 主要内容 | 详细阅读 |
| --- | ---: | --- | --- |
| `Foundation` | 11 | 集合、循环链表、状态和框架兼容辅助 | [基础与 Windows](foundation-platform.md#foundation) |
| `Platform/Windows` | 15 | Win32 声明、消息与键码、PE 解析 | [基础与 Windows](foundation-platform.md#windows) |
| `Cad/Application` | 11 | 活动文档、环境、锁、系统变量、Idle、Beta 离屏预览 | [应用上下文](application-editor.md#application) |
| `Cad/Database` | 54 | 事务、符号表、对象、实体、数据、参照及 Beta 实体能力 | [数据库](database.md)、[实体](entities.md) |
| `Cad/Editor` | 23 | 输入、选择过滤、Jig、显示和命令 | [Editor](application-editor.md#editor) |
| `Cad/Geometry` | 20 | 坐标、点/曲线几何、四叉树、Beta 排序与三角形 | [几何与空间索引](geometry.md) |
| `Cad/Runtime` | 14 | 初始化发现、注册、Lisp 列表 | [运行时](runtime-ui.md#runtime) |
| `Cad/UI` | 8 | 窗口、对话框、状态栏和首选项 | [UI](runtime-ui.md#ui) |
| `Cad/Interop` | 3 | CAD native 导出解析、实体 DXF 访问、天正接口 | [宿主互操作](runtime-ui.md#interop) |
| `ExtensionMethod/Geometry/ToDo` | 0，保留 21 个文件 | 待整理几何扩展，不属于当前编译实现 | [ToDo 文件定位](geometry.md#todo) |

`CADShared` 是共享源码入口，不单独生成跨宿主通用 DLL。目录是逻辑所有权，公共类型仍主要位于 `Fs.Fox.Cad` 和 `Fs.Fox.Basal`；不能从文件夹名推断存在 `Fs.Fox.Cad.Database` 等公共命名空间。关联功能另有 `Fs.Fox.Cad.Assoc`。

主要阅读链如下：

```text
具体版本 .csproj
  -> src/Directory.Build.props：共同配置
  -> 对应 IFoxCAD.*/GlobalUsings.cs：宿主类型与别名
  -> CADShared/CADShared.projitems：共享文件、模块元数据与顺序
  -> Cad/Database/Transactions/DBTrans.cs：数据库与事务入口
  -> SymbolTable / SymbolTableRecordEx / ObjectIdEx：资源访问和落库
  -> 具体 Entity、Editor、Geometry、Application、Runtime 或 UI 实现
```

这只是高频调用路线，不是单向依赖保证。例如 `GeometryEx` 有活动 Editor 和绘制相关方法，`Rect` 能生成 CAD 实体，`CollectionEx` 还处理关键字集合；不能按目录直接拆出纯数学或无宿主程序集。

<a id="projects"></a>

## 项目、宿主别名与条件编译

版本目录主要包含 `.csproj`，共享行为应到 `CADShared` 查。下面的 TFM 和符号来自实际项目文件，只说明源码选择条件，不是实际 CAD 产品支持承诺：

| 项目文件 | 目标框架 | 主要符号 | 宿主 using |
| --- | --- | --- | --- |
| [AutoCad2019](../../src/Fs.Fox.AutoCad2019/Fs.Fox.AutoCad2019.csproj) | `net48` | `ACAD;AC_2019;AC_NET48` | [AutoCAD](../../src/IFoxCAD.AutoCad/GlobalUsings.cs) |
| [AutoCad2021](../../src/Fs.Fox.AutoCad2021/Fs.Fox.AutoCad2021.csproj) | `net48` | `ACAD;AC_2021;AC_NET48` | 同上；使用工作区外部 SDK props。 |
| [AutoCad2025](../../src/Fs.Fox.AutoCad2025/Fs.Fox.AutoCad2025.csproj) | `net8.0-windows7.0` | `ACAD;AC_2025` | 同上。 |
| [AutoCad2027](../../src/Fs.Fox.AutoCad2027/Fs.Fox.AutoCad2027.csproj) | `net8.0-windows7.0` | `ACAD;AC_2027` | 同上；外部 SDK 配置及[预备项目边界](../AC_2027-net8-compatibility-decision.md)。 |
| [ZwCad2022](../../src/Fs.Fox.ZwCad2022/Fs.Fox.ZwCad2022.csproj) | `net48` | `ZWCAD;ZW_2022` | [ZWCAD](../../src/IFoxCAD.ZwCad/GlobalUsings.cs) |
| [ZwCad2025](../../src/Fs.Fox.ZwCad2025/Fs.Fox.ZwCad2025.csproj) | `net48` | `ZWCAD;ZW_2025` | 同上。 |
| [Gcad2022](../../src/Fs.Fox.Gcad2022/Fs.Fox.Gcad2022.csproj) | `net48` | `GCAD;GC_2022` | [GStarCAD](../../src/IFoxCAD.Gcad/GlobalUsings.cs) |
| [Gcad2023](../../src/Fs.Fox.Gcad2023/Fs.Fox.Gcad2023.csproj) | `net48` | `GCAD;GC_2023` | 同上。 |
| [Gcad2026](../../src/Fs.Fox.Gcad2026/Fs.Fox.Gcad2026.csproj) | `net8.0-windows7.0` | `GCAD;GC_2026` | 同上。 |

三份 `GlobalUsings.cs` 分别选择 Autodesk、ZwSoft，以及 `GrxCAD` / `Gssoft.Gscad` 类型，并统一 `CadApp`、`CadCoreApp`、`CadDbServices`、`CadGI`、`CadGS`、`CadRuntime`、`CadException` 等源码别名。相同 C# 标识符最终对应不同厂商类型，不能跨宿主交换对象或直接复用宿主二进制。

另外四个配置入口也属于 `src` 的阅读范围：

| 文件 | 查找内容 |
| --- | --- |
| [Directory.Build.props](../../src/Directory.Build.props) | `x64`、语言版本、nullable、WPF/WinForms、包与文档输出等共同配置。 |
| [CADShared.shproj](../../src/CADShared/CADShared.shproj) | Visual Studio 共享项目描述；实际源文件清单在 `.projitems`。 |
| [AutoCAD.props](../../src/IFoxCAD.AutoCad/AutoCAD.props) | 随部分 AutoCAD 包携带的旧配置辅助；项目将它列为 `None`，不能当作所有正式项目自动导入的配置。 |
| [copyToDigBuild.ps1](../../src/IFoxCAD.AutoCad/copyToDigBuild.ps1) | 依赖 `DiGBuildDir` 的复制辅助脚本；与类库运行时 API 无关，本轮未执行。 |

完整编译/发布矩阵继续以[构建说明](../../编译说明.md)和实时工作流为准，本页不维护第二份发布规格。

### 文件列入编译不代表每个成员均启用

| 检索结果 | 当前需要核对的条件 |
| --- | --- |
| `RegionEx.ToCurves` | 在 `FSFOX_ENABLE_REGION_TO_CURVES` 内，当前正式项目未定义该符号。 |
| `JigExTransient`、`CurveEx.BreakCurve2dByQuadTree` | 在 `!GC_2022 && !GC_2023` 内；不能按文件存在认定早期 GStarCAD 有这些成员。 |
| `AssocUtils` | 主要实现受 `ACAD` 保护，命名空间为 `Fs.Fox.Cad.Assoc`。 |
| `AssocPersSubentityIdPEEx` | 排除 `GC_2022/GC_2023`，其 PE 访问实现还排除 `ZW_2022`。 |
| `HatchDialog`、`CheckFactory` | 前者只在 `ACAD` 内；后者只在 `DEBUG` 内。 |
| `CallerArgumentExpressionAttribute` | 供旧框架分支补充编译期能力，不是常规 CAD API。 |
| `#if DEBUG`、`#if Debug`、`#if RELEASE` | 大小写敏感；项目还包含 `$(Configuration)` 符号。看到代码时需要核对精确条件。 |

<a id="tasks"></a>

## 按问题定位

| 任务 / 症状 / 常见搜索词 | 第一个入口 | 下一步追踪 |
| --- | --- | --- |
| 不知道功能在哪个项目，AutoCAD 与 ZWCAD 类型冲突 | 本页[项目边界](#projects) | 目标 `.csproj` → `GlobalUsings.cs` → 共享源文件的 `#if`。 |
| 事务默认提交、异常后仍保留修改、事务栈不匹配 | [DBTrans](database.md#transactions) | 构造 → `_commit` → `Commit/Abort` → `Dispose(bool)`。 |
| `NoTopTransaction`、未创建事务、ObjectId 打不开 | [ObjectIdEx / TransactionEx](database.md#objects) | `DBTrans.GetTopTransaction` 和目标 `id.Database`。 |
| 后台开图、文件保存、文字偏移、WorkingDatabase 错乱 | [数据库和文件](database.md#files) | `DBTrans(fileName)`、`Task`、`SwitchDatabase`、`DatabaseEx.SaveFile`。 |
| 新图元创建后没有出现在图中 | [实体落库链路](entities.md#ownership) | 内存创建 → 指定 `BlockTableRecord.AddEntity` → 事务完成。 |
| 图层/文字样式已有记录却未更新 | [SymbolTable](database.md#symbol-tables) | `Add` 已存在时直接返回；对照 `AddOrChange`、`Change`。 |
| 写对象提示只读、锁定图层错误 | [写入作用域](database.md#objects) | `ForWrite`、文档锁、`LayerUnlockScope`，分别核对。 |
| 普通块、动态属性、属性文字、可见性状态 | [块参照](entities.md#blocks) | `InsertBlock` → `BlockReferenceEx` → `BlockVisibilityInfo`。 |
| 嵌套块位置、变换矩阵或包围框不对 | [块与包围框](entities.md#bounds) | `NestedForEach`、`GetBoundingBoxEx`、内部 `EntityBoundingInfo`。 |
| 曲线长度比例点、弦偏差、打断与交点 | [CurveEx](entities.md#curves) | 几何转换转到 `Curve2dEx/Curve3dEx`。 |
| 多段线采样、凸度、顶点宽度、闭合末点 | [PolylineEx](entities.md#polylines) | `GetVertexData`、两种采样和 `BulgeVertexWidth`。 |
| 文字镜像、DBText 对齐、MText 去格式 | [文字](entities.md#text) | 工厂返回内存对象，仍需落库。 |
| Hatch 边界提取、重建、关联、渐变 | [填充](entities.md#hatch) | 区分 `HatchEx`、`HatchConverter`、`HatchInfo`。 |
| XData 丢失、DXF 组码、Xrecord、Group | [字典和结果数据](database.md#result-data) | `DBObjectEx` → 数据列表 → `DBDictionaryEx`。 |
| 外参绑定、拆离、重载、相对路径 | [XrefFactory / XrefPath](database.md#xrefs) | `DBTrans.Task` 和 `Database.ResolveXrefs` 相关路径。 |
| 系统变量临时值未恢复、切图后恢复错误 | [系统变量](application-editor.md#system-variables) | `SystemVariableScope.Dispose` 的文档身份校验。 |
| 无命令才执行、Idle 队列、延迟修改 | [Idle 调度](application-editor.md#idle) | `IdleAction` 与文件名不同的 `IdleNoCmdAction`。 |
| 选择条件、按点选、选择集类型过滤 | [选择与过滤](application-editor.md#selection) | `EditorEx.SSGet`、`OpFilter`、`SelectionSetEx`。 |
| 单字母免回车、关键字异常、Esc 路径 | [输入](application-editor.md#input) | `SingleKeyWordHook`、`PromptOptionsEx`、`KeywordException`。 |
| Jig 重绘、瞬态残留、预览对象释放 | [Jig](application-editor.md#jig) | `JigEx` 队列与 `JigExTransient.Clear/Dispose`。 |
| UCS/WCS/DCS、视口矩阵、缩放 | [坐标](geometry.md#coordinates) | `GeometryEx.Trans` → `EditorEx.GetMatrix`。 |
| 点在多边形内、边界点、凸包、最小包围圆 | [点与区域](geometry.md#points) | `PointOnRegionType`，不要套用旧 `ToDo` 的契约。 |
| 快速范围候选、四叉树相交/包含、邻近查询 | [空间索引](geometry.md#quadtree) | `QuadTree` → `QuadTreeNode` → `Rect`。 |
| 搜到 `FilletAll`、三角形或重心方法但无法调用 | [未编译 ToDo](geometry.md#todo) | 核对 `.projitems`，不要直接宣传为当前 API。 |
| 初始化没有执行、执行次序、退出未清理 | [初始化](runtime-ui.md#initialization) | `AutoRegAssem` 构造 → `AutoReflection` → 终止实际入口。 |
| 程序集注册、自动加载、重复命令检查 | [注册](runtime-ui.md#registration) | `AutoReg`、`AssemInfo`、`CheckFactory`。 |
| Lisp 列表/执行、命令派发 | [Lisp 与命令](runtime-ui.md#lisp) | `LispList` → `EditorEx.RunLisp` / `PostCmd` → native。 |
| WPF Esc、模态状态、Palette 尺寸、DPI | [窗口](runtime-ui.md#ui) | `WindowEx`；`IsModel` 是当前 API 的原拼写。 |
| 进度条某宿主不显示、状态栏 Pane | [状态栏](runtime-ui.md#ui) | `ProgressMeterUtils` 的厂商分支和 `PaneEx`。 |
| `DllNotFound`、native 导出或委托解析 | [CAD interop](runtime-ui.md#interop) | `AcadPeInfo` → `PeInfo` → `WindowsAPI.GetProcAddress`。 |
| Win32 消息、键码、焦点、窗口句柄 | [Windows](foundation-platform.md#windows) | `PInvokeUser32`、`WindowsAPI` 和独立枚举文件。 |
| 循环链表删除/接链、循环停止、枚举说明 | [Foundation](foundation-platform.md#foundation) | `LoopList`、`LoopState`、`EnumEx`。 |

<a id="names"></a>

## 容易搜错的名称

| 文件名 / 旧直觉 | 实际搜索标识 |
| --- | --- |
| `Foundation/DebugHelper.cs` | 公共类型是 `DebugEx`。 |
| `Application/Scheduling/IdleNoCommandAction.cs` | 公共类型是 `IdleNoCmdAction`。 |
| `Platform/Windows/PortableExecutable/ProgramPE.cs` | 主要入口类型是 `PeInfo`。 |
| `QuadTreeEvn`、`FindNeibor` | 保留现有公开拼写；不要只搜索 `Env`、`Environment` 或 `FindNeighbor`。 |
| `GetCurrentMouthPoint`、`WindowEx.IsModel` | 保留现有源码名称；前者用于当前鼠标点，后者判断模态。 |
| `HatchConverter.CreateBoundarys...`、`HatchEx.ResetBoundarys` | 多处使用 `Boundarys`，不是统一的 `Boundaries`。 |
| `Cad/Geometry` 与 `Cad/Database/Entities/Curves` | 前者偏几何对象和算法；后者处理数据库 `Curve`、`Polyline` 实体。 |

## 纯静态检索命令

以下命令在仓库根目录运行，只读取源码或项目清单，不触发构建：

```powershell
# 只看受版本控制的源码，排除本地 obj/bin 干扰
git ls-files -- 'src/*.cs' 'src/**/*.cs'

# 按主类型或关键方法查位置
rg -n 'class DBTrans|GetTopTransaction|Dispose\(bool' src/CADShared/Cad/Database
rg -n 'GetSamplePointsBy|GetPointAtDistanceFraction' src/CADShared/Cad/Database/Entities
rg -n 'ClearAndDisposeDrawables|PreTranslateMessage|SSGet' src/CADShared/Cad/Editor

# 确认源码是否列入共享编译，以及当前宿主开关
rg -n 'FsFoxModule|RegionEx.cs|ToDo' src/CADShared/CADShared.projitems
rg -n '^#if|^#elif|FSFOX_ENABLE_' src/CADShared/Cad src/IFoxCAD.AutoCad src/IFoxCAD.ZwCad src/IFoxCAD.Gcad
```

## 文档维护范围

本组分册以人工整理的职责与定位说明为主，不维护逐成员完整签名、生成 API 中间文件或搜索索引。新增/删除/移动源码时，同步更新对应文件行和本页模块统计；新增公共行为时先更新 XML 注释，再调整定位说明。

示例与宿主场景只链接到已有 `tests/TestShared` 文件，不在 Markdown 中另写一套完整实现；测试和宿主验收仍分别遵守[仓库规则](../../AGENTS.md)。
