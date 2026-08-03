#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// Cross-host access to the application status-bar progress meter.
/// </summary>
public static class ProgressMeterUtils
{
    /// <summary>
    /// Creates a status-bar progress meter.
    /// </summary>
    /// <param name="label">Progress label.</param>
    /// <param name="minimum">Minimum position.</param>
    /// <param name="maximum">Maximum position.</param>
    public static void SetApplicationStatusBarProgressMeter(
        string label, int minimum, int maximum)
    {
        ArgumentNullException.ThrowIfNull(label);
        if (minimum > maximum)
        {
            throw new System.ArgumentOutOfRangeException(nameof(minimum), minimum,
                "The minimum progress position cannot exceed the maximum position.");
        }

#if ZWCAD
        EnsureZwcadSuccess(ZcedSetStatusBarProgressMeter(label, minimum, maximum),
            "create the status-bar progress meter");
#elif GCAD
        // GStarCAD: fallback to no-op until native API verified
#else
        Utils.SetApplicationStatusBarProgressMeter(label, minimum, maximum);
#endif
    }

    /// <summary>
    /// Updates the status-bar progress position.
    /// </summary>
    /// <param name="position">Absolute position, or a negative relative increment.</param>
    public static void SetApplicationStatusBarProgressMeter(int position)
    {
#if ZWCAD
        EnsureZwcadSuccess(ZcedSetStatusBarProgressMeterPos(position),
            "update the status-bar progress meter");
#elif GCAD
        // GStarCAD: fallback to no-op until native API verified
#else
        Utils.SetApplicationStatusBarProgressMeter(position);
#endif
    }

    /// <summary>
    /// Removes the progress meter and restores the application status bar.
    /// </summary>
    public static void RestoreApplicationStatusBar()
    {
#if ZWCAD
        ZcedRestoreStatusBar();
#elif GCAD
        // GStarCAD: fallback to no-op until native API verified
#else
        Utils.RestoreApplicationStatusBar();
#endif
    }

#if ZWCAD
    private const string ZwCadModule = "zwcad.exe";

    private const string SetProgressMeterEntryPoint =
        "?zcedSetStatusBarProgressMeter@@YAHPEB_WHH@Z";

    private const string SetProgressMeterPositionEntryPoint =
        "?zcedSetStatusBarProgressMeterPos@@YAHH@Z";

    private const string RestoreStatusBarEntryPoint = "?zcedRestoreStatusBar@@YAXXZ";

    [DllImport(ZwCadModule, CharSet = CharSet.Unicode,
        CallingConvention = CallingConvention.Cdecl,
        EntryPoint = SetProgressMeterEntryPoint, ExactSpelling = true)]
    private static extern int ZcedSetStatusBarProgressMeter(
        [MarshalAs(UnmanagedType.LPWStr)] string label, int minimum, int maximum);

    [DllImport(ZwCadModule, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = SetProgressMeterPositionEntryPoint, ExactSpelling = true)]
    private static extern int ZcedSetStatusBarProgressMeterPos(int position);

    [DllImport(ZwCadModule, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = RestoreStatusBarEntryPoint, ExactSpelling = true)]
    private static extern void ZcedRestoreStatusBar();

    private static void EnsureZwcadSuccess(int result, string operation)
    {
        if (result != 0)
        {
            throw new InvalidOperationException(
                $"ZWCAD failed to {operation}; native result: {result}.");
        }
    }
#endif
}
