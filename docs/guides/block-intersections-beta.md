# 块内曲线求交 Beta

> 状态：current；Beta API 契约，真实 CAD 宿主验收 `Not run`。<br>
> 实现：[BlockIntersectionExBeta](../../src/CADShared/Cad/Database/Entities/Blocks/BlockIntersectionExBeta.cs)。<br>
> 受众：调用方、维护者、宿主验收人员。

`block.GetCurveIntersectionsBeta(transaction, target, mode, tolerance)` 查询普通块及其嵌套普通块中的实际曲线，返回独立的 `IReadOnlyList<Point3d>` 只读快照。它不会使用块包围框近似交点，也不改变输入或当前工作数据库。调用方负责 CAD 主线程、文档读取上下文及必要的访问条件。

## 输入身份与坐标

根块必须已入库，并直接位于模型空间或图纸空间；传入的原生 `Transaction` 必须是该数据库当前最上层事务。不能把定义内部的块参照直接当作 WCS 根块。已入库的目标曲线必须与根块属于同一个数据库、同一个空间；定义内曲线或其他布局的曲线不能直接作为目标。

目标也可以是调用方持有的未入库曲线，此时调用方保证其坐标已经是 WCS。查询借用根块、目标和事务，不释放它们，不提交或中止事务，不升级对象写状态。其他数据库的事务、存在子事务时传入的父事务、未入库根块以及身份不符的目标均被拒绝。

## 查询与支持范围

- 仅支持真实 WCS 三维求交；不提供 XY 投影。高度不同但 XY 交叉的曲线不应产生投影交点。
- `mode` 默认是 `Intersect.OnBothOperands`，双方均不延伸；也接受 `Intersect.ExtendArgument`，只延伸 `target`。延伸能力和重合曲线返回哪些离散点仍由实际宿主 SDK 决定。
- 曲线白名单为原生 `Line`、`Arc`、`Circle`、`Ellipse`、轻量 `Polyline`、`Spline`、`Ray` 和 `Xline`。自定义派生类型及其他 `Curve` 明确抛出 `NotSupportedException`。
- 文字、属性、填充、网格及其他非 `Curve` 叶节点不属于本接口查询范围，不参与求交。查询使用存储几何，包含不可见曲线，不考虑图层关闭/冻结、视口可见性或裁剪；结果不能作为屏幕可见几何的交点列表。
- 动态块、外部参照、`MInsertBlock` 阵列及其他块派生类型明确拒绝。遍历到这些节点会使整个查询失败，不返回已经算出的部分交点。
- 旧式 `Polyline2d` / `Polyline3d` 明确拒绝。Autodesk 对 [GetTransformedCopy](https://help.autodesk.com/cloudhelp/2020/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_Entity_GetTransformedCopy_Matrix3d.html) 的说明警告，拥有顶点子对象的复杂实体可能浅克隆并变换原始子对象；首版采用保守白名单保持只读边界。

嵌套变换按“父累计矩阵 × 子块矩阵”组合，实际曲线转换到 WCS 后才与目标求交。非均匀缩放及组合变换由原生 `GetTransformedCopy` 决定是否支持；变换或求交异常原样传播，不改用包围框或丢弃叶节点。输入中若出现循环块定义路径，抛出 `InvalidOperationException`。路径检查只作用于当前祖先链，同一定义的不同实例仍按各自位置查询；实现使用显式栈，避免深层嵌套造成 CLR 递归栈溢出。

## 去重、顺序与资源

`tolerance` 是 WCS 单位下的有限非负去重距离，默认 `1e-6`。它只控制返回点去重，不改变原生几何求交容差。零表示三个坐标分量精确相等；正值使用三维欧氏距离，距离不大于该值时保留首次命中的点。每个后续点只与已保留的代表点比较，不通过被合并点做链式聚合；对角方向也按球形距离判断。

遍历按照块定义实体顺序深度优先进行。每条曲线内部的交点顺序沿用宿主返回顺序，因此不承诺不同宿主之间顺序或末位数值一致。计算复杂度取决于宿主求交，去重最坏为 O(k²)，其中 k 为原生返回点数；它不包含空间索引或批量加速。

所有变换副本以及 [Point3dCollection](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_Geometry_Point3dCollection.html) 使用确定的释放范围，正常与异常路径均清理。原生返回非有限坐标时拒绝整个结果；没有交点或没有曲线叶节点时返回空快照。返回值经 `Array.AsReadOnly` 包装，后续查询不会改变之前的结果。

## 来源与验证

能力来源是 `Fs.Zfgk.CAD` 基线 `a882003436fb4cc31b24c90f6779177c13a60abe` 中 `ObjectARX/Entity/BlockUtil.cs` 的 `IntersectWith`。原文件头记录作者 `TianTeng`，版权为 `Copyright © 北京智帆高科科技有限公司 2015`。本实现重新定义只读事务、WCS 身份、嵌套支持、支持类型及失败语义，不是原方法的无差别替换；来源仓库只作静态参考，不构建。

[Test_BlockIntersectionsBeta](../../tests/TestShared/TestBlockIntersectionsBeta.cs) 使用临时数据库创建直线、不可见圆弧、含弧轻量多段线和两级嵌套块。固定预期覆盖父子旋转/缩放顺序、同一定义重复实例、重复点、正/零去重距离、三维高度差、目标延伸、轻量多段线源顶点/凸度/平面守恒、非曲线叶节点、不可变结果、只读打开、异库/异空间/非最上层事务及非法参数。测试不读取或修改用户图纸。

| 证据 | 状态 |
| --- | --- |
| 新增生产与命令程序集编译 | 待集成人统一执行代表目标检查 |
| AutoCAD / ZWCAD / GStarCAD 命令实际运行 | `Not run` |
| 动态块、外参、阵列及旧式多段线 | 首版明确拒绝 |
| 循环定义、很深的有效定义链、非均匀变换异常清理 | 需在允许对应输入的实际宿主补充验收 |

真实宿主测试遵循 [HostAcceptance](../../tools/HostAcceptance/README.md) 的隔离与记录要求，分别记录产品、完整版本、测试程序集、提交和命令结果。构建通过不能替代实际宿主验收。
