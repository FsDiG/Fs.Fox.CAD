# Foundation 与 Windows 平台定位

> 状态：current；受众：maintainer；稳定 ID：`reference.source-foundation-platform`<br>
> 基线：[源码检索总入口](README.md)；覆盖 `Foundation` 11 个和 `Platform/Windows` 15 个共享编译文件。

本册处理经常被 CAD 入口间接调用的通用容器、兼容辅助、Win32 与 PE 结构。目录归属与公共命名空间是不同信息；绝大多数基础/Win32 类型位于 `Fs.Fox.Basal`，但 `ProgramPE.cs` 的 `PeInfo` 等仍位于 `Fs.Fox.Cad`。

<a id="foundation"></a>

## Foundation 文件地图

| 文件 | 类型 / 常用入口 | 职责和阅读线索 |
| --- | --- | --- |
| [ArrayEx.cs](../../src/CADShared/Foundation/ArrayEx.cs) | `ArrayEx.Combine2`、`Deduplication` | 合并数组、按自定义判等函数对列表去重。去重直接处理传入列表，不是创建独立的查询视图。 |
| [DebugHelper.cs](../../src/CADShared/Foundation/DebugHelper.cs) | 实际类型 `DebugEx`、`Printl` | 调试/跟踪输出；先检查用户环境变量 `debugx`，再按 `DEBUG` 分支写 Debug 或 Trace。文件名不是类型名。 |
| [EnumEx.cs](../../src/CADShared/Foundation/EnumEx.cs) | `GetAttribute`、`PrintNote`、`GetDescription`、`CleanCache` | 枚举说明特性、显示文本与缓存；看不到预期说明时核对字段特性和 fallback 参数。 |
| [LinqEx.cs](../../src/CADShared/Foundation/LinqEx.cs) | `FindByMax/FindByMin`、`FindByExt/FindExt`、自定义比较的 `OrderBy/ThenBy` | 取极值元素、极值键和排序辅助。返回元素与返回键是不同方法组；内部比较器也在此文件。 |
| [LoopList.cs](../../src/CADShared/Foundation/LoopList.cs) | `LoopList<T>`、`AddFirst/AddLast`、`AddBefore/AddAfter`、`Remove`、`Reverse`、`LinkTo`、`GetNodes` | 双向循环链表，供几何点环等场景使用；修改接链、首节点、删除节点时先看节点归属和 Count 维护。 |
| [LoopListNode.cs](../../src/CADShared/Foundation/LoopListNode.cs) | `LoopListNode<T>`、`GetNext` | 保存值、前后节点和所属链表；节点不是可随意复用到另一条链上的孤立数据。 |
| [LoopState.cs](../../src/CADShared/Foundation/LoopState.cs) | `IsRun/IsBreak/IsStop/IsCancel`、`Exceptional`、`Reset` | 循环回调使用的状态标记，Collection/SymbolTable/SelectionSet 的某些 ForEach 重载会消费它。 |
| [ProState.cs](../../src/CADShared/Foundation/ProState.cs) | `Start`、`None`、`Break/Stop/Cancel`、异常标记 | 程序级状态载体，初始为 None；与初始可运行的 LoopState 不相同，不自动驱动异步任务。 |
| [RandomEx.cs](../../src/CADShared/Foundation/RandomEx.cs) | `GetRandom`、`NextInt/NextDouble/NextBool/NextString/NextColor` | 随机数、集合随机项和颜色；提供使用调用方 Random 与默认来源的重载。 |
| [ArgumentNullEx.cs](../../src/CADShared/Foundation/Compatibility/ArgumentNullEx.cs) | `ArgumentNullEx.ThrowIfNull` | 旧框架缺少对应运行库方法时的条件兼容入口，在 `AC_NET48/ZWCAD/GC_2022/GC_2023` 下启用。 |
| [CallerArgumentExpressionAttribute.cs](../../src/CADShared/Foundation/Compatibility/CallerArgumentExpressionAttribute.cs) | internal `CallerArgumentExpressionAttribute` | 位于 `System.Runtime.CompilerServices`，补充旧框架的参数表达式注解；不是 CAD 类型，也不应作为独立业务接口调用。 |

### 循环控制的阅读链

`LoopList` 表示循环数据结构，`LoopState` 表示一次回调循环是否继续，`ProState` 表示程序流程状态。三者名字接近但没有同一套生命周期。排查 ForEach 提前终止时，应查看实际重载有没有检查 `LoopState.IsRun`；仅创建状态对象不会停止任意 foreach。

`GeometryEx.GetMinCircle` 等算法使用 `LoopList<Point2d>`，因此几何结果异常不一定在 CAD 数据库层。可从 [TestLoop.cs](../../tests/TestShared/TestLoop.cs) 阅读链表已有调用。

<a id="windows"></a>

## Win32 声明与系统辅助

