# Fs.Fox.CAD PR #138 独立代码评审总览与分模块索引

> 状态：historical；针对 `4064025` 的专项静态评审快照，2026-10-26 更正。<br>
> 仅供追溯，不是现行技术契约，也不进入产品站点或默认实现依据。使用接口请转到[当前 Beta 指南](../../guides/beta-capabilities.md)。

后续从 `979b799` 开始的全部 16 个核心文件重新审查，发现并修复了新的数值缺陷；详见[完整核心审查与修正](../../beta-implementation-notes.md#完整核心审查与修正)。本分册保留原评审的范围与处置，不代表当前完整审查结论。

**评审日期**：2026-10-26
**评审对象**：[PR #138 — Beta CAD 能力](https://github.com/FsDiG/Fs.Fox.CAD/pull/138)
**待审工作目录**：`D:\MakeX\Fs.Fox.CAD`
**基线（Base）**：`5729ef6e97a4f0e0f4132da86fd75495b2003079` (`main`)
**待审（HEAD）**：`40640258974d30eb3f72078828b204b0521e852c`
**改动规模**：11 个提交，53 个变更文件，新增 5352 行、删除 19 行（16 个生产源文件）
**记录边界**：保留原评审范围与历史验证记录；以下结论已按静态证据更正，不能代替真实宿主验收。

---

## 一、 分模块专项评审报告索引

本次代码评审将 PR #138 的 16 个生产源文件、5 组核心测试、6 组宿主测试及构建流拆分为 7 个专项模块展开深入审查：

| 专项报告编号与链接 | 审查能力域与涉及源文件 | 核心审查重点 | 模块审查结论 |
| :--- | :--- | :--- | :---: |
| [**01. 多段线核心操作**](01-polyline-core-and-fillet.md) | `PolylineCoreBeta.cs`<br>`PolylineExBeta.cs` | 借用语义、反向对合保序、半容差带直线清理与折返保护、唯一端点变宽连接桥接、全线直线整角圆角两端预算检查。 | **通过 (Approved)** |
| [**02. 含弧闭合多段线点关系**](02-polyline-containment.md) | `PolylineContainmentCoreBeta.cs`<br>`PolylineRegionExBeta.cs` | 展开圆方程分离推导、极值矢高有理化、最多 3 段 Y 单调弧划分、半开 Y 穿透计数、偶奇填充、OCS 平面距离解耦。 | **通过 (Approved)** |
| [**03. 嵌套块内曲线求交**](03-block-curve-intersections.md) | `BlockIntersectionExBeta.cs` | 父子变换矩阵乘法累乘、DFS 栈环路检测、曲线白名单、欧氏去重、临时对象释放、默认删除过滤与删除／恢复回归。 | **原 P2 撤销；宿主 Not run** |
| [**04. 空间排序与范围组合**](04-spatial-ordering-and-extents.md) | `OrderingBetaCore.cs`<br>`PointOrderingBeta.cs`<br>`CurveOrderingExBeta.cs`<br>`ExtentGroupingBeta.cs` | 固定锚点行列分组（防链式漂移）、严格传递性比较器、曲线实际里程复合键保序、原始矩形最短距离并查集（防虚假桥接）。 | **通过 (Approved)** |
| [**05. 三维几何与多面网格**](05-triangle-and-polyface-mesh.md) | `TriangleMathBeta.cs`<br>`Triangle3dBeta.cs`<br>`PolyFaceMeshDataBeta.cs`<br>`PolyFaceMeshBuilderBeta.cs` | Neumaier 补偿求和保留 Z 轴、无量纲退化相对容差、坡度 [0,90) / 坡向 360/0 舍入边界、多面网格惰性序列预验证、显式释放范围。 | **静态审查无新增缺陷；宿主 Not run** |
| [**06. 文字度量与 DWG 预览**](06-text-metrics-and-dwg-preview.md) | `TextMetricsBeta.cs`<br>`DwgPreviewCoreBeta.cs`<br>`DwgPreviewBeta.cs` | 单行文字度量 SwitchDatabase 逆序恢复、字体加载成功位校验、多宿主条件编译即时拦截、DWG 预览 AcGs 设备与内核配对归还。 | **通过 (Approved)** |
| [**07. 架构治理与 CI 验证**](07-architecture-build-and-ci.md) | `CADShared.projitems`<br>`Verify-CADSharedModuleMap.ps1`<br>`build-and-deploy.yml`<br>`Baseline.json` | 159 项编译项模块归属（Geometry/Database/Application）、模块基线零债务膨胀、纯计算核心测试 CI 前置熔断、宿主 Not run 准则。 | **通过 (Approved)** |

---

## 二、 审查发现与问题汇总清单（经复核修正）

原评审将块定义默认枚举已删除实体定为 P2，但该触发前提与已检查的 AutoCAD 运行库实现不符。当前没有从这批意见中确认必须修改生产代码的缺陷。

### 1. 撤销原 Finding 1，保留有价值的删除场景回归

- 本机 AutoCAD 2019／2026 运行库的静态检查显示，`BlockTableRecord` 构造时设置 `SkipDeleted=true`；`GetEnumerator` 和枚举器的 `start`／`step` 沿用该值。`IncludingErased` 返回副本并将此值置为 false，而生产查询未使用该入口。
- [官方原生迭代器说明](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-RefGuide/files/OREF-AcDbBlockTableRecord__newIterator_AcDbBlockTableRecordIterator___bool_bool_const.html)也定义 `skipDeleted=true` 默认跳过已删除对象。因此撤销原 P2 及两份 `ValidId` 预过滤 Diff，不将 `eWasErased` 异常推断为进程崩溃。
- 补充已提交删除、当前事务内删除、恢复后结果与旧快照独立三个回归场景。它们用于后续宿主验证，当前均为 `Not run`；AutoCAD 静态证据不代表其他厂商已经验证。

### 2. 保留网格 `using`，撤销未经证实的机制与性能推断

[官方网格示例](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-DevGuide-Managed/files/GUID-6B49EE98-D513-46EB-9D7A-282F70A45B24.htm)对网格和面记录使用 `using`，顶点循环没有相同写法。保留现有生产实现，但不据此声称所有宿主的内部所有权机制完全相同，也没有基准支持“必然产生数万终结器并严重停顿”的判断。

### 3. 修正规范描述，不扩大功能范围

- 点关系仍明确限制非零绝对凸度为 `[1e-8, 1e8]`。有限性守卫不能检测所有仍为有限值的舍入误差，不能根据某一地理坐标量级断言必然抛错。
- ZWCAD 2025 的现行生产与测试项目目标框架为 `net48`，已修正专项报告 07 的表述。
- 各模块的历史 Approved 只表示当时静态评审意见，不代表宿主验收或最终合入授权。

---

## 三、 本地实际验证执行证据

以下保留原评审者针对 `4064025` 记录的结果，本轮文档更正没有重跑五组核心测试：

1. **架构与模块基线检查**：
   - 命令：`pwsh -NoProfile -File 'Build/Verify-CADSharedModuleMap.ps1'`
   - 结果：`CADShared module verification passed: 159 items, 42 debt-tagged files, 17 boundary findings.`（零新增架构债务）。
2. **纯计算核心自动化回归（5 组工程、全部 PASS）**：
   - `PolylineBeta.CoreTests`：19 场景、**2,929 项断言**全部通过；
   - `PolylineRegionBeta.CoreTests`：10 场景、**80,885 项断言**全部通过（涵盖密集解析圆盘真值裁切对照）；
   - `OrderingBeta.CoreTests`：**29 项检查**全部通过；
   - `GeometryBeta.CoreTests`：**85 项断言**全部通过；
   - `DwgPreviewBeta.CoreTests`：**55 项检查**全部通过。
3. **真实 CAD 宿主状态**：
   - 依据协作规则与 Issue #40 隔离协议，`TestShared` 中的真实 CAD 场景均保持为 **`Not run`**。

本轮增量验证单独记录：`TestAcad2025` Debug x64 编译通过（0 警告、0 错误）；模块检查通过（159 items、42 debt-tagged files、17 boundary findings）；与 `6a2dc10` 比较，生产源码及模块哈希基线没有变更。新增删除／恢复入口仅确认可编译，实际宿主仍为 `Not run`。

---

## 四、 总体评审结论

**原 P2 阻塞结论撤销，生产代码保持现状，补充删除／恢复回归。** 现有编译和自动检查可作为参考，真实 CAD 宿主验收及新增场景仍为 `Not run`。本卷宗不据此宣布全部工程条件满足，最终合入以 PR 当前提交的复核和检查结果为准。
