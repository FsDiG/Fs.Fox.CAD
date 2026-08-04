# Fs.Fox.CAD

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/FsDiG/Fs.Fox.CAD/blob/main/LICENSE)
[![AutoCAD 2019](https://img.shields.io/nuget/v/IFox.CAD.ACAD2019.svg?label=AutoCAD%202019)](https://www.nuget.org/packages/IFox.CAD.ACAD2019/)
[![AutoCAD 2025](https://img.shields.io/nuget/v/IFox.CAD.ACAD2025.svg?label=AutoCAD%202025)](https://www.nuget.org/packages/IFox.CAD.ACAD2025/)
[![ZWCAD 2021-2024](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2022.svg?label=ZWCAD%202021--2024)](https://www.nuget.org/packages/IFox.CAD.ZCAD2022/)
[![ZWCAD 2025-2026](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2025.svg?label=ZWCAD%202025--2026)](https://www.nuget.org/packages/IFox.CAD.ZCAD2025/)

Fs.Fox.CAD 是面向 Windows x64 的 .NET CAD 二次开发基础类库。它在 AutoCAD 和 ZWCAD 托管 API 之上提供轻量的事务封装、符号表访问、结果数据模型、选择过滤器及常用扩展方法，用于减少插件项目中的重复代码。

项目源于 [IFoxCAD](https://gitee.com/inspirefunction/ifoxcad)，由 Fs 团队独立维护。NuGet 包 ID 为兼容既有使用方式继续保留 `IFox.CAD.*`，公共命名空间为 `Fs.Fox.Cad`。

## 设计定位

- 以 `DBTrans` 为事务入口，集中访问当前文档、数据库、编辑器、符号表和命名字典。
- 通过扩展方法补充实体、几何、选择集、块、图层、扩展数据和 XRecord 等常用操作。
- 共享一套 `CADShared` 源码，并由不同平台项目绑定 Autodesk 或 ZwSoft 的托管程序集。
- 保留宿主原生对象模型和类型语义，不试图用一套自定义类型替代厂商 SDK。

Fs.Fox.CAD 不是 CAD SDK 的替代品，也不是可同时加载到所有宿主的单一二进制文件。插件项目仍需选择与目标宿主/API 代际匹配的包，并引用对应厂商命名空间。

## 支持矩阵

当前解决方案和发布工作流构建以下七个 NuGet 包：

| NuGet 包 | 目标宿主/API 代际 | 目标框架 | 输出程序集 |
| --- | --- | --- | --- |
| [`IFox.CAD.ACAD2019`](https://www.nuget.org/packages/IFox.CAD.ACAD2019/) | AutoCAD 2019 | .NET Framework 4.8 | `Fs.Fox.AutoCad.dll` |
| [`IFox.CAD.ACAD2025`](https://www.nuget.org/packages/IFox.CAD.ACAD2025/) | AutoCAD 2025 | `net8.0-windows7.0` | `Fs.Fox.AutoCad.dll` |
| [`IFox.CAD.ZCAD2022`](https://www.nuget.org/packages/IFox.CAD.ZCAD2022/) | ZWCAD 2021-2024 | .NET Framework 4.8 | `Fs.Fox.ZwCad.dll` |
| [`IFox.CAD.ZCAD2025`](https://www.nuget.org/packages/IFox.CAD.ZCAD2025/) | ZWCAD 2025-2026 | .NET Framework 4.8 | `Fs.Fox.ZwCad.dll` |
| [`IFox.CAD.GCAD2022`](https://www.nuget.org/packages/IFox.CAD.GCAD2022/) | GstarCAD 2022 | .NET Framework 4.8 | `Fs.Fox.Gcad.dll` |
| [`IFox.CAD.GCAD2023`](https://www.nuget.org/packages/IFox.CAD.GCAD2023/) | GstarCAD 2023 | .NET Framework 4.8 | `Fs.Fox.Gcad.dll` |
| [`IFox.CAD.GCAD2026`](https://www.nuget.org/packages/IFox.CAD.GCAD2026/) | GstarCAD 2026 | `net8.0-windows7.0` | `Fs.Fox.Gcad.dll` |

以上目标均按 x64 构建。表中的 ZWCAD 范围表示厂商文档给出的二进制兼容代际；自动构建成功不等同于每个 CAD 宿主版本都已完成运行时验收。ZWCAD 2026 复用 2025 产物，依据、限制和待完成的宿主检查见 [ZWCAD 版本兼容性与迁移说明][zwcad-compatibility]。

仓库还包含 `Fs.Fox.AutoCad2021` 和 `Fs.Fox.AutoCad2027` 项目，但它们未纳入当前 `IFoxCAD.sln` 和 NuGet 发布工作流，不应据此推断为公开发布目标。AutoCAD 2027 当前的内部兼容策略见 [AC_2027 .NET 8 兼容策略记录][acad-2027-decision]。

## 选择和安装

一个插件项目只应引用一个目标平台包。需要支持多个宿主或 API 代际时，应建立独立的宿主项目并共享业务源码，避免在同一项目中同时引用多个 `IFox.CAD.*` 包。

例如，面向 AutoCAD 2025：

```powershell
dotnet add .\YourPlugin.csproj package IFox.CAD.ACAD2025
```

面向 ZWCAD 2025 或 2026：

```powershell
dotnet add .\YourPlugin.csproj package IFox.CAD.ZCAD2025
```

如需安装预发布版本，请在命令后增加 `--prerelease`，或在项目文件中明确指定版本。其他包名见上方支持矩阵。

## 快速开始

以下 AutoCAD 示例使用 `DBTrans` 和 `AddEntity` 扩展方法在当前空间创建一条直线：

```csharp
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Fs.Fox.Cad;

public sealed class FoxCommands
{
    [CommandMethod("FOX_LINE")]
    public static void CreateLine()
    {
        using DBTrans tr = new();
        var line = new Line(
            new Point3d(0, 0, 0),
            new Point3d(100, 100, 0));

        tr.CurrentSpace.AddEntity(line);
    }
}
```

`DBTrans` 默认在释放时提交事务。需要放弃修改时，应显式调用 `Abort()`，或在创建事务时传入 `commit: false`。

ZWCAD 项目使用相同的 `Fs.Fox.Cad` API；将示例中的 Autodesk 命名空间替换为对应的 `ZwSoft.ZwCAD` 命名空间即可。编译插件后，在目标 CAD 中使用 `NETLOAD` 加载插件程序集，而不是直接加载 NuGet 包。

## 项目结构

```text
src/
  CADShared/                 跨平台共享实现
  IFoxCAD.AutoCad/           AutoCAD 平台 using、别名和构建辅助文件
  IFoxCAD.ZwCad/             ZWCAD 平台 using 和别名
  Fs.Fox.AutoCad20xx/        AutoCAD 各 API 代际项目
  Fs.Fox.ZwCad20xx/          ZWCAD 各 API 代际项目
  Fs.Fox.Gcad20xx/           GstarCAD 各 API 代际项目
tests/
  TestShared/                共享的 CAD 命令与宿主测试代码
  TestAcad20xx/              AutoCAD 测试入口
  TestZcad20xx/              ZWCAD 测试入口
tools/
  CadDiagnostics/            独立、多版本 CAD 诊断工具
third_party/
  Autodesk.MgdDbg/           原始 MgdDbg 导入快照（不参与构建）
```

平台项目导入相同的 `CADShared.projitems`，因此公共功能尽量保持一致；底层对象仍分别来自 Autodesk 和 ZwSoft 程序集。更详细的设计说明见 [Fs.Fox.CAD 架构说明][architecture]。

## 构建与验证

本地开发和 CI 的目标、工具链、条件编译符号及输出目录见 [构建说明][building]。当前四个发布目标的产物统一输出到 `Build\<平台>_<版本>_<配置>\`。

测试项目主要用于编译覆盖和 CAD 宿主内的命令验证。构建通过只能证明源码与编译期引用兼容；发布前仍需在目标 CAD 中验证程序集解析、`NETLOAD`、命令注册、数据库操作和实际使用的界面入口。

## 文档

- [文档索引与阅读顺序][documentation-index]
- [文档与代码协同治理方案][documentation-architecture]
- [构建与项目结构][building]
- [架构与核心抽象][architecture]
- [DBTrans 生命周期与释放契约][dbtrans-lifecycle]
- [ZWCAD 版本兼容性与迁移说明][zwcad-compatibility]
- [AutoCAD 2027 .NET 8 兼容策略][acad-2027-decision]
- [AutoCAD 多版本诊断工具][cad-diagnostics]
- [CAD 界面文字规范][cad-ui-text-style]
- [上游 IFoxCAD 与本项目的关系][upstream]
- [NuGet 发布工作流][release-workflow]

问题和改进建议请提交到 [GitHub Issues](https://github.com/FsDiG/Fs.Fox.CAD/issues)。

## 许可证

本项目采用 [MIT License][license]。原项目作者及贡献者信息见 [上游说明][upstream]。

[documentation-index]: docs/README.md
[documentation-architecture]: docs/documentation-architecture.md
[building]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/%E7%BC%96%E8%AF%91%E8%AF%B4%E6%98%8E.md
[architecture]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/docs/%E5%85%B3%E4%BA%8EIFoxCAD%E7%9A%84%E6%9E%B6%E6%9E%84%E8%AF%B4%E6%98%8E.md
[dbtrans-lifecycle]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/docs/dbtrans-lifecycle-contract.md
[zwcad-compatibility]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/docs/ZWCAD-version-compatibility.md
[acad-2027-decision]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/docs/AC_2027-net8-compatibility-decision.md
[cad-diagnostics]: tools/CadDiagnostics/README.md
[cad-ui-text-style]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/docs/guides/cad-ui-text-style-guide.md
[upstream]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/IFoxCAD%20%E8%AF%B4%E6%98%8E.md
[release-workflow]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/.github/workflows/release.yml
[license]: https://github.com/FsDiG/Fs.Fox.CAD/blob/main/LICENSE
