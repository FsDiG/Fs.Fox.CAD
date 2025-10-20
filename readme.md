# IFox.CAD - CAD 二次开发基础类库

基于 .NET 的 CAD 二次开发基础类库，源于 IFoxCAD，为 Fs.Fox 团队维护版本。

## 支持的平台

本系列库支持多个 CAD 平台和版本：

- **IFox.CAD.ACAD2019** - AutoCAD 2019 (.NET Framework 4.8)
- **IFox.CAD.ACAD2025** - AutoCAD 2025 (.NET 8.0)
- **IFox.CAD.ZCAD2022** - 中望CAD 2022 (.NET Framework 4.8)
- **IFox.CAD.ZCAD2025** - 中望CAD 2025 (.NET Framework 4.8)

## 安装

使用 NuGet 包管理器安装对应版本：

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

或在 Package Manager Console 中：

```powershell
Install-Package IFox.CAD.ACAD2019
Install-Package IFox.CAD.ACAD2025
Install-Package IFox.CAD.ZCAD2022
Install-Package IFox.CAD.ZCAD2025
```

## 快速开始

```csharp
using Fs.Fox.CAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

// 使用 IFox.CAD 提供的扩展方法和工具类
public void ExampleCommand()
{
    var doc = Application.DocumentManager.MdiActiveDocument;
    var db = doc.Database;
    var ed = doc.Editor;
    
    // 使用库提供的功能
    // ...
}
```

## 特性

- 🚀 简化 CAD 二次开发流程
- 📦 提供常用的扩展方法和工具类
- 🔧 支持多个 CAD 版本
- 📖 完善的中文文档
- 🎯 面向 .NET Framework 和 .NET 8

## 文档

详细文档请访问：
- GitHub: https://github.com/FsDiG/Fs.Fox.CAD
- 编译说明: 请参考仓库中的 `编译说明.md`

## 源项目

本项目基于 IFoxCAD 开发：
- 官方地址: https://gitee.com/inspirefunction/ifoxcad

## 许可证

详见 LICENSE 文件

## 贡献

欢迎提交 Issue 和 Pull Request！
