# PR #138 模块专项评审报告：文字度量与 DWG 预览

> 状态：historical；针对 `4064025` 的专项静态评审快照。<br>
> 仅供追溯，不是现行技术契约；测试数字沿用原评审者记录，非本轮重跑。使用限制以[当前 Beta 指南](../../guides/beta-capabilities.md)为准，宿主验收 `Not run`。
> 后续完整审查与修正见[实现记录](../../beta-implementation-notes.md#完整核心审查与修正)。

**评审模块**：单行文字样式度量（`TextMetricsBeta`）与外部 DWG 离屏预览（`DwgPreviewBeta`）
**生产源文件**：
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Database\Entities\Text\TextMetricsBeta.cs`](../../../src/CADShared/Cad/Database/Entities/Text/TextMeasurementBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Application\Graphics\DwgPreviewCoreBeta.cs`](../../../src/CADShared/Cad/Application/Graphics/DwgPreviewCoreBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\src\CADShared\Cad\Application\Graphics\DwgPreviewBeta.cs`](../../../src/CADShared/Cad/Application/Graphics/DwgPreviewRendererBeta.cs)
**配套测试**：
- [`D:\MakeX\Fs.Fox.CAD\tests\DwgPreviewBeta.CoreTests\Program.cs`](../../../tests/DwgPreviewBeta.CoreTests/Program.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestTextMetricsBeta.cs`](../../../tests/TestShared/TestTextMetricsBeta.cs)
- [`D:\MakeX\Fs.Fox.CAD\tests\TestShared\TestDwgPreviewBeta.cs`](../../../tests/TestShared/TestDwgPreviewBeta.cs)
**对应文档**：
- [`docs/guides/text-metrics-beta.md`](../../../docs/guides/text-metrics-beta.md)
- [`docs/guides/dwg-preview-beta.md`](../../../docs/guides/dwg-preview-beta.md)

---

## 1. 文字样式度量（`TextMetricsBeta`）技术分析

### 1.1 语义定位与第一性原理
- **非 MText 排版器**：
  明确定义为**单行字符串在文字自身局部坐标系中的二维范围测量**。
  - 严密拦截多行字符：检查 `char.IsControl(c)`、换行符 `\r`、`\n` 以及 Unicode 行段分隔符 `\u2028`、`\u2029`；
  - 空串在通过参数校验后直接返回原点零范围 `new Extents2d(Point2d.Origin, Point2d.Origin)`，不发起昂贵的字体查找；
  - 严格不解析 MText 堆叠花括号格式，不按实体 WCS 旋转包围框计算，语义高度专一。

### 1.2 异常安全性与 WorkingDatabase 逆序恢复
- **C# 作用域逆序析构保证**：
  在测量时需要临时将宿主的 `HostApplicationServices.WorkingDatabase` 切换为指定的目标数据库：
  ```csharp
  using var databaseScope = new SwitchDatabase(database);
  using var transaction = database.TransactionManager.StartTransaction();
  if (transaction.GetObject(textStyleId, OpenMode.ForRead) is not TextStyleTableRecord)
      throw new ArgumentException("指定的对象不是文字样式。", nameof(textStyleId));
  ```
  **生命周期时序**：`databaseScope` 先于 `transaction` 声明。当方法无论是正常退出还是中间抛出异常，C# 语言规范保证内部读取事务 `transaction` **先被 Dispose 关闭**，之后 `databaseScope` **再将工作数据库恢复为调用方的原始数据库**。绝不会出现在原工作数据库已被还原时、内部事务依然挂在后台操作目标数据库的严重时序倒错。

### 1.3 字体加载位掩码规范
- **精确对应 Autodesk `loadStyleRec` 规范**：
  ```csharp
  var loadStatus = graphicsStyle.LoadStyleRec;
  if ((loadStatus & 0x1e) != 0 || (loadStatus & 0x01) == 0)
      throw new InvalidOperationException($"文字样式字体未确认成功加载或发生替代，状态码 0x{loadStatus:X2}。");
  ```
  - `0x01`（bit 0）：已成功加载；
  - `0x02`（bit 1）：主字体加载失败；
  - `0x04`（bit 2）：大字体（SHX）加载失败；
  - `0x08`（bit 3）：主字体发生替代（如替换为 simplex.shx）；
  - `0x10`（bit 4）：大字体发生替代；
  - **判定严格度**：一旦缺失字体、加载失败或发生替代，或者未报告成功位 `0x01`，坚决抛出异常拒绝，绝不静默以替代字体返回错误的测量包围框。

### 1.4 多宿主条件编译隔离
- 在 `ZW_2022`、`GC_2022`、`GC_2023` 三个编译目标中，由于对应第三方 SDK 缺少完整的 GI 样式转换接口，代码在方法入口最前沿通过条件编译：
  ```csharp
  #if ZW_2022 || GC_2022 || GC_2023
      throw new NotSupportedException("当前编译目标的 SDK 不提供完整的 GI 文字样式转换与范围测量 API，暂不支持此测量接口。");
  #else
  ```
  在**任何参数校验、数据库切换或事务启动之前**立即抛出不支持异常，零侵入、零副作用。

