# 多段线几何操作 Beta

> 状态：current；说明 Beta API 的行为约定，不代表真实 CAD 宿主已经验收。<br>
> 来源：Zfgk `PolylineUtil`，原作者张帆；依据源码快照 `a882003436fb4cc31b24c90f6779177c13a60abe` 重写。

[`PolylineExBeta`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineExBeta.cs)提供反向、整理、双线连接和整线圆角四个入口。它们借用输入，不修改、释放或擦除输入，不启动事务，也不自动落库。输入可为只读打开的数据库对象；调用者仍需保证其读取生命周期有效。

返回值是调用方拥有的内存多段线，复制顶点、凸度、段宽、Normal、Elevation、Thickness 和 Closed。它不复制图层、颜色、线型、XData、扩展字典及其他非几何数据，不能作为完整 `DBObject.Clone` 使用。调用者负责释放结果，或用 Fox 现有落库入口将其登记到目标数据库的事务。

[`PolylineRegionExBeta`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineRegionExBeta.cs)另提供含弧闭合多段线的点关系查询，返回枚举，不创建实体；同样只借用输入。

## 反向

`source.CreateReversedBeta()`创建反向副本。反向时改变每条实际圆弧的 bulge 符号，并交换每段起止宽度；闭合线固定原首顶点。开线最后一个顶点未参与实际段的元数据保留，因此反向两次可以恢复原始数据。

## 整理

`source.CreateCleanedBeta(tolerance)`先移除精确零长度直线段，再保守合并连续直线。容差是 OCS 平面中的有限非负距离，默认 `1e-6`。

- 只把精确重合顶点之间的直线段当作零长度段；不会仅因短段长度小于容差而删除它。
- 合并的每段必须为直线，且起止宽度与整个被合并直线段的宽度相同。圆弧及其端点、变宽段、回折点保留。
- 使用固定参考方向及半容差带，控制每个被替换的原始顶点到最终直线段的距离不超过容差，避免逐次合并累积误差。
- 闭合线保留原首顶点，不跨该点合并。没有可整理内容时仍返回独立对象。

局部相邻直线段转向达到 90 度的折点保持不动，包括在窄容差带内、沿参考方向仍单调的斜向折返；投影距离相等时也不会跨点合并。

## 连接

`first.TryCreateJoinedBeta(second, out result, tolerance)`只连接两条开多段线，保持第一条线的行进方向。容差为 OCS 平面内的有限非负端点距离，默认 `1e-6`。

两个输入的 Normal 分量、Elevation 和 Thickness 必须完全相同；此入口不转换不同 OCS。四种起终点组合中必须恰好有一组在容差内匹配。闭合输入、自连接、无匹配、多个端点匹配或平面/厚度不一致时返回 false，结果为 null。

端点精确重合时合并公共顶点。存在非零小间隙时保留两侧原始端点，并添加一条直线桥接段，其宽度从前段终宽线性变化到后段起宽。这样不会为连接而移动圆弧端点，也不会自动把结果闭合。成功结果仍为独立内存对象；失败或释放结果都不会消费输入。

## 整线圆角

`source.CreateFilletedBeta(radius, tolerance)`创建全部角的圆角副本，半径必须为有限正数。容差为 OCS 平面的有限非负距离，默认 `1e-6`，小尺度图形可传入零。容差用于检查长度及计算精度，不作为角度阈值。

- 实际段必须全部为直线；所有实际段的起止宽度必须完全相同，包括跨段宽度。零宽支持；含弧、变宽或宽度不连续时抛出 `NotSupportedException`，不会把宽度丢弃或猜测弧上宽度。新直段和圆弧都使用原恒定宽度。
- 开放线保留首末点，末点未使用的 bulge 和宽度元数据也保留。闭合线至少三个顶点，包括索引零和最后一个顶点的角都会处理；首角需要圆角时，以其入切点作为结果首点。
- 同向精确共线顶点原样保留，不用距离容差把小转角误判为共线；180 度折返拒绝。原始段、圆角回退距离和圆弧弦长均须大于容差；零段或过短段应先由调用者另行处理。
- 先计算所有角的回退量，再检查每条原边的两端预算。原边长度减去两端回退量必须严格大于容差；两个圆角切点恰好相接也拒绝。例如长度为 1 的共享边两端各回退 0.75，虽然单个回退小于边长，整次操作仍失败。
- 半径不会静默缩小。过大半径、不可表示的切点、剩余直段塌缩或无法保留指定半径时抛出 `InvalidOperationException`，整个操作失败，输入不变，不返回部分结果。零容差允许以计算尺度为基准的双精度舍入误差，不代表任意坐标都能精确构造。

该入口只检查局部圆角和共享原边的长度预算，不判断非相邻边、圆角之间的全局自交，也不检查等宽条带的偏移边界。Normal、Elevation、Thickness 和 Closed 沿用输入，全部计算使用原 OCS 平面。

