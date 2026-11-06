#if ACAD
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ArgumentException = System.ArgumentException;
using ArgumentNullException = System.ArgumentNullException;
using CadUniqueString = Autodesk.AutoCAD.UniqueString;

namespace Fs.Fox.Cad;

/// <summary>在 AutoCAD 宿主中将外部 DWG 模型空间导出为新的 PNG。Beta：真实宿主验收尚待执行。</summary>
/// <remarks>
/// 使用调用方显式提供的当前文档的图形管理器，不能作为无 CAD 进程的转换器。
/// 必须在 AutoCAD 允许的主线程命令上下文执行；调用方负责文档生命周期和必要的锁，不支持并发渲染。
/// </remarks>
public static class DwgPreviewRendererBeta
{
    /// <summary>以固定 WCS 俯视方向、保留宽高比，将外部 DWG 模型空间输出为新 PNG 文件。</summary>
    /// <param name="graphicsDocument">当前活动 AutoCAD 文档，只借用其 GraphicsManager，不改变文档或当前视图。</param>
    /// <param name="sourceDwgPath">可读取的 .dwg 路径；读取磁盘内容，不包含未保存的文档修改。</param>
    /// <param name="outputPngPath">尚不存在的独立 .png 文件路径；父目录必须存在，不接受数据流路径，不覆盖任何现有文件。</param>
    /// <param name="pixelWidth">1 至 8192 像素；总像素不能超过 16777216。</param>
    /// <param name="pixelHeight">1 至 8192 像素；总像素不能超过 16777216。</param>
    /// <remarks>
    /// 仅支持 AutoCAD 编译目标。源 DWG 使用只读共享打开；本方法不修改 WorkingDatabase、UCS、当前视图或全局配置。
    /// 范围筛选检查模型空间顶层实体的可见性及其图层关闭/冻结状态；复合实体使用宿主返回的整体范围。
    /// 没有通过筛选的顶层实体或无法取得有效范围时失败；不检查输出像素是否空白，也不消除块内隐藏几何造成的留白。
    /// 先在输出目录写临时 PNG，再移动为目标文件；竞争创建目标文件也不会导致覆盖。
    /// 创建的外部数据库、只读事务、图形设备/模型/视图和 Bitmap 全部由本方法释放；借用的文档与 GraphicsManager 不释放。
    /// 图形内核由 AcquireGraphicsKernel/ReleaseGraphicsKernel 配对管理。渲染或 IO 失败时异常传播；临时文件尽力清理。
    /// </remarks>
    /// <exception cref="ArgumentNullException">文档为 null。</exception>
    /// <exception cref="ArgumentException">文档不是当前活动文档、路径/扩展名非法或图形范围无法建立视图。</exception>
    /// <exception cref="ArgumentOutOfRangeException">像素尺寸超出支持范围。</exception>
    /// <exception cref="FileNotFoundException">源文件不存在或不可访问。</exception>
    /// <exception cref="DirectoryNotFoundException">输出父目录不存在。</exception>
    /// <exception cref="IOException">输出已存在或文件访问失败。</exception>
    /// <exception cref="InvalidOperationException">图形为空、图形系统创建失败或视图关联失败。</exception>
    public static void RenderModelSpaceToPng(Document graphicsDocument, string sourceDwgPath, string outputPngPath,
        int pixelWidth, int pixelHeight)
    {
        ValidateGraphicsDocument(graphicsDocument);
        var request = DwgPreviewCoreBeta.ValidateRequest(sourceDwgPath, outputPngPath, pixelWidth, pixelHeight);
        // GS 视图保留源 DBObject 引用。外部数据库和读取事务必须包住整个 GS 生命周期，
        // 不能在视图解绑前关闭，即使异常发生在截图或 PNG 编码阶段也必须遵守此顺序。
        using var drawing = new Database(false, true);
        drawing.ReadDwgFile(request.DrawingPath, FileOpenMode.OpenForReadAndAllShare, false, null);
        using var transaction = drawing.TransactionManager.StartTransaction();
        var table = (BlockTable)transaction.GetObject(drawing.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)transaction.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        var drawables = new List<Entity>();
        Extents3d? combined = null;
        foreach (ObjectId id in modelSpace)
        {
            if (transaction.GetObject(id, OpenMode.ForRead) is not Entity entity || !entity.Visible)
                continue;
            var layer = (LayerTableRecord)transaction.GetObject(entity.LayerId, OpenMode.ForRead);
            if (layer.IsOff || layer.IsFrozen)
                continue;
            // 对复合实体不另做展开；它的整体范围不等于所有可见叶节点的紧致范围。
            // 范围读取失败必须传播，否则跳过该实体后成功输出会悄悄裁掉图形。
            var extent = entity.GeometricExtents;
            if (combined.HasValue)
            {
                var current = combined.Value;
                current.AddExtents(extent);
                combined = current;
            }
            else
                combined = extent;
            drawables.Add(entity);
        }
        if (!combined.HasValue || drawables.Count == 0)
            throw new InvalidOperationException("源 DWG 模型空间中没有可绘制实体。");
        var bounds = combined.Value;
        var fit = DwgPreviewCoreBeta.Fit(bounds.MinPoint.X, bounds.MinPoint.Y, bounds.MinPoint.Z,
            bounds.MaxPoint.X, bounds.MaxPoint.Y, bounds.MaxPoint.Z, pixelWidth, pixelHeight);

        RenderBitmap(graphicsDocument, drawables, pixelWidth, pixelHeight, view =>
        {
            view.SetView(new Point3d(fit.CenterX, fit.CenterY, fit.EyeZ),
                new Point3d(fit.CenterX, fit.CenterY, fit.CenterZ), Vector3d.YAxis,
                fit.FieldWidth, fit.FieldHeight);
        }, bitmap => SaveNewPng(bitmap, request.OutputPath));
    }

