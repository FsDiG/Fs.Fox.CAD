namespace Fs.Fox.Cad;

/// <summary>
/// Native CAD entry points shared by the supported x64 hosts.
/// </summary>
internal static class PInvokeCad
{
    private const string GetAdsNameEntryPoint =
        "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z";

    private const string GetZdsNameEntryPoint =
        "?zcdbGetZdsName@@YA?AW4ErrorStatus@Zcad@@AEAY01_JVZcDbObjectId@@@Z";

    // P/Invoke resolves the PE export names, not the C++ symbols exposed by ZWCAD.lib.
    private const string ZcdbEntGetEntryPoint = "zcdbEntGet";
    private const string ZcdbEntModEntryPoint = "zcdbEntMod";
    private const string ZcdbEntUpdEntryPoint = "zcdbEntUpd";

#if ZWCAD
    [DllImport("ZwDatabase.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = GetZdsNameEntryPoint, ExactSpelling = true)]
    private static extern int ZcdbGetZdsName(out CadAdsName adsName, ObjectId objectId);

    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = ZcdbEntGetEntryPoint, ExactSpelling = true)]
    private static extern IntPtr ZcdbEntGet(ref CadAdsName adsName);

    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = ZcdbEntModEntryPoint, ExactSpelling = true)]
    private static extern int ZcdbEntMod(IntPtr buffer);

    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = ZcdbEntUpdEntryPoint, ExactSpelling = true)]
    private static extern int ZcdbEntUpd(ref CadAdsName adsName);

    internal static int GetAdsName(ObjectId objectId, out CadAdsName adsName)
    {
        return ZcdbGetZdsName(out adsName, objectId);
    }

    internal static IntPtr EntGet(ref CadAdsName adsName)
    {
        return ZcdbEntGet(ref adsName);
    }

    internal static int EntMod(IntPtr buffer)
    {
        return ZcdbEntMod(buffer);
    }

    internal static int EntUpd(ref CadAdsName adsName)
    {
        return ZcdbEntUpd(ref adsName);
    }
#elif GCAD
    // GStarCAD native P/Invoke
    // The GetAdsName/EntGet/EntMod/EntUpd interop is not supported on GStarCAD
    // because the native entry points and ADS name structures differ per version.
    // These stubs prevent CS0117 on callers; callers must guard with #if !GCAD.
    internal static int GetAdsName(ObjectId objectId, out CadAdsName adsName)
    {
        adsName = default;
        return 0;
    }

    internal static IntPtr EntGet(ref CadAdsName adsName)
    {
        return IntPtr.Zero;
    }

    internal static int EntMod(IntPtr buffer)
    {
        return 0;
    }

    internal static int EntUpd(ref CadAdsName adsName)
    {
        return 0;
    }
#else
#if AC_2019
    private const string AcDbModule = "acdb23.dll";
#elif AC_2021
    private const string AcDbModule = "acdb24.dll";
#elif AC_2025
    private const string AcDbModule = "acdb25.dll";
#elif AC_2027
    private const string AcDbModule = "acdb26.dll";
#else
#error Unsupported AutoCAD target. Add its acdb module before enabling EntGet.
#endif

    [DllImport(AcDbModule, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = GetAdsNameEntryPoint, ExactSpelling = true)]
    private static extern int AcdbGetAdsName(out CadAdsName adsName, ObjectId objectId);

    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntGet", ExactSpelling = true)]
    private static extern IntPtr AcdbEntGet(ref CadAdsName adsName);

    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntMod", ExactSpelling = true)]
    private static extern int AcdbEntMod(IntPtr buffer);

    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntUpd", ExactSpelling = true)]
    private static extern int AcdbEntUpd(ref CadAdsName adsName);

    internal static int GetAdsName(ObjectId objectId, out CadAdsName adsName)
    {
        return AcdbGetAdsName(out adsName, objectId);
    }

    internal static IntPtr EntGet(ref CadAdsName adsName)
    {
        return AcdbEntGet(ref adsName);
    }

    internal static int EntMod(IntPtr buffer)
    {
        return AcdbEntMod(buffer);
    }

    internal static int EntUpd(ref CadAdsName adsName)
    {
        return AcdbEntUpd(ref adsName);
    }
#endif
}

/// <summary>
/// x64 ads_name/zds_name storage: two contiguous 64-bit values.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CadAdsName
{
    public long First;
    public long Second;
}
