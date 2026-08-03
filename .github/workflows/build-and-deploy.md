# Fs.Fox.CAD 构建检查工作流

本页说明 `.github/workflows/build-and-deploy.yml` 的当前行为。项目、SDK、
输出和发布边界的权威说明仍以实时工作流、项目文件及
[构建说明](../../编译说明.md)为准。

## 触发与边界

工作流在以下情况运行：

- 推送到 `main`；
- 向 `main` 或历史兼容分支 `refactor/cad-modules` 提交 PR；
- 任意分支提交消息包含 `[build]` 或 `[deploy]`；
- 手动执行 `workflow_dispatch`。

`deploy` 输入和 `[deploy]` 目前只触发构建检查，不部署 DLL、不发布 NuGet、
不修改 CAD 配置，也不启动 CAD 宿主。

## 执行顺序

单个 Windows self-hosted 作业按以下顺序运行：

1. 检出源码，配置 .NET 与 MSBuild。
2. 检查 `TypeDef` 顺序比较器及 CADShared 模块映射。
3. 构建全部正式类库/测试入口的 Debug、Release。
4. 构建独立 CadDiagnostics AutoCAD 2019/2025 的 Debug、Release。
5. 检查 CADShared 兼容性基线。
6. 检查 CadDiagnostics 源码迁移清单、独立程序集引用、嵌入资源、
   对外类型和输出内容。

任何原生命令返回非零退出码都会终止作业。

## 构建矩阵

| 构建入口 | 工具 | 目标框架 | 输出前缀 |
| --- | --- | --- | --- |
| `tests/TestAcad2019` | MSBuild | .NET Framework 4.8 | `AC_2019` |
| `tests/TestAcad2025` | dotnet | .NET 8 | `AC_2025` |
| `tests/TestZcad2022` | MSBuild | .NET Framework 4.8 | `ZW_2022` |
| `tests/TestZcad2025` | MSBuild | .NET Framework 4.8 | `ZW_2025` |
| `tests/TestGcad2022` | MSBuild | .NET Framework 4.8 | `GC_2022` |
| `tests/TestGcad2023` | MSBuild | .NET Framework 4.8 | `GC_2023` |
| `tests/TestGcad2026` | dotnet | .NET 8 | `GC_2026` |
| `Fs.Fox.CAD.Diagnostics.AutoCad2019` | MSBuild | .NET Framework 4.8 | `AC_2019` |
| `Fs.Fox.CAD.Diagnostics.AutoCad2025` | dotnet | .NET 8 | `AC_2025` |

MSBuild 测试项目由工作流显式覆盖 `OutputPath`。两个诊断项目保留各自
项目文件中的版本化输出设置，因此与对应主类库共用 `Build/AC_*`，但不存在
项目或程序集依赖关系。诊断工具详情见
[CadDiagnostics 组件说明](../../tools/CadDiagnostics/README.md)。

## 受控基线和诊断检查

`Build/CADSharedModuleBaseline.json` 与
`Build/CADSharedCompatibilityBaseline.json` 是经评审的契约输入，不是可丢弃
输出。普通构建不会更新它们；只有公共面或模块归属确实变化并经评审时，才在
同一 PR 中使用相应脚本的 `-UpdateBaseline`。

`tools/CadDiagnostics/Verify-CadDiagnostics.ps1` 不保存二进制基线，而是针对
本次 Debug/Release 产物检查：

- 旧 MgdDbg 的 132 个编译源码路径已完整迁移；
- DLL 与 XML 文档位于预期 `Build/AC_*` 目录；
- 诊断程序集不引用 `Fs.Fox.AutoCad`，输出不复制 Autodesk SDK DLL；
- 19 个报表浏览资源均嵌入 DLL，且不嵌入 `Thumbs.db`；
- 顶层公开类型只有 AutoCAD 所需的 App/Command 类；
- 不产生 Bundle 或构建期报表资源目录。

这些检查只证明确定性的源码、构建和程序集边界，不代表 WinForms、命令、
反应器、数据库写操作或 native API 已在 AutoCAD 中运行通过。

## 执行器要求

- Windows x64；
- Visual Studio MSBuild 和 .NET Framework 4.8 Developer Pack；
- 支持仓库目标的 .NET SDK；
- PowerShell 7；
- 能访问项目声明的 NuGet 源及 ZWCAD 2022 所需仓库外 SDK 路径。

本地排查时可先确认：

```powershell
msbuild -version
dotnet --list-sdks
$PSVersionTable.PSVersion
git --version
```

## 与 Release 工作流的关系

日常构建工作流验证解决方案和工具；`.github/workflows/release.yml` 只负责当前
公开 NuGet 目标。CadDiagnostics、测试程序集和 Build-only 的 GstarCAD 目标
不会因加入本工作流而自动成为发布包。
