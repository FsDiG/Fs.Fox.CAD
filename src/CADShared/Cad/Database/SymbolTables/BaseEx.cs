namespace Fs.Fox.Cad;

/// <summary>
/// 基础扩展
/// </summary>
public static class BaseEx
{
    /// <summary>
    /// 判断图层名是否合法
    /// </summary>
    /// <param name="layerName">图层名</param>
    /// <returns>是则返回<c>true</c></returns>
    public static bool IsLegalLayerName(this string layerName)
    {
        return !string.IsNullOrWhiteSpace(layerName) && SymbolUtilityServices.RepairSymbolName(layerName, true) == layerName;
    }
}