现行 `CurveEx.ChamferAt` 只支持内部索引的单角编辑，不能覆盖闭合首末角和整线预算检查；此 Beta 入口补齐该能力，不改变稳定入口的行为。来源 `PolylineUtil.Fillet` 的隐式减小半径、丢失段宽和逐角重叠问题没有沿用。

## 含弧闭合线的点关系

[`PolylineRegionExBeta`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineRegionExBeta.cs) 的 `source.GetPointRelationBeta(wcsPoint, tolerance)` 返回 `PolylinePointRelationBeta`：`Inside`、`Outside`、`Boundary` 或 `OffPlane`。输入必须 `Closed = true`，至少两个顶点；支持两顶点双半圆构成的完整圆。宽度和 Thickness 不参与区域，按多段线基准平面的直线/圆弧中心线边界查询。

- 查询点使用 **WCS**。先完整读取并验证源的全部坐标、bulge 和几何段，再按源 Normal/Elevation 转换到 OCS。因此即使点明显在平面外，非法源仍抛异常，不能绕过验证。
- 到基准平面的距离大于容差时返回 `OffPlane`；其余点投影到 OCS 查询。平面距离与平面内边界距离分别使用同一有限非负容差，默认 `1e-6`，允许零；它们不是合并的三维欧氏距离。
- 实际边界距离在容差内时优先返回 `Boundary`。零容差仍保留按局部几何运算估计的双精度舍入误差；不会以全局坐标绝对值扩大边界带。
- 非零绝对 bulge 的支持范围明确限定为 **`[1e-8, 1e8]`**，两端包含。支持正负 bulge、超过半圆的主弧及混合直弧段；超界抛出 `NotSupportedException`，不会把浅弧或近整圆静默改成直线。范围内仍无法可靠表示的坐标、方向或运算尺度抛出 `InvalidOperationException`。
- 自交、重复绕行和重合边界采用**偶奇填充**；输入边界优先返回 `Boundary`。重复顶点间的零直段忽略，全部零段和零弦圆弧拒绝。共线或折返闭合轮廓可能没有内部，此时按规则返回边界或外部。

生产核心不依赖 Brep 或宿主求交：每条圆弧有限拆分为最多三段 Y 单调弧，以半开 Y 区间及稳定二次根统计横射线交点。切线极值用有理化公式保留浅弧矢高，边界计算使用展开圆方程，避免大中心/半径相减损失几何信息。不使用随机或无界射线重试，也不以顶点直连替代 bulge。

## 输入与失败边界

反向、整理和参与实际连接的输入至少含两个顶点、一条非零长度段。坐标和 bulge 必须为有限数，段宽必须为有限非负数。全部零长度或圆弧端点精确重合时拒绝处理。非法容差、非法数据和宿主对象访问错误会抛出异常；`TryCreateJoinedBeta` 的 false 表示无法按其连接约定连接，不吞掉异常。

不对非有限或无法安全归一化的超大坐标进行推测性简化；原始可读取段可被保留。几何结果能否由指定 CAD 宿主完整接受，仍需要该宿主的实际验收。

## 验证入口

- [直接链接生产核心的测试](../../tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj)：`dotnet run --project tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj --configuration Release`。覆盖手算圆弧/宽度、反向两次恢复、闭合起点、清理误差界限、四种连接端点、桥接宽度、歧义和失败输出；圆角覆盖顺逆绕序、凹角、首末角、共享段预算、恒定宽度、指定半径与切向、共线/折返及数值限制。
- [宿主验收命令](../../tests/TestShared/TestPolylineBeta.cs)：`Test_PolylineBeta`。仅使用临时实体和临时数据库，检查非默认 OCS、实际弧长/圆角半径、只读输入和对象归属，不写入当前图纸。新增圆角场景的编译与真实宿主执行单独记录，未执行保持 `Not run`。
- [点关系生产核心测试](../../tests/PolylineRegionBeta.CoreTests/PolylineRegionBeta.CoreTests.csproj)：`dotnet run --project tests/PolylineRegionBeta.CoreTests/PolylineRegionBeta.CoreTests.csproj --configuration Release`。直接链接生产实现，覆盖圆/主弧解析样例、同圆异弧、切线、凸度范围阈值、浅弧、旋转/平移/缩放/反向、偶奇填充及独立圆盘切割判据。
- 同一宿主测试文件的 `Test_PolylineRegionBeta` 仅使用临时实体和临时数据库，检查非默认 OCS、平面外关系、平面/边界容差、完整源验证与只读归属；已在四个代表目标编译，真实 CAD 执行仍为 `Not run`。
- 圆角及点关系增量分别运行对应生产核心测试，集成后集中编译受影响的四个代表目标；没有逐次重跑全部发布组合，也没有运行 CAD。首轮完整构建和本轮增量证据由[迁移清单](../zfgk-migration-inventory.md)统一记录。

段长、顶点数据快照和采样继续使用 Fox 的 [`PolylineEx`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineEx.cs)；现有单角编辑继续使用 [`CurveEx`](../../src/CADShared/Cad/Database/Entities/Curves/CurveEx.cs)。
