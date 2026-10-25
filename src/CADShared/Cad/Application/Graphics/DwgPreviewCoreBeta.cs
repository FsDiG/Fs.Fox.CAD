using System;
using System.IO;

namespace Fs.Fox.Cad;

// SDK-free policy and camera fitting used by the actual off-screen rendering adapter.
internal readonly struct DwgPreviewRequestBeta
{
    internal readonly string DrawingPath;
    internal readonly string OutputPath;
    internal DwgPreviewRequestBeta(string drawingPath, string outputPath)
    {
        DrawingPath = drawingPath;
        OutputPath = outputPath;
    }
}

internal readonly struct DwgPreviewFitBeta
{
    internal readonly double CenterX;
    internal readonly double CenterY;
    internal readonly double CenterZ;
    internal readonly double EyeZ;
    internal readonly double FieldWidth;
    internal readonly double FieldHeight;
    internal DwgPreviewFitBeta(double centerX, double centerY, double centerZ, double eyeZ,
        double fieldWidth, double fieldHeight)
    {
        CenterX = centerX;
        CenterY = centerY;
        CenterZ = centerZ;
        EyeZ = eyeZ;
        FieldWidth = fieldWidth;
        FieldHeight = fieldHeight;
    }
}

internal static class DwgPreviewCoreBeta
{
    internal static void ValidateDimensions(int width, int height)
    {
        if (width < 1 || width > 8192)
            throw new ArgumentOutOfRangeException(nameof(width), "图片宽度必须为 1 至 8192 像素。");
        if (height < 1 || height > 8192)
            throw new ArgumentOutOfRangeException(nameof(height), "图片高度必须为 1 至 8192 像素。");
        if ((long)width * height > 16777216)
            throw new ArgumentOutOfRangeException(nameof(width), "图片总像素数不能超过 16777216。");
    }

    internal static DwgPreviewRequestBeta ValidateRequest(string drawingPath, string outputPath,
        int width, int height)
    {
        ValidateDimensions(width, height);
        if (string.IsNullOrWhiteSpace(drawingPath))
            throw new ArgumentException("必须指定源 DWG 文件。", nameof(drawingPath));
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("必须指定新的 PNG 文件。", nameof(outputPath));
        var drawing = Path.GetFullPath(drawingPath);
        var output = Path.GetFullPath(outputPath);
        ValidateOrdinaryFilePath(drawing, nameof(drawingPath));
        ValidateOrdinaryFilePath(output, nameof(outputPath));
        if (!string.Equals(Path.GetExtension(drawing), ".dwg", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("源文件必须使用 .dwg 扩展名。", nameof(drawingPath));
        if (!string.Equals(Path.GetExtension(output), ".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("输出文件必须使用 .png 扩展名。", nameof(outputPath));
        if (string.Equals(drawing, output, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("输出不能覆盖源文件。", nameof(outputPath));
        if (!File.Exists(drawing))
            throw new FileNotFoundException("源 DWG 文件不存在或不可访问。", drawing);
        var parent = Path.GetDirectoryName(output);
        if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
            throw new DirectoryNotFoundException("PNG 输出目录必须已经存在。");
        if (File.Exists(output) || Directory.Exists(output))
            throw new IOException("PNG 输出路径已存在；预览不会覆盖任何现有文件。");
        return new DwgPreviewRequestBeta(drawing, output);
    }

    private static void ValidateOrdinaryFilePath(string path, string parameterName)
    {
        var root = Path.GetPathRoot(path) ?? string.Empty;
        if (path.Substring(root.Length).IndexOf(':') >= 0)
            throw new ArgumentException("必须使用独立文件路径，不能使用文件数据流路径。", parameterName);
    }

    internal static DwgPreviewFitBeta Fit(double minX, double minY, double minZ,
        double maxX, double maxY, double maxZ, int width, int height)
    {
        ValidateDimensions(width, height);
        if (!Finite(minX) || !Finite(minY) || !Finite(minZ) ||
            !Finite(maxX) || !Finite(maxY) || !Finite(maxZ) ||
            minX > maxX || minY > maxY || minZ > maxZ)
            throw new ArgumentException("图形范围必须有限且有序。", "bounds");
        var spanX = maxX - minX;
        var spanY = maxY - minY;
        if (!Finite(spanX) || !Finite(spanY))
            throw new ArgumentException("图形范围过大，无法建立有限视图。", "bounds");
        if (spanX == 0 && spanY == 0)
        {
            spanX = 1;
            spanY = 1;
        }
        var aspect = (double)width / height;
        var fieldHeight = Math.Max(spanY, spanX / aspect) * 1.1;
        var fieldWidth = fieldHeight * aspect;
        var centerX = minX / 2 + maxX / 2;
        var centerY = minY / 2 + maxY / 2;
        var centerZ = minZ / 2 + maxZ / 2;
        var eyeZ = maxZ + Math.Max(1, Math.Abs(maxZ) * 1e-12);
        if (!Finite(fieldWidth) || !Finite(fieldHeight) || fieldWidth <= 0 || fieldHeight <= 0 ||
            !Finite(eyeZ) || eyeZ <= centerZ)
            throw new ArgumentException("图形范围无法形成有效的 WCS 俯视相机。", "bounds");
        return new DwgPreviewFitBeta(centerX, centerY, centerZ, eyeZ, fieldWidth, fieldHeight);
    }

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
