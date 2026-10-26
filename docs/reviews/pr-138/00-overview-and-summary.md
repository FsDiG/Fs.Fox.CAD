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
| [**03. 嵌套块内曲线求交**](03-block-curve-intersections.md) | `BlockIntersectionExBeta.cs` | 父子变换矩阵乘法累乘、DFS 栈环路检测、白名单原生安全曲线、欧氏球形无链式去重、临时对象释放、**复用 ValidId 防御 Erased 图元**。 | **微调后通过 (Finding 1)** |
| [**04. 空间排序与范围组合**](04-spatial-ordering-and-extents.md) | `OrderingBetaCore.cs`<br>`PointOrderingBeta.cs`<br>`CurveOrderingExBeta.cs`<br>`ExtentGroupingBeta.cs` | 固定锚点行列分组（防链式漂移）、严格传递性比较器、曲线实际里程复合键保序、原始矩形最短距离并查集（防虚假桥接）。 | **通过 (Approved)** |
| [**05. 三维几何与多面网格**](05-triangle-and-polyface-mesh.md) | `TriangleMathBeta.cs`<br>`Triangle3dBeta.cs`<br>`PolyFaceMeshDataBeta.cs`<br>`PolyFaceMeshBuilderBeta.cs` | Neumaier 补偿求和保留 Z 轴、无量纲退化相对容差、坡度 [0,90) / 坡向 360/0 舍入边界、多面网格惰性序列预验证、子对象 `using var` 官方范式。 | **通过 (Approved)** |
| [**06. 文字度量与 DWG 预览**](06-text-metrics-and-dwg-preview.md) | `TextMetricsBeta.cs`<br>`DwgPreviewCoreBeta.cs`<br>`DwgPreviewBeta.cs` | 单行文字度量 SwitchDatabase 逆序恢复、字体加载成功位校验、多宿主条件编译即时拦截、DWG 预览 AcGs 设备与内核配对归还。 | **通过 (Approved)** |
| [**07. 架构治理与 CI 验证**](07-architecture-build-and-ci.md) | `CADShared.projitems`<br>`Verify-CADSharedModuleMap.ps1`<br>`build-and-deploy.yml`<br>`Baseline.json` | 159 项编译项模块归属（Geometry/Database/Application）、模块基线零债务膨胀、纯计算核心测试 CI 前置熔断、宿主 Not run 准则。 | **通过 (Approved)** |

---

## 二、 审查发现与问题汇总清单（经复核修正）

本次全量独立代码评审严格排除主观经验推断，结合 AutoCAD 底层运行时机制与官方 SDK 规范进行二次技术复核。最终提炼出 1 项 P2 阻塞缺陷及 1 项 P3 规范说明，并纠正了初审中对网格生命周期的误判。

### 1. 唯一建议合入前修复的改进项 [P2 鲁棒性改进 / 建议合入前修复]
- **问题**：[`BlockIntersectionExBeta.cs#L82, L122`](file:///D:/MakeX/Fs.Fox.CAD/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs#L82) 在遍历块定义时，未过滤已被删除的图元（`id.IsErased`）。
- **影响**：若图纸在当前会话中对块内实体执行过删除且尚未 Purge/保存重开，`definition.Cast<ObjectId>()` 会枚举出带有 `IsErased` 标记的图元；调用未开启 `openErased` 标志的 `transaction.GetObject(id, OpenMode.ForRead)` 会直接触发 `Autodesk.AutoCAD.Runtime.Exception: eWasErased` 导致求交崩溃。
- **一致性发现**：在 `BlockIntersectionExBeta.cs` 的 L135-L136 中，作者已封装了完善的有效性断言函数：
  ```csharp
  private static bool ValidId(ObjectId id, Database database) =>
      !id.IsNull && id.IsValid && !id.IsErased && id.Database == database;
  ```
  该函数在根块、目标曲线、空间、块定义校验中被严格使用，但在遍历子图元时被意外遗漏。
- **推荐修复 Diff（方案 A：弹栈校验，O(1) 零分配，最推荐）**：
  ```diff
  --- a/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  +++ b/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  @@ -80,7 +80,9 @@ public static class BlockIntersectionExBeta
                   continue;
               }

  -            var child = transaction.GetObject(frame.Children[frame.NextIndex++], OpenMode.ForRead);
  +            var childId = frame.Children[frame.NextIndex++];
  +            if (!ValidId(childId, database)) continue;
  +            var child = transaction.GetObject(childId, OpenMode.ForRead);
               if (child is BlockReference nested)
               {
                   PushDefinition(nested, frame.ToWorld);
  ```
- **备选修复 Diff（方案 B：入栈预过滤，快照阶段统一过滤）**：
  ```diff
  --- a/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  +++ b/src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs
  @@ -121,3 +121,3 @@ public static class BlockIntersectionExBeta
               stack.Push(new TraversalFrame(definitionId, parentToWorld * reference.BlockTransform,
  -                definition.Cast<ObjectId>().ToArray()));
  +                definition.Cast<ObjectId>().Where(id => ValidId(id, database)).ToArray()));
  ```

### 2. 生命周期复核结论：`PolyFaceMeshBuilderBeta` 的 `using var` 确认为官方推荐最佳实践（非缺陷）
- **复核说明**：初审中曾根据常规经验法则推测网格及子对象上的 `using var` 存在跨宿主悬空或重复释放风险。
- **底层机制确证**：在 AutoCAD .NET 内部，`PolyFaceMeshVertex` 与 `FaceRecord` 均为 `DisposableWrapper`。当调用 `transaction.AddNewlyCreatedDBObject(item, true)` 后，内部的非托管所有权标志 `_autoDelete` 即被置为 `false`，非托管 C++ `AcDbObject` 已完全由 Database/Transaction 接管。此时作用域退出触发的 `Dispose()` **仅释放托管包装器本身并调用 `GC.SuppressFinalize(this)`**，绝不会调用 C++ 的 `delete` 销毁数据库驻留对象。
- **性能价值**：网格构造可达 32,767 个顶点与面，若不使用 `using var` 显式及时释放，多达 6.5 万个托管包装会被推入 .NET GC 终结器队列导致严重的 GC 阻塞停顿。Autodesk 官方《AutoCAD .NET Developer's Guide》中的 `CreatePolyfaceMeshExample` 亦明确采用此模式。因此该写法为正向最佳实践，予以肯定。

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

### 综合裁定：**【建议在合并 Finding 1 推荐微调修复后，合入 main 分支 (Mergeable after addressing Finding 1)】**

**裁定依据**：
1. **第一性原理深度贯穿**：彻底消除了历史遗留的人为扰动键、无界重试、有损折线逼近等缺陷，纯算法内核与 CAD SDK 彻底解耦，可维护性极高；
2. **所有权严格自洽**：全面履行只读借用语义，资源申请与释放顺序严密，网格落库采用了符合 Autodesk 官方高性能规范的子对象即时 Dispose 范式；
3. **架构与多目标治理合规**：16 个生产源文件在 7 个目标下的条件编译边界精确，模块基线未发生任何债务膨胀；
4. **唯一改进项极度轻微且定位清晰**：仅需在块求交遍历中复用 `ValidId` 跳过 Erased 图元，即可杜绝图元删除时的崩溃风险。
