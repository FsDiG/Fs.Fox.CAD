# CAD Host Acceptance Runner

该目录提供 AutoCAD 和 ZWCAD 共用的宿主验收 runner。它把 `tests/TestShared` 中的 `CommandMethod` 命令转换为可重复执行的宿主任务，但不把 CAD 宿主测试伪装成普通单元测试。

当前实现是 Issue #40 的 Phase 1 骨架：脚本生成和日志分析可自动验证，真实 CAD 启动路径仍需要按产品、年度和测试机器逐项校准。

该工具要求 PowerShell 7 或更高版本，统一通过 `pwsh` 调用；Windows PowerShell 5.1 不在支持范围内。

## 当前状态

| 产品 | runner 建模 | runner 真实宿主验证 |
| --- | --- | --- |
| AutoCAD 2019 | 已支持 | 未验证 |
| AutoCAD 2025 | 已支持 | 未验证 |
| ZWCAD 2022 | 已支持 | 未使用本 runner 重新验证；#36 有更早的半自动证据 |
| ZWCAD 2025 | 已支持 | 未验证 |

“已支持”只表示场景能够选择该产品并生成启动脚本，不表示插件已经在该宿主中加载或通过。

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
  -CadExecutable 'C:\Program Files\Autodesk\AutoCAD 2025\acad.exe' `
  -TestAssembly .\Build\AC_2025_Release\TestAcad2025.dll `
  -HostLabel 'AutoCAD 2025' `
  -GenerateOnly
```

将 `Product`、可执行文件和测试程序集替换为 ZWCAD 路径，即可验证 ZWCAD 脚本生成。`Generated` 不是宿主通过状态。

## 启动真实宿主

去掉 `-GenerateOnly` 后，runner 会使用 `/b <generated-script>` 启动指定可执行文件。含写操作的场景应只使用新建图或可丢弃图纸；传入 `-Drawing` 时，runner 会复制到本次输出目录，并只把副本交给 CAD。

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

## 安全和可移植性

- runner 不设置 `SECURELOAD=0`，测试程序集目录必须通过专用测试配置或受信任路径允许加载。
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

Issue #40 继续跟踪：隔离实例、Trusted Paths、错误窗口、超时清理、四个真实宿主校准、`[PASS]/[SKIP]/[FAIL]` 统一协议，以及仅 `workflow_dispatch` 的 self-hosted GitHub Actions。矩阵稳定前，该 runner 不应成为普通 PR 的必需检查。
