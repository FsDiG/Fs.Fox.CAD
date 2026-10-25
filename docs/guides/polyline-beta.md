# 多段线几何操作 Beta

> 状态：current；说明 Beta API 的行为约定，不代表真实 CAD 宿主已经验收。<br>
> 来源：Zfgk `PolylineUtil`，原作者张帆；依据源码快照 `a882003436fb4cc31b24c90f6779177c13a60abe` 重写。

[`PolylineExBeta`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineExBeta.cs)提供反向、整理和双线连接三个入口。它们借用输入，不修改、释放或擦除输入，不启动事务，也不自动落库。输入可为只读打开的数据库对象；调用者仍需保证其读取生命周期有效。

返回值是调用方拥有的内存多段线，复制顶点、凸度、段宽、Normal、Elevation、Thickness 和 Closed。它不复制图层、颜色、线型、XData、扩展字典及其他非几何数据，不能作为完整 `DBObject.Clone` 使用。调用者负责释放结果，或用 Fox 现有落库入口将其登记到目标数据库的事务。

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

## 输入与失败边界

反向、整理和参与实际连接的输入至少含两个顶点、一条非零长度段。坐标和 bulge 必须为有限数，段宽必须为有限非负数。全部零长度或圆弧端点精确重合时拒绝处理。非法容差、非法数据和宿主对象访问错误会抛出异常；`TryCreateJoinedBeta` 的 false 表示无法按其连接约定连接，不吞掉异常。

不对非有限或无法安全归一化的超大坐标进行推测性简化；原始可读取段可被保留。几何结果能否由指定 CAD 宿主完整接受，仍需要该宿主的实际验收。

## 验证入口

- [直接链接生产核心的测试](../../tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj)：`dotnet run --project tests/PolylineBeta.CoreTests/PolylineBeta.CoreTests.csproj --configuration Release`。覆盖手算圆弧/宽度、反向两次恢复、闭合起点、清理误差界限、四种连接端点、桥接宽度、歧义和失败输出。
- [宿主验收命令](../../tests/TestShared/TestPolylineBeta.cs)：`Test_PolylineBeta`。仅使用临时实体和临时数据库，检查 OCS、实际弧长、只读输入和对象归属，不写入当前图纸。
- 本工作线验证：纯核心测试与 AutoCAD 2025 API 目标构建结果随本次提交记录；真实 CAD 宿主执行为 `Not run`。全部发布目标的集成构建和最终证据由[迁移清单](../zfgk-migration-inventory.md)统一记录。

段长、顶点数据快照、采样及现有圆角入口继续使用 Fox 的 [`PolylineEx`](../../src/CADShared/Cad/Database/Entities/Curves/Polylines/PolylineEx.cs) 与 [`CurveEx`](../../src/CADShared/Cad/Database/Entities/Curves/CurveEx.cs)。
