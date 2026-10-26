# 应用上下文、输入与 Editor 定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-application-editor`<br>
> 基线：[源码检索总入口](README.md)；覆盖 `Cad/Application` 11 个和 `Cad/Editor` 23 个共享编译文件。

按需要跳转：[应用上下文](#application)、[系统变量](#system-variables)、[Idle](#idle)、[Editor](#editor)、[选择过滤](#selection)、[输入](#input)、[Jig](#jig)、[显示与命令](#display-command)。

Beta 外部 DWG 预览另见 [DwgPreviewRendererBeta.cs](../../src/CADShared/Cad/Application/Graphics/DwgPreviewRendererBeta.cs)及[使用说明](../guides/dwg-preview-beta.md)；其路径、尺寸和相机拟合核心为 [DwgPreviewCoreBeta.cs](../../src/CADShared/Cad/Application/Graphics/DwgPreviewCoreBeta.cs)。公开预览入口只在 AutoCAD 编译目标提供，不修改当前文档视图；生产核心回归与真实渲染分别记录。

<a id="application"></a>

## 当前环境与文档锁

| 文件 / 类型 | 主要入口 | 功能及边界 |
| --- | --- | --- |
| [Env.cs](../../src/CADShared/Cad/Application/Context/Env.cs) | `Env.Database`、`Document`、`Editor`、`GsManager` | 动态读取当前工作数据库、活动文档、Editor 和 GraphicsManager，不是构造时固定的上下文快照。无文档时不要据此推断每个属性都可用。 |
| 同上 | `GetVar/SetVar`、`GetEnv/SetEnv`、配置 section 读取 | 系统变量、环境变量与用户 profile 读取/修改；部分配置访问受厂商 `#if` 限制。 |
| 同上 | `AppendSupportPath/RemoveSupportPath`、`AppendTrustedPath/RemoveTrustedPath` | 修改支持路径或 Trusted Paths，有持久配置影响；本轮只静态阅读，没有调用。 |
| 同上 | `DimblkType`、`OSModeType`、`GetDimblkId`、`GetDefaultDwgVersion`、`GetAcadYear` | 标注箭头、捕捉模式及宿主/文件版本辅助，具体映射要查看版本分支。 |
| 同上 | `EntGet`、`EntMod`、`EntUpd` | 按 ObjectId / typed value 访问实体 DXF 数据，继续到 `PInvokeCad`；动态块可见性也会走这条路径。 |
| [DocumentLockManager.cs](../../src/CADShared/Cad/Application/Documents/DocumentLockManager.cs) | `DocumentLockManager`、`Dispose` | 文档没有锁时取得锁，释放时只释放自己取得的锁；不创建数据库事务。构造为 internal。 |
| [DocumentLockManagerExtension.cs](../../src/CADShared/Cad/Application/Documents/DocumentLockManagerExtension.cs) | `Document.SecurelyLock` | 对外创建上述锁作用域的扩展入口。 |

文档锁、对象写模式和数据库事务是三个层面。模型外窗口写图、多文档或 Session 命令出现错误时，分别核对文档、数据库、锁和事务，而不是只追加一个 `ForWrite`。

<a id="system-variables"></a>

## 系统变量和修改状态

| 文件 | 类型 / 入口 | 定位细节 |
| --- | --- | --- |
| [SystemVariableManager.cs](../../src/CADShared/Cad/Application/SystemVariables/SystemVariableManager.cs) | 同名管理器的属性，如 `CLayer`、`CEColor`、`Aperture` | 通过 `Get/SetSystemVariable` 把常用变量包装为属性；同时存在实例和静态属性，不是完整变量字典，也不自动恢复旧值。 |
| [SystemVariableScope.cs](../../src/CADShared/Cad/Application/SystemVariables/SystemVariableScope.cs) | 构造、`VariableName`、`OriginalValue`、`Dispose` | 捕获变量原值与活动文档身份，设置临时值，释放时恢复；保留原始运行时类型，不统一转换成 int。 |
| [DBmod.cs](../../src/CADShared/Cad/Application/SystemVariables/DBmod.cs) | `DBmod` | 图纸修改状态位枚举，和记录业务修改的自定义状态不是一回事。 |
| [DBmodEx.cs](../../src/CADShared/Cad/Application/SystemVariables/DBmodEx.cs) | `DBmodTask`、`DatabaseNoModifies`、`AcdbSetDbmod` | 读修改状态并通过 native 委托设置；任务路径在 action 正常返回后才检查是否恢复“未修改”，不是 `try/finally` 恢复器。 |

`SystemVariableScope.Dispose` 的活动文档检查是明确行为：切到别的文档会拒绝恢复并抛 `InvalidOperationException`，也不把作用域标记为已释放；恢复原上下文后可以重试。它没有把旧值写到另一张图的 fallback。

`DBmodEx.DatabaseNoModifies` 带静态一次性标记，是特殊初始化辅助，不能把它当作每次业务操作后清除“待保存”状态的通用流程。

已有入口：[TestEnv.cs](../../tests/TestShared/TestEnv.cs)。本轮未执行变量修改或宿主验收。

<a id="idle"></a>

## Idle 调度

| 文件 / 真实类型名 | 入口 | 行为 |
| --- | --- | --- |
| [IdleAction.cs](../../src/CADShared/Cad/Application/Scheduling/IdleAction.cs)：`IdleAction` | `Add`、`Count` | 静态队列，第一次添加时挂接 `CadCoreApp.Idle`；每次 Idle 出队一个任务，队列为空后解绑。执行任务的异常被捕获并忽略。 |
| [IdleNoCommandAction.cs](../../src/CADShared/Cad/Application/Scheduling/IdleNoCommandAction.cs)：`IdleNoCmdAction` | `Add`、`Count` | 额外检查 `CMDACTIVE`，有命令时不出队。异常捕获处在 `#if RELEASE` 内，应核对实际符号，不能只依据配置名称推断。 |

这些队列不自动固定入队时的 Document、Database 或事务上下文。延迟到下一次空闲时执行，仍需重新确认目标；也不要把“CAD 空闲”解释为可在任意后台线程使用所有托管 API。

<a id="editor"></a>

## EditorEx 大文件如何分区阅读

[EditorEx.cs](../../src/CADShared/Cad/Editor/EditorEx.cs) 超过 1,000 行，包含多个职责。按方法组查比按文件顺序读更快：

| 方法组 | 入口 | 功能 |
| --- | --- | --- |
| 选择 | `SelectAtPoint`、`SelectByLineWeight`、`SSGet`、`SsgetAddKeys` | 点选、线宽、选择选项、过滤及关键字回调。 |
| 消息 | `StreamMessage`、`Message`、`InfoMessageBox`、`WriteMessage`、`HasEditor` | 命令行/消息框输出和可用上下文判断。 |
| 临时矢量 | `DrawVectors`、`DrawCircles`、`DrawCircle`、`DrawLineVectors`、`DrawEndToEndVectors` | Editor 显示，不会据此创建永久数据库实体。 |
| 矩阵 | `GetMatrixFromUcsToWcs`、`GetMatrixFromWcsToUcs`、模型/图纸 DCS 矩阵、`GetMatrix` | 对应活动坐标和视口状态，详见[坐标分册](geometry.md#coordinates)。 |
| 视图 | `ZoomWindow`、`Zoom`、`ZoomExtents`、`ZoomObject`、`GetValidExtents3d` | 视图范围与缩放；原始/扩展范围及二维/三维重载分别看。 |
| 输入便利重载 | `GetPoint`、`GetDouble`、`GetInteger`、`GetString` | 设置提示、基点或默认值，保留宿主 Prompt 结果，不是统一的业务取消协议。 |
| Lisp/导出 | `RunLisp`、`RunLispFlag`、`ComExportWMF` | 运行路径选择及 COM 导出，涉及 native、COM 和文件副作用。 |
| Jig 辅助 | `PrepareForJig`、`GetCurrentMouthPoint` | 准备绘制、读取当前鼠标位置；保留 `Mouth` 的公开拼写。 |

已有阅读入口：[TestEditor.cs](../../tests/TestShared/TestEditor.cs)。

<a id="selection"></a>

## 选择集和 DXF 条件组合

`EditorEx.SSGet` 把部分 mode 片段转换为 `PromptSelectionOptions`，例如单选、重复、当前空间、锁定层和子对象选项，再挂接关键字回调并调用宿主 `GetSelection`。它不是完整 AutoLISP `ssget` 语法解析器。

| 文件 | 类型 / 入口 | 职责 |
| --- | --- | --- |
| [SelectionSetEx.cs](../../src/CADShared/Cad/Editor/Selection/SelectionSetEx.cs) | `GetObjectIds<T>`、`GetObjectIdGroup`、`GetEntities<T>`、`ForEach` | 对选择集按类型取 ID、分组、打开实体和控制循环。实体查询依赖有效数据库事务；返回可枚举值时关注枚举时间。 |
| [OpFilter.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpFilter.cs) | `OpFilter`、嵌套 `Op`、`Build`、`ToArray`、隐式 `SelectionFilter` 转换 | 条件树的公共入口及运算符构造 DSL；最终输出 typed value 序列。 |
| [OpEqual.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpEqual.cs) | `OpEqual`、`SetValue` | 一个 DXF 组码和值的相等条件。 |
| [OpComp.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpComp.cs) | `OpComp` | 比较操作字符串加 DXF 组码和值。 |
| [OpLogi.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpLogi.cs) | `OpLogi` | 逻辑条件的抽象序列和 typed value 包装。 |
| [OpList.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpList.cs) | `OpList`、`Add` | 可包含多个子条件的列表基类。 |
| [OpAnd.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpAnd.cs) | `OpAnd` | AND 组合。 |
| [OpOr.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpOr.cs) | `OpOr` | OR 组合。 |
| [OpNot.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpNot.cs) | `OpNot` | 单个子条件取反。 |
| [OpXor.cs](../../src/CADShared/Cad/Editor/Selection/Filters/OpXor.cs) | `OpXor` | 两个子条件异或。 |

典型定位链：选择没有命中 → 先查条件树生成的 typed value / DXF 名称 → 再查 Prompt 选项和当前空间 → 最后查选择结果的实体类型转换。条件构造和宿主选择是两层，不要把空结果直接归为几何错误。

已有入口：[TestSelectfilter.cs](../../tests/TestShared/TestSelectfilter.cs)。

<a id="input"></a>

## 单键、关键字与取消

| 文件 | 入口 | 当前行为 |
| --- | --- | --- |
| [SingleKeyWordHook.cs](../../src/CADShared/Cad/Editor/Input/SingleKeyWordHook.cs) | 构造、`AddKeys`、`Reset`、`Pause`、`Working`、`Dispose` | 挂接 CAD 的 `PreTranslateMessage`，识别配置的单键，并按工作模式发送 Escape、Enter 或命令行文本；不是 Windows 全局快捷键管理器。Dispose 负责解绑。 |
| [SingleKeyWordWorkType.cs](../../src/CADShared/Cad/Editor/Input/SingleKeyWordWorkType.cs) | `SingleKeyWordWorkType` | 定义不同键盘回应模式。直接构造 Hook 的默认值是 `ESCAPE`。 |
| [SingleKeywordHookEx.cs](../../src/CADShared/Cad/Editor/Input/SingleKeywordHookEx.cs) | `KeywordCollection.HookSingleKeyword` | 从关键字集合创建 Hook；此扩展的默认值是 `WRITE_LINE`，与构造默认值不同。 |
| [PromptOptionsEx.cs](../../src/CADShared/Cad/Editor/PromptOptionsEx.cs) | `AddKeywords`、`ThrowKeywordAsException` | 给选择选项追加关键字，并可把 KeywordInput 转为异常。 |
| [KeywordException.cs](../../src/CADShared/Cad/Editor/KeywordException.cs) | `KeywordException` | 携带关键字输入信息；不是所有 Prompt 失败或 Esc 的统一异常。 |

循环输入时 `Reset` 清除上次回应状态；`Pause` 不等于 Dispose。命令结束前应明确 Hook 的归属和解绑。不要因为输入过 Escape 就假定关键字、None 和 Cancel 是同一结果。

已有入口：[TestSingleKeyWordHook.cs](../../tests/TestShared/TestSingleKeyWordHook.cs)。

<a id="jig"></a>

## Jig 与瞬态容器

| 文件 | 入口 / 数据 | 对象所有权 |
| --- | --- | --- |
| [JigEx.cs](../../src/CADShared/Cad/Editor/Jig/JigEx.cs) | `SetOptions`、`DatabaseEntityDraw`、`Entities`、`MousePointWcsLast`、`DimensionEntities` | 封装 `DrawJig` 的采样、WorldDraw、关键字和动态尺寸。鼠标回调中的队列用于频繁重建的临时实体；已有数据库实体走专门绘制回调。 |
| [WorldDrawEvent.cs](../../src/CADShared/Cad/Editor/Jig/WorldDrawEvent.cs) | `WorldDrawEvent` 委托 | 为 `JigEx` 的已有对象绘制回调提供签名。 |
| [JigExTransient.cs](../../src/CADShared/Cad/Editor/Jig/JigExTransient.cs) | `Add/Remove`、`Update/UpdateAll`、`Clear`、`ClearAndDisposeDrawables`、`Dispose` | 包装 `TransientManager`，在 `!GC_2022 && !GC_2023` 下启用；Clear 只移除显示和集合，带 Dispose 的清理还销毁 Drawable。 |

需要分别判断：

1. 预览显示不等于永久落库。成功后仍需把目标实体交给数据库容器；取消路径则清理未使用对象。
2. `JigEx.Dispose` 会销毁队列中尚未加入数据库的最后一批实体；已经落库的对象不按同一条件销毁。
3. `JigExTransient.Dispose` 会执行 `ClearAndDisposeDrawables`。若 Drawable 是借来的数据库对象，不要假定容器只拥有显示状态。
4. `JigEx.Drag()` 已标记过时，旧注释建议使用 `Editor.Drag(jig)`；参数和 PromptStatus 仍由具体交互流程处理。

已有入口：[TestJig.cs](../../tests/TestShared/TestJig.cs)、[TestJigExTransient.cs](../../tests/TestShared/TestJigExTransient.cs)。

<a id="display-command"></a>

## 显示刷新与命令派发

| 文件 | 入口 | 职责 |
| --- | --- | --- |
| [RedrawEx.cs](../../src/CADShared/Cad/Editor/Display/RedrawEx.cs) | Editor/Entity 的 `Redraw` | 图形 flush、刷新与亮显状态处理，包含 native 路径。 |
| [BrightEditor.cs](../../src/CADShared/Cad/Editor/Display/BrightEditor.cs) | `BrightEditor` | Editor 刷新/高亮相关模式。 |
| [BrightEntity.cs](../../src/CADShared/Cad/Editor/Display/BrightEntity.cs) | `BrightEntity` | 单实体显示模式。 |
| [PostCmd.cs](../../src/CADShared/Cad/Editor/Commands/PostCmd.cs) | `SendCommand`、`RunCmdFlag` | 将 typed value、指针或字符串分派到 native 命令执行、投递或 COM 路径。同步/异步取决于分支和上下文，不能只依据方法名判断。 |

`PostCmd` 与 `EditorEx.RunLisp` 的 native 委托依赖[AcadPeInfo](runtime-ui.md#interop)。这里只提供查找路线，不把旧注释关于多线程、权限或历史 CAD 版本的经验提升为全宿主保证。

返回[源码检索总入口](README.md)。
