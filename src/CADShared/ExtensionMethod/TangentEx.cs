namespace Fs.Fox.Cad;

/// <summary>
/// 天正接口
/// </summary>
public static class TangentEx
{
    #region 获取天正比例

    /// <summary>
    /// 获取天正绘图比例
    /// </summary>
    /// <returns></returns>
    public static double TgetPscale()
    {
        return DocGetPScale();
    }

    [DllImport("tch_kernal.arx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?DocGetPScale@@YANXZ")]
    private static extern double DocGetPScale();

    #endregion
}