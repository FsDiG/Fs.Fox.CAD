# CAD 真实宿主验收 Runner

该目录是 Fs.Fox.CAD 真实 CAD 宿主验收的统一入口。它把 `tests/TestShared` 中的 `CommandMethod` 命令组织成可重复执行、默认失败关闭且可追溯到具体构建产物的宿主任务，用于回答“这个提交的这个测试程序集，是否在这个真实 CAD 版本中完成了这些场景”。

它不是普通单元测试框架，也不以建设完整 CAD UI 自动化平台为目标。脚本生成、日志分析或项目构建通过都不能代替真实宿主结论；FsHolu 产品验收也不属于本工具和 [Issue #40](https://github.com/FsDiG/Fs.Fox.CAD/issues/40) 的范围。

当前实现是 Issue #40 的 Phase 1 骨架，已由 [PR #41](https://github.com/FsDiG/Fs.Fox.CAD/pull/41) 合并到 `main` @ `c65010a`。离线 smoke、Scenario Schema 和基础日志分类已经完成，所有使用本 runner 的真实 CAD 执行仍为 `Not run`。

该工具要求 PowerShell 7 或更高版本，统一通过 `pwsh` 调用；Windows PowerShell 5.1 不在支持范围内。

## 当前验收矩阵

项目名表示编译时使用的 SDK/API 代际，实际验收宿主由本机可用版本和二进制兼容关系单独确定：

| 编译目标 | 测试程序集 | 实际验收宿主 | 当前状态 |
| --- | --- | --- | --- |
| AutoCAD 2019 API | `TestAcad2019.dll` | AutoCAD 2020 | `Not run` |
| AutoCAD 2025 API | `TestAcad2025.dll` | AutoCAD 2026 | `Not run` |
| ZRX 2022 | `TestZcad2022.dll` | ZWCAD 2022 | `Not run`；第一优先级；#36 有更早的半自动证据 |
| ZRX 2025 | `TestZcad2025.dll` | 无 | Build only；当前不要求真实宿主测试 |

runner 的 `Product` 参数只区分 `AutoCAD` 与 `ZWCAD` 两类脚本，不表示具体 SDK 或宿主年度。“能够生成脚本”也不表示程序集已经在对应宿主中加载或通过。

## Phase 1 的已知限制

- 没有创建或验证隔离 CAD profile，也不会配置 Trusted Paths。
- `HostLabel` 由调用方提供，`fileVersion` 来自可执行文件；尚无宿主内产品身份和版本握手。
- 结果仍依赖自由文本匹配，没有结构化的命令级 `[PASS]` / `[SKIP]` / `[FAIL]` 协议。
- 当前执行状态与宿主进程退出绑定，尚未把“场景执行完成”和“CAD 是否退出”建模为两个独立状态。
- AutoCAD/ZWCAD 的单实例转发、日志刷新、脚本退出和超时行为尚未按实际宿主校准。

因此，Phase 2 完成前只把真实启动用于隔离环境中的人工监督校准，不在日常 profile、已打开的 CAD 会话或生产图纸上无人值守运行。

## 文件

- `Invoke-CadHostAcceptance.ps1`：生成脚本、可选启动 CAD、等待完成标记、解析日志并输出报告。
- `Test-Runner.ps1`：不启动 CAD 的 smoke 检查，覆盖 AutoCAD/ZWCAD 生成以及 Passed/Failed/Skipped/异常日志分类。
- `scenario.schema.json`：场景结构定义。
- `scenarios/shared-smoke.json`：两类宿主共用的自包含数据库/互操作场景。
- `scenarios/progress-meter.json`：正常、异常、再次正常的进度条序列。
- `scenarios/zwcad-environment.json`：仅 ZWCAD 的 `GetEnv/SetEnv` 场景。
- `scenarios/dynamic-block-visibility.json`：需要调用方提供 DWG 的动态块只读扫描。

默认输出写入 `tools/HostAcceptance/artifacts`，该目录不会进入 Git。

## GenerateOnly

`GenerateOnly` 不启动 CAD，也不要求 `CadExecutable` 已安装。它仍会验证场景和测试程序集，并生成 `.scr`、`result.json` 和 `summary.md`。
场景会在执行前完整校验 `scenario.schema.json`；未知字段、错误产品值、空命令集合或字段类型错误均报告为 `InfrastructureError`。

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File .\tools\HostAcceptance\Invoke-CadHostAcceptance.ps1 `
  -Product AutoCAD `
  -Scenario .\tools\HostAcceptance\scenarios\shared-smoke.json `
  -CadExecutable 'C:\Program Files\Autodesk\AutoCAD 2026\acad.exe' `
  -TestAssembly .\Build\AC_2025_Release\TestAcad2025.dll `
  -HostLabel 'AutoCAD 2026 (AC_2025 build)' `
  -GenerateOnly
```

将 `Product`、可执行文件和测试程序集替换为 ZWCAD 路径，即可验证 ZWCAD 脚本生成。`Generated` 不是宿主通过状态。

## 启动真实宿主

去掉 `-GenerateOnly` 后，runner 会使用 `/b <generated-script>` 启动指定可执行文件。含写操作的场景应只使用新建图或可丢弃图纸；传入 `-Drawing` 时，runner 会复制到本次输出目录，并只把副本交给 CAD。

在当前 Phase 1 使用真实启动前，调用方必须确认：

- 使用专用测试机器或专用、可丢弃的 CAD profile，且没有同产品的既有进程会接收命令。
- 测试程序集目录已经由该专用 profile 配置为 Trusted Path；runner 不会自动修改用户配置。
- CAD 许可证可用，测试图纸可丢弃，输出目录可写，并有人观察可能出现的加载或错误窗口。
- 编译目标、测试程序集和 `HostLabel` 与本页矩阵一致；不能把 `HostLabel` 当作宿主内身份验证。

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File .\tools\HostAcceptance\Invoke-CadHostAcceptance.ps1 `
  -Product ZWCAD `
  -Scenario .\tools\HostAcceptance\scenarios\shared-smoke.json `
  -CadExecutable 'C:\Program Files\ZWSOFT\ZWCAD 2022\ZWCAD.exe' `
  -TestAssembly .\Build\ZW_2022_Release\TestZcad2022.dll `
  -HostLabel 'ZWCAD 2022' `
  -TimeoutSeconds 240
```

产品特有的配置或 profile 参数可通过 `-AdditionalArguments` 传入。它们会位于图纸和 `/b` 参数之前。

默认超时后只报告创建的 PID，不终止进程。`-TerminateOnTimeout` 只应在隔离测试机器使用；CAD 的单实例/转发行为尚未逐版本校准。

## 分析已有日志

`LogFile` 参数集不需要 CAD 或测试 DLL，可复核历史日志，也用于 runner 自身的脱离宿主测试。

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File .\tools\HostAcceptance\Invoke-CadHostAcceptance.ps1 `
  -Product ZWCAD `
  -Scenario .\tools\HostAcceptance\scenarios\shared-smoke.json `
  -LogFile C:\path\to\Drawing1.log
```

对于由本 runner 生成且可能包含多次运行的日志，可从 `result.json` 取得目标 `runId`，再传入 `-LogRunId <runId>`，只分析对应 begin/end token 之间的内容。没有 token 的旧日志不能使用该参数。

## 结果判定

- `Passed`：本次运行的唯一日志区间内，所有必需文本达到期望次数，并且没有失败模式。
- `Failed`：缺少必需文本、日志包含失败模式、marker 无效或日志不存在。
- `Skipped`：仅一个唯一必需文本缺失，且日志存在 `[SKIP]` 行；多个命令或重复期望出现缺失时，在尚未采用结构化 `[SKIP] <command>` 协议前一律按 `Failed` 处理。
- `TimedOut`：宿主在时间限制内没有退出。
- `InfrastructureError`：输入、场景或启动配置无效。
- `Generated`：只生成，没有启动宿主。

退出码为：`Passed/Generated = 0`、`Skipped = 2`、其他状态为 `1`。CAD 自身退出码会写入报告，但不会单独决定通过或失败。

真实运行生成的脚本会在 CAD 日志中写入带本次 `runId` 的 begin/end token，runner 只分析两者之间的文本，避免旧运行残留的通过或失败文本污染结论。`-LogFile` 分析模式在指定 `-LogRunId` 时采用相同边界；否则默认分析调用方提供的整个文件。复核不带 token 的重复使用日志前应先裁剪到单次运行。

一条可作为宿主验收结论的最终证据应包含：Git commit、编译目标、测试程序集及 SHA-256、宿主内报告的产品和完整版本、逐命令结构化结果、原始 CAD 日志、输入 fixture 的来源/哈希及必要的人工观察。Phase 1 的 `result.json` 已记录其中一部分，但调用方填写的 `HostLabel` 和可执行文件 `fileVersion` 不能替代宿主内身份握手，所以当前输出只用于校准，不足以单独宣告矩阵通过。

## 安全和可移植性

- runner 不设置 `SECURELOAD=0`，测试程序集目录必须通过专用测试配置或受信任路径允许加载。
- runner 不自动修改 Trusted Paths、注册表、启动组或全局 CAD 配置。
- Phase 2 隔离完成前，不以日常 CAD profile 或已打开的会话执行自动验收。
- 不提交厂商示例 DWG；动态块场景由调用方通过 `-Drawing` 提供，并运行其临时副本。
- 不硬编码 CAD 安装、仓库、用户或日志路径。
- 初始 `.scr` 使用 ASCII 编码。嵌入脚本的测试 DLL 和输出目录必须是 ASCII 路径；遇到非 ASCII 路径会明确拒绝，而不是生成可能被旧宿主误读的文件。
- 双引号会在 AutoLISP 字符串中转义；换行和 NUL 字符会拒绝。Windows 进程参数中的双引号当前也会拒绝。
- UI 截图、图形刷新、撤销栈和主题/DPI 相关行为不进入首阶段自动通过条件。

## Runner smoke 检查

以下命令不启动 CAD：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File .\tools\HostAcceptance\Test-Runner.ps1
```

它会优先在仓库的被忽略 `artifacts` 目录创建 ASCII 临时根目录，并在该目录不可用时探测公共或系统临时位置；随后创建空的测试 DLL 和合成日志，验证 AutoCAD/ZWCAD 脚本生成、`SECURELOAD` 不被关闭、重复期望计数、产品/fixture 限制以及日志状态分类，完成后清理临时目录。若没有可写的 ASCII 位置，脚本会明确失败，不会将路径编码问题误报为 runner 基础设施错误。

## 后续阶段

1. Phase 2：消费专用隔离 profile，校验 Trusted Paths，增加宿主内身份握手和结构化 `[PASS]` / `[SKIP]` / `[FAIL] <command>` 协议，并把场景执行状态与 CAD 退出状态分开。
2. Phase 3：优先在 ZWCAD 2022 校准成功、受控失败和超时路径，保留完整结构化证据。
3. Phase 4：分别在 AutoCAD 2020（`AC_2019` 产物）和 AutoCAD 2026（`AC_2025` 产物）完成同类校准。
4. 可选后续：在隔离 Windows self-hosted runner 上增加仅 `workflow_dispatch` 的工作流。矩阵稳定前不作为普通 PR 必需检查。

UI 截图和视觉比较不属于 #40 的核心完成条件。图形刷新、撤销栈、进度条和其他 UI 行为按场景保留人工观察证据；如果未来确有稳定自动化价值，再建立独立工作项。
