# Fs.Fox.CAD 维护说明

本文保留原“Fs 分支说明”的稳定入口。当前项目已由早期分支形态演进为独立维护的 [Fs.Fox.CAD](README.md) 仓库。

## 维护目标

- 在生产插件中提供可控、可追溯的 CAD 基础类库版本。
- 按 CAD 厂商和 API 代际拆分构建产物，降低程序集误用和版本冲突风险。
- 共享核心源码，并在确有价值且兼容的情况下向上游 IFoxCAD 反馈改进。
- 将自动构建与真实 CAD 宿主验收分开记录，不用编译成功代替运行时结论。

## 命名约定

- 仓库和类库名称：`Fs.Fox.CAD`。
- 公共命名空间：`Fs.Fox.Cad`、`Fs.Fox.Basal`。
- AutoCAD 程序集：`Fs.Fox.AutoCad.dll`。
- ZWCAD 程序集：`Fs.Fox.ZwCad.dll`。
- NuGet 包 ID：因兼容历史消费者继续使用 `IFox.CAD.*`。

支持版本、安装方式和文档导航见 [README](README.md)；上游历史与维护关系见 [上游 IFoxCAD 与 Fs.Fox.CAD 的关系](<IFoxCAD 说明.md>)。

## 远程仓库

本仓库的主远程地址为：

```text
https://github.com/FsDiG/Fs.Fox.CAD.git
```

如需向 IFoxCAD 上游贡献，请将上游仓库配置为单独命名的 remote（例如 `upstream`），不要为同一个 remote 配置两个推送目标，以免误推：

```powershell
git remote add upstream https://gitee.com/inspirefunction/ifoxcad.git
```
