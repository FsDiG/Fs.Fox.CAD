# Fs.Fox.CAD

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![AutoCAD 2019](https://img.shields.io/nuget/v/IFox.CAD.ACAD2019.svg?label=AutoCAD%202019)](https://www.nuget.org/packages/IFox.CAD.ACAD2019/)
[![AutoCAD 2025](https://img.shields.io/nuget/v/IFox.CAD.ACAD2025.svg?label=AutoCAD%202025)](https://www.nuget.org/packages/IFox.CAD.ACAD2025/)
[![中望CAD 2022](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2022.svg?label=中望CAD%202022)](https://www.nuget.org/packages/IFox.CAD.ZCAD2022/)
[![中望CAD 2025](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2025.svg?label=中望CAD%202025)](https://www.nuget.org/packages/IFox.CAD.ZCAD2025/)

## 项目简介

Fs.Fox.CAD 是基于 .NET 的 CAD 二次开发基础类库，源于 [IFoxCAD](https://gitee.com/inspirefunction/ifoxcad) 项目。

本项目在 IFoxCAD 的基础上，将命名空间改为 Fs.Fox，作为 Fs 团队 AutoCAD 开发的基础库之一。我们致力于提供稳定、易用的 CAD 二次开发解决方案，支持 AutoCAD 和中望CAD 多个版本。

> AutoCAD 2027 封装当前按 `net8.0-windows7.0` 构建，并用 ObjectARX2025 的 managed reference 避免 C# 编译期 `CS1705`；平台语义、包名和输出目录仍保持 2027。详见 [docs/AC_2027-net8-compatibility-decision.md](docs/AC_2027-net8-compatibility-decision.md)。

### 主要特性

- 🚀 **多版本支持**: 支持 AutoCAD 2019/2025/2027 和中望CAD 2022/2025
- 📦 **NuGet 发布**: 通过 NuGet 轻松集成到您的项目
- 🔧 **丰富的 API**: 提供完整的 CAD 二次开发 API 封装
- 📝 **详细文档**: 包含完整的使用文档和示例代码
- ⚡ **持续更新**: 定期更新和维护，快速响应问题

### 官方资源

- **官方地址**: [IFoxCAD: 基于.NET的Cad二次开发类库](https://gitee.com/inspirefunction/ifoxcad)
- **详细说明**: 参考本仓库的 [IFoxCAD 说明.md](./IFoxCAD%20说明.md)
- **在线导图**: [.NET ARX_Fox 思维导图](https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2)

## NuGet 包

本项目提供以下 NuGet 包，支持通过 GitHub Actions 自动发布。当推送版本标签时，将自动构建并发布到 NuGet.org。

| 包名称 | 版本 | 支持平台 | .NET 框架 | 下载链接 |
|--------|------|----------|-----------|----------|
| **IFox.CAD.ACAD2019** | [![NuGet](https://img.shields.io/nuget/v/IFox.CAD.ACAD2019.svg)](https://www.nuget.org/packages/IFox.CAD.ACAD2019/) | AutoCAD 2019 | .NET Framework 4.8 | [下载](https://www.nuget.org/packages/IFox.CAD.ACAD2019/) |
| **IFox.CAD.ACAD2025** | [![NuGet](https://img.shields.io/nuget/v/IFox.CAD.ACAD2025.svg)](https://www.nuget.org/packages/IFox.CAD.ACAD2025/) | AutoCAD 2025 | .NET 8.0 | [下载](https://www.nuget.org/packages/IFox.CAD.ACAD2025/) |
| **IFox.CAD.ACAD2027** | 当前内部构建 | AutoCAD 2027 | .NET 8.0 | 当前随产品链路构建 |
| **IFox.CAD.ZCAD2022** | [![NuGet](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2022.svg)](https://www.nuget.org/packages/IFox.CAD.ZCAD2022/) | 中望CAD 2022 | .NET Framework 4.8 | [下载](https://www.nuget.org/packages/IFox.CAD.ZCAD2022/) |
| **IFox.CAD.ZCAD2025** | [![NuGet](https://img.shields.io/nuget/v/IFox.CAD.ZCAD2025.svg)](https://www.nuget.org/packages/IFox.CAD.ZCAD2025/) | 中望CAD 2025 | .NET Framework 4.8 | [下载](https://www.nuget.org/packages/IFox.CAD.ZCAD2025/) |

### 安装方式

#### 使用 NuGet 包管理器

```bash
# AutoCAD 2019
Install-Package IFox.CAD.ACAD2019

# AutoCAD 2025
Install-Package IFox.CAD.ACAD2025

# 中望CAD 2022
Install-Package IFox.CAD.ZCAD2022

# 中望CAD 2025
Install-Package IFox.CAD.ZCAD2025
```

#### 使用 .NET CLI

```bash
# AutoCAD 2019
dotnet add package IFox.CAD.ACAD2019

# AutoCAD 2025
dotnet add package IFox.CAD.ACAD2025

# 中望CAD 2022
dotnet add package IFox.CAD.ZCAD2022

# 中望CAD 2025
dotnet add package IFox.CAD.ZCAD2025
```

## 快速开始

### 基本使用示例

```csharp
using Fs.Fox.CAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

// 创建直线
[CommandMethod("CreateLine")]
public void CreateLine()
{
    var doc = Application.DocumentManager.MdiActiveDocument;
    var db = doc.Database;
    var ed = doc.Editor;
    
    using (var tr = db.TransactionManager.StartTransaction())
    {
        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        
        var line = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
        btr.AppendEntity(line);
        tr.AddNewlyCreatedDBObject(line, true);
        
        tr.Commit();
    }
}
```

### 发布新版本

```bash
# 创建版本标签
git tag v1.0.0

# 推送标签到远程仓库
git push origin v1.0.0
```

详细说明请参考：[发布工作流程文档](.github/workflows/release.md)

## Fs.Fox 分支说明

### 项目背景

本分支在 IFoxCAD 的基础上进行了以下改进：

- **命名空间调整**: 将命名空间改为 `Fs.Fox`，便于 Fs 团队在生产环境中使用
- **版本管理**: 避免 DLL 版本冲突，提供更灵活的维护和开发
- **稳定性优先**: 保持 .NET Framework 4.8 支持，确保生产环境稳定性
- **快速响应**: 更快速的问题响应和功能更新

### 为什么选择 Fs.Fox？

1. **生产就绪**: 专为生产环境设计，稳定可靠
2. **版本兼容**: 避免 DLL 版本冲突，支持多版本共存
3. **灵活维护**: 独立维护，快速响应需求变化
4. **持续更新**: 定期更新，紧跟 CAD 平台最新版本

## 开发路线

### 当前版本

- ✅ 支持 AutoCAD 2019/2025
- ✅ 支持中望CAD 2022/2025
- ✅ GitHub Actions 自动化发布
- ✅ NuGet 包发布

## 许可证

本项目采用 [MIT License](LICENSE) 开源协议。
