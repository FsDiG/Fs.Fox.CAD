# 单行文字测量 Beta

> 状态：Beta 使用说明；真实 CAD 宿主验收 `Not run`。<br>
> 实现：[TextMetricsBeta](../../src/CADShared/Cad/Database/Entities/Text/TextMetricsBeta.cs)。<br>
> 来源：ZFGK.AutoCAD 的 `TextUtil.GetTextHeightAndWidth`（张帆）；重新定义样式身份、文本语义和资源归属。

`TextMetricsBeta.MeasureSingleLine` 接收显式数据库、属于该数据库的文字样式 ID、单行内容、字高及宽度因子，返回文字局部坐标的 `Extents2d`。宽高由范围最大点减最小点得到。调用方负责 CAD 主线程执行上下文和文档访问条件。

## 内容与字体

- 字高和宽度因子必须是有限正数，并覆盖临时测量样式的对应设置；不修改数据库样式记录。
- `includeSpaces` 默认开启，计入首尾空格的进笔距离。
- `raw` 默认为 `false`，由宿主解释 DBText 的 `%%` 转义；设为 `true` 时按字面测量。
- 不解析 MText 格式，不删除 `{a^b}` 等内容，不处理换行、制表符或其他控制字符。不能把本接口当作 MText 排版或实际实体的 WCS 包围框计算。
- 空串在参数和样式校验通过后返回原点零范围，不加载字体。
- 宿主报告字体/大字体加载失败、字体替代，或未设置加载成功位 `0x01` 时抛出 `InvalidOperationException`。官方未把 `0x00` 定义为成功，本接口保守拒绝该状态；其他 SDK 异常原样传播。

测量用的 GI 对象和内部只读事务均在退出时释放；数据库由调用方拥有。方法通过现有 `SwitchDatabase` 临时设置显式数据库，正常或异常返回后恢复原工作数据库。该方法会接触宿主全局状态，不支持并发调用，也不是独立于 CAD 的离线字体服务。

字体加载位掩码和文字转义语义依据 Autodesk 的 [LoadStyleRec](https://help.autodesk.com/cloudhelp/2026/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsInterface_TextStyle_LoadStyleRec.html)、[原生字体加载状态](https://help.autodesk.com/cloudhelp/2018/ENU/OARXMAC-RefGuide/files/OREFMAC-AcGiTextStyle__loadStyleRec.html)和 [ExtentsBox](https://help.autodesk.com/cloudhelp/2024/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsInterface_TextStyle_ExtentsBox_string__MarshalAsUnmanagedType_U1__bool__MarshalAsUnmanagedType_U1__bool_WorldDraw.html) 文档。ZWCAD、GStarCAD 使用各自 `CadGI` 绑定，编译兼容和实际字体行为分别验证。

## 宿主边界

| 编译目标 | 测量入口 | 真实宿主验收 |
| --- | --- | --- |
| AutoCAD 2019 / 2025 | 使用各自 SDK 的 GI 测量 API。 | `Not run` |
| ZWCAD 2022 | SDK 缺少 `FromTextStyleTableRecord` 和 `ExtentsBox`；调用在校验参数和切换数据库之前抛出 `NotSupportedException`，空串也不例外。 | `Not run` |
| ZWCAD 2025 | SDK 提供 GI 测量 API。 | `Not run` |
| GStarCAD 2022 / 2023 | SDK 缺少 `FromTextStyleTableRecord`；调用在校验参数和切换数据库之前抛出 `NotSupportedException`，空串也不例外。 | `Not run` |
| GStarCAD 2026 | 使用 SDK 的 GI 测量 API。 | `Not run` |

ZWCAD 2022 的限制由本机 ZRX 2022 SDK 元数据及编译结果确认；GStarCAD 2022/2023 则由对应 NuGet SDK 元数据及编译结果确认。后续适配需要验证完整的样式转换和等价的文本/空格/转义语义，不用 DBText 实体包围框代替。字体加载状态位依据上述 Autodesk 契约；ZWCAD 2025 和 GStarCAD 2026 的真实字体加载、替代和范围行为仍需要各自宿主验收。

## 属性能力的处置

块属性读写继续复用 `BlockReferenceEx.GetAttributes`、`ChangeBlockAttribute` 和现有对象写入辅助。Zfgk 对字体高度、宽度因子和位置的简单属性包装不再增加同义 API。

## 验证入口

[TestTextMetricsBeta](../../tests/TestShared/TestTextMetricsBeta.cs) 在支持测量的编译目标中提供三个宿主命令：

| 命令 | 场景 |
| --- | --- |
| `Test_TextMetricsBeta` | 独立背景数据库、Arial 样式，字高/宽度缩放、空格、转义、花括号文本、空串及样式只读。 |
| `Test_TextMetricsBetaInvalidInput` | 非有限数、控制字符、无效/错误类型/跨数据库样式，工作数据库恢复。 |
| `Test_TextMetricsBetaMissingFont` | 随机缺失 SHX，拒绝失败或替代的字体结果，保持原样式及工作数据库。 |

ZW_2022、GC_2022、GC_2023 只编译 `Test_TextMetricsBetaUnsupported`，检查其明确抛出不支持异常且不改变工作数据库。

这些命令仅在明确安排的隔离 CAD 宿主中执行，遵守[宿主验收说明](../../tools/HostAcceptance/README.md)。命令编译成功不代表已运行；当前真实宿主均为 `Not run`。
