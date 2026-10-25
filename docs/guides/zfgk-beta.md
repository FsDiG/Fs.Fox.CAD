# Zfgk 迁入的 Beta 能力

> 状态：current；描述当前源码契约。Beta 表示实际 CAD 宿主验收仍未完成。

迁移保持 Fox 的 `Fs.Fox.Cad` 命名空间和既有平台程序集，不引入 `ZFGK.dll`。49 个来源文件的迁入、复用和排除结论见[来源处置清单](../zfgk-migration-inventory.md)。Zfgk 只作静态来源；构建和自动测试运行于 Fox。自绘表格、专用布局和模型依赖保持排除。

## 按能力定位

| 能力 | 入口 | 使用说明 |
| --- | --- | --- |
| 多段线整理、方向、连接与批量圆角 | `PolylineExBeta.CreateReversedBeta`、`CreateCleanedBeta`、`TryCreateJoinedBeta`、`CreateFilletedBeta` | [多段线 Beta](polyline-beta.md)；返回调用方拥有的新内存实体，输入不被修改或消费；圆角支持闭合首末角并检查共享边预算。 |
| 含弧闭合多段线点关系 | `PolylineRegionExBeta.GetPointRelationBeta` | [多段线 Beta](polyline-beta.md)；解析处理圆弧，区分内、外、边界和平面外；不按顶点直连替代。 |
| 嵌套块内曲线求交 | `BlockIntersectionExBeta.GetCurveIntersectionsBeta` | [块求交 Beta](block-intersections-beta.md)；显式只读事务，累计变换后使用真实三维曲线求交，返回独立只读点快照。 |
| 行列与曲线排序、邻近范围组合 | `PointOrderingBeta`、`CurveOrderingExBeta`、`ExtentGroupingBeta` | [排序与范围 Beta](ordering-beta.md)；返回索引或独立结果，保留重复输入，排序与分组规则明确。 |
| 按文字样式测量单行字符串 | `TextMetricsBeta.MeasureSingleLine` | [文字测量 Beta](text-metrics-beta.md)；显式数据库与样式，处理字体失败及替代，不是 MText 排版。 |
| 三角形坡度、坡向、重心；多面网格构造 | `Triangle3dBeta`、`PolyFaceMeshBuilderBeta.Append` | [几何与构造 Beta](geometry-construction-beta.md)；三维重心保留 Z，网格索引先完整验证。 |
| AutoCAD 外部 DWG 图片预览 | `DwgPreviewBeta.SavePng` | [DWG 预览 Beta](dwg-preview-beta.md)；使用明确的宿主图形上下文，读取外部数据库，不修改当前视图。 |

类型名中的 Beta 可用于检索试验接口；扩展方法同时在方法名体现 Beta。未通过宿主验收的能力不因构建成功而转为正式接口，原有稳定入口不自动依赖这些新增能力。

## 对象和宿主边界

- 多段线工厂只复制文档明确列出的几何字段，不等价于完整数据库对象克隆；输出的释放或落库由调用方负责。
- 排序和三角形计算不提交数据库；网格构造使用显式目标数据库和调用方事务，调用方负责提交、异常时中止。
- 含弧点关系忽略宽度/厚度，按声明的平面和数值范围查询；块求交查询存储曲线，动态块、外参、阵列及旧式多段线明确拒绝，不等同于屏幕可见几何求交。
- 文字测量与图片预览依赖真实 CAD 宿主上下文；不支持在普通后台线程中并发调用。支持范围以各能力指南为准。
- 文字测量在 `ZW_2022`、`GC_2022`、`GC_2023` 目标中明确抛出 `NotSupportedException`。DWG 预览仅在 AutoCAD 编译目标开放，不为其他 SDK 提供伪实现。

## 验证与后续转正

纯计算回归工程直接链接生产核心源码，避免在测试中维护第二份算法。CI 运行这些回归后，继续执行七个发布目标的正常构建。CAD 命令程序集和命令用例见各能力指南；未在指定产品、完整版本中运行的宿主场景保持 `Not run`。

具体验证结果见[来源清单的验证记录](../zfgk-migration-inventory.md#验证记录)。后续转正需要该能力声明支持范围内的真实证据及稳定的接口契约；Beta 兼容过渡根据实际调用方安排。
