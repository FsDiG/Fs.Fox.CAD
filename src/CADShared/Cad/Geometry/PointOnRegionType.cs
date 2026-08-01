namespace Fs.Fox.Cad;

/// <summary>
/// 点与多边形的关系类型枚举
/// </summary>
public enum PointOnRegionType
{
    /// <summary>
    /// 多边形内部
    /// </summary>
    Inside,

    /// <summary>
    /// 多边形上
    /// </summary>
    On,

    /// <summary>
    /// 多边形外
    /// </summary>
    Outside,

    /// <summary>
    /// 错误
    /// </summary>
    Error
}
