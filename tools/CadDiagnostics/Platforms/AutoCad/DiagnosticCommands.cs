using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(Fs.Fox.CAD.Diagnostics.AutoCad.DiagnosticCommands))]

namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Provides diagnostic commands that describe the loaded binary and runtime host.
/// </summary>
public sealed class DiagnosticCommands
{
    /// <summary>
    /// Prints the diagnostic tool version, SDK baseline, target framework, runtime
    /// AutoCAD identity and loaded assembly path.
    /// </summary>
    [CommandMethod("MgdDbgAbout", CommandFlags.Modal)]
    public void About()
    {
        var assembly = typeof(DiagnosticCommands).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "unknown";
        var targetFramework = assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName ?? "unknown";

        var lines = new[]
        {
            "MgdDbg / Fs.Fox.CAD.Diagnostics",
            $"Tool version: {informationalVersion}",
            $"Binary target: {HostCapabilities.TargetApi}",
            $"Compile-time SDK: {HostCapabilities.SdkPackageVersion}",
            $"Target framework: {targetFramework}",
            $"Runtime product: {TryGetRuntimeProduct()}",
            $"Runtime AutoCAD version: {TryGetSystemVariable("ACADVER")}",
            $"Host executable version: {TryGetHostExecutableVersion()}",
            $"Assembly: {assembly.Location}",
        };

        var message = string.Join(Environment.NewLine, lines);
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
        {
            CadApplication.ShowAlertDialog(message);
            return;
        }

        document.Editor.WriteMessage($"\n{message.Replace(Environment.NewLine, "\n")}");
    }

    private static string TryGetSystemVariable(string name)
    {
        try
        {
            return Convert.ToString(CadApplication.GetSystemVariable(name),
                       System.Globalization.CultureInfo.InvariantCulture)
                   ?? "unknown";
        }
        catch (System.Exception)
        {
            return "unavailable";
        }
    }

    private static string TryGetRuntimeProduct()
    {
        var product = TryGetSystemVariable("PRODUCT");
        if (!string.Equals(product, "unavailable", StringComparison.OrdinalIgnoreCase))
            return product;

        try
        {
            var versionInfo = Process.GetCurrentProcess().MainModule?.FileVersionInfo;
            return versionInfo?.ProductName ?? Process.GetCurrentProcess().ProcessName;
        }
        catch (System.Exception)
        {
            return "unavailable";
        }
    }

    private static string TryGetHostExecutableVersion()
    {
        try
        {
            var versionInfo = Process.GetCurrentProcess().MainModule?.FileVersionInfo;
            return versionInfo?.FileVersion ?? versionInfo?.ProductVersion ?? "unavailable";
        }
        catch (System.Exception)
        {
            return "unavailable";
        }
    }
}
