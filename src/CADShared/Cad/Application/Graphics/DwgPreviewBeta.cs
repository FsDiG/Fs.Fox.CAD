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
/// 能力来源：Zfgk GsPreviewUtil。使用调用方显式提供的当前文档的图形管理器，不能作为无 CAD 进程的转换器。
/// 必须在 AutoCAD 允许的主线程命令上下文执行；调用方负责文档生命周期和必要的锁，不支持并发渲染。
/// </remarks>
public static class DwgPreviewBeta
{
    /// <summary>以固定 WCS 俯视方向、保留宽高比，将外部 DWG 模型空间输出为新 PNG 文件。</summary>
    /// <param name="graphicsDocument">当前活动 AutoCAD 文档，只借用其 GraphicsManager，不改变文档或当前视图。</param>
    /// <param name="drawingPath">可读取的 .dwg 路径；读取磁盘内容，不包含未保存的文档修改。</param>
    /// <param name="outputPngPath">尚不存在的独立 .png 文件路径；父目录必须存在，不接受数据流路径，不覆盖任何现有文件。</param>
    /// <param name="width">1 至 8192 像素；总像素不能超过 16777216。</param>
    /// <param name="height">1 至 8192 像素；总像素不能超过 16777216。</param>
    /// <remarks>
    /// 仅支持 AutoCAD 编译目标。源 DWG 使用只读共享打开；本方法不修改 WorkingDatabase、UCS、当前视图或全局配置。
    /// 不可见实体及关闭/冻结图层不参与范围计算和绘制。没有可绘制实体或有效范围时抛出异常，不输出空白成功结果。
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
    public static void SavePng(Document graphicsDocument, string drawingPath, string outputPngPath,
        int width, int height)
    {
        if (graphicsDocument is null)
            throw new ArgumentNullException(nameof(graphicsDocument));
        if (!ReferenceEquals(graphicsDocument, CadApp.DocumentManager.MdiActiveDocument))
            throw new ArgumentException("必须使用当前活动文档的图形管理器。", nameof(graphicsDocument));
        var request = DwgPreviewCoreBeta.ValidateRequest(drawingPath, outputPngPath, width, height);
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
            // An unsupported/invalid entity extent is a failure, not a reason to silently crop its geometry.
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
            bounds.MaxPoint.X, bounds.MaxPoint.Y, bounds.MaxPoint.Z, width, height);

        // The manager belongs to the document. Only objects created for this render are owned here.
        var manager = graphicsDocument.GraphicsManager;
        using var descriptor = new CadGS.KernelDescriptor();
        descriptor.addRequirement(CadUniqueString.Intern("3D Drawing"));
        var kernel = CadGS.Manager.AcquireGraphicsKernel(descriptor);
        if (kernel is null)
            throw new InvalidOperationException("AutoCAD 未提供所需的图形内核。");
        try
        {
            using var device = manager.CreateAutoCADOffScreenDevice(kernel);
            if (device is null)
                throw new InvalidOperationException("无法创建离屏图形设备。");
            device.OnSize(new Size(width, height));
            using var model = manager.CreateAutoCADModel(kernel);
            if (model is null)
                throw new InvalidOperationException("无法创建图形模型。");
            using var view = new CadGS.View();
            var attached = false;
            try
            {
                if (!device.Add(view))
                    throw new InvalidOperationException("无法将离屏视图关联到设备。");
                attached = true;
                foreach (var entity in drawables)
                    if (!view.Add(entity, model))
                        throw new InvalidOperationException("无法将源图形加入离屏视图。");
                view.SetView(new Point3d(fit.CenterX, fit.CenterY, fit.EyeZ),
                    new Point3d(fit.CenterX, fit.CenterY, fit.CenterZ), Vector3d.YAxis,
                    fit.FieldWidth, fit.FieldHeight);
                view.Invalidate();
                device.Update();
                using var bitmap = device.GetSnapshot(new Rectangle(0, 0, width, height));
                if (bitmap is null || bitmap.Width != width || bitmap.Height != height)
                    throw new InvalidOperationException("图形系统返回了无效尺寸的截图。");
                SaveNewPng(bitmap, request.OutputPath);
            }
            finally
            {
                try { view.EraseAll(); }
                finally { if (attached) device.Erase(view); }
            }
        }
        finally
        {
            // Acquired kernels are reference-counted; use the manager's release operation, not a second native delete.
            CadGS.Manager.ReleaseGraphicsKernel(kernel);
        }
    }

    private static void SaveNewPng(Bitmap bitmap, string outputPath)
    {
        var temporary = Path.Combine(Path.GetDirectoryName(outputPath)!, ".fox-preview-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                bitmap.Save(stream, ImageFormat.Png);
            File.Move(temporary, outputPath);
        }
        finally
        {
            // Preserve rendering/IO exceptions; a cleanup failure may leave this clearly named owned temporary file.
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
#endif
