# Fs.Fox.CAD PR #138 独立代码评审总览与分模块索引

**评审日期**：2026-10-26
**评审对象**：[PR #138 — feat: migrate selected ZFGK CAD helpers as Beta APIs](https://github.com/FsDiG/Fs.Fox.CAD/pull/138)
**待审工作目录**：`D:\MakeX\Fs.Fox.CAD`
**基线（Base）**：`5729ef6e97a4f0e0f4132da86fd75495b2003079` (`main`)
**待审（HEAD）**：`40640258974d30eb3f72078828b204b0521e852c` (`codex/zfgk-beta-migration`)
**改动规模**：11 个提交，53 个变更文件，新增 5352 行、删除 19 行（16 个生产源文件）
**交付策略**：分模块深度审查、建立独立 Markdown 评审卷宗、保持工作区只读、在 Git 存储中保留评审结果供后续清理。

---

## 一、 分模块专项评审报告索引

本次代码评审将 PR #138 的 16 个生产源文件、5 组核心测试、6 组宿主测试及构建流拆分为 7 个专项模块展开深入审查：

| 专项报告编号与链接 | 审查能力域与涉及源文件 | 核心审查重点 | 模块审查结论 |
| :--- | :--- | :--- | :---: |
| [**01. 多段线核心操作**](01-polyline-core-and-fillet.md) | `PolylineCoreBeta.cs`<br>`PolylineExBeta.cs` | 借用语义、反向对合保序、半容差带直线清理与折返保护、唯一端点变宽连接桥接、全线直线整角圆角两端预算检查。 | **通过 (Approved)** |
| [**02. 含弧闭合多段线点关系**](02-polyline-containment.md) | `PolylineContainmentCoreBeta.cs`<br>`PolylineRegionExBeta.cs` | 展开圆方程分离推导、极值矢高有理化、最多 3 段 Y 单调弧划分、半开 Y 穿透计数、偶奇填充、OCS 平面距离解耦。 | **通过 (Approved)** |
| [**03. 嵌套块内曲线求交**](03-block-curve-intersections.md) | `BlockIntersectionExBeta.cs` | 父子变换矩阵乘法累乘、DFS 栈环路检测、白名单原生安全曲线、欧氏球形无链式去重、临时对象释放、**已删除图元过滤缺陷**。 | **修复后通过 (Finding 1)** |
| [**04. 空间排序与范围组合**](04-spatial-ordering-and-extents.md) | `OrderingBetaCore.cs`<br>`PointOrderingBeta.cs`<br>`CurveOrderingExBeta.cs`<br>`ExtentGroupingBeta.cs` | 固定锚点行列分组（防链式漂移）、严格传递性比较器、曲线实际里程复合键保序、原始矩形最短距离并查集（防虚假桥接）。 | **通过 (Approved)** |
| [**05. 三维几何与多面网格**](05-triangle-and-polyface-mesh.md) | `TriangleMathBeta.cs`<br>`Triangle3dBeta.cs`<br>`PolyFaceMeshDataBeta.cs`<br>`PolyFaceMeshBuilderBeta.cs` | Neumaier 补偿求和保留 Z 轴、无量纲退化相对容差、坡度 [0,90) / 坡向 360/0 舍入边界、多面网格惰性序列预验证、子对象生命周期。 | **通过 (Approved)** |
| [**06. 文字度量与 DWG 预览**](06-text-metrics-and-dwg-preview.md) | `TextMetricsBeta.cs`<br>`DwgPreviewCoreBeta.cs`<br>`DwgPreviewBeta.cs` | 单行文字度量 SwitchDatabase 逆序恢复、字体加载成功位校验、多宿主条件编译即时拦截、DWG 预览 AcGs 设备与内核配对归还。 | **通过 (Approved)** |
| [**07. 架构治理与 CI 验证**](07-architecture-build-and-ci.md) | `CADShared.projitems`<br>`Verify-CADSharedModuleMap.ps1`<br>`build-and-deploy.yml`<br>`Baseline.json` | 159 项编译项模块归属（Geometry/Database/Application）、模块基线零债务膨胀、纯计算核心测试 CI 前置熔断、宿主 Not run 准则。 | **通过 (Approved)** |

---

## 二、 审查发现与问题汇总清单

本次全量独立代码评审未发现任何 P0 致命级（如缓冲区溢出、内存越界、永久破坏图形文件）缺陷。共提炼出 1 项 P2 阻塞缺陷、1 项 P2 待宿主核验风险及 2 项 P3 规范说明。

### 1. 唯一建议合入前修复的阻塞项 [P2 缺陷]
- **问题**：[`BlockIntersectionExBeta.cs#L82, L122`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs#L82) 在遍历块定义时，未过滤已被删除的图元（`id.IsErased`）。
- **影响**：若图纸在当前会话中对块内实体执行过删除且尚未 Purge/保存重开，调用该方法会触发 AutoCAD 原生异常 `Autodesk.AutoCAD.Runtime.Exception: eWasErased` 导致求交崩溃。
- **推荐修复 Diff**：
  ```diff
  --- a/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  +++ b/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  @@ -80,7 +80,9 @@ public static class BlockIntersectionExBeta
                   continue;
               }

  -            var child = transaction.GetObject(frame.Children[frame.NextIndex++], OpenMode.ForRead);
  +            var childId = frame.Children[frame.NextIndex++];
  +            if (childId.IsErased) continue;
  +            var child = transaction.GetObject(childId, OpenMode.ForRead);
               if (child is BlockReference nested)
               {
                   PushDefinition(nested, frame.ToWorld);
  ```

### 2. 跨宿主重点探针核验项 [P2 风险 / 待宿主确认]
- **关注点**：[`PolyFaceMeshBuilderBeta.cs#L56-L75`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Meshes/PolyFaceMeshBuilderBeta.cs#L56-L75) 在实体已通过 `AddNewlyCreatedDBObject(..., true)` 转交事务托管后，依然保留了对 `PolyFaceMesh`、`PolyFaceMeshVertex`、`FaceRecord` 的 `using var` 显式释放。
- **策略**：保持当前实现不变（在 AutoCAD 原生 SDK 下语义合法），将其列为后续真实 CAD 宿主验收的重点探针，核验在国产宿主（ZWCAD、GStarCAD）下事务执行 `Commit()` 后的持久化稳定性。

### 3. 规范与边界说明 [P3]
- **点关系极端尺度**：非零绝对凸度显式受限于 `[1e-8, 1e8]`，超大坐标系下设有浮点防溢出守卫（见专项报告 02）。
- **生命周期范例**：`TextMetricsBeta` 的逆序 WorkingDatabase 恢复与 `DwgPreviewBeta` 的 AcGs 释放模型展示了极高的工业级资源管理素养（见专项报告 06）。

---

## 三、 本地实际验证执行证据

评审人在待审工作目录 `D:\MakeX\Fs.Fox.CAD` 独立执行并记录了以下实测结果：

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
   - 依据协作规则与 Issue #40 隔离协议，`TestShared` 中的 6 个 CAD 命令用例均严格保持为 **`Not run`**。

---

## 四、 总体评审结论

### 综合裁定：**【建议在合并 Finding 1 推荐修复后，合入 main 分支 (Mergeable after addressing Finding 1)】**

**裁定依据**：
1. **第一性原理深度贯穿**：彻底消除了历史遗留的人为扰动键、无界重试、有损折线逼近等缺陷，纯算法内核与 CAD SDK 彻底解耦，可维护性极高；
2. **所有权严格自洽**：全面履行只读借用语义，资源申请与释放顺序严密，无句柄泄漏风险；
3. **架构与多目标治理合规**：16 个生产源文件在 7 个目标下的条件编译边界精确，模块基线未发生任何债务膨胀；
4. **唯一阻断项极度轻微且定位清晰**：仅需在块求交遍历中加入单行 `if (childId.IsErased) continue;` 防御，即可彻底杜绝图元删除时的崩溃风险。
