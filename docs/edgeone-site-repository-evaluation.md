# EdgeOne Makers 站点仓库架构评估与落地提案

> 状态：提案（Proposal）<br>
> 调研日期：2026-08-01<br>
> 源码基线：`FsDiG/Fs.Fox.CAD` `main` @ `416c65f`<br>
> 跟踪：[Issue #48](https://github.com/FsDiG/Fs.Fox.CAD/issues/48)<br>
> 现行约定：[文档与代码协同治理方案](documentation-architecture.md)<br>
> 站点仓库：[FsDiG/Fs.Fox.CAD.Site](https://github.com/FsDiG/Fs.Fox.CAD.Site)（Bootstrap 已创建，EdgeOne 尚未连接）<br>
> 本轮范围：评估并建立仓库边界、精确来源获取和 GitHub 同步；不选择最终前端框架，不代替仓库所有者配置 EdgeOne

## 1. 结论

`FsDiG/Fs.Fox.CAD.Site` 已创建，并由其自身 Actions 验证精确 source commit 获取、Git tree 校验和 Bootstrap 静态构建；后续由 EdgeOne Makers 直接连接该仓库完成云端构建和部署。这个仓库不是第二个帮助文档仓库，也不是生成 HTML 的镜像仓库；它是**可编辑的展示与部署实现仓库**。

三个边界已经明确：

1. `Fs.Fox.CAD` 始终是产品内容的唯一可编辑事实源，拥有源码、C# XML 注释、手写 Markdown、示例以及内容级导航元数据。
2. `Fs.Fox.CAD.Site` 只拥有站点框架、主题、组件、布局、搜索适配、内容获取脚本、来源锁和 EdgeOne 配置。人工不得在其中维护第二份产品指南、API 说明或示例。
3. HTML、搜索索引、API 中间文件和其他生成内容只存在于构建工作区、CI artifact 或 EdgeOne 部署中，不提交到两个 Git 仓库。

目标链路如下：

```text
Fs.Fox.CAD
  code + XML comments + Markdown + samples
  唯一产品内容源
        |
        | 精确 commit/tag + 可选 API 数据包
        v
Fs.Fox.CAD.Site
  framework + theme + adapters + source lock
  展示与部署实现
        |
        | EdgeOne Git 集成直接构建
        v
EdgeOne Makers
  production + preview + custom domain
```

这个拆分是合理的，因为它同时保留了代码与文档的原子演进，又把 Node/前端依赖、主题迭代和腾讯云部署权限从 CAD 类库仓库隔离出来。关键不是“是否有第二个仓库”，而是第二个仓库是否拥有产品内容。答案必须始终为否。

## 2. 官方能力与事实边界

以下结论在 2026-08-01 按腾讯云官方文档核对。EdgeOne Pages 已更名为 EdgeOne Makers；本文统一使用当前名称。

| 主题 | 官方已确认能力 | 对本项目的影响 |
| --- | --- | --- |
| Git 仓库接入 | Makers 可连接 GitHub、GitLab、Bitbucket 和 Gitee；部署分支出现新提交后自动拉取并部署。 | 可以让 Makers 直接连接 `Fs.Fox.CAD.Site`，不必由源码仓库上传 HTML。 |
| 构建设置 | 可配置根目录、Bash 构建命令和输出目录；明确支持 npm、yarn、pnpm 和多个 Node.js 版本。 | 前端依赖和构建脚本应固定在站点仓库；lockfile 必须提交。 |
| 文档框架 | 官方“其他框架”页明确列出 Docusaurus，并给出 `npm run build` / `build` 默认值；另有 Hugo 指南。 | Node 路线有直接官方证据，但这不等于现在就选定 Docusaurus。 |
| Python | Makers 构建指南没有把 Python 或 MkDocs 列为受支持的站点构建运行时。 | 不能未经 POC 就假定 MkDocs 可在 Makers 原生构建；这只是“尚无官方证据”，不是断言其一定不可用。 |
| 环境 | 每个项目有不可删除的 production 和 preview 环境，各自拥有分支、域名和环境变量。 | 站点代码 PR 可使用 preview；源码内容 PR 的跨仓预览需另建受控链路。 |
| 触发方式 | 支持 Git 自动触发、控制台重新部署和部署钩子。部署钩子是无需额外认证的秘密 URL。 | 生产链路优先使用站点仓库提交触发；部署钩子只作 POC/应急候选，必须按密钥保护。 |
| CLI/Actions | 官方提供 `edgeone makers deploy` 和 GitHub Actions 示例，需要 Makers API Token。 | 如果使用原生 Git 集成，GitHub 无需持有 EdgeOne API Token；CLI 仅作备用路径。 |
| API Token | Token 可设置 1 天至 1 年过期时间；官方页没有说明项目级细粒度权限。 | 不把 EdgeOne API Token 作为默认生产方案；使用时必须限期、轮换并只放在需要的环境。 |
| 部署记录 | 每次部署有唯一 URL、构建日志和物料；成功部署超过三条后，最早记录的构建产物会被自动清理，可按原配置重新部署。 | EdgeOne 记录不能充当长期版本归档；回滚基线必须保存在 Git 来源锁中。 |
| 自定义域名 | 自定义域名可绑定 production 或 preview，并始终指向该环境最新成功部署；域名添加后不会自动生成证书。 | 正式站点应使用自定义域名，并单独管理 CNAME、HTTPS 和证书生命周期。 |
| 区域与备案 | 中国大陆或全球含大陆区域需要 ICP 备案；全球不含大陆区域不要求备案。 | 域名、加速区域和备案属于上线前产品/运维决策，不阻塞仓库 POC。 |
| 免费版配额 | 500 次构建/月、1 个并发构建、单次 20 分钟、4 核 6 GB；单项目 20,000 文件、单文件 25 MB、总存储 5 GB。 | 必须避免每个无关源码提交都触发站点构建，并控制 API 页面数量和构建时间。 |
| 商业化 | 官方当前表示商业化版本仍在规划，具体定价和配额尚未发布。 | 配额只能作为当前 POC 基线，不能写成长期承诺。 |

官方触发页称“目前默认只有主分支提交自动触发”，构建指南同时描述 preview 环境关联其他 Git 分支。两者并不充分说明所有分支的默认自动部署细节，因此分支预览行为必须在 POC 中实测，不能仅依据文档推断。

## 3. 三类仓库不能混淆

| 类型 | 是否需要 | 是否可人工编辑 | 典型内容 |
| --- | --- | --- | --- |
| 产品内容源仓库 | 已有：`Fs.Fox.CAD` | 是 | 代码、XML 注释、Markdown、samples、内容状态和代码关联。 |
| 展示/部署仓库 | 已创建：[Fs.Fox.CAD.Site](https://github.com/FsDiG/Fs.Fox.CAD.Site) | 是，但仅限站点实现 | 框架依赖、主题、布局、组件、搜索适配、来源锁、EdgeOne 配置。 |
| 生成产物仓库 | 当前不需要 | 否 | HTML、搜索索引、生成 API 页面、压缩包等可重建输出。 |

原治理方案排除的是“第二个可人工编辑的产品内容仓库”和“只提交生成结果的仓库”。现在提出的 `Fs.Fox.CAD.Site` 不属于这两类：它拥有可维护的前端实现，但不能拥有产品帮助内容。

## 4. 仓库职责

### 4.1 `Fs.Fox.CAD`

必须保留：

- `src` 中的生产代码和公共 API XML 注释；
- `docs` 中的指南、概念、参考、维护者契约、ADR 和计划；
- `samples` 中可编译的完整示例；
- 页面 `id`、状态、受众、公开标记、逻辑导航和代码关联；
- Markdown、链接、元数据、样例及 API 数据的校验；
- 后续由 Windows/CAD SDK 构建环境产生的 API 文档数据包。

不得移出：

- 产品使用说明的正文；
- AutoCAD/ZWCAD 兼容性结论；
- 公共 API 语义与宿主差异；
- 影响代码评审的维护者契约。

### 4.2 `Fs.Fox.CAD.Site`

允许人工编辑：

- 站点生成器及其锁定依赖；
- 主题、CSS、布局、可访问性和响应式实现；
- 通用页面组件与 API 渲染器；
- 搜索、站点地图、RSS、`llms.txt` 等生成适配；
- 从精确来源提交获取内容的脚本；
- `content-source.json` 等来源锁；
- EdgeOne 构建、路由、header、域名环境说明和站点发布工作流；
- 只面向站点维护者的 README、AGENTS 和贡献说明。

禁止人工编辑：

- `getting started`、guides、concepts、compatibility、API 说明等产品内容；
- 从 `Fs.Fox.CAD` 复制后再单独维护的 Markdown；
- 生成 HTML、生成 API Markdown/YAML、搜索索引或内容缓存；
- DLL、XML 构建输出和第三方 CAD SDK；
- 以站点组件私有语法改写内容源，迫使产品文档依赖某一前端框架。

站点仓库可以有自己的开发文档和 UI 文案，但这类内容必须明确服务于“如何维护站点”，不得变成产品帮助正文。产品导航标题、页面顺序和受众属于内容语义，优先由 `Fs.Fox.CAD` 元数据提供；站点仓库只决定这些数据如何显示。

### 4.3 生成内容

下列内容只能出现在临时目录、CI artifact、不可变数据包或 EdgeOne 部署中：

- 过滤和转换后的发布 Markdown；
- 从程序集/XML 生成的 API inventory 或页面模型；
- HTML、CSS/JS bundle、搜索索引和站点地图；
- `latest` / `stable` 组合后的站点目录；
- 带来源提交的 `build-manifest.json`。

## 5. 来源锁与可追溯性

EdgeOne 只监听所连接的站点仓库。`Fs.Fox.CAD` 自身出现新提交，不会自动改变 `Fs.Fox.CAD.Site`，也不会自动让 EdgeOne 构建新内容。因此站点仓库需要一个很小、可审计的来源锁文件。

建议的框架无关格式：

```json
{
  "schema_version": 1,
  "source_repository": "FsDiG/Fs.Fox.CAD",
  "channels": {
    "latest": {
      "commit": "<40-character-source-commit>",
      "content_digest": "sha256:<digest>"
    },
    "stable": {
      "tag": "<release-tag>",
      "commit": "<40-character-source-commit>",
      "content_digest": "sha256:<digest>"
    }
  }
}
```

规则：

1. 构建只接受完整提交哈希，不能在构建期间直接跟随浮动的 `main`、`latest` tag 或分支压缩包。
2. `stable.tag` 用于显示，实际获取仍使用已解析的完整 commit，避免 tag 漂移改变历史构建。
3. `content_digest` 表示发布输入或 API 数据包的内容摘要。相同摘要不更新锁，以节省 EdgeOne 构建次数。
4. 来源锁只记录输入，不记录生成 HTML；变更历史本身就是 latest/stable 的可审计时间线。
5. 站点构建输出必须生成清单，至少包含 `source_repository`、`source_commit`、`site_commit`、`channel`、`package_version`、`generated_at` 和可选 `api_bundle_digest`。
6. POC 必须确认 Makers 构建环境如何取得站点提交哈希，例如 Git checkout 元数据或平台环境变量；未确认前不能伪造该字段。

来源锁不应存储访问令牌、部署 URL 或易过期的 artifact 下载链接。数据包地址若必须使用，清单中应同时保存不可变标识和 SHA-256，并在获取时校验。

## 6. 内容获取与构建

### 6.1 手写 Markdown

站点构建根据来源锁下载 `Fs.Fox.CAD` 对应 commit 的 archive，或执行精确 SHA checkout。获取后：

1. 校验仓库和提交与来源锁一致；
2. 只选择 `published: true` 且允许状态的页面；
3. 校验稳定 `id`、链接、资源和导航元数据；
4. 把 GFM/front matter 转换为站点生成器需要的内存模型或临时文件；
5. 构建结束后丢弃内容缓存。

源 Markdown 应保持 GFM 和框架无关 front matter。不得为了某个站点生成器，在 `Fs.Fox.CAD` 中大面积引入专有组件、专有路由或只能由该框架解释的正文语法。

### 6.2 API 参考

Makers 官方构建环境主要给出 Node/前端能力；Fs.Fox.CAD 的正式程序集构建依赖 AutoCAD ObjectARX、ZWCAD ZRX SDK、Windows/MSBuild 和多个宿主目标。站点构建不能承担这部分编译责任。

后续应由 `Fs.Fox.CAD` 的受控 CI：

1. 从与来源锁相同的 commit 构建正式程序集和 XML；
2. 只提取 `Fs.Fox.Cad`、`Fs.Fox.Basal` 等项目拥有的 UID；
3. 分开生成 AutoCAD 与 ZWCAD API 模型；
4. 生成包含 source commit、包版本、工具版本和文件摘要的清单；
5. 打包为不可变 API 数据包；
6. 由站点构建下载并核对 commit 与 SHA-256 后渲染。

数据包承载位置尚不在本轮确定：

| 候选 | 优点 | 风险/限制 |
| --- | --- | --- |
| GitHub Actions artifact | 实现简单，天然由 CI 产生 | 有保留期和下载鉴权，不适合作为长期 stable 来源。 |
| GitHub Release asset | tag 版本清晰，适合 stable | latest 需要滚动 Release 或其他通道，管理较笨重。 |
| GitHub Packages/OCI | 可按 digest 寻址，适合不可变数据包 | 需要额外发布、拉取权限和 EdgeOne 侧鉴权验证。 |
| 腾讯云 COS/对象存储 | 可按对象版本和摘要管理，EdgeOne 访问自然 | 增加云权限、费用和生命周期管理。 |

第一阶段不应被这个选择阻塞：先发布手写 Markdown；API 参考在数据包链路具备确定性后加入。

## 7. 推荐触发链路

### 7.1 `latest`

```text
Fs.Fox.CAD main 合入
  -> 源码仓库完成文档/API 输入校验
  -> 计算 content digest
  -> digest 变化时发送 repository_dispatch
  -> Fs.Fox.CAD.Site 校验来源仓库、commit 和 digest
  -> 只更新 content-source.json 的 latest
  -> 机器人提交到站点仓库 main
  -> EdgeOne 看到站点仓库新提交并直接构建
  -> 页面记录 source commit + site commit
```

### 7.2 `stable`

`stable` 只能由正式 Release 成功事件推进。tag 创建本身不够；应在包构建、检查和发布成功后，发送包含 tag、解析 commit、包版本和数据包摘要的受控事件。站点工作流校验 tag 与 commit 后更新 `stable`，不能让 stable 自动跟随 main。

### 7.3 触发方案比较

| 方案 | 可追溯性 | 权限面 | 结论 |
| --- | --- | --- | --- |
| 更新站点来源锁后由 EdgeOne Git 集成构建 | 精确 commit 有 Git 历史，最好 | GitHub 跨仓最小写权限；EdgeOne 只读站点仓库 | **推荐生产方案**。 |
| 从源码仓库直接调用 EdgeOne 部署钩子 | 钩子只触发重建，若来源锁未变仍构建旧内容 | 需保护无需认证的秘密 URL | 只适合手工 POC 或应急，不作为确定性生产链路。 |
| 源码仓库 Actions 构建并用 EdgeOne CLI 上传 | 可由 Actions 记录 commit | GitHub 必须保存 EdgeOne API Token，且绕过“EdgeOne 直接构建站点仓库”目标 | 备用方案。 |
| 站点构建时直接读取 `Fs.Fox.CAD/main` | 构建重试可能得到不同内容 | 权限简单 | 禁止用于 production/stable。 |

为了避免 500 次/月配额被无关提交消耗，自动化必须在来源内容摘要没有变化时退出且不提交来源锁。初期无法稳定计算 API 摘要时，可以先采用手工更新锁或仅监听明确文档路径，不能为了“自动”而制造不可控构建风暴。

## 8. 权限与安全

### 8.1 GitHub

建议使用只安装在 `Fs.Fox.CAD.Site` 的 GitHub App 发送跨仓事件，不使用组织级 classic PAT。权限按实际工作流压缩：

- EdgeOne GitHub 集成只授权读取 `Fs.Fox.CAD.Site`，不要选择全部组织仓库；
- 跨仓发送方只需要对站点仓库创建 dispatch 所需的最小权限；GitHub 当前 REST 文档要求 repository dispatch 的 fine-grained token 具有目标仓库 `Contents: write`；
- 站点 workflow 的 `GITHUB_TOKEN` 只在更新来源锁的 job 中声明 `contents: write`，其他 job 使用 `contents: read`；
- App 私钥只存于需要发事件的受控 GitHub 环境，并设置轮换和撤销流程；
- payload 必须校验固定源仓库、完整 SHA、事件类型和 schema，拒绝任意仓库 URL、分支名、构建命令或下载命令；
- 站点 main 的机器人写入应有独立审计身份，并受 ruleset 的明确例外约束；不得使用个人账号长期推送。

POC 可以临时使用只绑定站点仓库、带过期时间的 fine-grained PAT，但上线前应迁移到 GitHub App。无论哪种身份，都不能把 token 写入来源锁、构建产物或 Issue。

### 8.2 EdgeOne

原生 Git 集成由 EdgeOne 自己读取站点仓库和运行构建，因此默认链路不需要把 Makers API Token 放入 GitHub。只有采用 CLI 部署时才创建 Token，并应：

- 设置尽可能短的过期时间并定期轮换；
- 分离 production 与 preview 使用环境；
- 禁止在来自 fork 的 PR 或任意 `pull_request_target` 代码路径中暴露；
- 不直接复制官方 Actions 示例后执行不受信任的 PR 代码；
- 一旦泄露立即撤销，而不是只删除日志。

部署钩子同样按密钥处理。它无需额外认证，泄露后唯一可靠的恢复方式是删除并重新生成。

## 9. `latest`、`stable` 与预览

| 通道 | 来源 | 触发 | 公开用途 |
| --- | --- | --- | --- |
| `latest` | 已通过来源校验的 main commit | 内容摘要变化后的来源锁提交 | 下一发布版本文档。 |
| `stable` | 已完成正式发布的 tag 对应 commit | Release 成功后的来源锁提交 | 与当前稳定 NuGet 包一致。 |
| site preview | `Fs.Fox.CAD.Site` 的功能分支/PR | EdgeOne preview 或受控 CLI | 主题、布局和组件评审。 |
| source preview | `Fs.Fox.CAD` PR 的精确 head SHA | 后续受控跨仓预览流程 | 内容和 API 变更评审。 |

首轮只要求 `latest` 和 `stable` 可重现。根域名最终指向 stable 还是 latest 是产品入口决策，不影响 POC，可在正式域名启用前由 Issue #48 收口。

源码 PR 预览不应在第一阶段自动开放给所有 fork。后续可采用手工 `workflow_dispatch`：输入并验证源 PR SHA，更新临时站点分支，使用 preview 环境部署，完成后清理分支。若将预览链接写回 PR，评论权限与构建权限应分离，并禁止预览任务读取 production secret。

## 10. 回滚与故障恢复

生产回滚以 Git 为准：

1. 站点代码故障：revert 对应 `Fs.Fox.CAD.Site` 提交；
2. latest 内容故障：revert `latest` 来源锁提交；
3. stable 错误推进：提交修正后的 tag/commit 锁，不移动已发布 tag；
4. API 数据包错误：拒绝摘要或来源 commit 不匹配的数据包，恢复上一锁定摘要；
5. EdgeOne 构建失败：保持上一成功部署，修复后重新构建相同锁；
6. 跨仓事件丢失：提供手工 reconciliation，比较预期 source commit/digest 与当前来源锁后补发。

EdgeOne 可以从特定部署记录重新部署，但成功记录超过三条后旧构建产物可能被清理。因此“在控制台点旧部署”只是短期便利，不是回滚策略。任何可恢复版本都必须能由站点 commit、来源锁和不可变 API 数据重新构建。

## 11. 建议的站点仓库骨架

框架未选定前，只固定职责，不固定所有文件名：

```text
Fs.Fox.CAD.Site/
  README.md                    # 只说明站点开发和部署
  AGENTS.md                    # 禁止在此维护产品内容
  package.json
  <lockfile>
  config/
    content-source.json        # latest/stable 精确来源锁
  site/
    components/                # 展示组件
    theme/                     # 样式与布局
    adapters/                  # GFM、API 模型、导航、搜索适配
  scripts/
    acquire-content.*          # 获取精确 source commit
    verify-content.*           # schema、commit、digest 和边界校验
    build-manifest.*           # 生成来源清单
  static/                      # 站点自有图标等，不含产品文档副本
  .github/workflows/
    update-source-lock.yml
    validate-site.yml
  edgeone.json                 # 只有实际需要路由/header 时才建立
```

`dist`、`build`、`.cache`、下载的 source archive、API 数据包、搜索索引和生成页面全部忽略。站点仓库 CI 应主动扫描常见生成目录和意外复制的产品 Markdown。

## 12. 框架选择门槛

本次不选择 DocFX、Docusaurus、VitePress、MkDocs 或其他框架。真正选择前必须用相同最小内容集完成 POC，并比较：

- 纯 GFM/front matter 的兼容程度，是否迫使源码仓库使用框架专有语法；
- 中文搜索、API UID、AutoCAD/ZWCAD 变体和 latest/stable 的实现复杂度；
- EdgeOne 上冷构建时间、输出文件数、单文件大小和缓存行为；
- 主题可维护性、可访问性、移动端表现和链接稳定性；
- source archive/API 数据包的获取与校验能力；
- 构建失败时是否明确返回非零退出码；
- 本地预览和编码代理修改站点代码的可理解性。

候选边界：

| 候选 | 当前证据 | 进入 POC 的条件 |
| --- | --- | --- |
| Docusaurus | EdgeOne 官方明确支持，Node 路线成熟 | 可作为 Node 基准候选，但不因官方列出就自动胜出。 |
| VitePress/Astro 等静态 Node 方案 | 通常能输出静态目录，但官方页面未逐一确认 | 在 Makers 实测安装、构建、路由和输出。 |
| MkDocs/Material | 内容体验成熟，但官方未确认 Python 构建运行时 | 先证明 Makers 可重复安装/运行 Python，或改用外部预构建 artifact。 |
| DocFX | 适合 .NET API，但 Fs.Fox.CAD 编译依赖 CAD SDK | API 提取只能放在源码 CI；站点端是否使用其输出另行比较。 |
| Hugo | EdgeOne 有独立官方指南 | 比较中文搜索、API 模型和主题维护成本。 |

## 13. 分阶段落地

截至 2026-08-01，站点仓库 Bootstrap 已完成：`latest` 锁定 source `main` @ `416c65f`，`stable` 锁定 Release `v1.0.3` @ `d4120af`；本地和 GitHub Actions 均已验证精确 SHA 获取、Git tree、一致性测试和静态构建。EdgeOne 连接、最终框架和产品内容渲染仍未执行。

### Phase 0：确认架构和边界

- 评审本文与更新后的治理方案；
- 在 Issue #48 记录接受项、待决项和验收；
- 本源码仓库不引入前端依赖；本轮不创建 EdgeOne 云资源或密钥。

退出条件：所有参与者能区分产品内容源、展示部署仓库和生成产物。

### Phase 1：最小站点 POC

- [x] 创建 `FsDiG/Fs.Fox.CAD.Site`，加入零依赖 Node Bootstrap、维护规则和来源集成契约；
- [x] 锁定 latest/stable 的完整 source commit 和 Git tree；
- [x] 验证精确 SHA 获取、远端一致性、构建清单和生成目录排除；
- [ ] 比较最小站点框架候选，并选择 3 至 5 篇公开 Markdown 验证产品内容渲染；
- [ ] 由仓库所有者连接 EdgeOne，验证 production、一次站点 PR preview 和失败保持上一成功部署；
- [ ] 保持本阶段不生成 API、不绑定正式域名。

退出条件：干净构建可重现，页面显示 source/site commit，站点仓库没有产品内容副本。

### Phase 2：自动更新 `latest`

- [ ] 创建最小权限 GitHub App；
- [ ] 来源 CI 计算发布内容摘要并发送 dispatch；
- [x] 站点 workflow 支持 schedule、手工 reconciliation 和受控 `repository_dispatch`，校验后只提交来源锁；
- [x] 加入 concurrency、相同 commit/tree 跳过和失败非零退出；
- [ ] 增加失败通知，并统计每月构建数、耗时和输出文件数。

退出条件：一次合格内容变更只产生一次可追溯部署；无关源码提交不消耗站点构建。

### Phase 3：API 与 `stable`

- 在 Windows/CAD SDK CI 生成 AutoCAD/ZWCAD API 数据包；
- 选定不可变数据包承载位置并校验摘要；
- Release 成功后推进 stable；
- 验证包版本、tag、source commit 与 API 清单一致。

退出条件：stable 可由 tag、站点 commit 和数据包摘要完整重建。

### Phase 4：域名与源码 PR 预览

- 决定根入口、加速区域和备案策略；
- 配置自定义域名、CNAME、HTTPS 和证书续期；
- 为源码 PR 增加不暴露 production secret 的受控预览；
- 演练来源锁回滚、站点代码回滚和事件丢失恢复。

退出条件：正式域名稳定、证书可维护、预览与生产权限隔离、回滚演练有记录。

## 14. 风险与控制

| 风险 | 控制 |
| --- | --- |
| 站点仓库逐渐成为第二个内容源 | AGENTS/CODEOWNERS/CI 明确禁止产品内容目录；页面正文只能从来源锁获取。 |
| 构建读取浮动 main 导致不可重现 | 完整 commit 锁、摘要校验和构建清单。 |
| API 数据与 Markdown 不属于同一提交 | 数据包 manifest 中记录 source commit；不匹配立即失败。 |
| ObjectARX/ZRX 编译进入 EdgeOne | API 提取固定在源码仓库 Windows CI；Makers 只渲染数据。 |
| 跨仓 token 权限过大 | GitHub App 只安装站点仓库；job 级 permissions；禁止 classic PAT。 |
| fork PR 窃取密钥 | 不在不受信任代码路径提供 App 私钥、EdgeOne Token 或部署钩子。 |
| 构建配额被频繁 main 提交耗尽 | 内容摘要去重、并发取消、分阶段启用预览、监控月度构建数。 |
| EdgeOne 旧部署被清理后无法回滚 | 通过 Git revert + 精确来源锁重建，不依赖平台保存旧物料。 |
| 域名可访问但 HTTPS/备案不完整 | 正式上线前设置独立 gate，记录区域、备案、证书与续期责任。 |
| 框架绑架源 Markdown | GFM/front matter 契约；专有组件只存在站点适配层。 |

## 15. 已决定、建议与待决

### 已决定的边界

- `Fs.Fox.CAD` 是唯一产品内容源；
- 新仓库只负责展示、前端依赖和部署；
- EdgeOne 直接连接并构建新仓库；
- 生成内容不提交回任一仓库；
- 当前不选择文档前端框架。

### 本提案建议

- 已按建议创建 `FsDiG/Fs.Fox.CAD.Site`；
- 用 `content-source.json` 锁定 latest/stable 的完整 source commit；
- 使用 GitHub App + repository dispatch 更新锁；
- EdgeOne 原生 Git 集成负责生产构建；
- 第一阶段只验证手写 Markdown，API 由后续源码 CI 数据包接入。

### 不阻塞 POC 的待决项

- 最终站点框架；
- API 数据包使用 Release、Packages/OCI 还是 COS；
- 根域名默认指向 stable 还是 latest；
- 加速区域、ICP 备案、自定义域名和证书责任；
- 源码 PR 是否自动创建 EdgeOne preview。

这些项目继续由 Issue #48 跟踪。只有当某项需要独立负责人、权限审批或会阻塞相应 Phase 时，再拆分子 Issue；当前无需为了尚未开始的实施提前创建空工作项。

## 16. POC 完成定义

- [ ] EdgeOne 只连接 `Fs.Fox.CAD.Site`，GitHub 授权没有覆盖无关仓库。
- [ ] 站点由完整 source commit 构建，不在构建期间跟随浮动分支。
- [ ] `Fs.Fox.CAD.Site` 不含人工维护的产品帮助正文和生成页面。
- [ ] `Fs.Fox.CAD` 的 Markdown 无需依赖所选框架的专有正文语法。
- [ ] 输出页面和 `build-manifest.json` 同时显示 source commit 与 site commit。
- [ ] 失败构建不会替换上一成功 production 部署。
- [ ] 可以通过 revert 来源锁重建上一版本，而不依赖 EdgeOne 保存旧产物。
- [ ] 构建时间、文件数和月构建预算满足当前免费版限制。
- [ ] 所有 token、部署钩子和云配置均未进入 Git、日志或前端 bundle。

## 17. 参考资料

腾讯云 EdgeOne Makers 官方文档：

- [产品简介](https://cloud.tencent.com/document/product/1552/127365)
- [导入 Git 仓库](https://cloud.tencent.com/document/product/1552/127369)
- [其他框架（含 Docusaurus）](https://cloud.tencent.com/document/product/1552/127378)
- [构建指南](https://cloud.tencent.com/document/product/1552/127392)
- [触发部署](https://cloud.tencent.com/document/product/1552/127395)
- [管理部署](https://cloud.tencent.com/document/product/1552/127396)
- [使用 GitHub Action](https://cloud.tencent.com/document/product/1552/127398)
- [域名管理概览](https://cloud.tencent.com/document/product/1552/127403)
- [自定义域名](https://cloud.tencent.com/document/product/1552/127404)
- [API Token](https://cloud.tencent.com/document/product/1552/127422)
- [EdgeOne CLI](https://cloud.tencent.com/document/product/1552/127423)
- [限制与配额](https://cloud.tencent.com/document/product/1552/132789)
- [价格与套餐](https://cloud.tencent.com/document/product/1552/132790)

GitHub 官方文档：

- [Create a repository dispatch event](https://docs.github.com/en/rest/repos/repos#create-a-repository-dispatch-event)
- [Choosing permissions for a GitHub App](https://docs.github.com/en/apps/creating-github-apps/registering-a-github-app/choosing-permissions-for-a-github-app)
- [Automatic token authentication and job permissions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/automatic-token-authentication)