---

## 2. 外部 DWG 离屏预览（`DwgPreviewBeta`）技术分析

### 2.1 文件 IO 竞争安全与 NTFS 数据流拦截
- **NTFS 备用数据流（ADS）阻断**：
  代码通过检查盘符根路径之后的冒号：
  `if (path.Substring(root.Length).IndexOf(':') >= 0) throw new ArgumentException(...);`
  有效阻断了类似 `file.dwg:preview.png` 的 NTFS 隐藏流注入。
- **文件所有权与原子发布（Atomic Publishing）**：
  为防止覆盖调用者已有文件或多并发冲突，`DwgPreviewCoreBeta.WriteNewOutput` 实现了极其严密的临时文件管理：
  1. 采用 `FileMode.CreateNew` 在输出同目录下创建随机命名的临时文件；
  2. 只有在 `CreateNew` 成功后才将 `ownsTemporary` 标记为 true；若该临时路径已存在则创建失败，且**绝不误删别人的已有文件**；
  3. 待 Bitmap 完整编码保存到流中并关闭后，调用原子性的 `File.Move(temporaryPath, outputPath)` 发布为最终文件（若目标文件已被抢先创建，Move 失败拒绝覆盖）；
  4. 移动成功后立即置 `ownsTemporary = false`，放弃原路径删除权；仅当中间发生编码异常或 IO 错误时，`finally` 才会尽力清理本次自己创建的临时文件。

### 2.2 相机拟合（Camera Fit）与多实体几何包围框
- **视口 10% 余量保真**：
  自动匹配输出图片的像素宽高比 $aspect = width / height$，取 $fieldHeight = \max(spanY, spanX / aspect) \times 1.1$，确保图形在任何高宽比下都不会被视口裁剪。
- **点状退化图形防护**：
  若模型空间仅有一个点（$spanX = 0, spanY = 0$），自动退回为 1 个图形单位的基础范围再加余量，防止相机视野退化为 0。
- **俯视相机 Z 轴安全高度**：
  相机视点设置为 $eyeZ = maxZ + \max(1, |maxZ| \times 1e-12)$，视线朝向 $-Z$ 轴，目标点为图形中心点，始终居高临下俯视全图。

### 2.3 AcGs 图形系统生命周期与内核引用计数
- **三层嵌套清理模型**：
  在 AutoCAD 渲染架构中，AcGs（Graphics System）对象的释放顺序直接决定宿主进程是否会在退出时崩溃或泄漏显存：
  1. *第一层（图元解绑）*：`try { view.EraseAll(); } finally { if (attached) device.Erase(view); }`。必须先从视图中移除所有 Drawable 实体指针，并从离屏 Device 中注销该 View。
  2. *第二层（设备与视图销毁）*：使用 `using var view`、`using var model`、`using var device`，逆序释放托管包装与其内部 C++ 句柄。
  3. *第三层（内核归还）*：`CadGS.Manager.ReleaseGraphicsKernel(kernel)` 放在最外层 `finally` 中配对释放。依据 Autodesk 官方规范，Acquire 的内核带有引用计数，必须调用 Release 归还，绝不能二次调用 delete。
  4. *最外层（外部数据库释放）*：整个图形渲染生命周期被外部 `Database` 和只读 `Transaction` 完全包裹，确保当 GS 视图访问图元几何时数据库始终有效，GS 释放完毕后外部数据库方才关闭，时序极其严密。

---

## 3. 验证证据与测试覆盖

- **生产核心独立测试 (`DwgPreviewBeta.CoreTests`)**：
  - 运行命令：`dotnet run --project tests/DwgPreviewBeta.CoreTests/DwgPreviewBeta.CoreTests.csproj --configuration Release`
  - 实际结果：**55 项检查全部 PASS**。
  - 覆盖场景：尺寸范围限制（1~8192、1600万像素上限）、相机高宽比自适应、退化单点与极端大范围拟合、NTFS 数据流拦截、真实文件 IO 下的临时文件冲突、写入中途异常清理、并发目标已被占用时不覆盖等。
- **宿主集成命令 (`TestTextMetricsBeta` & `TestDwgPreviewBeta`)**：
  - 文字测量命令：独立背景数据库、Arial 样式字高/宽度倍增验证、空格进笔验证、转义 `%%d` 比对、外层写事务中临时样式隔离、缺失 SHX 字体拒绝、不支持目标即时拦截；
  - DWG 预览命令：创建临时 DWG、连续渲染 3 次不同尺寸 PNG、核对源 DWG 哈希未变、当前文档视图与 WorkingDatabase 保持不变、空图与损坏文件安全拦截；
  - 状态：编译通过，真实 CAD 宿主执行状态依规保持为 `Not run`。

---

## 4. 评审结论

- **结论**：**通过（Approved）**。
- **边界**：静态审查与预览核心回归可支持已列出的范围／文件行为；字体、WorkingDatabase 及 GS 原生资源生命周期尚无实际宿主验收证据。
