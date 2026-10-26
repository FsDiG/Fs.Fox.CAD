# PR #138 模块专项评审报告：架构治理、多目标编译与 CI 验证

> 状态：historical；针对 `4064025` 的专项静态评审快照，2026-10-26 更正。<br>
> 仅供追溯，不是现行技术契约；以下测试／构建记录沿用原评审者记录，非本轮重跑。目标框架以实时项目为准，宿主验收 `Not run`。
> 后续完整审查与修正见[实现记录](../../beta-implementation-notes.md#完整核心审查与修正)。

**评审模块**：工程架构、模块基线、多目标条件编译与 CI 流水线
**涉及基础设施文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\CADShared.projitems`](../../../src/CADShared/CADShared.projitems)
- [`D:\MakeX\Fs.Fox.CAD\Build\Verify-CADSharedModuleMap.ps1`](../../../Build/Verify-CADSharedModuleMap.ps1)
- [`D:\MakeX\Fs.Fox.CAD\Build\CADSharedModuleBaseline.json`](../../../Build/CADSharedModuleBaseline.json)
- [`D:\MakeX\Fs.Fox.CAD\.github\workflows\build-and-deploy.yml`](../../../.github/workflows/build-and-deploy.yml)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestShared.projitems`](../../../tests/TestShared/TestShared.projitems)
**对应规范**：[`AGENTS.md`](../../../AGENTS.md)、[`docs/README.md`](../../../docs/README.md)

---

## 1. 共享模块映射与架构基线审查

### 1.1 模块映射合规性
依据 Issue #25 和 PR #100 的模块化治理约定，`CADShared` 单程序集采用逻辑模块划分。
本次新增的 16 个源文件在 `CADShared.projitems` 中被分配了严格的模块标记（`FsFoxModule`）和构建编译序号（`FsFoxOrder`）：

| 序号区间 | 归属逻辑模块 | 纳入的新增源文件 |
| :--- | :--- | :--- |
| `1000 ~ 1030` | `Cad.Database` | `PolylineCoreBeta.cs`, `PolylineExBeta.cs`, `PolylineContainmentCoreBeta.cs`, `PolylineRegionExBeta.cs` |
| `1100 ~ 1110` | `Cad.Geometry` | `OrderingBetaCore.cs`, `PointOrderingBeta.cs` |
| `1120 ~ 1130` | `Cad.Database` | `CurveOrderingExBeta.cs`, `ExtentGroupingBeta.cs` |
| `1200` | `Cad.Database` | `TextMetricsBeta.cs` |
| `1300 ~ 1301` | `Cad.Geometry` | `TriangleMathBeta.cs`, `Triangle3dBeta.cs` |
| `1310 ~ 1311` | `Cad.Database` | `PolyFaceMeshDataBeta.cs`, `PolyFaceMeshBuilderBeta.cs` |
| `1320` | `Cad.Database` | `BlockIntersectionExBeta.cs` |
| `1400 ~ 1410` | `Cad.Application` | `DwgPreviewCoreBeta.cs`, `DwgPreviewBeta.cs` |

- **依赖单向流动审查**：
  `Cad.Geometry`（纯数学核心）不依赖 `Cad.Database` 和 `Cad.Application`；
  `Cad.Database` 依赖 `Cad.Geometry`；
  `Cad.Application`（图形预览）依赖 `Cad.Database`；
  **审查结论**：依赖层级完全单向流动，未产生任何底层模块对上层模块的反向依赖或循环依赖。

### 1.2 模块校验脚本与基线对比
评审人在本地执行模块自检脚本：
```powershell
pwsh -NoProfile -File 'D:\MakeX\Fs.Fox.CAD\Build\Verify-CADSharedModuleMap.ps1'
```
- **执行输出**：
  `CADShared module verification passed: 159 items, 42 debt-tagged files, 17 boundary findings.`
- **指标对比分析**：
  - 总文件数由基线的 143 个精确增加到 159 个（+16 项）；
  - `Cad.Geometry` 文件数由 16 增加到 20（+4 项）；
  - `Cad.Database` 文件数由 44 增加到 54（+10 项）；
  - `Cad.Application` 文件数由 9 增加到 11（+2 项）；
  - **既有架构债务未扩散**：既有的 42 个历史债务文件与 17 项边界发现完全保持不变，未新增任何债务标记；原有 143 个既有文件的 SHA256 哈希未受篡改。

