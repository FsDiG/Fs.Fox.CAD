# Fs.Fox.CAD 仓库协作规则

本文件适用于整个仓库。它只提供任务路由和证据规则；完整文档状态、受众和阅读顺序见 [文档索引](docs/README.md)。

## 开始工作前

1. 先检查实时源码、项目文件、工作流、测试和当前 Git/GitHub 状态，再采用文档结论。
2. 从 `docs/README.md` 确认任务对应的现行文档、提案或历史材料。
3. `current` 文档可以说明当前约定，但不能覆盖与实时代码不一致的事实；发现冲突时应在同一变更中修正文档。
4. `proposal` 只作为待决建议，`superseded` 和 `historical` 默认只用于追溯，不能作为新实现规格。
5. GitHub Issue 用于讨论、跟踪和验收，不是现行技术契约；稳定结论应回写到代码、XML 注释或仓库 Markdown。

## 分支与任务路由

- `main` 承载正常功能、修复、发布和文档治理，相关工作使用从最新 `main` 建立的短期分支和 PR。
- Issue #25 的 `CADShared` 单程序集逻辑模块化在长期分支 `refactor/cad-modules` 上实施，相关 PR 以该分支为 base。
- `refactor/cad-modules` 只合并最新 `main`，不 rebase、不强推；主线修复和文档变更进入 `main` 后再同步过去。
- 不在逻辑模块化的机械移动中夹带行为修改、公共 API 修改、全文件格式化或无关文档重写。
- 文档治理与发布能力由 Issue #48 跟踪；`Fs.Fox.CAD` 保持唯一产品内容源，已创建的展示/部署仓库及其边界见 [EdgeOne 站点仓库评估](docs/edgeone-site-repository-evaluation.md)和 [Fs.Fox.CAD.Site](https://github.com/FeiSiPub/Fs.Fox.CAD.Site)。站点框架尚未确定，不要在本仓库提前引入框架专用目录或生成产物。

## 代码与文档责任

| 变更 | 至少检查 |
| --- | --- |
| 公共 API、参数、返回值或异常 | C# XML 注释、相关 guide/reference、宿主差异。 |
| 默认行为、生命周期或所有权 | concept/contract、回归证据和必要的 CAD 宿主验收。 |
| SDK、TFM、包或发布流程 | 兼容性文档、构建说明、工作流说明和包元数据。 |
| 内部模块边界或依赖方向 | 维护者架构文档和 active plan；通常不修改用户指南。 |
| 纯文件移动 | 相对链接、`related_sources` 和项目编译项；公共符号关联应保持稳定。 |
| 完整示例 | 可编译的 sample/test 入口；不要在 Markdown 中维护第二份完整实现。 |

## 验证与证据

- 文档变更至少执行 `git diff --check`、相对链接检查，并检查 GitHub Flavored Markdown 的实际渲染。
- 构建命令和正式目标以 [构建与项目结构](编译说明.md)及实时项目/工作流为准。
- 构建或 CI 通过只代表编译和自动检查通过，不能表述为 AutoCAD/ZWCAD 宿主验收通过。
- 真实 CAD 宿主验收优先使用 [`tools/HostAcceptance`](tools/HostAcceptance/README.md)，并分别记录编译目标、测试程序集和实际运行的 CAD 产品/完整版本；不能用项目名中的 SDK 年代代替宿主版本。
- 涉及加载、事务、数据库、Editor/Jig、UI、native interop 或宿主生命周期时，明确记录实际运行的宿主、版本和未执行项。
- 在 Issue #40 的隔离 profile、宿主内身份握手和命令级结果协议完成前，不使用日常 CAD profile 或已打开的 CAD 会话执行自动验收，也不自动修改 Trusted Paths、注册表、启动组或全局 CAD 配置。
- 除非任务明确要求，不启动 CAD 进行人工验收；未运行时写明 `Not run`，不要推断结果。

## 上下文与生成内容

- 当前 main 尚无 `Build/HostAcceptance`；只有目标 Git 状态确实包含该目录且任务需要追溯对应 Issue、PR、提交或宿主结果时，才读取其中的历史记录。
- `tests/TestShared/readme.md` 目前只是未完成的子树说明，不作为测试覆盖清单。
- `MgdDbg/README.md` 只描述该组件，不代表 Fs.Fox.CAD 的整体构建或支持矩阵。
- 不提交生成站点、API 中间文件、搜索索引、DLL/XML 副本或其他可重建输出。
- 不把 Issue 评论、AI 对话转录或临时分析批量保存为长期文档。
