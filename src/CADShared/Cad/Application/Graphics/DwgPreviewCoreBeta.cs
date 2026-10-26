using System;
using System.IO;

namespace Fs.Fox.Cad;

// 将路径、发布与相机数值规则和 GS 调用隔离，核心回归可直接调用这些生产实现。
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
    // 同时限制单边和总像素数，控制离屏截图的内存开销；乘积使用 long，避免限制调整后发生整数溢出。
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
        // 先固定绝对路径，后续读取、编码与发布使用同一名称；输出竞争仍由 CreateNew/Move 处理。
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
        // 盘符根中的冒号合法，剩余路径中的冒号却可能指向 NTFS 数据流，绕过独立文件的归属约定。
        var root = Path.GetPathRoot(path) ?? string.Empty;
        if (path.Substring(root.Length).IndexOf(':') >= 0)
            throw new ArgumentException("必须使用独立文件路径，不能使用文件数据流路径。", parameterName);
    }

    // 适配层验证请求后提供同目录且不同名的临时/目标路径。
    // 编码写入成功不等于发布成功；将 IO 归属独立出来，才能用真实文件覆盖两阶段的失败路径。
    internal static void WriteNewOutput(string temporaryPath, string outputPath, Action<Stream> writeContent)
    {
        var ownsTemporary = false;
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                // CreateNew 失败时路径可能属于其他调用方；只有成功取得流后才有清理该文件的权利。
                ownsTemporary = true;
                writeContent(stream);
            }
            // 路径预检不能阻止随后创建目标的竞争者；不覆盖的 Move 是最终发布保护。
            File.Move(temporaryPath, outputPath);
            // 移动成功后不再拥有旧名称，之后同名出现的文件不能由本次 finally 删除。
            ownsTemporary = false;
        }
        finally
        {
            if (ownsTemporary)
            {
                // 清理失败不能掩盖原始编码/IO 异常；只有本次成功创建的临时文件才可尝试删除。
                try { File.Delete(temporaryPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    internal static DwgPreviewFitBeta Fit(double minX, double minY, double minZ,
        double maxX, double maxY, double maxZ, int width, int height)
    {
        ValidateDimensions(width, height);
        if (!IsFinite(minX) || !IsFinite(minY) || !IsFinite(minZ) ||
            !IsFinite(maxX) || !IsFinite(maxY) || !IsFinite(maxZ) ||
            minX > maxX || minY > maxY || minZ > maxZ)
            throw new ArgumentException("图形范围必须有限且有序。", "bounds");
        var spanX = maxX - minX;
        var spanY = maxY - minY;
        if (!IsFinite(spanX) || !IsFinite(spanY))
            throw new ArgumentException("图形范围过大，无法建立有限视图。", "bounds");
        if (spanX == 0 && spanY == 0)
        {
            spanX = 1;
            spanY = 1;
        }
        var aspect = (double)width / height;
        // 先容纳两个投影轴，再增加总计 10% 的余量；相机宽高比必须与像素宽高比相同。
        var fieldHeight = Math.Max(spanY, spanX / aspect) * 1.1;
        var fieldWidth = fieldHeight * aspect;
        // 分别折半再求和，避免同号大坐标的中点计算先发生加法溢出。
        var centerX = minX / 2 + maxX / 2;
        var centerY = minY / 2 + maxY / 2;
        var centerZ = minZ / 2 + maxZ / 2;
        // 眼点高于全部几何；相对偏移避免大标高下加 1 被浮点舍入吞掉、视线退化为零。
        var eyeZ = maxZ + Math.Max(1, Math.Abs(maxZ) * 1e-12);
        // 有限眼点与目标仍可能在相减时溢出；GS 使用这条方向向量，不能只验证两个端点。
        var viewDistance = eyeZ - centerZ;
        if (!IsFinite(fieldWidth) || !IsFinite(fieldHeight) || fieldWidth <= 0 || fieldHeight <= 0 ||
            !IsFinite(eyeZ) || !IsFinite(viewDistance) || viewDistance <= 0)
            throw new ArgumentException("图形范围无法形成有效的 WCS 俯视相机。", "bounds");
        return new DwgPreviewFitBeta(centerX, centerY, centerZ, eyeZ, fieldWidth, fieldHeight);
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
