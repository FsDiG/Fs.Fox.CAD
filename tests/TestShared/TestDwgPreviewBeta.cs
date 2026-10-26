#if ACAD
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace Test;

/// <summary>仅供显式安排的隔离 AutoCAD 验收，不自动执行或操作用户图纸。</summary>
public static class TestDwgPreviewBeta
{
    /// <summary>生成临时源 DWG，连续渲染三次，核对 PNG 和当前文档状态。</summary>
    [CommandMethod(nameof(Test_DwgPreviewBeta))]
    public static void Test_DwgPreviewBeta()
    {
        var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        var working = HostApplicationServices.WorkingDatabase;
        using var originalView = document.Editor.GetCurrentView();
        var folder = CreateFolder();
        try
        {
            var source = Path.Combine(folder, "source.dwg");
            CreateDrawing(source, withGeometry: true);
            var sourceHash = Hash(source);
            for (var index = 0; index < 3; index++)
            {
                var output = Path.Combine(folder, "preview-" + index + ".png");
                DwgPreviewBeta.SavePng(document, source, output, 640, 320);
                using var bitmap = new Bitmap(output);
                Require(bitmap.Width == 640 && bitmap.Height == 320, "PNG dimensions");
                Require(HasContrast(bitmap), "PNG must contain rendered geometry rather than one uniform background");
                Require(Hash(source) == sourceHash, "Source DWG must remain unchanged");
            }
            Require(ReferenceEquals(working, HostApplicationServices.WorkingDatabase), "WorkingDatabase preserved");
            Require(ReferenceEquals(document, Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument), "Active document preserved");
            using var currentView = document.Editor.GetCurrentView();
            Require(currentView.Target.IsEqualTo(originalView.Target) &&
                    currentView.ViewDirection.IsEqualTo(originalView.ViewDirection) &&
                    currentView.CenterPoint.IsEqualTo(originalView.CenterPoint) &&
                    currentView.Width == originalView.Width && currentView.Height == originalView.Height &&
                    currentView.ViewTwist == originalView.ViewTwist, "Current view preserved");
            Env.Printl("DwgPreviewBeta: three images verified; active document, view and source preserved.");
        }
        finally { Cleanup(folder); }
    }

    /// <summary>验证空图、输出保护和尺寸失败不会产生输出或改变当前数据库。</summary>
    [CommandMethod(nameof(Test_DwgPreviewBetaFailures))]
    public static void Test_DwgPreviewBetaFailures()
    {
        var document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        var working = HostApplicationServices.WorkingDatabase;
        var folder = CreateFolder();
        try
        {
            var source = Path.Combine(folder, "empty.dwg");
            var output = Path.Combine(folder, "empty.png");
            CreateDrawing(source, withGeometry: false);
            var sourceHash = Hash(source);
            Throws<InvalidOperationException>(() => DwgPreviewBeta.SavePng(document, source, output, 100, 100));
            Require(!File.Exists(output), "Empty drawing must not produce success output");
            File.WriteAllText(output, "preserve");
            Throws<IOException>(() => DwgPreviewBeta.SavePng(document, source, output, 100, 100));
            Require(File.ReadAllText(output) == "preserve", "Existing output preserved");
            Throws<System.ArgumentException>(() => DwgPreviewBeta.SavePng(document, source, source, 100, 100));
            Throws<System.ArgumentOutOfRangeException>(() => DwgPreviewBeta.SavePng(document, source, Path.Combine(folder, "size.png"), 8193, 100));
            Throws<FileNotFoundException>(() => DwgPreviewBeta.SavePng(document, Path.Combine(folder, "missing.dwg"), Path.Combine(folder, "missing.png"), 100, 100));
            var corrupt = Path.Combine(folder, "corrupt.dwg");
            var corruptOutput = Path.Combine(folder, "corrupt.png");
            File.WriteAllText(corrupt, "This is deliberately not a DWG file.");
            Throws<Autodesk.AutoCAD.Runtime.Exception>(() => DwgPreviewBeta.SavePng(document, corrupt, corruptOutput, 100, 100));
            Require(!File.Exists(corruptOutput), "Unreadable drawing must not produce output");
            Require(Hash(source) == sourceHash, "Failure paths preserve source");
            Require(ReferenceEquals(working, HostApplicationServices.WorkingDatabase), "Failure paths preserve WorkingDatabase");
            Require(!Directory.EnumerateFiles(folder, ".fox-preview-*.tmp").Any(), "No rendering temporary files expected");
            Env.Printl("DwgPreviewBeta: empty/source/output/size failure checks passed.");
        }
        finally { Cleanup(folder); }
    }

    private static void CreateDrawing(string path, bool withGeometry)
    {
        using var database = new Database(true, true);
        using (var transaction = database.TransactionManager.StartTransaction())
        {
            if (withGeometry)
            {
                var table = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                var model = (BlockTableRecord)transaction.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                using var line = new Line(new Point3d(-10, -5, 0), new Point3d(10, 5, 0));
                model.AppendEntity(line);
                transaction.AddNewlyCreatedDBObject(line, true);
                using var circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 3);
                model.AppendEntity(circle);
                transaction.AddNewlyCreatedDBObject(circle, true);
            }
            transaction.Commit();
        }
        database.SaveAs(path, DwgVersion.Current);
    }

    private static string CreateFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "FsFoxDwgPreviewBeta-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string Hash(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return BitConverter.ToString(sha.ComputeHash(stream));
    }

    private static bool HasContrast(Bitmap bitmap)
    {
        var background = bitmap.GetPixel(0, 0).ToArgb();
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
                if (bitmap.GetPixel(x, y).ToArgb() != background)
                    return true;
        return false;
    }

    private static void Cleanup(string folder)
    {
        var fullPath = Path.GetFullPath(folder);
        var temporaryRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(fullPath).StartsWith("FsFoxDwgPreviewBeta-", StringComparison.Ordinal))
            throw new InvalidOperationException("Unexpected preview test directory.");
        try { if (Directory.Exists(fullPath)) Directory.Delete(fullPath, recursive: true); }
        catch (IOException) { Env.Printl("Temporary preview files remain at: " + fullPath); }
        catch (UnauthorizedAccessException) { Env.Printl("Temporary preview files remain at: " + fullPath); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Throws<T>(Action action) where T : System.Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
#endif
