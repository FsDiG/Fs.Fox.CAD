# Fs.Fox.CAD 文档索引

> 状态：当前入口（Current）<br>
> 治理方案：[文档与代码协同治理方案](documentation-architecture.md)<br>
> 跟踪：[Issue #48](https://github.com/FsDiG/Fs.Fox.CAD/issues/48)<br>
> 适用范围：仓库手写 Markdown 的权威状态、受众、阅读顺序和排除规则

本页是 Fs.Fox.CAD 文档的阅读入口，不复制代码事实或长篇技术结论。公共 API、项目目标、工作流和运行时行为发生变化时，仍应先核对实时源码、项目文件、XML 注释、测试和 GitHub 状态。

## 1. 权威顺序

遇到信息冲突时，按以下顺序处理：

1. 实时源码、项目文件、工作流、测试和真实 CAD 宿主证据；
2. 本页标记为 `current` 的文档；
3. 与当前任务直接相关的 active plan；
4. `proposal`、GitHub Issue 和 PR 讨论；
5. `superseded`、`historical` 及专项证据快照。

`current` 表示当前维护依据，不表示可以跳过源码核对。active plan 只约束其声明的实施范围，不自动改变产品行为。Issue 关闭只证明对应工作项已收口，不能自动把提案升级为已实施契约。

## 2. 状态含义

| 状态 | 含义 | 使用规则 |
| --- | --- | --- |
| `current` | 当前有效的事实、约定或已接受决策 | 可作为维护依据，仍需核对实时实现。 |
| `active` | 已批准并正在实施的限时计划 | 只约束声明的分支和范围；完成或取消后转为 `historical`。 |
| `draft` | 尚未完成或尚未复核 | 不作为覆盖范围或行为清单。 |
| `proposal` | 已记录但尚未全部批准或实施 | 只能作为讨论输入，不能推断现状。 |
| `superseded` | 已由另一份文档取代 | 只追溯决策背景，实施时转到取代文档。 |
| `historical` | 已完成、取消或仅保留证据 | 只解释特定时间点，不指导新实现。 |

front matter schema 和自动校验尚未落地。在迁移完成前，本页状态与文件顶部的显式状态共同生效；两者不一致时采用更保守的状态并提交修正。

## 3. 按任务阅读

| 任务 | 先读 | 注意 |
| --- | --- | --- |
| 选择包、安装或编写第一个命令 | [产品 README](../README.md)、[ZWCAD 版本兼容性](ZWCAD-version-compatibility.md) | 按宿主和 API 代际选择包；构建兼容不等于宿主通过。 |
| 修改项目、构建或发布 | [构建与项目结构](../编译说明.md)、[`build-and-deploy.yml`](../.github/workflows/build-and-deploy.yml)、[`release.yml`](../.github/workflows/release.yml) | 以实时 `.csproj` 和 `.yml` 为最终事实，不维护重复的工作流说明。 |
| 理解整体架构 | [架构说明](关于IFoxCAD的架构说明.md)、[文档治理方案](documentation-architecture.md) | 前者描述产品，后者描述文档。 |
| 移动或重组 `CADShared` | [架构说明](关于IFoxCAD的架构说明.md)、根 [`AGENTS.md`](../AGENTS.md)、[`CADShared.projitems`](../src/CADShared/CADShared.projitems)、[`CADSharedModuleBaseline.json`](../Build/CADSharedModuleBaseline.json) | 从最新 `main` 建立短期分支；历史设计与实施过程从 Issue #25 和 Git 历史追溯。 |
| 修改 `DBTrans` | 实时 [`DBTrans.cs`](../src/CADShared/Cad/Database/Transactions/DBTrans.cs)、[生命周期设计提案](dbtrans-lifecycle-contract.md) | 文档中的 Confirmed 可作证据；Decision 仍需独立实现和验证。 |
| 修改宿主、SDK 或目标框架 | [ZWCAD 兼容性](ZWCAD-version-compatibility.md)、[AutoCAD 2027 决策](AC_2027-net8-compatibility-decision.md)、[构建说明](../编译说明.md) | 检查公开发布目标与仓库中的实验/预备项目差异。 |
| 构建或验收独立 CAD 诊断工具 | [CadDiagnostics 组件说明](../tools/CadDiagnostics/README.md)、[构建说明](../编译说明.md)、[Issue #124](https://github.com/FsDiG/Fs.Fox.CAD/issues/124) | 诊断 DLL 不依赖主类库；分别记录 SDK 基线和实际宿主，未执行功能保持 `Not run`。 |
| 修改 CAD 命令行或 UI 文案 | [CAD/UI 文案风格指南](guides/cad-ui-text-style-guide.md) | 只机械修正低风险格式；流程和业务含义需要单独评审。 |
| 执行或追溯真实 CAD 宿主验收 | [CAD 真实宿主验收 Runner](../tools/HostAcceptance/README.md)、[Issue #40](https://github.com/FsDiG/Fs.Fox.CAD/issues/40)及对应 Issue/PR | 分别核对编译目标、测试程序集、实际宿主、提交和场景；`Build/HostAcceptance` 中的历史快照只在目标 Git 状态确实包含时读取。 |
| 新增或调整文档 | [文档治理方案](documentation-architecture.md)、根 [`AGENTS.md`](../AGENTS.md) | 先维护事实源、状态和关联，不提前选择站点框架。 |
| 规划站点仓库或 EdgeOne 发布 | [文档治理方案](documentation-architecture.md)、[EdgeOne 站点仓库评估](edgeone-site-repository-evaluation.md)、[Issue #48](https://github.com/FsDiG/Fs.Fox.CAD/issues/48) | 产品内容仍只在本仓库维护；展示仓库、来源锁和生成产物是不同边界。 |

## 4. 当前文档

| 稳定 ID | 状态 | 文档 | 受众 | 摘要 |
| --- | --- | --- | --- | --- |
| `entry.product` | `current` | [产品 README](../README.md) | user | 产品定位、支持矩阵、安装和最小示例。 |
| `entry.documentation` | `current` | [文档索引](README.md) | maintainer | 文档状态、受众、任务阅读顺序和排除规则。 |
| `governance.agent-routing` | `current` | [仓库协作规则](../AGENTS.md) | maintainer | 编码代理与维护者的任务路由、证据和分支边界。 |
| `architecture.overview` | `current` | [Fs.Fox.CAD 架构说明](关于IFoxCAD的架构说明.md) | user, maintainer | 当前共享源码、宿主边界和核心抽象。 |
| `guide.building` | `current` | [构建与项目结构](../编译说明.md) | maintainer | 正式项目、工具链、条件编译、输出和验证边界。 |
| `guide.host-acceptance` | `current` | [CAD 真实宿主验收 Runner](../tools/HostAcceptance/README.md) | maintainer | 真实 CAD 验收的目标矩阵、证据契约、安全边界、当前限制和执行入口。 |
| `guide.cad-diagnostics` | `current` | [CadDiagnostics 多版本诊断工具](../tools/CadDiagnostics/README.md) | maintainer | 独立依赖边界、AutoCAD 构建矩阵、NETLOAD、命令兼容与宿主验证状态。 |
| `reference.zwcad-compatibility` | `current` | [ZWCAD 版本兼容性与迁移说明](ZWCAD-version-compatibility.md) | user, maintainer | ZRXSDK 代际、当前发布策略、Build-only 边界和未验证状态。 |
| `reference.gstarcad-support-design` | `current` | [GStarCAD 支持边界](关于IFoxCAD的架构说明.md#gstarcad浩辰cad) | maintainer | 浩辰 CAD 2022/2023/2026 的共享源码和条件编译边界。 |
| `decision.autocad-2027-net8` | `current` | [AutoCAD 2027 .NET 8 兼容策略](AC_2027-net8-compatibility-decision.md) | maintainer | 预备项目当前使用 .NET 8 的原因、限制和回迁条件。 |
| `concept.upstream-relationship` | `current` | [上游 IFoxCAD 与 Fs.Fox.CAD 的关系](<../IFoxCAD 说明.md>) | user, maintainer | 项目来源、独立维护边界和问题归属。 |
| `governance.documentation` | `current` | [文档与代码协同治理方案](documentation-architecture.md) | maintainer | 唯一产品内容源、展示/部署仓库边界、公开范围、版本发布和 Vibe Coding 上下文约定。 |
| `guide.cad-ui-text` | `current` | [CAD/UI 文案风格指南](guides/cad-ui-text-style-guide.md) | maintainer | CAD 命令行和桌面 UI 的用户可见文字规则。 |

## 5. Active Plan 与提案

| 状态 | 稳定 ID | 文档 | 使用边界 |
| --- | --- | --- | --- |
| `proposal` | `contract.dbtrans-lifecycle` | [DBTrans 生命周期与释放契约](dbtrans-lifecycle-contract.md) | 同时记录 Confirmed、Decision 与 Not run；不能把后续决定表述为已实施。 |
| `proposal` | `proposal.edgeone-site-repository` | [EdgeOne Makers 站点仓库架构评估](edgeone-site-repository-evaluation.md) | Issue #48 的实施提案；展示仓库与精确来源链路已创建，最终框架、GitHub App、EdgeOne 和云资源仍未完成。 |
| `proposal` | `proposal.cad-diagnostics-open-source-intake` | [CadDiagnostics 开源诊断项目评估](../tools/CadDiagnostics/OPEN-SOURCE-EVALUATION.md) | Issue #124 下的候选、去重依据和阶段边界；P1 已实施，P2 仍需独立 PR。 |

## 6. 历史材料

已完成的一次性计划、迁移分析和能力筛选不再作为工作区 Markdown 保留。需要审计时从 Git 历史及对应 Issue/PR 追溯；仍有效的结论必须进入当前源码、XML 注释或本索引列出的现行文档。

## 7. 专项 Markdown 与排除规则

以下文件保留在其所有者目录，不进入产品文档导航：

| 路径 | 状态/用途 | 默认处理 |
| --- | --- | --- |
| `.github/ISSUE_TEMPLATE.zh-CN.md`、`.github/PULL_REQUEST_TEMPLATE.zh-CN.md` | GitHub 操作模板 | 由 GitHub UI 使用，不作为技术文档。 |
| 未来可能进入特定 PR/提交的 `Build/HostAcceptance/*.md` | `historical` 宿主证据快照 | 当前 main 尚无该目录；只在目标 Git 状态确实包含它且需追溯匹配 Issue、PR、提交和宿主版本时读取，不发布。 |
| `third_party/Autodesk.MgdDbg/README.md` | `historical` 上游导入快照 | 只追溯原始 MgdDbg；现行构建和使用入口为 [CadDiagnostics 组件说明](../tools/CadDiagnostics/README.md)。 |

生成站点、API 中间文件、搜索索引、DLL/XML 副本及其他可丢弃的构建输出不属于手写 Markdown，不得进入本仓库或展示仓库的 Git 历史，也不得进入编码代理默认上下文。仓库只保留用于校验共享源码模块归属和顺序的 `Build/CADSharedModuleBaseline.json`；不维护二进制或 NuGet 包内容快照。

## 8. 维护本索引

- 新增手写 Markdown 时，在同一 PR 中加入本索引，或由第 7 节的路径规则明确排除。
- 状态变化时，同时修改源文件顶部状态、本索引和关联 Issue；不要仅在 Issue 评论中宣布结论。
- 文件移动时保持稳定 ID，修复相对链接和 Issue 链接；公开页面后续还需维护重定向。
- 当前文档与实时实现冲突时，以实现为事实并修正文档；若实现本身可能错误，另开 Issue，不用文档掩盖差异。
- active/archive plans 不进入公共站点或精选 `llms.txt`；只有提炼后的 current contract、guide 或 ADR 才进入公开导航。
