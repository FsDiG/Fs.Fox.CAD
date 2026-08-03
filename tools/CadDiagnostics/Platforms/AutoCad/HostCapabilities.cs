namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Describes the SDK baseline used to compile the current diagnostic assembly.
/// Runtime-host compatibility must still be validated inside AutoCAD.
/// </summary>
internal static class HostCapabilities
{
#if AC_2019
    internal const string TargetApi = "AutoCAD 2019 API";
    internal const string SdkPackageVersion = "AutoCAD.NET 23.0.0";
#elif AC_2025
    internal const string TargetApi = "AutoCAD 2025 API";
    internal const string SdkPackageVersion = "AutoCAD.NET 25.0.1";
#else
#error A supported AutoCAD API target must be selected.
#endif

    internal static void ReportUnavailable(string feature, string reason)
    {
        var message = $"\n{feature} is not available for {TargetApi}: {reason}";
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        if (document is null)
        {
            CadApplication.ShowAlertDialog(message.Trim());
            return;
        }

        document.Editor.WriteMessage(message);
    }
}