    /// <summary>在调用方的有效读取事务内，将明确选择的实体同步绘制为独立 Bitmap。</summary>
    /// <param name="graphicsDocument">当前活动文档，只借用其图形管理器。</param>
    /// <param name="sourceDatabase">实体所属数据库，由调用方保持有效。</param>
    /// <param name="transaction">覆盖本次调用的来源读取事务，由调用方结束。</param>
    /// <param name="entityIds">同一来源数据库中的实体 ID，选择与业务过滤由调用方负责。</param>
    /// <param name="bounds">业务取景范围，固定 WCS 俯视，ZoomExtents 后 Zoom(0.9)。</param>
    /// <param name="pixelWidth">像素宽度，限制与文件入口一致。</param>
    /// <param name="pixelHeight">像素高度，限制与文件入口一致。</param>
    /// <param name="backgroundColor">离屏设备背景。</param>
    /// <param name="logicalPalette">可选的 256 色 ACI 调色板，TrueColor 仍由实体属性决定。</param>
    /// <returns>已经脱离 GS 设备的独立 Bitmap，由调用方 Dispose。</returns>
    /// <remarks>
    /// 仅 AutoCAD 主线程命令上下文，同步且不支持并发。方法结束前解除所有 drawable 引用并释放自有 GS 资源。
    /// 不提交或释放调用方事务，不释放来源数据库、文档或 GraphicsManager。参数、读取和绘制失败传播给调用方。
    /// </remarks>
    public static Bitmap RenderEntitiesToBitmap(Document graphicsDocument, Database sourceDatabase,
        Transaction transaction, IReadOnlyCollection<ObjectId> entityIds, Extents3d bounds,
        int pixelWidth, int pixelHeight, DrawingColor backgroundColor, DrawingColor[]? logicalPalette = null)
    {
        ValidateGraphicsDocument(graphicsDocument);
        if (sourceDatabase is null) throw new ArgumentNullException(nameof(sourceDatabase));
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));
        if (entityIds is null) throw new ArgumentNullException(nameof(entityIds));
        if (sourceDatabase.IsDisposed || transaction.IsDisposed)
            throw new ArgumentException("来源数据库和读取事务必须保持有效。");
        if (logicalPalette != null && logicalPalette.Length != 256)
            throw new ArgumentException("逻辑调色板需要 256 项颜色。", nameof(logicalPalette));
        var fit = DwgPreviewCoreBeta.Fit(bounds.MinPoint.X, bounds.MinPoint.Y, bounds.MinPoint.Z,
            bounds.MaxPoint.X, bounds.MaxPoint.Y, bounds.MaxPoint.Z, pixelWidth, pixelHeight);
        var drawables = new List<Entity>(entityIds.Count);
        foreach (var id in entityIds)
        {
            if (id.IsNull || !id.IsValid || id.IsErased || id.Database.UnmanagedObject != sourceDatabase.UnmanagedObject)
                throw new ArgumentException("实体 ID 必须属于有效的来源数据库。", nameof(entityIds));
            if (transaction.GetObject(id, OpenMode.ForRead) is not Entity entity)
                throw new ArgumentException("输入 ID 必须指向实体。", nameof(entityIds));
            drawables.Add(entity);
        }
        if (drawables.Count == 0) throw new InvalidOperationException("没有选定可绘制实体。");
        Bitmap? result = null;
        try
        {
            RenderBitmap(graphicsDocument, drawables, pixelWidth, pixelHeight, view =>
            {
                view.SetView(new Point3d(fit.CenterX, fit.CenterY, fit.EyeZ),
                    new Point3d(fit.CenterX, fit.CenterY, fit.CenterZ), Vector3d.YAxis, pixelWidth, pixelHeight);
                view.ZoomExtents(bounds.MinPoint, bounds.MaxPoint);
                view.Zoom(0.9);
            }, bitmap => result = bitmap.Clone(new Rectangle(0, 0, pixelWidth, pixelHeight), PixelFormat.Format32bppArgb),
                backgroundColor, logicalPalette);
            return result!;
        }
        catch
        {
            result?.Dispose();
            throw;
        }
    }

    private static void ValidateGraphicsDocument(Document graphicsDocument)
    {
        if (graphicsDocument is null) throw new ArgumentNullException(nameof(graphicsDocument));
        if (!ReferenceEquals(graphicsDocument, CadApp.DocumentManager.MdiActiveDocument))
            throw new ArgumentException("必须使用当前活动文档的图形管理器。", nameof(graphicsDocument));
    }

    // 所有图片消费者都在 drawable 解绑前完成工作，设备截图的寿命不会越过本方法。
    private static void RenderBitmap(Document graphicsDocument, IReadOnlyList<Entity> drawables,
        int pixelWidth, int pixelHeight, Action<CadGS.View> configureView, Action<Bitmap> consumeBitmap,
        DrawingColor? backgroundColor = null, DrawingColor[]? logicalPalette = null)
    {
        var manager = graphicsDocument.GraphicsManager;
        CadGS.GraphicsKernel? kernel = null;
        CadGS.Device? device = null;
        CadGS.Model? model = null;
        CadGS.View? view = null;
        var attached = false;
        Exception? failure = null;
        try
        {
            using var descriptor = new CadGS.KernelDescriptor();
            descriptor.addRequirement(CadUniqueString.Intern("3D Drawing"));
            kernel = CadGS.Manager.AcquireGraphicsKernel(descriptor);
            if (kernel is null) throw new InvalidOperationException("AutoCAD 未提供所需的图形内核。");
            device = manager.CreateAutoCADOffScreenDevice(kernel);
            if (device is null)
                throw new InvalidOperationException("无法创建离屏图形设备。");
            if (backgroundColor.HasValue) device.BackgroundColor = backgroundColor.Value;
            if (logicalPalette != null) device.SetLogicalPalette(logicalPalette);
            device.OnSize(new Size(pixelWidth, pixelHeight));
            model = manager.CreateAutoCADModel(kernel);
            if (model is null)
                throw new InvalidOperationException("无法创建图形模型。");
            view = new CadGS.View();
            if (!device.Add(view)) throw new InvalidOperationException("无法将离屏视图关联到设备。");
            attached = true;
            foreach (var entity in drawables)
                if (!view.Add(entity, model)) throw new InvalidOperationException("无法将源图形加入离屏视图。");
            configureView(view);
            view.Invalidate();
            device.Update();
            using var bitmap = device.GetSnapshot(new Rectangle(0, 0, pixelWidth, pixelHeight));
            if (bitmap is null || bitmap.Width != pixelWidth || bitmap.Height != pixelHeight)
                throw new InvalidOperationException("图形系统返回了无效尺寸的截图。");
            consumeBitmap(bitmap);
        }
        catch (Exception error) { failure = error; throw; }
        finally
        {
            List<Exception>? cleanupErrors = null;
            void Cleanup(Action release)
            {
                try { release(); }
                catch (Exception error) { (cleanupErrors ??= new List<Exception>()).Add(error); }
            }
            if (view != null) Cleanup(view.EraseAll);
            if (attached && device != null && view != null) Cleanup(() => device.Erase(view));
            if (view != null) Cleanup(view.Dispose);
            if (model != null) Cleanup(model.Dispose);
            if (device != null) Cleanup(device.Dispose);
            if (kernel != null) Cleanup(() => CadGS.Manager.ReleaseGraphicsKernel(kernel));
            if (cleanupErrors != null)
            {
                var cleanupFailure = new AggregateException("GS 资源释放失败。", cleanupErrors);
                if (failure == null) throw cleanupFailure;
                failure.Data["Fs.Fox.Cad.GraphicsCleanup"] = cleanupFailure;
                Debug.WriteLine(cleanupFailure);
            }
        }
    }

    private static void SaveNewPng(Bitmap bitmap, string outputPath)
    {
        var temporary = Path.Combine(Path.GetDirectoryName(outputPath)!, ".fox-preview-" + Guid.NewGuid().ToString("N") + ".tmp");
        DwgPreviewCoreBeta.WriteNewOutput(temporary, outputPath, stream => bitmap.Save(stream, ImageFormat.Png));
    }
}
#endif
