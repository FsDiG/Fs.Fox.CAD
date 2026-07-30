# ZWCAD 版本兼容性与迁移说明

## 1. 文档目的

本文记录 ZWCAD 版本之间的 SDK/API 兼容关系，以及 `Fs.Fox.CAD` 对 ZWCAD 2025、2026 的支持策略。内容仅面向 Windows x64 版 ZWCAD；AutoCAD 的版本兼容策略另行记录。

## 2. 资料来源

- 资料名称：中望 CAD 2026 .NET 移植指南（CHM）
- 本地文件：`D:\Downloads\ZWCAD_DotNET_Migration_chs_2026\ZWCAD_DotNET_Migration_chs_2026.chm`
- 文件大小：294,605 字节
- 文件修改时间：2025-03-18 17:51:24
- SHA-256：`43DA872BD40A59C0A5A353C47AB3887EC5D6655BB1999329682080D4F58406FE`

以下兼容性结论均来自该 CHM。仓库现状和 `Fs.Fox.CAD` 的工程决策会单独标注，避免将项目选择误写成厂商承诺。

## 3. ZWCAD/ZRXSDK 兼容矩阵

| ZWCAD/ZRXSDK 版本 | 开发环境 | 平台工具集 | .NET Framework | 对应的 ObjectARX API 代际 |
| --- | --- | --- | --- | --- |
| 2017-2020 | Visual Studio 2010 | v140 | 4.0 | ObjectARX 2008 |
| 2021-2024 | Visual Studio 2017 15.9 或更高 | v141 | 4.7 | ObjectARX 2019 |
| 2025-2026 | Visual Studio 2017 15.9 或更高 | v141 | 4.7 | ObjectARX 2024 |

> 注：迁移指南原表第三行写作 `ZRXSDK2025~`。本文只确认该资料实际覆盖的 ZWCAD 2025-2026，不向 2027 及后续版本外推。第一行的 “Visual Studio 2010 / v140” 组合与常见的 Visual Studio 工具集对应关系不一致，实际构建旧版本前应再以对应年度 ZRXSDK 的工程和发布说明为准。

### 版本边界

- ZWCAD 2021-2024 属于同一二进制兼容代际，可使用其中任一年度的 ZRXSDK 构建，接口与 ObjectARX 2019 兼容。
- ZWCAD 2025 将接口代际升级为与 ObjectARX 2024 兼容。使用更早 SDK 构建的扩展需要基于 ZRXSDK 2025 或更高版本重新编译。
- ZWCAD 2026 的接口与 ZWCAD 2025 兼容。迁移指南明确说明，使用 ZWCAD 2025 编译的 ZRX 程序无需重新编译，可直接在 ZWCAD 2026 中加载。

## 4. Fs.Fox.CAD 的 2025/2026 支持决策

`Fs.Fox.CAD` 使用现有 ZWCAD 2025 构建产物同时支持 ZWCAD 2025 和 ZWCAD 2026：

- 复用 `src/Fs.Fox.ZwCad2025/Fs.Fox.ZwCad2025.csproj` 生成的 `Fs.Fox.ZwCad.dll`。
- 继续使用条件编译符号 `ZWCAD`、`ZW_2025`，不新增 `ZW_2026`。
- 继续使用 `ZWCAD.NetApi` 20.25.0 作为编译期依赖，不仅为支持 2026 而升级到 20.26.0。
- 不新增 `Fs.Fox.ZwCad2026`、`TestZcad2026` 或单独的 `IFox.CAD.ZCAD2026` 包。
- 继续使用 `TestZcad2025` 作为 2025/2026 共享产物的构建和宿主验收入口。

该决策建立在厂商声明的 2025/2026 接口兼容性之上，但最终仍以 ZWCAD 2026 真实宿主加载和功能回归为发布门槛。

## 5. 运行时和架构要求

### 厂商基线

迁移指南给出的 ZWCAD 2026 托管开发环境是 `.NET Framework 4.7`，不是 `.NET 8`。因此不能因为产品年度为 2026，就将插件目标框架改为 `net8.0-windows`。

### 当前仓库目标

当前 `Fs.Fox.ZwCad2025` 和 `TestZcad2025` 实际目标框架均为 `net48`，测试工程平台目标为 x64。这意味着：

- ZWCAD 2026 目标机器还必须安装 .NET Framework 4.8；只有厂商文档所列的 4.7 基线不足以运行 `net48` 插件。
- .NET Framework 4.x 是原地更新系列。安装 4.8 后，ZWCAD 2026 与插件在同一个 CLR v4 进程中运行。
- 发布物及其托管/原生依赖必须统一按 x64 构建和验证。

## 6. 托管程序集和命名空间

从其他 CAD 平台迁移托管插件时，应按功能引用 ZWCAD SDK 提供的程序集，常用项包括：

| 程序集 | 主要用途 |
| --- | --- |
| `ZwManaged.dll` | 应用程序、编辑器、运行时、发布等托管 API |
| `ZwDatabaseMgd.dll` | 数据库、几何、图形接口等托管 API |
| `ZwDatabaseMgdBrep.dll` | BRep 相关 API |
| `ZcWindows.dll` | Ribbon、窗口及部分界面 API |
| `ZdWindows.dll` | 部分界面及工具栏 API |

