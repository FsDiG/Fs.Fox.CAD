# 运行时、UI 与宿主互操作定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-runtime-ui`<br>
> 基线：[源码检索总入口](README.md)；覆盖 `Cad/Runtime` 14 个、`Cad/UI` 8 个和 `Cad/Interop` 3 个共享编译文件。

阅读入口：[初始化](#initialization)、[注册](#registration)、[Lisp](#lisp)、[UI](#ui)、[native](#interop)。这些模块有注册表、事件、窗口和 native 生命周期影响；本轮只静态查阅，没有执行其中的操作。

<a id="runtime"></a>
<a id="initialization"></a>

## 程序集初始化、反射与终止

| 文件 | 类型 / 入口 | 职责 |
| --- | --- | --- |
| [AutoRegAssem.cs](../../src/CADShared/Cad/Runtime/Registration/AutoRegAssem.cs) | 抽象 `AutoRegAssem : IExtensionApplication`、构造、`Location/CurrDirectory/GetDirectory` | 程序集入口基类，构造时按配置执行注册和反射初始化。查“启动没执行”首先看派生类型如何构造、传了什么标记。 |
| [AutoReflection.cs](../../src/CADShared/Cad/Runtime/Initialization/AutoReflection.cs) | `Initialize/Terminate`、`AppDomainGetTypes` | 从目标程序集发现特性/接口方法，按 Sequence 排序执行，缓存同一实例供终止使用；运行列表执行后清空。 |
| [IFoxInitializeAttribute.cs](../../src/CADShared/Cad/Runtime/Initialization/IFoxInitializeAttribute.cs) | `IFoxInitializeAttribute` | 方法级特性，记录执行顺序及属于初始化还是终止。 |
| [IFoxAutoGo.cs](../../src/CADShared/Cad/Runtime/Initialization/IFoxAutoGo.cs) | `SequenceId`、`Initialize`、`Terminate` | 实例接口形式的初始化/终止入口。 |
| [Sequence.cs](../../src/CADShared/Cad/Runtime/Initialization/Sequence.cs) | `Sequence.First/Last` | 当前只有两种次序值；不要据 `[Flags]` 注释推断存在完整优先级调度器。 |
| [MethodInfoHelper.cs](../../src/CADShared/Cad/Runtime/Initialization/MethodInfoHelper.cs) | 内部 `MethodInfoHelper.Invoke` | 根据静态/实例方法创建和复用实例，供反射执行链使用。 |
| [RunClass.cs](../../src/CADShared/Cad/Runtime/Initialization/RunClass.cs) | 内部 `RunClass.Run` | 绑定方法、Sequence 和实例的执行载体。 |
| [CheckFactory.cs](../../src/CADShared/Cad/Runtime/Initialization/CheckFactory.cs) | `CheckDuplicateCommand` | 在 `DEBUG` 条件内反射公开类型与 `CommandMethodAttribute`，提示重复全局命令名；不属于所有配置都存在的运行时 API。 |
| [AcadEMR.cs](../../src/CADShared/Cad/Runtime/Initialization/AcadEMR.cs) | 内部 `AcadEMR.Remove` | 历史 AutoCAD 教育版相关处理，由配置分支调用；不是常规加载诊断或通用清理流程。 |

实际调用链：

```text
业务程序集派生 AutoRegAssem
  -> 基类构造：读取调用程序集 AssemInfo
  -> 依 AutoRegConfig 执行注册 / ACAD 特定处理
  -> AutoReflection.Initialize
       -> 收集 IFoxInitializeAttribute 与 IFoxAutoGo
       -> 按 Sequence 升序运行初始化项
       -> 方法调用与实例复用
```

### 终止路径不能只看接口和 XML 描述

当前 `AutoRegAssem.Initialize()` 与 `AutoRegAssem.Terminate()` 方法体为空。反射初始化在基类构造函数执行；`_autoRef.Terminate()` 的现有调用位于 `AutoRegAssem` 终结器中。

因此，“实现了 IExtensionApplication”不等于宿主调用其 Terminate 时必然运行已收集的终止方法，也不能依赖终结器及时完成 UI/native 清理。排查结束未释放时应直接看这个调用点和派生入口。本导航记录当前控制流，不修改生命周期，也不把该行为写成已完成的可靠退出契约。

`AutoReflection.AppDomainGetTypes` 会跳过动态程序集、特定通讯程序集和取类型失败的程序集；发现不到某个方法时，先核对目标 DLL 名称、类型加载和特性/接口标记，不要先改 Sequence。

<a id="registration"></a>

## 自动加载注册

| 文件 | 类型 / 入口 | 作用 |
| --- | --- | --- |
| [AutoReg.cs](../../src/CADShared/Cad/Runtime/Registration/AutoReg.cs) | `GetAcAppKey`、`SearchForReg`、`RegApp`、`UnRegApp` | 使用当前宿主用户产品注册表根路径下的 `Applications`，读取或写入 `LOADER`、`LOADCTRLS` 等程序集加载信息。 |
| [AssemInfo.cs](../../src/CADShared/Cad/Runtime/Registration/AssemInfo.cs) | `AssemInfo` | 程序集路径、名称、完整名和加载类型的载体。 |
| [AssemLoadType.cs](../../src/CADShared/Cad/Runtime/Registration/AssemLoadType.cs) | `Starting`、`ByCommand`、`Disabled` | 自动加载配置值。 |
| [AutoRegConfig.cs](../../src/CADShared/Cad/Runtime/Registration/AutoRegConfig.cs) | `Regedit`、`ReflectionAttribute`、`ReflectionInterface`、`RemoveEMR`、`All` | 选择基类构造执行的动作。`All` 包含注册及 AutoCAD 特定历史处理，不只是“发现初始化方法”。 |

`SearchForReg` 会比较同名注册项的 LOADER 路径；它调用的 `GetAcAppKey` 使用可写打开并可能创建 `Applications` 子项，所以不能从 `Search` 这个名称推断整个路径一定无副作用。

查看注册逻辑不要求执行注册。本轮没有修改用户注册表、Trusted Paths、启动组或 CAD 全局配置。

<a id="lisp"></a>

## Lisp typed value 与执行入口

[LispList.cs](../../src/CADShared/Cad/Runtime/Lisp/LispList.cs) 的 `LispList` 继承数据库模块的 `TypedValueList`，为布尔、数字、字符串、ObjectId、选择集、二维/三维点和嵌套列表提供 `Add`，并支持 `ResultBuffer` / 数组转换。

这条继承关系不代表 Lisp 数据就是实体 XData：使用的组码与消费协议不同。执行继续到 [`EditorEx.RunLisp`](../../src/CADShared/Cad/Editor/EditorEx.cs)，需要命令/字符串派发则查看 [`PostCmd`](../../src/CADShared/Cad/Editor/Commands/PostCmd.cs)，native 符号解析见本页后半部分。

已有入口：[TestLisp.cs](../../tests/TestShared/TestLisp.cs)。

<a id="ui"></a>

## 窗口、状态栏、对话框和首选项

| 文件 | 入口 | 功能及真实宿主差异 |
| --- | --- | --- |
| [WindowEx.cs](../../src/CADShared/Cad/UI/Windows/WindowEx.cs) | `AddEscQuit`、`IsModel`、`GetScreenResolutionFromWindowHandle`、`SetSizeByScreenResolution`、`GetScreenScale` | WPF Esc 关闭、模态检查、屏幕/Palette 尺寸与缩放。`IsModel` 通过反射读取 WPF 内部字段；它不是稳定公开 WPF 契约。 |
| [PaneEx.cs](../../src/CADShared/Cad/UI/StatusBar/PaneEx.cs) | `SetMargin`、`SetLeftMargin`、`SetRightMargin` | 设置宿主状态栏 Pane 的边距样式。 |
| [PaneMarginType.cs](../../src/CADShared/Cad/UI/StatusBar/PaneMarginType.cs) | `PaneMarginType` | Pane 边距模式，不是 WPF Thickness。 |
| [ProgressMeterUtils.cs](../../src/CADShared/Cad/UI/StatusBar/ProgressMeterUtils.cs) | 创建/更新 `SetApplicationStatusBarProgressMeter`、`RestoreApplicationStatusBar` | AutoCAD 走内部 Utils，ZWCAD 调用 native 入口并检查返回值，GStarCAD 当前分支是 no-op。源码共用方法名不等于三个宿主功能相同。 |
| [HatchDialog.cs](../../src/CADShared/Cad/UI/Dialogs/HatchDialog.cs) | `Name`、`ShowCustom`、`ShowDialog` | `ACAD` 条件内的填充图案选择封装，调用 `acad.exe` 的图案面板 native 入口；不是通用 WinForms/WPF Dialog，也不是 ZWCAD 可用类。 |
| [ErrorInfoEx.cs](../../src/CADShared/Cad/UI/Dialogs/ErrorInfoEx.cs) | 内部扩展 `AcErrorInfo` | CAD 异常错误状态的说明/提示映射。类虽 public，主要扩展入口为 internal，不应按类可见性推断成员均对外开放。 |
| [AcPreferences.cs](../../src/CADShared/Cad/UI/Preferences/AcPreferences.cs) | 嵌套静态 `Display` 的属性 | 动态访问 `CadApp.Preferences.Display`，处理布局、光标、颜色等首选项；依赖宿主的动态对象模型，不是纯本地配置对象。 |
| [IFoxUtils.cs](../../src/CADShared/Cad/UI/IFoxUtils.cs) | `RegenLayers`、`RegenLayers2`、`ShowBubbleWindow`、`VetoMouseDoubleClickEvent`、`CreateTransparency` | 图层刷新、气泡窗口、双击否决与透明度转换的杂项入口；部分刷新路径另有宿主分支。 |

`WindowEx.AddEscQuit` 在窗口 KeyDown 上判断当前窗口已经加载且激活、按下 Escape 且没有 Ctrl/Shift/Alt 时关闭；窗口 Closed 后解绑。它没有建立全局 Esc 钩子，也没有替业务窗口实现“是否保存”的决策。

状态栏进度计应区分创建、位置更新和恢复三个动作。GStarCAD 未显示时先核对 no-op 分支，不要把问题误定位到进度数值或 WPF 绑定。

<a id="interop"></a>

## CAD native 与第三方边界

| 文件 | 类型 / 入口 | 调用链 |
| --- | --- | --- |
| [AcadPeInfo.cs](../../src/CADShared/Cad/Interop/Native/AcadPeInfo.cs) | `AcadPeInfo.GetDelegate`、`AcadPeEnum`、`PeFunction`、`GetMethodErrorNum`、`GetPeMethodException` | 读取当前进程/核心库/数据库库的 PE 导出信息，按函数名搜索，缓存匹配项，再取 native 地址并转换为委托。 |
| [PInvokeCad.cs](../../src/CADShared/Cad/Interop/Native/PInvokeCad.cs) | 内部 `PInvokeCad`、`CadAdsName`、`GetAdsName/EntGet/EntMod/EntUpd` | 根据 ZWCAD、GStarCAD 和 AutoCAD 版本选择 P/Invoke 模块/入口；AutoCAD 分支明确枚举支持的 AC 符号，否则 `#error`。 |
| [TangentEx.cs](../../src/CADShared/Cad/Interop/ThirdParty/Tianzheng/TangentEx.cs) | `TangentEx.TgetPscale` | 调用 `tch_kernal.arx` 的绘图比例入口，是特定天正 native 依赖，不是 CAD SDK 的通用比例 API。 |

高频追踪关系：

```text
PostCmd / RedrawEx / DBmodEx 等
  -> AcadPeInfo：宿主文件与导出匹配
  -> Platform/Windows/PortableExecutable/ProgramPE.cs 中的 PeInfo
  -> WindowsAPI.GetProcAddress / Marshal 委托转换

Env.EntGet / EntMod / EntUpd
  -> PInvokeCad：按厂商及 API 代际选择声明

BlockReferenceEx.GetVisibilityInfo
  -> Env.EntGet：读取动态块参数 typed value
```

读取到 DLL 文件、找到导出名、得到非零地址和正确调用 ABI 是不同检查层。源代码中的厂商分支、字符集、调用约定、结构布局和委托生命周期都应随实际宿主核对，静态文档不宣称已验证这些调用。

Win32 与 PE 数据结构的文件地图见[基础与 Windows](foundation-platform.md#windows)。返回[源码检索总入口](README.md)。