| 文件 | 类型 / 入口 | 用途与副作用 |
| --- | --- | --- |
| [PInvokeUser32.cs](../../src/CADShared/Platform/Windows/Interop/PInvokeUser32.cs) | `FindWindow`、`SendMessage/PostMessage`、焦点/窗口/键盘 API、嵌套 `GuiThreadInfo` | Windows user32 入口声明，用于句柄、线程/进程、焦点、键盘状态和消息。成功调用这些函数不证明 CAD 文档/命令上下文正确。 |
| [WindowsAPI.cs](../../src/CADShared/Platform/Windows/Interop/WindowsAPI.cs) | `GetModuleHandle/GetProcAddress`、内存与结构转换、输入法、Hook、窗口矩形等 | 同文件多段 partial 类型。部分代码在精确的条件编译分支内，不能从一次文本命中推断目标程序集包含该成员。 |
| 同上 | `GlobalLockTask`、`BytesToStruct`、`StructToBytes`、`StructToPtr` | HGlobal、指针及托管结构间转换。所有权和锁定/释放参数要顺着分配位置检查，不等于自动管理任意 native 指针。 |
| 同上 | `KeyboardHookStruct`、`IntRect`、`IntSize`、`Point3D` | 与 ABI、窗口或消息关联的辅助结构，注意字段宽度和对齐；`Point3D` 不是厂商 `Point3d`。 |
| [WmMessage.cs](../../src/CADShared/Platform/Windows/Interop/WmMessage.cs) | internal `WmMessage.Close` | 内部窗口消息常量，和公开 WM 枚举不同。 |
| [SystemEx.cs](../../src/CADShared/Platform/Windows/Interop/SystemEx.cs) | `CloseProc` | 按进程名枚举并调用 `Process.Kill`；方法实际会结束进程，不是只发送关闭窗口请求。本轮未调用。 |

### 消息与标记的定义位置

这些文件主要提供枚举。看 P/Invoke 参数时可直接到对应定义确认值域和底层整数类型，不需要先阅读大型 WindowsAPI 文件：

| 文件 / 类型 | 参数场景 |
| --- | --- |
| [GWL.cs](../../src/CADShared/Platform/Windows/Interop/GWL.cs) | WindowLong 索引。 |
| [GetWindowCmd.cs](../../src/CADShared/Platform/Windows/Interop/GetWindowCmd.cs) | 查询相关窗口时的方向/关系。 |
| [HookType.cs](../../src/CADShared/Platform/Windows/Interop/HookType.cs) | Windows Hook 种类。 |
| [MOUSEEVENTF.cs](../../src/CADShared/Platform/Windows/Interop/MOUSEEVENTF.cs) | 鼠标事件位。 |
| [NCmdShow.cs](../../src/CADShared/Platform/Windows/Interop/NCmdShow.cs) | 显示/隐藏/最小化等窗口方式。 |
| [SC.cs](../../src/CADShared/Platform/Windows/Interop/SC.cs) | 系统命令值。 |
| [TH32CS.cs](../../src/CADShared/Platform/Windows/Interop/TH32CS.cs) | Toolhelp 快照标记。 |
| [VK.cs](../../src/CADShared/Platform/Windows/Interop/VK.cs) | 虚拟键码。 |
| [WM.cs](../../src/CADShared/Platform/Windows/Interop/WM.cs) | Windows 消息标识。 |
| [WS.cs](../../src/CADShared/Platform/Windows/Interop/WS.cs) | 窗口样式位。 |

CAD 单字母输入 Hook 另有 [`SingleKeyWordHook`](application-editor.md#input)，它主要挂接 CAD `PreTranslateMessage`；不要因名称相似就把它与 `SetWindowsHookEx` 全局/线程 Hook 混为同一实现。

## ProgramPE.cs：类型名是 PeInfo

[ProgramPE.cs](../../src/CADShared/Platform/Windows/PortableExecutable/ProgramPE.cs) 超过 1,500 行，包含解析器与多种数据载体。先从 `PeInfo(fullName)` 进入，它读取文件字节并解析头、节、导出/导入和资源结构，构造后文件流被关闭。

| 同文件类型 / 成员 | 阅读内容 |
| --- | --- |
| `PeInfo`、`OpenFile`、`FullName`、`GetPETable` | 主入口、打开结果和用于查看解析结果的 System.Data 表集合。 |
| `DosHeader`、`DosStub`、`PEHeader`、`OptionalHeader` | 文件头、位数相关信息及可选头。 |
| `OptionalDirAttrib`、`DirAttrib`、`SectionTable`、`SectionData` | 数据目录、节表和节数据。 |
| `ExportDirectory`、`FunctionNames` | 导出名称、地址及序号表，供 CAD 导出解析使用。 |
| `ImportDirectory`、`ImportDate`、`FunctionList` | 导入模块与函数信息；`ImportDate` 是现有类型名。 |
| `ResourceDirectory`、`DirectoryEntry`、`DataEntry` | 资源目录树与数据条目。 |

上层 [`AcadPeInfo`](runtime-ui.md#interop) 会把这个解析器与当前 CAD 进程的模块句柄连接起来。磁盘结构解析提供的是导出信息，实际地址调用的 ABI 和宿主兼容性仍需单独核对；它也不是程序集 CLR 类型反射器。

已有入口：[TestMarshal.cs](../../tests/TestShared/TestMarshal.cs)。本轮没有加载 native DLL、操作系统 Hook 或调用任何结束进程的辅助方法。

返回[源码检索总入口](README.md)。
