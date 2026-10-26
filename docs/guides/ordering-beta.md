# 排序与范围组合 Beta

> 状态：当前实现（Current），公共 API 保留 Beta<br>
> 稳定 ID：`guide.ordering-beta`<br>
> 受众：调用方、维护者、宿主验收人员<br>
> 复核日期：2026-10-26

这些入口返回独立的只读结果，不修改传入点、曲线、范围或数据库。调用方先使用现行事务与对象访问方式取得代表点或包围框，再按结果索引重排自己的业务集合；相同坐标、相同里程和重复范围都有各自的原始索引。

## API 与源码入口

| 需要 | API | 生产源码 |
| --- | --- | --- |
| 按行读取二维点 | `PointOrderingBeta.GetRowOrderIndices` | [PointOrderingBeta.cs](../../src/CADShared/Cad/Geometry/Ordering/PointOrderingBeta.cs) |
| 按列读取二维点 | `PointOrderingBeta.GetColumnOrderIndices` | [PointOrderingBeta.cs](../../src/CADShared/Cad/Geometry/Ordering/PointOrderingBeta.cs) |
| 沿有限曲线排列 WCS 点 | `curve.GetPointOrderIndicesBeta` | [CurveOrderingExtensionsBeta.cs](../../src/CADShared/Cad/Database/Entities/Curves/CurveOrderingExtensionsBeta.cs) |
| 组合邻近 XY 包围框并追溯成员 | `ExtentsGroupingBeta.GroupByXYProximity` → `ExtentsGroupBeta` | [ExtentsGroupingBeta.cs](../../src/CADShared/Cad/Database/Entities/Bounds/ExtentsGroupingBeta.cs) |
| 维护排序和分组算法 | 内部纯计算核心 | [OrderingAndGroupingCoreBeta.cs](../../src/CADShared/Cad/Geometry/Ordering/OrderingAndGroupingCoreBeta.cs) |

普通类型使用 `Beta` 后缀，扩展方法在调用位置也保留 `Beta`。新入口不会替换已有稳定 API，也不自动选择实体的代表点。实体包围框仍由现有 `BoundingBox9`、`GeometricExtents` 等读取能力提供。

## 行列的确定规则

行分组始终从最高的尚未分组 Y 建立锚点，将距离锚点不超过 `rowTolerance` 的点归为同一行。列分组始终从最低的尚未分组 X 建立锚点，使用 `columnTolerance`。容差允许零，必须有限且非负；点坐标必须有限。

分组与读取方向分开：改变从上到下或从左到右的选项会改变输出次序，但不会改变成员归属。行内先比较 X，再比较 Y；列内先比较 Y，再比较 X，各坐标方向遵守对应参数。两个坐标完全相同时保留输入顺序。分组使用固定锚点，比较器只比较精确排序键，避免把不传递的“容差内相等”交给排序器。

例如 Y 为 `0、0.75、1.5`、行容差为 `1` 时，顶部锚点是 `1.5`，其组内包含 `0.75`；`0` 独立成行。不会通过 `0.75` 将三点链式归入同一行。输入置换不改变几何分组；完全相同坐标仍以新的输入顺序为稳定顺序。空集合返回空结果。

## 沿曲线的距离与所有权

`GetPointOrderIndicesBeta` 使用宿主 SDK 的三维最近点查询，参数 `extend` 固定为 `false`，再以曲线起点为零点计算实际里程。超过开曲线端点的投影采用有限曲线上的最近点。`descending` 只反转里程排序方向，相等里程仍按原始索引排列，不能人为扰动距离。

曲线必须具有有限、有序的参数域和正长度；`Ray`、`Xline` 明确不支持。闭合曲线保留原来的起点及接缝；自交曲线或多个等距最近点沿用 SDK 的选择结果，不承诺跨宿主选择同一个解。输入点采用 WCS，三个坐标必须有限。空点集合仍会校验曲线；有效曲线返回空结果。

