# CadDiagnostics 开源诊断项目评估

> 状态：`proposal`，第一优先级及 P2 已实施，P2 后候选仍待独立评估<br>
> 评估日期：2026-08-04<br>
> Fs.Fox.CAD 基线：`main@18c9cb5e2828655db26e9ed303eef82d1e60491e`<br>
> 跟踪：[父路线 #124](https://github.com/FsDiG/Fs.Fox.CAD/issues/124)、
> [第一优先级 #131](https://github.com/FsDiG/Fs.Fox.CAD/issues/131)、
> [第二优先级 #132](https://github.com/FsDiG/Fs.Fox.CAD/issues/132)

本文记录 GitHub 上 CAD 调试、对象检查、诊断和宿主测试项目的评估基线，
以及可以进入 Fs.Fox.CAD Diagnostics 的明确边界。它不是第三方项目的功能
汇总，也不授权整体复制某个仓库。后续采用任何候选代码前，仍须重新核对
实时 revision、许可、当前源码和 AutoCAD SDK 差异。

## 1. 评估方法与当前基线

本轮围绕 `AutoCAD inspector`、`MgdDbg`、`ObjectARX debug`、
`AutoCAD diagnostic` 和 `AutoCAD test framework` 等方向检索，并进一步
检查已知项目的源码、提交历史、项目依赖和根目录许可。筛选标准依次为：

1. 是否直接补足对象检查或故障定位能力；
2. 是否能同时适配 AutoCAD 2019 / 2025 两个现有目标；
3. 是否保持诊断 DLL 独立、只读和无宿主配置副作用；
4. 是否与当前 MgdDbg 功能重复；
5. 许可和来源是否允许维护派生代码。

当前实现已经具备以下能力，因此不能把同类实现误判为新增价值：

- 原 MgdDbg 的 Snoop、Reactors、ObjTests、Prompts、DwgStats 和 WinForms；
- `GenericPropGrid` 反射浏览以及对象/类型继续下钻；
- ObjectId、DBObject、数据库、扩展字典、References to / Referenced by；
- 块内实体、块引用、布局视口、DataTable、Polyline 和 Mesh 子对象集合；
- AutoCAD 2019 / 2025 独立 DLL 与版本化输出目录。

## 2. 决策摘要

| 优先级 | 来源 | 决定 | 原因 |
| --- | --- | --- | --- |
| P1 | ADN-DevTech/MgdDbg | 采纳两项确定性缺陷修复 | 与当前派生代码同源，改动小，异常原因可由装箱类型直接证明。 |
| P2 | gileCAD/Gile.Inspector | 只迁移缺失的只读 CAD 语义下钻 | 动态块允许值、注释比例和 Hatch loop 有明确增量；现有 UI、反射浏览和引用关系不重复迁移。 |
| P3 | JordanMarr/autodesk-dll-inspector | 仅保留独立能力候选 | ClrMD 外部进程附加能诊断程序集冲突，但会引入较重依赖和另一种交付形态，不应塞入当前插件 DLL。 |
| 暂缓 | PropertyInspector、CadAddinManager | 不迁移 | 依赖 AutoCAD 内部 COM 或动态重载语义，与多平台诊断和安全生命周期边界冲突。 |
| 排除 | ARXDBG、ObjectARX-Natvis | 不进入托管诊断 DLL | 面向原生 ObjectARX/Visual Studio 调试，且部分项目缺少清晰许可。 |
| 路由到 #40 | AcadTestRunner、CADtest、RxBim.AcadTests 等 | 不由 CadDiagnostics 吸收 | 属于宿主测试编排与结果协议，不是对象检查能力。 |

## 3. 候选项目记录

### 3.1 ADN-DevTech/MgdDbg

- 仓库：<https://github.com/ADN-DevTech/MgdDbg>
- 评估 revision：`007b80b4f82c`
- 许可：MIT，Copyright (c) 2016 Autodesk Developer Network
- 关系：当前 CadDiagnostics 的直接上游之一

当前归档基线已经包含 DXF code 90 的 `Int32` 特殊处理，与上游提交
`7c7b8c4a919b73ff63b543ddc896ce22a0de5e3a` 的核心行为一致。本仓库导入时
没有保存精确上游 revision，因此这里只记录行为比对，不反向推断导入来源。

上游提交 `599643a7985f7682e41bc3734c7bb18c5ae7ec88` 仍有两项当前实现缺失的修复：

- 补齐 code 5/105 的 `handle/string)` 标签右括号；
- code 91–99 的 `TypedValue.Value` 直接解箱为 `Int32`，避免把装箱
  `Int32` 当作 `Int64` 解箱而触发 `InvalidCastException`。

上游的 .NET 8 和 SDK 项目迁移不再导入。当前仓库已有自己的多目标项目、
输出契约、资源嵌入和生命周期适配，覆盖范围更符合本仓库边界。

### 3.2 gileCAD/Gile.Inspector

- 仓库：<https://github.com/gileCAD/Gile.Inspector>
- 评估 revision：`1d7d95f70c32`
- 许可：MIT，Copyright (c) 2022 Gilles Chanteau
- 定位：反射优先的轻量 AutoCAD Database / Entity 检查器

项目最有价值的是对 CAD 特殊对象补充“可继续检查”的语义集合，而不是其
WPF 窗口或完整 ViewModel。与当前实现逐项比对如下：

| Gile.Inspector 能力 | 当前状态 | 决定 |
| --- | --- | --- |
| 通用反射属性列表和继续下钻 | 已有 `GenericPropGrid` 与 `ObjectUnknown` | 不迁移 UI 或完整反射框架。 |
| References to / Referenced by | 已有 `ReferenceFiler` 和两个 Snoop 数据入口 | 不重复迁移。 |
| 块内实体、块引用、布局视口、DataTable | 当前显式 collector 已覆盖 | 不重复迁移。 |
| Polyline2d/3d、PolygonMesh、PolyFaceMesh 顶点 | 当前已提供 ObjectId 集合下钻 | 不重复迁移。 |
| DynamicBlockReferenceProperty 允许值与状态 | 当前只显示 BlockId、描述、名称、类型和单位 | 纳入 P2-A。 |
| ObjectContextManager 注释比例集合 | 当前只显示 manager 对象，没有语义集合 | 纳入 P2-A。 |
| Hatch loop 集合 | 当前保留一段因 `NotApplicable` 而禁用的旧实现 | 纳入 P2-B，并要求逐项异常隔离。 |
| BoundaryRepresentation / Brep | 当前未引用对应边界表示程序集 | 暂缓；需单独评估依赖和 `IDisposable` 所有权。 |
| 关联阵列参数 | 当前未覆盖 | 暂缓；先确认 2019/2025 的对象生命周期和失败语义。 |

如果 P2 后续复制或派生实质性实现，必须同步更新
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) 并保留来源注释。当前 P1
没有复制 Gile.Inspector 代码，因此本轮不增加其许可文件副本。

### 3.3 JordanMarr/autodesk-dll-inspector

- 仓库：<https://github.com/JordanMarr/autodesk-dll-inspector>
- 评估 revision：`f5eeeb0f779a`
- 许可：MIT，Copyright (c) 2026 Jordan Marr
- 定位：使用 Microsoft.Diagnostics.Runtime（ClrMD）从外部只读附加到
  AutoCAD / Revit 进程，列出 CLR、程序集版本和路径

程序集冲突诊断具有实际价值，但该实现是 .NET 8 自包含 EXE，发布体积约
70 MB，并要求进程附加权限。它不适合直接合并到 AutoCAD 2019 / 2025 的
同一个插件 DLL，也不应让 CadDiagnostics 新增 ClrMD 运行时依赖。

后续如确有需求，应在 #124 下建立独立工具 Issue，先比较两种方案：

1. 插件内使用 `AppDomain.CurrentDomain.GetAssemblies()` 输出当前宿主已加载
   程序集，不新增依赖，但只能观察当前进程和运行时可见范围；
2. 独立 EXE 使用 ClrMD 附加，覆盖更完整，但采用独立交付和权限模型。

### 3.4 不建议迁移的类库和调试工具

| 项目与评估 revision | 许可 | 结论 |
| --- | --- | --- |
| [ActivistInvestor/AcMgdLib](https://github.com/ActivistInvestor/AcMgdLib) `82e71a4e4759` | MIT | 通用 AutoCAD 扩展库，源码自身也声明高度互相依赖、接近 all-or-nothing；不是诊断组件，不引入。 |
| [ActivistInvestor/PropertyInspector](https://github.com/ActivistInvestor/PropertyInspector) `d43226c68d9d` | MIT | 封装 AutoCAD 内部 Properties Palette COM；内部接口、命令取消和 COM 生命周期风险较高，且不利于 ZWCAD/GstarCAD 路线。 |
| [chuongmep/CadAddinManager](https://github.com/chuongmep/CadAddinManager) `cea835872e94` | MIT | 解决插件动态加载和调试周转，不是对象诊断；卸载/重载语义会扩大当前生命周期边界。 |
| [comphoner/ARXDBG](https://github.com/comphoner/ARXDBG) `1cbbc4741c4c` | 根目录无明确许可 | 原生 ObjectARX 学习/调试项目，不能迁入托管多版本 DLL。 |
| [ADN-DevTech/ObjectARX-Natvis](https://github.com/ADN-DevTech/ObjectARX-Natvis) `d72c0a24e2b5` | 根目录无明确许可 | Visual Studio 原生类型可视化，与 NETLOAD 诊断工具的交付边界不同。 |

### 3.5 宿主测试项目

以下项目提供 Core Console、NUnitLite、进程启动或多版本测试编排思路，但应
归入 [真实 CAD 宿主验收 #40](https://github.com/FsDiG/Fs.Fox.CAD/issues/40)，
不与对象检查 DLL 绑定：

| 项目 | 评估 revision | 许可/风险 | 本轮决定 |
| --- | --- | --- | --- |
| [wtertinek/AcadTestRunner](https://github.com/wtertinek/AcadTestRunner) | `1590a28eed47` | MIT；2016 年 CoreConsole runner | 仅参考启动思路。 |
| [CADbloke/CADtest](https://github.com/CADbloke/CADtest) | `869d7a2bb205` | MIT；旧版 NUnitLite / AutoCAD | 仅参考命令和结果输出。 |
| [ReactiveBIM/RxBim.AcadTests](https://github.com/ReactiveBIM/RxBim.AcadTests) | `87f998babe13` | 活跃的多版本编排；根目录未发现明确许可 | 不复制代码，只在 #40 需要时重新评估协议。 |
| JPPGroup/AcTestFramework、MichaelLiddiard/AutocadNUnitTester | `78800d7336e6`、`9fa45ba31a67` | 根目录未发现明确许可，且技术基线较旧 | 不迁移。 |

## 4. 第一优先级实施结果

第一优先级由 [Issue #131](https://github.com/FsDiG/Fs.Fox.CAD/issues/131)
跟踪，只修改维护中的 `CADDiagnosticsShared/Snoop/CollectorExts/DbMisc.cs`：

- 修正 handle 标签；
- 直接解箱 code 91–99 的 `Int32`，并在代码中说明装箱类型约束；
- 不修改 `third_party/Autodesk.MgdDbg` 归档快照。

验证结果：

| 目标 | 结果 |
| --- | --- |
| AutoCAD 2019 / .NET Framework 4.8 / Release x64 | Build passed |
| AutoCAD 2025 / .NET 8 / Release x64 | Build passed；保留 2 个既有 `MSB3825` WinForms 资源警告 |
| CAD 宿主加载 | `Not run`，按本轮边界不执行 |

## 5. 第二优先级详细拆分

第二优先级由 [Issue #132](https://github.com/FsDiG/Fs.Fox.CAD/issues/132)
跟踪，并按 P2-A、P2-B 两个独立 PR 依次实施。

### P2-A：动态块属性与注释比例（已实施）

P2-A 只修改维护中的只读 collector：

1. `DynamicBlockReferenceProperty` 已补充 Value、ReadOnly、Show 和
   `GetAllowedValues()`；原有属性与新增属性均逐 getter 隔离异常，允许值复用
   `Snoop.Data.Enumerable` 下钻。
2. `ObjectContextManager` 已提供 `ACDB_ANNOTATIONSCALES` 集合入口；仅枚举
   数据库拥有的 manager 和 context collection，不创建、删除、切换或处置
   context。
3. 单个 getter、集合获取或枚举失败会生成 `Snoop.Data.Exception`，其余属性
   继续收集，不静默吞错。
4. 未新增公共类型、命令、WPF 依赖或 Fs.Fox.AutoCad 引用。实现仅采用
   Gile.Inspector 的行为思路，没有复制其 wrapper 或 UI 代码。

验证结果：

| 目标 | 结果 |
| --- | --- |
| AutoCAD 2019 / .NET Framework 4.8 / Release x64 | Build passed |
| AutoCAD 2025 / .NET 8 / Release x64 | Build passed；保留 2 个既有 `MSB3825` WinForms 资源警告 |
| 迁移边界与输出静态检查 | Passed |
| 动态块与注释比例 CAD 宿主下钻 | `Not run`，按本阶段边界不执行 |

### P2-B：Hatch loop（已实施）

P2-B 使用独立 PR 恢复原 MgdDbg 中被禁用的集合入口，并补齐原本不可达的
`HatchLoop` collector：

1. 按 loop index 调用 `GetLoopAt()`，不按 `HatchLoopTypes` 过滤；成功的
   polyline 与非 polyline loop 都进入现有 `Snoop.Data.ObjectCollection`。
2. 每个 loop 独立捕获 AutoCAD 宿主异常，以带 index 的
   `Snoop.Data.Exception` 显示；一个失败项不会丢失其他 loop 或整个实体信息。
3. `HatchLoop` 下钻按 `IsPolyline` 只读取匹配的 `Polyline` 或 `Curves` 集合，
   避免主动触发另一种边界访问器的 `NotApplicable`。
4. `HatchLoop` 在两个目标 SDK 中均为不实现 `IDisposable` 的托管值对象；窗口
   仅在自身生命周期内保留结果，不处置数据库拥有的 Hatch 或事务对象。
5. 实现基于仓库已有的禁用代码和 collector 修正，仅采用 Gile.Inspector 的
   集合行为思路，没有复制其 `HatchLoopCollection` wrapper 或 UI 代码。

验证结果：

| 目标 | 结果 |
| --- | --- |
| AutoCAD 2019 / .NET Framework 4.8 / Release x64 | Build passed |
| AutoCAD 2025 / .NET 8 / Release x64 | Build passed |
| 迁移边界与输出静态检查 | Passed |
| polyline / curve loop 与逐项异常 CAD 宿主场景 | `Not run`，按本阶段边界不执行 |

### P2 之后才重新评估

- Brep：先确认额外 Autodesk 程序集是否会进入输出、不同 SDK 的 API 差异，
  再设计窗口关闭时的确定性释放；
- 关联阵列：先确认返回参数对象的事务/宿主所有权以及无关联数据时的异常；
- 程序集冲突报告：在插件内轻量命令与 ClrMD 独立 EXE 之间先做架构选择；
- 自动宿主测试：继续由 #40 维护，不通过 CadDiagnostics 暗中启动 CAD。

## 6. 后续评估记录规则

1. 新候选先追加精确仓库 URL、revision、评估日期和许可，再写迁移建议。
2. 先与当前源码逐项去重；“另一个项目也有”不是迁移理由。
3. 复制或派生实质性代码时，同一 PR 更新第三方声明和源文件来源注释。
4. AutoCAD 内部 API、COM、native interop、事务或 `IDisposable` 对象必须单独
   写明所有权和失败语义，不能仅凭编译通过采纳。
5. 每个实施阶段使用独立分支和 PR；未执行宿主场景统一记录为 `Not run`。
