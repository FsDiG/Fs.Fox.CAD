using System.Reflection;

namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Extracts the embedded legacy report browser beside a generated XML report.
/// Only files owned by this assembly are overwritten; unrelated files are never removed.
/// </summary>
internal static class ReportResourceExtractor
{
    private const string ResourcePrefix = "Fs.Fox.CAD.Diagnostics.ReportBrowser/";
    private const string OutputDirectoryName = "FsFoxCadDiagnostics.ReportBrowser";

    internal static string ExtractBeside(string reportPath)
    {
        if (string.IsNullOrEmpty(reportPath))
            throw new ArgumentException("A report path is required.", nameof(reportPath));

        var reportDirectory = Path.GetDirectoryName(Path.GetFullPath(reportPath))
                              ?? throw new InvalidOperationException("The report path has no parent directory.");
        var outputRoot = Path.GetFullPath(Path.Combine(reportDirectory, OutputDirectoryName));
        Directory.CreateDirectory(outputRoot);

        var assembly = typeof(ReportResourceExtractor).Assembly;
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal))
                continue;

            var relativePath = resourceName.Substring(ResourcePrefix.Length)
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var outputPath = Path.GetFullPath(Path.Combine(outputRoot, relativePath));
            if (!outputPath.StartsWith(outputRoot + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Invalid embedded report path: {resourceName}");

            var outputDirectory = Path.GetDirectoryName(outputPath)
                                  ?? throw new InvalidOperationException("The embedded report path has no parent directory.");
            Directory.CreateDirectory(outputDirectory);
            using var input = assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            input.CopyTo(output);
        }

        return Path.Combine(outputRoot, "ObjCountReport.html");
    }
}
