# 上游 IFoxCAD 与 Fs.Fox.CAD 的关系

## 项目来源

Fs.Fox.CAD 源于开源项目 [IFoxCAD](https://gitee.com/inspirefunction/ifoxcad)。IFoxCAD 又是在 NFox 思路基础上的重构：雪山飞狐最早公开相关类库，落魄山人在获得授权后整理、补充注释并继续演进，后续以 IFoxCAD 名称发布于 Inspire Function（跃动方程）组织。

本仓库保留对原作者、整理者和历次贡献者的感谢。更完整的历史、教程和上游 API 文档应以 IFoxCAD 项目当前发布的信息为准：

- [IFoxCAD 源码仓库](https://gitee.com/inspirefunction/ifoxcad)
- [IFoxCAD 类库从入门到精通](https://www.kdocs.cn/l/cc6ZXSa0vMgD)
- [IFoxCAD API 文档](https://inspirefunction.github.io/ifoxdoc/)

外部资料可能对应不同 IFoxCAD 分支或版本。使用示例前应核对命名空间、目标框架和 CAD SDK 代际，不应默认其可直接用于当前 Fs.Fox.CAD 包。

## 当前维护边界

Fs.Fox.CAD 是独立维护的派生项目，不是 IFoxCAD 的官方镜像或同步发布渠道。当前仓库的主要差异包括：

- 公共命名空间使用 `Fs.Fox.Cad` 和 `Fs.Fox.Basal`。
- 程序集按 AutoCAD/ZWCAD 平台及 API 代际分别构建。
- NuGet 包 ID 为兼容既有消费者继续保留 `IFox.CAD.*`。
- 版本节奏、支持矩阵、构建流程和宿主验收由 Fs 团队独立维护。

本仓库中的变更不保证已被上游采纳，上游的新功能或修复也不会自动同步到本仓库。当前支持范围以根目录 [README](README.md) 和对应版本兼容性文档为准。

## 问题与贡献

- Fs.Fox.CAD 的构建、包、兼容性或代码问题，请提交到 [Fs.Fox.CAD Issues](https://github.com/FsDiG/Fs.Fox.CAD/issues)。
- 仅能在原版 IFoxCAD 重现的问题，或希望贡献给上游的改进，请使用 [IFoxCAD Issues](https://gitee.com/inspirefunction/ifoxcad/issues)。
- 提交修复时应说明目标 CAD、目标框架、复现方式和是否完成真实宿主验证。

本仓库采用 [MIT License](LICENSE)，复用或分发时请保留许可证与原作者归属。
