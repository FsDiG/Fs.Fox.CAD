# AC_2027 .NET 8 兼容策略记录

## 结论

`Fs.Fox.AutoCad2027` 当前按 `net8.0-windows7.0` 构建和打包，不按 `net10.0-windows` 构建。

ObjectARX 2027 SDK 的托管 DLL 本身是 `.NETCoreApp,Version=v10.0`，但本仓库的 AutoCAD 2027 封装暂时遵循上层产品链路，统一使用 .NET 8。

## 决策原因

- 上层 LightningCAD 的 `AC_2027` 插件链统一按 .NET 8 构建。
- 当前保护/混淆工具只支持 .NET 8。
- C# `net8.0-windows7.0` 项目直接引用 ObjectARX2027 net10 托管 DLL 会触发 `CS1705`。
- `Fs.Fox.AutoCad2027` 是上层 CAD C# 链路的一部分，必须与 Main、Launcher、Common、Kit、AecBase 保持同一 TFM。

## 当前实现

`src/Fs.Fox.AutoCad2027/Fs.Fox.AutoCad2027.csproj` 中保留：

```xml
<TargetFrameworks>net8.0-windows7.0</TargetFrameworks>
<ACADVersion>2027</ACADVersion>
<AutoCADManagedSdkVersion>2027</AutoCADManagedSdkVersion>
<AutoCADManagedReferenceSdkVersion>2025</AutoCADManagedReferenceSdkVersion>
```

含义：

- `ACADVersion=2027` 和 `AutoCADManagedSdkVersion=2027` 保持包名、输出目录、条件编译和平台语义。
- `AutoCADManagedReferenceSdkVersion=2025` 只影响 C# 编译期 AutoCAD managed reference 路径。
- NuGet `PackagePath` 使用 `lib/net8.0-windows7.0/`。

## 风险和约束

- 这是兼容策略，不是 ObjectARX 2027 SDK 原生 net10 对齐方案。
- 不能在 `Fs.Fox.AutoCad2027` 内直接使用只有 ObjectARX2027 managed DLL 才暴露的 API，除非先重新评估 net10 迁移或做隔离适配。
- 不要单独把本项目改成 `net10.0-windows`；它会导致上层 net8 项目重新遇到 `CS1705` 或保护流程无法处理。

## 后续回迁条件

当保护/混淆工具支持 .NET 10，且上层产品链路确认整体迁移后，再评估 `Fs.Fox.AutoCad2027` 是否回迁到 `net10.0-windows` 并改回 ObjectARX2027 managed references。