ZWCAD 托管 API 的根命名空间为 `ZwSoft.ZwCAD`。迁移时不能只替换程序集引用，还需要检查命名空间及少量平台特有的类型、函数名称。完整的程序集/命名空间和 API 映射表应查阅对应年度 SDK 与原始迁移指南，本文不复制这些明细。

## 7. 版本检测、目录和注册表

### 版本检测

- ZWCAD 不维护 `ACADVER` 系统变量，不应使用它判断 ZWCAD 年度版本。
- 使用 `VERNUM` 获取产品版本。迁移指南示例为 `26.00_2025.02.21(#17071-e130dec1767)_x64_T`。
- `VERNUM` 包含内部版本、构建日期和提交标识等信息。代码判断兼容能力时应避免只依赖显示名称或安装目录。

### ZWCAD 2026 默认位置

- 漫游配置目录：`%AppData%\ZWSOFT\ZWCAD\2026\zh-CN`
- 本地模板目录：`%LocalAppData%\ZWSOFT\ZWCAD\2026\zh-CN\Template`
- 当前用户注册表根：`HKEY_CURRENT_USER\SOFTWARE\ZWSOFT\ZWCAD\2026\zh-CN`
- 本机注册表根：`HKEY_LOCAL_MACHINE\SOFTWARE\ZWSOFT\ZWCAD\2026\zh-CN`

代码中优先使用 `ROAMABLEROOTPREFIX`、`LOCALROOTPREFIX` 等宿主提供的系统变量或宿主服务获取实际位置，不应把上述年度和语言目录硬编码为通用路径。

## 8. 加载、自动加载和调试

### 手动加载 .NET 插件

1. 将项目编译为 DLL。
2. 启动 ZWCAD 2026。
3. 执行 `NETLOAD`，选择插件 DLL。

### 自动加载方式

迁移指南列出的机制包括：

- 在 `ZWCAD.lsp`、`ZWCAD2026.lsp` 或 `ZWCAD2026DOC.lsp` 中执行 `NETLOAD` 或加载脚本。
- 使用 `APPLOAD` 启动组；配置记录在支持文件搜索路径下的 `AppAutoLoad.app`。
- 在注册表 `...\Applications` 下配置应用，常用值包括 `LOADER`、`LOADCTRLS`，并可带 `Commands`、`Groups` 子项。
- 使用 `/b` 启动参数执行脚本，再由脚本加载托管插件。
- `/ld` 启动参数和 `ZWCAD.rx` 用于加载原生 ZRX 程序，不应直接当作 .NET DLL 的加载方式。

迁移指南明确指出 ZWCAD 暂不支持通过 `.bundle` 文件夹自动加载。部署方案不能直接照搬依赖 `.bundle` 的加载流程。

### 调试

- 可在 Visual Studio 项目调试设置中将外部程序指向 `ZWCAD.exe`，然后启动调试。
- 也可先启动 ZWCAD，再将调试器附加到 `ZWCAD.exe`。
- 附加时应包含“本机”和“托管 (v4.x)”代码类型，以便同时诊断原生/托管边界问题。

## 9. ZWCAD 2026 宿主验收

自动构建只能验证源代码和引用关系，不能证明 .NET Framework 插件能在 CAD 宿主中正确绑定、加载和运行。发布前应在 ZWCAD 2026 x64 中完成以下检查：

文档编写时已执行以下构建检查：

```powershell
dotnet build .\tests\TestZcad2025\TestZcad2025.csproj -c Release
```

2026-07-30 的执行结果为构建成功，0 个警告、0 个错误；生成了 `Build\ZW_2025_Release\Fs.Fox.ZwCad.dll` 和 `Build\ZW_2025_Release\TestZcad2025.dll`。该结果不是 ZWCAD 2026 宿主验收结论。

- [ ] 记录测试机器上的 `VERNUM` 完整值。
- [ ] 确认系统已安装 .NET Framework 4.8。
- [ ] 使用 `NETLOAD` 加载 ZWCAD 2025 构建目录中的 `TestZcad2025.dll`。
- [ ] 确认 `Fs.Fox.ZwCad.dll` 及其他托管/x64 原生依赖均能解析，没有程序集版本冲突。
- [ ] 验证测试命令能够注册和执行。
- [ ] 验证代表性的数据库读写、事务和实体操作。
- [ ] 验证 WPF/WinForms 或 Ribbon 等实际使用的界面入口。
- [ ] 关闭并重新启动 ZWCAD，验证采用的自动加载方案。

验收记录：

| 项目 | 结果 |
| --- | --- |
| ZWCAD `VERNUM` | 待记录 |
| Windows 版本 | 待记录 |
| .NET Framework 4.8 | 待确认 |
| `NETLOAD` | 待验证 |
| 命令及核心数据库操作 | 待验证 |
| 界面入口 | 待验证 |
| 自动加载 | 待验证 |

## 10. 资料边界

- 本文是版本兼容性和项目决策摘要，不替代 ZRXSDK 头文件、托管程序集元数据、官方示例或对应年度发布说明。
- CHM 对“2025 编译产物可直接用于 2026”的说明仍需通过本项目真实插件验证，特别是第三方依赖、界面组件和自动加载行为。
- ObjectARX 版本仅用于标识 ZWCAD API 的兼容代际；AutoCAD 自身的目标框架、加载机制和年度兼容策略不属于本文范围。
