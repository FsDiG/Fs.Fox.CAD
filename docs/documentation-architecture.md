# Fs.Fox.CAD 文档与代码协同治理方案

> 状态：已确认方案（Accepted）<br>
> 基线：`main` @ `416c65f`，2026-08-01<br>
> 跟踪：[Issue #48](https://github.com/FsDiG/Fs.Fox.CAD/issues/48)；关联重构：[Issue #25](https://github.com/FsDiG/Fs.Fox.CAD/issues/25)<br>
> 适用范围：`Fs.Fox.CAD` 单产品仓库、API 注释、示例、维护者文档和公开静态站点<br>
> 参考项目：[FastAPI](https://github.com/fastapi/fastapi)、[uv](https://github.com/astral-sh/uv)、[Aider](https://github.com/Aider-AI/aider)、[Continue](https://github.com/continuedev/continue)、[Codex](https://github.com/openai/codex)、[ifoxdoc](https://github.com/InspireFunction/ifoxdoc)

## 1. 决策摘要

Fs.Fox.CAD 是一个面向多种 CAD 宿主的单产品通用基础类库。文档治理采用“**一个可编辑事实源、按代码版本生成、发布产物不可手工修改**”的模式。

### 1.1 已确认的三个边界

| 边界 | 已确认决定 | 当前不做 |
| --- | --- | --- |
| 事实源与仓库边界 | `Fs.Fox.CAD` 是代码、XML API 注释、手写 Markdown、示例和内容级元数据的唯一可编辑事实源；代码与相关文档在同一个 PR 中评审和合入。后续 `Fs.Fox.CAD.Site` 只承载前端展示、构建适配和部署配置。 | 不建立第二个可人工编辑的产品内容仓库，也不建立只存放生成结果的 `Fs.Fox.CAD.Docs`。 |
| 内容与公开边界 | 公开站点同时服务类库使用者和维护者，并分为两个导航区；当前架构、生命周期契约和适合公开的 ADR 可以发布。active/archive plans、临时分析和未验证假设不进入公共导航，也不进入精选代理语料。 | 不把所有 Markdown、Issue 评论、AI 对话或开发过程等同于产品文档。 |
| 版本与发布边界 | 首轮只发布 `latest` 和当前 `stable`；站点仓库用来源锁固定源码 commit/tag，EdgeOne Makers 直接连接站点仓库完成构建。 | 不一次性托管全部历史 NuGet 版本，不把 HTML、搜索索引或生成 API 页面提交回任一 Git 仓库。 |

### 1.2 由边界导出的约定

1. API 事实以源码 XML 注释和真实构建产物为准；指南不复制维护公共签名，完整示例以可编译项目为准。
2. Markdown 同时是开发者和编码代理的上下文。仓库必须显式区分现行契约、提案、已取代文档和历史材料，不能把所有 `.md` 当成同等权威语料。
3. 重要决策证据通过 current contract、ADR 或必要的 historical 记录保留；普通阶段计划完成后应提炼结论并归档或删除，不能长期与现行规范并列。
4. 文档站点框架暂不确定。目录、元数据、关联和发布契约不得依赖 DocFX、VitePress、Docusaurus、MkDocs 或其他具体实现。
5. 可编辑的站点实现与可编辑的产品内容是两个边界。主题、组件和部署配置可以独立演进，但页面正文、API 语义、示例和内容级导航只能从 `Fs.Fox.CAD` 的确定提交取得。

独立展示/部署仓库 [FeiSiPub/Fs.Fox.CAD.Site](https://github.com/FeiSiPub/Fs.Fox.CAD.Site) 已创建，当前提供框架无关的精确来源锁、GitHub 同步和 Bootstrap 构建；EdgeOne 尚未连接，最终框架尚未选择。详细能力、权限、API 数据包和 POC 门槛见 [EdgeOne Makers 站点仓库架构评估](edgeone-site-repository-evaluation.md)。当前仍不创建 `Fs.Fox.CAD.Docs` 等生成产物仓库；即使未来创建，它也只能接收自动生成内容，不能成为第二个文档源。

## 2. 为什么采用同仓事实源

### 2.1 与代码原子演进

Fs.Fox.CAD 的文档高度依赖真实代码和宿主矩阵：

- `DBTrans` 文档涉及事务默认语义、文档锁、WorkingDatabase 和释放顺序；
- AutoCAD/ZWCAD 兼容文档依赖项目目标框架、SDK 代际和条件编译；
- API 参考来自 `Fs.Fox.AutoCad.xml` / `Fs.Fox.ZwCad.xml` 和对应程序集；
- 架构与逻辑模块化文档需要跟随源码归属和公共 API 演进；
- 示例只有进入真实构建或测试链路，才能避免长期失真。

将这些内容放入独立可编辑仓库，会迫使一次产品变更跨两个仓库、两个 PR 和两套版本基线完成。失败时无法从单个提交回答“这份文档对应哪一版代码”。同仓使代码、测试、XML 注释、示例和概念说明可以在同一个变更中保持一致。

### 2.2 成熟开源项目的边界取舍

| 项目 | 可编辑文档源 | 代码关联 | 发布边界 | 对本方案的启示 |
| --- | --- | --- | --- | --- |
| [FastAPI](https://github.com/fastapi/fastapi/tree/master/docs) | 与代码同仓 | `docs_src` 示例、文档和构建脚本共同触发严格构建 | 构建 artifact 部署到 Cloudflare Pages | 单产品可以不使用发布仓库。 |
| [Aider](https://github.com/Aider-AI/aider/tree/main/aider/website) | 与代码同仓 | 产品站点与实现同版本 | GitHub Pages workflow artifact | 面向 Vibe Coding 的产品也采用同仓来源。 |
| [Continue](https://github.com/continuedev/continue/tree/main/docs) | 与代码同仓 | `.continue/rules` 约束文档风格和基于 PR 的文档更新 | GitHub Pages workflow artifact | 可让代理规则和文档共同参与仓库维护。 |
| [uv](https://github.com/astral-sh/uv/tree/main/docs) | 与代码同仓 | CI 从 Rust 代码生成 CLI、配置、环境变量等 reference | 生成结果通过 PR 发布到 `astral-sh/docs` | 独立输出仓库用于 Ruff、uv、ty 多产品聚合，不是文档事实源。 |
| [Codex](https://github.com/openai/codex/blob/main/AGENTS.md) | 仓库仅保留代码维护所需文档 | `AGENTS.md` 明确限制一般用户文档进入仓库 | 官方产品文档在外部 | 同仓语料应以相关和权威为目标，而不是越多越好。 |
| [.NET docs](https://github.com/dotnet/docs) / [Kubernetes website](https://github.com/kubernetes/website) | 独立可编辑仓库 | 服务多个代码库、产品区域或大量语言 | 独立内容与本地化流程 | 适合多产品聚合和独立文档组织，不符合当前单产品规模。 |

[ifoxdoc](https://github.com/InspireFunction/ifoxdoc) 可继续作为历史内容参考，但不复制其维护方式。该仓库需要手工复制 DLL/XML，并跟踪大量 `_site` HTML；生成页面还包含 Autodesk、System、Mono、NFox 等依赖类型。新方案必须自动获取同一提交的构建产物、限制 API 命名空间，并把生成目录排除出源码历史和编码代理语料。

## 3. 仓库职责边界

| 位置 | 是否可人工编辑 | 内容 | 约束 |
| --- | --- | --- | --- |
| `Fs.Fox.CAD/src` | 是 | 生产代码和 XML API 注释 | API 事实源；行为和签名变化必须同步注释。 |
| `Fs.Fox.CAD/docs` | 是 | 手写指南、概念、参考、维护者契约、决策和计划 | Markdown 唯一事实源；必须声明状态和受众。 |
| `Fs.Fox.CAD/samples` | 是 | 面向文档的最小可编译示例 | 由 CI 构建；文档不得复制维护另一份完整实现。 |
| `Fs.Fox.CAD/tools/docs` | 是 | 文档校验、API 数据生成和来源发布辅助 | 使用确定版本；失败必须传播非零退出码。 |
| `Fs.Fox.CAD.Site` | 是，但仅限站点实现 | 前端框架、主题、组件、搜索/导航渲染适配和 EdgeOne 配置 | 不保存产品帮助正文；不成为第二个文档事实源。 |
| `Fs.Fox.CAD.Site/config/content-source.json` | 自动化为主 | `latest` / `stable` 的精确源码 commit 和内容摘要 | 构建不得直接跟随浮动 `main`；变更必须可审计。 |
| CI artifact / API 数据包 | 否 | 从正式程序集/XML 生成的 API 模型和来源清单 | 完全可重建；与 source commit 和摘要绑定，不提交回 Git。 |
| EdgeOne Makers 部署 | 否 | HTML、搜索索引、生成 API 和 `llms.txt` | 由站点仓库直接构建；记录 source/site commit 和产品版本。 |
| GitHub Issue | 是 | 工作项、讨论、验收状态 | 不是现行技术契约；最终结论必须回写 Markdown。 |
| 可选生成产物仓库 | 否 | 多产品或多版本静态产物 | 当前不创建；只允许自动化身份写入。 |

任何手写内容都不能从发布站点反向复制回源码仓库。发现线上错误时，在 `Fs.Fox.CAD` 提交修正，再重新生成站点。

## 4. 目标文档结构

```text
Fs.Fox.CAD/
  AGENTS.md
  docs/
    README.md
    getting-started/
    guides/
    concepts/
    reference/
      compatibility/
    maintainers/
      architecture/
      contracts/
      contributing/
    decisions/
    plans/
      active/
      archive/
    assets/
  samples/
  tools/
    docs/               # 源内容校验/API 数据生成工具确定后再建立
```

目录按读者问题组织，不与 `src/CADShared` 的九个逻辑模块一一镜像。源码目录回答“谁拥有代码”，文档目录回答“读者想完成什么、理解什么或维护什么”。模块与文档通过元数据关联，不通过相同目录名绑定。

站点实现不放入此树。`Fs.Fox.CAD.Site` 的建议职责与骨架见 [EdgeOne 站点仓库评估](edgeone-site-repository-evaluation.md)，但框架未确定前不据此引入框架专用目录。

### 4.1 各目录职责

| 目录 | 受众 | 内容 | 公开站点 |
| --- | --- | --- | --- |
| `getting-started` | 使用者 | 安装、选择包、最小插件、首个命令 | 发布 |
| `guides` | 使用者 | 事务、符号表、实体、选择过滤、Jig、UI 等任务指南 | 发布 |
| `concepts` | 使用者/维护者 | 对象模型、事务语义、宿主差异和设计概念 | 发布 |
| `reference` | 使用者/维护者 | 支持矩阵、配置、包、版本兼容和稳定事实 | 发布 |
| `maintainers/architecture` | 维护者 | 当前架构、模块边界和依赖方向 | 发布到维护者区 |
| `maintainers/contracts` | 维护者 | 生命周期、所有权、异常优先级等代码契约 | 发布到维护者区 |
| `maintainers/contributing` | 维护者/编码代理 | 构建、测试、文案、评审和文档规则 | 发布到维护者区 |
| `decisions` | 维护者 | 已接受 ADR、取舍依据和替代关系 | 公开相关决策；敏感内容除外 |
| `plans/active` | 维护者 | 正在执行且仍具操作价值的计划 | 不进入公共导航或 `llms.txt` |
| `plans/archive` | 维护者 | 已完成、取消或被取代但仍需留证的计划 | 不发布，默认不作为代理上下文 |

### 4.2 文件命名

- 路径和文件名使用稳定的 ASCII `kebab-case`，中文标题写在 H1 或元数据中。
- 文件名表达概念或任务，不包含日期，除非内容本身是按日期归档的报告。
- ADR 使用固定编号，例如 `0001-autocad-2027-net8.md`；编号分配后不复用。
- 移动已发布页面时保留重定向映射，不能静默制造外部死链。
- 图片、示意图和下载资源放在 `docs/assets`，不使用开发者个人绝对路径。

### 4.3 现有文档的建议归属

以下是迁移目标，不在本方案文档提交中执行批量移动：

| 当前文件 | 目标位置 | 处理 |
| --- | --- | --- |
| `docs/关于IFoxCAD的架构说明.md` | `docs/maintainers/architecture/overview.md` | 作为当前架构入口，补充状态元数据。 |
| `docs/dbtrans-lifecycle-contract.md` | `docs/maintainers/contracts/dbtrans-lifecycle.md` | 保留 Issue 和基线证据，明确 Proposal/Accepted 状态。 |
| `docs/ZWCAD-version-compatibility.md` | `docs/reference/compatibility/zwcad.md` | 去除本机绝对路径，保留可复核来源。 |
| `docs/AC_2027-net8-compatibility-decision.md` | `docs/decisions/0001-autocad-2027-net8.md` | 转为 ADR，记录状态和回迁条件。 |
| `docs/guides/cad-ui-text-style-guide.md` | `docs/maintainers/contributing/cad-ui-text.md` | 已取消多仓库同名文件人工同步要求；后续移动到维护者规则目录。 |
| `docs/logical-modularization-plan.md` | `docs/plans/active/cad-modules.md` | 实施期间保持 active；完成后提炼 ADR/架构并归档。 |
| `docs/refactoring-proposal.md` | `docs/plans/archive/refactoring-proposal.md` | 已标记 `superseded` 并指向逻辑模块化执行计划；后续移动归档。 |
| `docs/ifoxcad-v0.9-upstream-merge-analysis.md` | `docs/plans/archive/upstream-v0.9.md` | Issue #26 已完成，已标记 `historical`；后续移动归档。 |
| `编译说明.md` | `docs/maintainers/contributing/building.md` | 与实际构建矩阵和 CI 命令统一。 |
| `Fs分支说明.md` | `docs/maintainers/contributing/branches.md` 或归档 | 先验证当前分支策略，删除过时内容。 |
| `IFoxCAD 说明.md` | `docs/concepts/history-and-positioning.md` | 只保留仍准确的来源与定位。 |

## 5. 文档状态与元数据契约

每份受治理的 Markdown 使用框架无关的 YAML front matter。首轮至少支持以下字段：

```yaml
---
id: contract.dbtrans-lifecycle
title: DBTrans 生命周期与释放契约
status: current
audience:
  - maintainer
published: true
module: Cad.Database
related_symbols:
  - T:Fs.Fox.Cad.DBTrans
related_issues:
  - 46
superseded_by: null
---
```

### 5.1 必填字段

| 字段 | 含义 | 规则 |
| --- | --- | --- |
| `id` | 跨路径稳定的文档标识 | 全仓唯一；文件移动时保持不变。 |
| `title` | 页面标题 | 与 H1 一致或由站点生成 H1。 |
| `status` | 权威状态 | 只能取允许集合。 |
| `audience` | 目标读者 | 至少一个；首轮为 `user`、`maintainer`。 |
| `published` | 是否进入公开站点 | `plans` 默认 `false`。 |

### 5.2 可选关联字段

| 字段 | 用途 |
| --- | --- |
| `module` | 关联逻辑模块，例如 `Cad.Database`、`Cad.Editor`。 |
| `related_symbols` | .NET XML 文档 UID；公共代码的首选关联键。 |
| `related_sources` | 仓库相对路径；只用于没有稳定 UID 的内部实现。 |
| `related_tests` | 证明契约或示例的测试/样例路径。 |
| `related_issues` | 讨论和执行 Issue 编号。 |
| `superseded_by` | 取代本文档的稳定文档 `id`。 |

不在手写元数据中固定 `source_commit` 或 `generated_at`；这两个字段由发布流水线写入站点构建清单，避免每次提交产生无意义修改。

### 5.3 状态集合

| 状态 | 含义 | 编码代理使用规则 |
| --- | --- | --- |
| `current` | 当前有效的使用说明、事实或契约 | 可以作为现行依据，但仍需核对实时代码。 |
| `draft` | 未完成草稿 | 只能作为作者工作区输入，不能推断现状。 |
| `proposal` | 尚未批准的建议 | 不得当作实现规格，除非任务明确引用。 |
| `superseded` | 已被另一文档取代 | 必须设置 `superseded_by`；默认不检索正文。 |
| `historical` | 已完成/取消但需留证 | 只用于追溯，不指导新实现。 |

同一主题不能同时存在两份互相冲突的 `current` 文档。提案转为执行计划或 ADR 后，旧文档必须在同一 PR 中改为 `superseded` 或 `historical`。

## 6. 文档与代码的关联规则

### 6.1 API 事实靠近代码

- 公共类型、成员、参数、返回值、异常和使用限制写入 C# XML 注释。
- 使用 `<see cref="..."/>` / `<seealso cref="..."/>` 建立类型间关系，避免手写易失真的完整签名。
- XML 注释不承载长篇教程；复杂工作流链接到 `guides` 或 `concepts`。
- 自动 API 参考只包含 Fox 拥有的命名空间，例如 `Fs.Fox.Cad` 和 `Fs.Fox.Basal`，不生成 Autodesk、ZwSoft、System 或工具依赖的完整 API 页面。
- AutoCAD 与 ZWCAD 的公共签名包含不同厂商类型，API 参考必须保留宿主维度，不能把两份程序集塞入同一个 UID 空间后假装完全一致。

### 6.2 概念文档使用稳定符号

`related_symbols` 使用 XML 文档 UID，例如 `T:Fs.Fox.Cad.DBTrans`。类型在逻辑模块化过程中移动文件但不改公共命名空间时，文档关联不需要变化。只有内部代码没有稳定 UID 时才使用 `related_sources`，并由链接检查在移动后要求修复。

### 6.3 示例必须可执行或可编译

- 完整示例放入 `samples` 项目，并进入最小构建矩阵。
- Markdown 可以嵌入由工具从样例提取的片段，但不能复制一份无人验证的完整程序。
- 示例明确标注适用宿主、目标框架、包和前置条件。
- 涉及 CAD 宿主行为的示例，构建成功只能记为编译证据；运行结论必须来自对应宿主验收。

### 6.4 链接使用规则

- 现行维护文档链接当前源码时使用仓库相对路径，并由 CI 检查。
- 历史分析引用某一时点证据时使用固定提交 permalink，不能链接会漂移的 `main` 行号。
- 用户指南优先链接生成 API UID，不把源码文件位置当作公共导航。
- 不在公开文档中保留 `D:\...` 等个人路径；本地资料应记录可识别标题、版本、校验值和可共享来源边界。

## 7. 代码变更的文档责任

代码与文档默认在同一 PR 中完成。是否需要更新文档由行为和契约决定，不由修改行数决定。

| 变更类型 | 必须检查/更新 |
| --- | --- |
| 新增或修改公共 API | XML 注释；相关 guide/reference；API 差异和兼容说明。 |
| 修改运行时行为、默认值或异常语义 | guide/concept/contract；对应测试或宿主验收；必要时 ADR。 |
| 修改支持宿主、SDK、TFM、包或部署 | `reference/compatibility`、安装说明和发布元数据。 |
| 纯内部重构 | 通常不改用户指南；若所有权、生命周期或依赖方向变化，则更新 maintainer 文档。 |
| 纯文件移动 | 稳定 symbol 关联不变；修复 `related_sources` 和相对源码链接。 |
| 新增复杂示例 | 增加/修改 `samples`，文档只引用已验证入口。 |
| 架构决策 | 新增或更新 ADR；旧决策用 `superseded_by` 串联。 |
| 实施计划完成、取消或被替代 | 提炼仍有效结论到 current/ADR，将计划转为 historical/superseded。 |

PR 模板应提供文档影响选项：`无用户可见变化`、`XML 注释`、`指南/参考`、`兼容/迁移`、`维护者契约`。选择“无变化”时需要一句可审查理由；首轮只做提醒，规则稳定后再决定是否成为阻断检查。

## 8. Vibe Coding 上下文治理

同仓 Markdown 可以提高代理理解，但未经治理的 Markdown 也会把旧假设放大。上下文入口采用渐进式披露：

```text
AGENTS.md
  -> docs/README.md（文档目录、状态和阅读顺序）
      -> current 文档摘要
          -> 与任务相关的 guide / contract / decision
              -> 真实源码、测试和构建配置
```

### 8.1 `AGENTS.md` 的职责

- 告诉代理文档事实源、文档索引和状态含义。
- 要求先核对实时源码/项目文件，再采用文档结论。
- 默认忽略 `plans/archive` 和 `superseded` 正文，除非任务要求追溯。
- 指定代码变更对应的文档责任和最小验证命令。
- 不复制长篇架构说明，只路由到权威文档。

### 8.2 `docs/README.md` 的职责

- 列出现行文档的 `id`、标题、状态、受众和一句摘要。
- 为常见任务提供阅读路径，例如“修改 DBTrans”“增加宿主支持”“移动 CADShared 文件”。
- 单独列出 active plans 和 historical/superseded 文档，避免搜索结果看起来同等有效。
- 由校验脚本确认所有受治理 Markdown 都被索引，或显式声明不纳入索引的原因。

### 8.3 控制语料噪声

- `_site`、生成 API Markdown/YAML/HTML、搜索索引、依赖 API 和构建产物全部进入 `.gitignore`。
- 不把 Issue 评论、AI 对话转录或临时分析原样批量提交到 `docs`。
- 一个主题只保留一个现行入口；历史材料必须短路到当前文档。
- 未来生成的 `llms.txt` 只聚合 `published: true` 且 `status: current` 的精选页面；不得把整个仓库 Markdown 拼接成单个上下文。
- 页面顶部和站点构建清单显示来源仓库、提交、产品版本和文档状态，使人和代理都能判断时效。

`docs/refactoring-proposal.md` 与 `docs/logical-modularization-plan.md` 已完成首批状态收口：前者明确标记为 `superseded` 并指向后者，后者是 Issue #25 的 active plan。后续目录迁移仍按第 4.3 节分批完成。

## 9. 公开站点边界

### 9.1 发布内容

公开站点分为两个主区域：

1. **使用 Fs.Fox.CAD**：getting started、guides、concepts、reference 和生成 API。
2. **维护 Fs.Fox.CAD**：architecture、contracts、contributing 和适合公开的 ADR。

维护者文档公开不意味着把全部开发过程公开为导航内容。生命周期契约和兼容决策能帮助使用者理解边界，也能给编码代理提供稳定语义，因此应该发布；active/archive plans、临时调查、未验证假设和仓库操作过程不发布。

### 9.2 不发布内容

- `plans/active` 和 `plans/archive`；
- `draft`、`proposal`、`superseded`、`historical` 页面正文，除非站点提供明确的历史专区；
- 个人绝对路径、本机 SDK 布局、密钥、Runner 细节和内部部署凭据；
- 未经宿主验证却表述为已通过的运行时结论；
- 生成工具的中间文件和第三方依赖 API 页面。

## 10. 版本与宿主维度

### 10.1 文档版本

首轮发布两个逻辑版本：

| 入口 | 来源 | 用途 |
| --- | --- | --- |
| `latest` | `main` 的已验证提交 | 展示下一发布版本的当前能力。 |
| `stable` | 当前正式 NuGet Release tag | 与用户安装的当前稳定包一致。 |

每次站点生成记录 `source_repository`、`source_commit`、`site_commit`、`package_version`、`channel` 和 `generated_at`。`Fs.Fox.CAD.Site` 使用来源锁保存 `latest` / `stable` 的完整 source commit；新稳定版本发布后，`stable` 原子切换到新 tag 对应的确定 commit。首轮不长期托管所有旧版本页面，旧 tag 的 Markdown 和 XML 注释仍可在 Git 中追溯。

### 10.2 AutoCAD 与 ZWCAD

共享源码不等于公共签名完全相同。当前 Release XML 中，AutoCAD 2025 与 ZWCAD 2025 的 Fox 成员只有约 1195 个 XML UID 完全相同，数百个成员因 Autodesk/ZwSoft 参数类型形成不同 UID。因此：

- 手写概念和指南尽量使用中性 Fox 术语，并在必要处标出宿主差异；
- 生成 API 至少分为 AutoCAD 与 ZWCAD 两个变体；
- 同一宿主的旧/新目标先通过 API diff 暴露少量差异，兼容性页面记录例外；首轮不复制四套几乎相同的手写指南；
- API 站点必须显示对应程序集、包、宿主和目标框架，不能把一个宿主的结论外推到另一个宿主。

## 11. 校验与发布流水线契约

具体站点框架后续选择，但流水线职责现在固定。

### 11.1 PR 检查

1. 校验 Markdown/GFM 结构、代码围栏、相对链接和资源路径。
2. 校验 front matter schema、`id` 唯一性、状态值和 `superseded_by` 目标。
3. 校验 `docs/README.md` 索引覆盖；禁止未登记的 current 文档。
4. 从 XML/API inventory 校验 `related_symbols`；从工作区校验 `related_sources` 和 `related_tests`。
5. 构建受影响的 `samples`；文档片段若由样例提取，检查生成结果无漂移。
6. 源码 PR 校验可发布内容契约；站点仓库建立后，可按受控的精确 source SHA 做集成预览，但不得部署 production。
7. 站点仓库 PR 使用固定来源或测试 fixture 严格构建站点，并可部署 preview；不得在其中修补产品正文来让构建通过。
8. 两个仓库都扫描 `_site`、生成 API、DLL/XML 副本、内容缓存和个人绝对路径，防止生成内容或敏感信息进入提交。
9. PR 描述记录文档影响；公共 API/行为变化没有文档说明时先警告，规则成熟后再评估阻断。

### 11.2 `main` 发布

1. `Fs.Fox.CAD` 从已合入的确定提交校验手写内容，并在 Windows/CAD SDK CI 中生成与该 commit 绑定的 AutoCAD/ZWCAD API 数据包。
2. 内容摘要变化时，源码仓库向 `Fs.Fox.CAD.Site` 发送受控事件；站点工作流校验后只更新 `latest` 来源锁。
3. EdgeOne Makers 看到站点仓库提交后，直接检出站点代码，并按来源锁获取精确 source commit 和匹配的 API 数据包。
4. 只发布允许命名空间和 `published: true` 内容，构建 `latest`、搜索索引和精选 `llms.txt`。
5. 输出清单记录 source/site commit、内容摘要、包版本和生成时间；生成内容不提交回任一仓库。
6. 部署失败不更新生产入口并保留上一成功站点；回滚通过 revert 站点代码或来源锁重建。

### 11.3 Release 发布

Release tag 完成正式构建、包检查和发布后，源码仓库才发送 stable 事件；站点仓库校验 tag 与完整 commit 后更新 `stable` 来源锁。文档发布不能先于对应包/API 基线，也不能从工作区未提交文件生成。站点与 Release 互相链接，并显示精确 tag、source commit 和 site commit。

## 12. 独立站点仓库与生成产物仓库

### 12.1 展示/部署仓库

已创建的 [Fs.Fox.CAD.Site](https://github.com/FeiSiPub/Fs.Fox.CAD.Site) 落实了展示/部署仓库边界：它隔离 Node/前端依赖、主题代码和后续 EdgeOne 权限，同时允许站点展示独立迭代。它必须通过精确来源锁读取 `Fs.Fox.CAD`，不得人工维护产品内容或提交生成页面。最终框架、EdgeOne 和产品内容 POC 仍按 [专项评估](edgeone-site-repository-evaluation.md) 分阶段执行。

### 12.2 生成产物仓库

当前不建立 `Fs.Fox.CAD.Docs` 或类似的 HTML 输出仓库。只有出现多产品聚合、长期保存大量历史版本、托管平台必须读取持久化产物，或有量化证据表明产物仓库能降低运维成本时，再开独立 Issue 评估。即使创建，也必须遵守源仓库单向生成、自动化身份最小权限、禁止手工编辑产物和生成清单关联源提交等规则。

## 13. 简要实施计划

### Phase A：建立治理入口

- [x] 新增本方案和跟踪 Issue。
- [x] 新增 `docs/README.md`，建立 current/proposal/superseded/historical 索引。
- [x] 新增根 `AGENTS.md` 的文档路由规则。
- [x] 记录 EdgeOne 展示/部署仓库的边界、约束和分阶段 POC 提案。
- [ ] 定义 front matter schema 和最小校验脚本。

退出条件：开发者或编码代理能从一个入口找到现行契约，并能识别旧提案不具权威性。

### Phase B：整理现有文档

- 按第 4.3 节逐批移动文件，修复 README、Issue 和站内链接。
- 为现有文档补齐元数据；优先处理两份重构文档的取代关系。
- 去除个人绝对路径和多仓库人工同步要求。
- 每批只做分类、元数据和链接修复，不夹带生产代码行为修改。

退出条件：所有受治理 Markdown 都有唯一 `id`、状态、受众和索引位置，没有互相冲突的 current 文档。

### Phase C：建立代码关联和可验证示例

- 生成 Fox API UID inventory，校验 `related_symbols`。
- 确定首批高价值 guides，并把完整示例迁入 `samples`。
- 在 PR 模板增加文档影响项，建立链接、元数据、样例和生成文件检查。
- 先以非阻断报告运行，稳定后再把确定性检查设为 required。

退出条件：API/行为变更能明确找到相关文档，示例由真实构建保护，生成内容不会进入 Git。

### Phase D：发布 `latest` 与 `stable`

- 在已创建且只承载展示/部署实现的 `Fs.Fox.CAD.Site` 中，继续用最小内容集完成 EdgeOne POC。
- 在不改变事实源结构的前提下比较并选择站点生成器。
- 建立精确来源锁、使用者区、维护者区、AutoCAD/ZWCAD API 变体和来源清单。
- 站点 PR 构建 preview，来源 main 推进 latest，Release 成功事件推进 stable。
- EdgeOne 直接连接站点仓库构建；不创建生成产物仓库。

退出条件：线上页面可追溯到精确提交/版本，构建可从干净 checkout 重现，失败不会污染源码历史。

### Phase E：基于证据演进

- 观察文档变更频率、构建时间、站点体积、死链、搜索质量和代理误用旧文档的情况。
- 只有第 12.2 节条件出现时才评估生成产物仓库或更多历史版本。
- 站点框架替换不得改变文档 `id`、状态、代码关联和唯一事实源原则。

## 14. 完成定义

- [ ] `Fs.Fox.CAD` 是唯一可编辑产品内容事实源，`Fs.Fox.CAD.Site` 没有第二套帮助正文。
- [ ] 公开使用者文档与维护者文档分区清晰，plans 默认不发布。
- [ ] 所有现行文档具有唯一 `id`、明确状态、受众和索引。
- [ ] proposal/superseded/historical 不会被编码代理误当成当前规格。
- [ ] 公共 API 注释来自源码，生成 API 只包含 Fox 命名空间并区分 AutoCAD/ZWCAD。
- [ ] 完整示例可编译，宿主运行结论与编译结论明确区分。
- [ ] PR 能检查链接、元数据、关联符号、样例和生成文件污染。
- [ ] `latest` 对应已验证 main 提交，`stable` 对应当前正式 Release tag。
- [ ] 站点能显示 source/site commit、包版本、宿主变体和生成时间。
- [ ] 静态产物不进入任一 Git 历史；展示仓库与生成产物仓库边界清晰。

## 15. 非目标

- 本方案不选择最终站点框架、主题、搜索服务或域名。
- 本方案正文不承载站点实现；已创建的 `Fs.Fox.CAD.Site` 在独立仓库迭代。本方案仍不代替所有者配置 EdgeOne，也不创建 GitHub App 或云端密钥。
- 本方案不立即生成 API 网站、`llms.txt` 或历史版本站点。
- 本方案不在一次提交中移动全部现有文档或重写内容。
- 本方案不把文档完整性等同于 CAD 宿主验收。
- 本方案不要求每个源码文件对应一份 Markdown，也不按源码目录复制文档树。
- 本方案不把所有开发过程、Issue 评论或 AI 对话保存为永久语料。