---

## 2. CI 工作流与自动化测试矩阵审查

### 2.1 GitHub Actions 工作流编排
审查 [`.github/workflows/build-and-deploy.yml`](../../../.github/workflows/build-and-deploy.yml) 的改动：
- PR 在构建矩阵启动前，新增了独立的自动化测试阶段：
  ```yaml
  - name: Test Beta production numeric cores
    shell: pwsh
    run: |
      $projects = @(
        'tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj',
        'tests/OrderingBeta.CoreTests/OrderingBeta.CoreTests.csproj',
        'tests/GeometryBeta.CoreTests/GeometryBeta.CoreTests.csproj',
        'tests/DwgPreviewBeta.CoreTests/DwgPreviewBeta.CoreTests.csproj',
        'tests/PolylineRegionBeta.CoreTests/PolylineRegionBeta.CoreTests.csproj'
      )
      foreach ($project in $projects) {
        dotnet run --project $project --configuration Release
        if ($LASTEXITCODE -ne 0) {
          throw "Production-core tests failed: $project"
        }
      }
  ```
- **关键设计优势**：
  五组生产核心测试直接链接生产源码（零外部 CAD SDK 依赖）。在耗时巨大的跨宿主多目标编译之前，首先执行纯计算回归，若有任何数学或几何断言失败立即熔断，节省 CI 资源。

### 2.2 多目标发布矩阵与条件编译合规性
PR 支持仓库现行的 7 个正式发布目标（涵盖 .NET Framework 4.8 与 .NET 8）：
1. `AutoCAD 2019`（.NET 4.8）
2. `AutoCAD 2025`（.NET 8）
3. `ZWCAD 2022`（.NET 4.8）
4. `ZWCAD 2025`（.NET Framework 4.8；生产与测试项目均为 `net48`）
5. `GStarCAD 2022`（.NET 4.8）
6. `GStarCAD 2023`（.NET 4.8）
7. `GStarCAD 2026`（.NET 8）

- **条件编译符号边界核对**：
  - `ACAD`：仅在 AutoCAD 目标开放 `DwgPreviewBeta`，其他宿主不提供伪实现；
  - `ZW_2022 || GC_2022 || GC_2023`：在缺少 GI 测量 API 的旧版 SDK 目标中，`TextMetricsBeta` 即时拦截抛错；
  - 命名空间（`Autodesk.AutoCAD` vs `ZwSoft.ZwCAD` vs `GrxCAD` / `Gssoft.Gscad`）通过 Fox 既有的平台别名系统严密隔离。
- **既有编译警告溯源**：
  - .NET 4.8 目标的 `MSB3277`（System.ValueTuple 冲突）与 GStarCAD 2022 的 `MSB3270`（x86 元数据警告）已在迁移前基线 `5729ef6` 复现，确认为既有环境背景，本次 PR 未隐瞒或盲目压制。

---

## 3. 证据准则与真实 CAD 宿主边界审查

依据仓库根协作文件 [`AGENTS.md`](../../../AGENTS.md) 的铁律：
1. **构建或 CI 通过只能证明编译与自动检查通过，绝不代表 CAD 宿主验收通过**；
2. 在 Issue #40 的安全隔离 profile、宿主身份握手与协议完成前，不使用日常 CAD 运行未受控验收，不自动修改注册表或全局 CAD 配置；
3. **未运行的宿主场景必须诚实标注为 `Not run`**。

**审查核验结论**：
- PR #138 配套指南、[实现记录](../../beta-implementation-notes.md)、[开发计划](../../beta-development-plan.md)及本系列记录均应将未执行的实际宿主场景标为 `Not run`；该标记不表示功能已经验收。

---

## 4. 评审结论

- **结论**：**通过（Approved）**。
- **边界**：原评审记录模块基线未扩大债务；现行目标以项目文件及工作流为准，CI 编译不替代 CAD 宿主验收。