方法借用曲线，不释放它、不打开事务、不升级对象写状态。调用方负责曲线的有效生命周期与必要的读取上下文。SDK 几何异常原样传播；非有限参数域或距离由本入口拒绝。各异常类型见源码 XML 注释。

## 邻近范围的连通关系

将每个输入 `Extents3d` 的 XY 投影视为轴对齐矩形，两个原始矩形之间的最短欧氏距离不大于 `maximumGap` 时建立邻近关系。距离为零允许相交或接触。A 邻近 B、B 邻近 C 时，三者属于同一连通分量，即使 A 与 C 不直接邻近。

全部关系基于原始范围；合并后的外包框不会用于吸收新成员。因此，L 形范围形成的外包框即使包含另一独立范围，也不会仅凭这种包含关系把它们合并。这里描述包围框关系，不能用来证明实体真实相交。

分组忽略 Z 间距，结果 `Bounds` 保留组内完整的 XYZ 范围。`SourceIndices` 按原输入顺序列出成员；同一个范围出现两次会保留两个索引。结果组按合并框的 `MinX、MinY、MinZ、MaxX、MaxY、MaxZ` 依次排序；相同框以组内最小原始索引决定次序。输入置换不影响连通分量及其合并框。

每一轴的坐标必须有限且满足最小值不大于最大值，零宽度或点范围有效。空集合返回空结果；距离必须有限且非负。实现使用成对判断和并查集，时间复杂度为 O(n²)、额外空间为 O(n)，适用于批次范围分析；大规模空间检索继续使用 Fox 现有空间索引。

距离比较先做轴向筛选，再以最大轴差归一化求斜距，避免平方溢出。次正规数阈值下先对轴差和阈值共乘精确的 `2^52`，避免最终斜距舍入成轴向间距；正常尺度的边界比较保留原规则。坐标与阈值仍受双精度表示限制，该处理不构成任意尺度的精确几何保证。

## 检查与宿主验收

[OrderingBeta.CoreTests](../../tests/OrderingBeta.CoreTests/OrderingBeta.CoreTests.csproj) 直接链接生产 `OrderingAndGroupingCoreBeta.cs`，不依赖 CAD SDK 或第三方测试包。运行命令：

```powershell
dotnet run --project tests/OrderingBeta.CoreTests/OrderingBeta.CoreTests.csproj -c Release
```

固定预期覆盖重复键、零容差、非传递容差、读取方向、输入置换、非有限值、范围间距、传递连通、合并框虚假桥接、重复范围及极大有限坐标。新增回归区分 `x=y=maximumGap=double.Epsilon` 的斜向间距与轴向阈值，并保留正常／次正规尺度的 `5-12-13` 边界。2026-10-26 实测 **30/30 项通过**，仅证明纯计算核心；下表保留原 29 项检查的历史记录。

[Test_OrderingBeta](../../tests/TestShared/TestOrderingBeta.cs) 是实际 CAD 的适配验收命令，使用临时曲线且不修改图纸。覆盖有限直线端点、WCS 点、相等里程、闭合曲线接缝、Ray 拒绝、行分组和范围 SDK 结果。命令程序集可编译不代表这些场景已经运行。

| 证据 | 2026-10-25 状态 |
| --- | --- |
| 生产核心自动检查 | 29/29 通过 |
| `TestAcad2025` Debug 与其引用的 Fox 目标 | 编译通过，0 警告、0 错误 |
| CADShared 模块校验 | 通过，本线新增 Geometry 2 项、Database 2 项，无新增边界债务 |
| AutoCAD / ZWCAD / GStarCAD 实际宿主 | `Not run` |

整体计划见[Beta 发展计划](../beta-development-plan.md)，构建矩阵与独立复核结果见[实现与验证记录](../beta-implementation-notes.md#验证记录)。实际宿主执行遵守 [HostAcceptance](../../tools/HostAcceptance/README.md) 的矩阵和隔离要求，另行记录产品、完整版本、测试程序集、提交和场景结果；未经运行的项目继续保持 `Not run`。
