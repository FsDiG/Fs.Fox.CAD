# IFoxCAD v0.9 上游重要变更评估

> 状态：历史记录（Historical）<br>
> 跟踪：[Issue #26](https://github.com/FsDiG/Fs.Fox.CAD/issues/26)（已完成）<br>
> 上游：[IFoxCAD v0.9](https://gitee.com/inspirefunction/ifoxcad/tree/v0.9)

> 本文只记录 2026-07-31 基线上的上游评估。Issue #26 的分阶段移植和暂缓决定已经收口；后续需求必须重新核对实时上游、当前 `main` 和新的宿主证据。

## 1. 结论

不建议把 IFoxCAD `v0.9` 整体合并或直接 cherry-pick 到 Fs.Fox.CAD。

两边虽然保留共同历史，但 Fs.Fox.CAD 已经改变公共命名空间、条件编译符号、平台项目、目标框架、SDK 代际和发布方式。上游提交经常同时包含版本号、项目文件、格式化和运行时代码，直接合并会把不相关的发布策略和兼容性风险带入 B。

建议采用以下路线：

1. 先移植三个小而确定的托管修复：Jig 释放顺序、XData 精确判断、块属性命中后再提权。
2. 单独修复 ZWCAD `GetEnv/SetEnv` 原生声明。上游找对了 DLL 和导出方向，但函数签名仍不完整，不能照搬。
3. 将 `EntGet` 作为只读基础能力独立引入，明确原生内存所有权后，再评估 `EntMod/EntUpd`。
4. 动态块可见性依赖 `EntGet`，ZWCAD 进度条依赖原生导出，分别作为后续 PR。
5. `IFoxDwgFiler` 当前实现质量不足，暂不移植。

## 2. 对比基线

| 项目 | 提交 | 日期 | 说明 |
| --- | --- | --- | --- |
| A：IFoxCAD `v0.9` | [`d7d32d8`](https://gitee.com/inspirefunction/ifoxcad/commit/d7d32d869743f72947cbed1125a11909e8b81222) | 2026-02-24 | Gitee `v0.9` 当前头提交 |
| B：Fs.Fox.CAD 源码基线 | [`80f8da3`](https://github.com/FsDiG/Fs.Fox.CAD/commit/80f8da3e0997597d1b6cf5cbd7009478daa07dc9) | 2026-07-31 | 本次分析前最后一个源码提交 |
| 共同提交 | [`ea73fac`](https://gitee.com/inspirefunction/ifoxcad/commit/ea73facda61fd47d87100f05067b35fa4995850f) | 2024-12-13 | 优化点在多边形内的判断逻辑 |

A 在共同提交之后有 105 个提交。B 当前 `main` 另有纯文档提交 `5061f02`，不改变源码比较结果。

B 的正式发布目标包括 AutoCAD 2019、AutoCAD 2025、ZWCAD 2022 和 ZWCAD 2025。同一份 `CADShared` 源码会绑定不同厂商和 SDK 代际，因此每个候选都必须分别判断编译兼容和宿主兼容。

## 3. 第一阶段：低风险缺陷修复

### 3.1 Jig 释放顺序

- 上游提交：[`bc7a242`](https://gitee.com/inspirefunction/ifoxcad/commit/bc7a2429c144ab627fcfaddde05f20d2be07589e)
- B 对应位置：[`JigEx.cs`](../src/CADShared/Cad/Editor/Jig/JigEx.cs)
- 建议：移植。

B 当前先访问 `ent.Database`，再判断 `ent.IsDisposed`。如果图元已在 Jig 外部释放，访问 `Database` 本身可能抛出异常。应先判断 `!ent.IsDisposed`，再检查它是否尚未加入数据库并执行释放。

这是短路条件顺序修复，不改变公共 API。构建只能证明语法和 SDK 兼容，仍需在 CAD 中覆盖“外部已释放”和“未入库待 Jig 清理”两条路径。

### 3.2 XData 按应用名精确判断

- 上游提交：[`e13eecd`](https://gitee.com/inspirefunction/ifoxcad/commit/e13eecd58995986775845548f9d46b828eb66724)
- B 对应位置：[`DBObjectEx.cs`](../src/CADShared/ExtensionMethod/DBObjectEx.cs)
- 建议：移植。

B 当前 `RemoveXData(obj, appName)` 只检查对象是否存在任意 XData。如果对象只有其他 RegApp 的数据，仍会进入目标应用的清理逻辑。应改为检查 `GetXDataForApplication(appName)`，目标应用不存在时直接返回。

验收应构造两个 RegApp：删除不存在或指定的应用数据后，另一个应用的数据必须保持不变。

### 3.3 块属性命中后再提权

- 上游提交：[`6744217`](https://gitee.com/inspirefunction/ifoxcad/commit/67442178d7b9909ded07536acf0527246ef17dbe)、[`0d3bf68`](https://gitee.com/inspirefunction/ifoxcad/commit/0d3bf6889574e55c41ce12c9101eb4c1fd26bd8f)、[`d600dbf`](https://gitee.com/inspirefunction/ifoxcad/commit/d600dbf9377af205d7cfa6c0adf34487541d3e2d)
- B 对应位置：[`BlockReferenceEx.cs`](../src/CADShared/Cad/Database/Entities/Blocks/BlockReferenceEx.cs)
- 建议：移植最终状态，不逐提交照搬。

最终上游状态是先执行 `TryGetValue(att.Tag, out value)`，只有命中目标属性时才调用 `ForWrite()`。这能避免无关属性进入写状态。

早期提交加入过“命中数量归零后提前退出”，后续已撤销。相同 Tag 可能出现在多个属性引用上，B 不应恢复该提前退出逻辑。

## 4. 第二阶段：ZWCAD 环境变量互操作

- 上游提交：[`159c076`](https://gitee.com/inspirefunction/ifoxcad/commit/159c0761cf6289c0195e8c5b7fe7ca99210b9fa1)
- B 对应位置：[`Env.cs`](../src/CADShared/Cad/Application/Context/Env.cs)
- 建议：按 SDK 重新实现，禁止直接复制上游声明。

B 当前 ZWCAD 分支从 `zced.dll` 导入 `zcedGetEnv/zcedSetEnv`，而 ZWCAD 2022 安装目录并不存在该 DLL。ZRX2022 与 ZRX2025 的头文件都声明：

```cpp
int zcedGetEnv(const ZTCHAR* sym, ZTCHAR* var, size_t nBufLen);
int zcedSetEnv(const ZTCHAR* sym, const ZTCHAR* val);
```

两个 SDK 的导入库都包含以下导出：

```text
?zcedGetEnv@@YAHPEB_WPEA_W_K@Z
zcedSetEnv
```

上游已经改为从 `zwcad.exe` 导入带缓冲区长度的 `GetEnv` 导出，但 C# 声明仍只有两个参数，遗漏 `size_t nBufLen`。这会让托管签名与原生 ABI 不一致。

实施要求：

- 使用 `CharSet.Unicode` 和 `UIntPtr`（或经验证等价的无符号指针宽度类型）表示 `size_t`。
- 将 `StringBuilder.Capacity` 作为第三个参数传入。
- `SetEnv` 从 `zwcad.exe` 导入公开的 `zcedSetEnv`。
- 分别在 ZWCAD 2022、ZWCAD 2025 验证读取、写入、恢复和不存在的变量。
- 不把静态导出检查或类库构建当作宿主调用成功的证明。

## 5. 第三、四阶段：EntGet 与写操作

### 5.1 EntGet 只读基础

- 上游提交：[`a6f4ca1`](https://gitee.com/inspirefunction/ifoxcad/commit/a6f4ca1ac382e7d325b0d7e16536e11b60a927b7)
- 后续 ZWCAD 支持：[`26a0557`](https://gitee.com/inspirefunction/ifoxcad/commit/26a05570b99d7711bc742dc2fbb03de3db3f80c2)
- 建议：可移植，但必须先解决原生结果缓冲区所有权。

上游增加了 AdsName 转换、`acdbEntGet/zcdbEntGet` 调用以及 resbuf 到 `TypedValue[]` 的转换。这是动态块可见性和 `EntNext` 的基础，但当前代码只做转换，没有在 API 层明确返回的 resbuf 由谁释放。

该 PR 应只提供读取能力，并满足：

- AutoCAD 和 ZWCAD 使用各自的 AdsName 类型和入口点，不伪造二进制兼容。
- 版本相关 DLL 名称不能只依赖 `Application.Version.Major` 的未验证映射。
- 无效 ObjectId、入口点失败、空 resbuf 和转换异常有确定行为。
- 明确并测试 resbuf 的释放责任，避免每次查询泄漏非托管内存。

### 5.2 EntMod/EntUpd 写操作

- 上游提交：[`6d612fd`](https://gitee.com/inspirefunction/ifoxcad/commit/6d612fd31116e357a7eecb2ab59b5f6a8fa89f4a)、[`3040c16`](https://gitee.com/inspirefunction/ifoxcad/commit/3040c16672c54a289502b093e8942bb476e27398)
- 建议：仅在 `EntGet` 已合并并完成宿主验证后实施。

写操作比读取多出三项风险：托管 `TypedValue` 转换产生的原生缓冲区释放、返回码解释，以及修改后是否需要显式刷新数据库/图形。应独立评审，不与 `EntGet` 放在同一个首发 PR。

## 6. 依赖型能力

### 6.1 动态块可见性

- 上游提交：[`a6f4ca1`](https://gitee.com/inspirefunction/ifoxcad/commit/a6f4ca1ac382e7d325b0d7e16536e11b60a927b7)
- 建议：在 `EntGet` 稳定后单独移植。

上游通过 `ACAD_ENHANCEDBLOCK` 扩展字典和 360、301、303 组码提取可见性参数。这是有实际价值的只读功能，但依赖内部 DXF 数据形态。验收必须包含：普通块、无可见性参数的动态块、具有多个可见性值的动态块，以及 AutoCAD/ZWCAD 的差异。

### 6.2 ZWCAD 状态栏进度条

- 上游提交：[`5340b97`](https://gitee.com/inspirefunction/ifoxcad/commit/5340b971fd4fe3c298a15a1646382a0a9b47329e)、[`98030b2`](https://gitee.com/inspirefunction/ifoxcad/commit/98030b2c17de7238539694d87b392402871eb9f9)
- 建议：单独移植。

ZRX2022 和 ZRX2025 导入库均能找到设置、更新和停止状态栏进度条的导出，说明静态可行性较高。但这些是 C++ 修饰名，仍需验证两个宿主版本中的调用约定、Unicode 标签、进度上下界和异常后恢复。

## 7. 暂不移植：IFoxDwgFiler

- 上游提交：[`444d175`](https://gitee.com/inspirefunction/ifoxcad/commit/444d175e1c8e081aac4bf6903c441c7414e011b5)
- 建议：拒绝按现状移植，保留为重新设计候选。

上游当前实现存在以下问题：

- 多个读取方法只判断集合是否为空，没有判断读取游标是否越界。
- `ReadBytes(byte[] value)` 给形参重新赋值，不会替换调用方缓冲区。
- `Position` 字段没有随读写更新。
- `Seek` 只向 Editor 输出方法名，没有改变游标。
- 所有存储集合和游标均为公共可变字段，调用者可以破坏读写序列。
- 测试主要验证 `DwgOut` 能收集部分引用，没有覆盖完整往返、越界、Seek 或不同实体类型。

只有先确定使用场景、字段顺序契约、ObjectId 生命周期和往返测试后，才适合设计 B 自己的实现。

## 8. 其他候选

以下提交有价值，但不进入 Issue #26 当前阶段。需要时应另行确认范围：

| 候选 | 上游提交 | 结论 |
| --- | --- | --- |
| 闭合多边形首尾点判断 | [`a0b1012`](https://gitee.com/inspirefunction/ifoxcad/commit/a0b1012f4143e977fedaef7b350c06864f4f5c77) | B 当前比较首点和第二点，属于明确缺陷；建议单独加入几何修复 PR |
| DBText 默认对齐判断 | [`22fa79c`](https://gitee.com/inspirefunction/ifoxcad/commit/22fa79cac7089015c070dfb294af60c5f532de8a) | 可用 `IsDefaultAlignment` 代替枚举特例，需验证四个 SDK |
| DIMBLK 空值恢复 | [`e5ae5fa`](https://gitee.com/inspirefunction/ifoxcad/commit/e5ae5fa5fec8eb41074cff11e0051d667b529bd3) | 值得修复，但应增加空值保护和 `try/finally`，不照搬表达式 |
| DBLCLKEDIT 系统变量 | [`a6f4ca1`](https://gitee.com/inspirefunction/ifoxcad/commit/a6f4ca1ac382e7d325b0d7e16536e11b60a927b7) | B 的 `DblClick` 会访问错误变量名；应保留兼容入口并增加正确名称 |
| 多行属性定义扩展 | [`fe4a838`](https://gitee.com/inspirefunction/ifoxcad/commit/fe4a8388af389d166a58ac4f3a0b9f2caa7d6326) | 小型公共 API 增量，按实际需求决定 |
| AddGroup 参数扩展 | [`b81b4c9`](https://gitee.com/inspirefunction/ifoxcad/commit/b81b4c93ffab19bc0081d9b9aa3bc56b6675d01d) | 小型公共 API 增量，避免引入无关语法改写 |
| RXClass 缓存 | [`8de5b7e`](https://gitee.com/inspirefunction/ifoxcad/commit/8de5b7e1112ed0587c2da6e69c01374beb5ca3ad) | 需要线程安全实现和热点证据，不建议只优化一个调用点 |

## 9. 明确排除的上游变更

- GstarCAD：上游先增加支持，随后因宿主持续崩溃停止打包；当前不应据此扩展 B 的正式支持矩阵。
- `AutoReg` 改回 `GetCallingAssembly`：会抵消上游此前针对 ZWCAD Release 获取错误改用 `GetExecutingAssembly` 的修复。
- `SystemVariableManager` 静态化、枚举成员重命名：会改变现有公共 API，收益不足以支持破坏性迁移。
- `ConvexHull` 对少于三个点改为返回 `null`：改变既有返回契约，且没有足够回归测试。
- 支持/信任路径整体重构：上游实现会统一转为小写、使用无序集合并过滤不存在的目录，不适合作为无行为变化重构。
- Gitee/GitHub 流水线、版本号、Slnx、编码转换和 ReSharper 设置：属于上游仓库维护策略，不是 B 的运行时能力。
- 已被上游回滚的 C# 13 参数、DXF 块读取、凸包实验等提交：不再作为候选。

## 10. 分阶段 PR 与验收

| 阶段 | 内容 | 最低自动验证 | 必需宿主验证 |
| --- | --- | --- | --- |
| 分析 PR | 本文档 | 链接、提交哈希、Markdown、范围检查 | 无 |
| 低风险修复 | Jig、XData、块属性 | 四个正式类库及对应测试项目构建 | 三个行为场景分别验证 |
| ZWCAD 环境变量 | `GetEnv/SetEnv` | ZRX2022/ZRX2025 头文件和导出核对；ZWCAD 项目构建 | ZWCAD 2022、2025 |
| EntGet | 只读原生查询 | 四平台构建；错误路径和内存策略检查 | AutoCAD 2019/2025、ZWCAD 2022/2025 |
| EntMod/EntUpd | 写入与刷新 | 四平台构建；返回码和释放检查 | 修改、失败、刷新、撤销 |
| 动态块可见性 | 读取可见性参数 | 四平台构建 | 普通块和多类动态块 |
| ZWCAD 进度条 | 开始、更新、停止 | 两代 ZRX 导出和 ZWCAD 项目构建 | ZWCAD 2022、2025 |

每个实施 PR 使用 `Refs #26` 关联总 Issue。当前 PR 经审核并合并后，下一阶段才从最新 `main` 创建分支。构建结果和 CAD 宿主结果必须分开记录。

## 11. 实施与验证状态

截至 2026-07-31，上游评估和计划内移植已经完成：分析见 #27，代码阶段见 #28 至 #34，ZWCAD 实体 API 导出名修复见 #38，补充宿主验收命令见 #39。详细宿主证据由 #36 记录。

本节使用以下状态：

- **已验证**：在明确记录的真实 CAD 产品和版本中执行过对应场景。
- **部分验证**：只覆盖了列出的行为，不能外推到未观察的宿主行为。
- **静态审查**：只检查了代码、SDK、导出或差异，没有执行对应宿主命令。
- **未验证**：没有该宿主和场景的运行证据，不表示失败，也不表示受支持。

| 范围 | 状态 | 备注 |
| --- | --- | --- |
| ZWCAD 2022：Jig、XData、块属性 | 已验证 | 三项核心行为已在 ZWCAD 2022 `22.20.301.8730` 中通过。#39 最后的 RegApp 清理验证修正未重新运行。 |
| ZWCAD 2022：`GetEnv/SetEnv` | 已验证 | 缺失变量、Unicode 写入/读回和原值恢复均通过。 |
| ZWCAD 2022：`EntGet` | 已验证 | 临时实体、重复读取、DXF 数据、Handle、无效 ObjectId 和清理均通过。 |
| ZWCAD 2022：`EntMod/EntUpd` | 部分验证 | 原生入口、修改/回读、失败返回、`EntUpd` 和清理已通过；图形显示和撤销栈未人工观察。 |
| ZWCAD 2022：动态块可见性 | 已验证 | 官方 `architectural.dwg` 样例覆盖带/不带可见性参数的动态块及只读查询；#39 后续改为遍历全部布局空间，该范围扩展未重新运行。 |
| ZWCAD 2022：状态栏进度条 | 已验证 | 正常、异常中断、再次正常和最终状态栏恢复已有视觉证据。 |
| AutoCAD 2019、AutoCAD 2025、ZWCAD 2025 | 未验证 | 当前没有这些真实宿主的运行结果；构建、SDK 和导出检查不能替代宿主结论。 |
| #39 最后两项评审修正 | 静态审查 | RegApp 擦除记录验证和全布局空间扫描已按代码契约修正，依维护者决定不再追加宿主测试。 |
| `IFoxDwgFiler` | 暂缓 | 不直接移植；消费者、数据模型、状态机和宿主往返门槛见 #35。 |

当前同步收口不要求补齐未安装宿主的运行矩阵。任何未验证项都必须保持上述标记，不得从相邻版本、构建成功或其他宿主的结果推导为已通过。
