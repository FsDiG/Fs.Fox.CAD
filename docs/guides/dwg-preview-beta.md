# AutoCAD DWG 预览 Beta

> 状态：current，Beta API；真实 AutoCAD 宿主渲染验收 `Not run`。<br>
> 来源：Zfgk 的 `GsPreviewUtil` 能力；重新定义视图、路径、空图和资源所有权。

[`DwgPreviewBeta.SavePng`](../../src/CADShared/Cad/Application/Graphics/DwgPreviewBeta.cs)在当前 AutoCAD 文档的图形管理器中创建自己的离屏设备、模型和视图，将外部 DWG 的模型空间输出为新 PNG。显式传入当前活动文档、源 `.dwg`、新 `.png`、像素宽高。它需要 AutoCAD 主线程命令上下文，不能用作脱离 CAD 的转换程序，不支持并发调用。

## 输出与范围

源文件按只读共享方式读取磁盘内容，不包含未保存的编辑。关闭或冻结图层及不可见实体不参与绘制和取范围；只处理模型空间，不自动输出布局。实体范围读取失败会传播异常，不能静默忽略该实体后输出被裁剪的成功结果。

相机固定 WCS 俯视，+Y 向上，覆盖输入范围并保留输出纵横比，留出总计 10% 的范围余量。点状范围使用 1 个图形单位的基本范围，再加余量。当前文档的 UCS、视图、WorkingDatabase 和全局配置不变；不借用或修改当前视图。

宽高各为 1 至 8192 像素，总像素不超过 16777216。无法建立有限相机范围时拒绝输入。空图或没有可绘制实体时抛出 `InvalidOperationException`，不会把空白图片作为成功结果。

PNG 父目录必须存在，目标文件不能存在。源和目标均须为独立文件路径，拒绝 `file.dwg:preview.png` 等 NTFS 数据流路径。先在同目录创建自有临时文件，完整编码后以不覆盖的 `File.Move` 移动为目标；已有文件及竞争创建的目标都受保护。只有 `CreateNew` 成功后才取得临时文件的删除权；若临时路径已存在，创建失败不会删除该文件。移动成功后释放临时路径的删除权，失败时只尽力清理本次创建的 `.fox-preview-*.tmp`；若文件系统拒绝清理，保留原失败异常且可能留下该临时文件。

## 所有权和宿主边界

外部数据库与事务由方法创建并释放；图形管理器属于调用方文档，方法不释放它。视图在退出时先移除 drawable 并从设备解绑，再释放自有视图、模型和设备，最后调用 `ReleaseGraphicsKernel` 归还获取的图形内核。

此规则依据 Autodesk 的 [AcquireGraphicsKernel 引用计数约定](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-RefGuide/files/OREF-AcGsManager__acquireGraphicsKernel_AcGsKernelDescriptor__.html)、[ReleaseGraphicsKernel](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Manager_ReleaseGraphicsKernel_GraphicsKernel.html)、[Device.Erase](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Device_Erase_View.html)和 [Device.GetSnapshot](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_GraphicsSystem_Device_GetSnapshot_Rectangle.html) API。

公共预览入口仅在 `ACAD` 编译目标中提供。ZWCAD 和 GStarCAD 当前不提供该入口，也不以空实现或静默失败替代。预览依赖当前 AutoCAD 图形驱动、字体和外部参照等资源；构建通过不能证明实际图形正确或驱动资源完全无泄漏。

## 验证入口

- [核心测试](../../tests/DwgPreviewBeta.CoreTests/DwgPreviewBeta.CoreTests.csproj)直接链接生产路径/尺寸/相机拟合与文件输出代码，验证纵横比、退化范围、极端数值、路径规范；通过真实文件 IO 检查临时路径冲突、编码委托异常和竞争创建输出时的所有权及不覆盖策略。它不执行 PNG 编码、DWG 读取或 CAD 渲染。
- [宿主命令](../../tests/TestShared/TestDwgPreviewBeta.cs)：`Test_DwgPreviewBeta` 生成自有临时 DWG 并连续输出三幅 PNG，检查尺寸、图像非单色、源文件哈希及当前文档/视图保持；`Test_DwgPreviewBetaFailures` 检查空图、损坏文件、尺寸、文件不存在和已有输出。
- 两个命令只能在明确安排的隔离 AutoCAD 环境运行，遵守[宿主验收说明](../../tools/HostAcceptance/README.md)。当前均为 `Not run`。
