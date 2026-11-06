# AutoCAD DWG 与实体预览 Beta

> 状态：current，Beta API；真实 AutoCAD 宿主渲染验收 `Not run`。

`DwgPreviewRendererBeta` 在当前 AutoCAD 文档的图形管理器中创建自有离屏设备、模型和视图。两条入口都需要 AutoCAD 主线程命令上下文，同步完成调用，由调用方保持文档和必要的锁有效。[API 源码](../../src/CADShared/Cad/Application/Graphics/DwgPreviewRendererBeta.cs)

| 入口 | 输入 | 输出与责任 |
| --- | --- | --- |
| `RenderModelSpaceToPng` | 当前文档、磁盘 DWG、新 PNG 路径和像素宽高 | 自有侧数据库的模型空间，成功后发布独立 PNG 文件。 |
| `RenderEntitiesToBitmap` | 当前文档、来源数据库、读取事务、实体 ID、业务范围、像素宽高、背景与可选调色板 | 同步借用来源，返回由调用方释放的独立 Bitmap。 |

## 输出与范围

本节的模型空间筛选、相机余量和文件发布规则适用于 `RenderModelSpaceToPng`。

源文件按只读共享方式读取磁盘内容，不包含未保存的编辑。只处理模型空间，不自动输出布局；筛选其顶层实体的 `Visible` 及所属图层关闭／冻结状态。复合实体使用宿主返回的整体 `GeometricExtents`，不逐项展开块内可见叶节点，因此内部隐藏几何仍可能扩大取景范围、产生留白。实体范围读取失败会传播异常，不能静默忽略该实体后输出被裁剪的成功结果。

相机固定 WCS 俯视，+Y 向上，覆盖输入范围并保留输出纵横比，留出总计 10% 的范围余量。点状范围使用 1 个图形单位的基本范围，再加余量。当前文档的 UCS、视图、WorkingDatabase 和全局配置不变；不借用或修改当前视图。

宽高各为 1 至 8192 像素，总像素不超过 16777216。无法建立有限相机范围时拒绝输入；眼点和目标点各自有限仍不够，两者之差构成的视线方向也须有限且非零。空图或没有通过筛选的顶层实体时抛出 `InvalidOperationException`；预览接口不检查最终像素是否非空白，不能以创建 PNG 成功证明实际渲染正确。

PNG 父目录必须存在，目标文件不能存在。源和目标均须为独立文件路径，拒绝 `file.dwg:preview.png` 等 NTFS 数据流路径。先在同目录创建自有临时文件，编码、GS 收尾和来源数据库释放全部成功后，以不覆盖的 `File.Move` 移动为目标。已有文件及竞争创建的目标都受保护。只有 `CreateNew` 成功后才取得临时文件的删除权。移动成功后释放临时路径的删除权，失败时尽力清理本次创建的 `.fox-preview-*.tmp`。若文件系统拒绝清理，保留原失败异常和临时文件供诊断。

## 所有权和宿主边界

文件入口创建并释放侧数据库与事务。实体入口同步借用调用方的数据库和事务，借用期覆盖 drawable 注册、绘制、截图和 GS 收尾。图形管理器属于调用方文档。公共内部会话在退出时先移除 drawable 并从设备解绑，再释放自有视图、模型和设备，最后调用 `ReleaseGraphicsKernel` 归还获取的图形内核。

此规则依据 Autodesk 的 [AcquireGraphicsKernel 引用计数约定](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-RefGuide/files/OREF-AcGsManager__acquireGraphicsKernel_AcGsKernelDescriptor__.html)、[ReleaseGraphicsKernel](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Manager_ReleaseGraphicsKernel_GraphicsKernel.html)、[Device.Erase](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Device_Erase_View.html)和 [Device.GetSnapshot](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Device_GetSnapshot_Rectangle.html) API。

公共预览入口仅在 `ACAD` 编译目标中提供。ZWCAD 和 GStarCAD 当前不提供该入口，也不以空实现或静默失败替代。预览依赖当前 AutoCAD 图形驱动、字体和外部参照等资源；构建通过不能证明实际图形正确或驱动资源完全无泄漏。

## 实体集合到 Bitmap

`DwgPreviewRendererBeta.RenderEntitiesToBitmap` 接收当前文档、来源数据库、调用方有效读取事务、实体 ID 集合、业务范围、像素宽高和背景，可选 256 色逻辑调色板。实体必须来自同一数据库，方法同步完成，事务与来源由调用方保持有效并负责结束。

相机固定 WCS 俯视，按业务范围执行 ZoomExtents 和 Zoom(0.9)，供现有 BOM 出图采用。实体筛选由业务层完成，ACI 调色板与 TrueColor 分别按 SDK 语义呈现。返回值是复制出的独立 Bitmap，方法返回前结束全部自有 GS 资源，调用方 Dispose 返回图像。

文件 PNG 与实体 Bitmap 共用设备、绘制、截图和释放路径。文件入口继续使用原有相机拟合及新文件发布规则。释放各项资源均独立尝试，已有绘制异常保留，清理异常附加到异常 Data 的 Fs.Fox.Cad.GraphicsCleanup；单独清理失败传播 AggregateException。

`Test_DwgEntitiesBitmapBeta` 检查三次出图、像素、空集合、跨数据库 ID、尺寸及借用事务仍可使用，并验证返回图片在来源关闭后可读取和编码。宿主执行结果按具体构建记录。

## 验证入口

- [核心测试](../../tests/DwgPreviewBeta.CoreTests/DwgPreviewBeta.CoreTests.csproj)直接链接生产路径、尺寸、相机拟合与文件输出代码，验证纵横比、退化范围、极端数值和路径规范。真实文件 IO 覆盖临时路径冲突、部分写入失败、完整写入后的收尾异常及竞争创建输出。测试中用可控异常模拟收尾失败，真实 GS 清理通过宿主验证。
- [宿主命令](../../tests/TestShared/TestDwgPreviewBeta.cs)：`Test_DwgPreviewBeta` 生成自有临时 DWG 并连续输出三幅 PNG，检查尺寸、图像非单色、源文件哈希及当前文档/视图保持；`Test_DwgPreviewBetaFailures` 检查空图、损坏文件、尺寸、文件不存在和已有输出。
- 实体 Bitmap 与两个文件命令均在维护者安排的隔离 AutoCAD 环境运行，遵守[宿主验收说明](../../tools/HostAcceptance/README.md)。准确提交、检查计数和宿主结果记录在关联 PR 或运行记录。
