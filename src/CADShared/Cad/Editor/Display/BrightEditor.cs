// ReSharper disable InconsistentNaming

namespace Fs.Fox.Cad;

/// <summary>
/// 刷新模式
/// </summary>
[Flags]
public enum BrightEditor
{
    /// <summary>
    /// 刷新屏幕,图元不生成(例如块还是旧的显示)
    /// </summary>
    UpdateScreen = 1,

    /// <summary>
    /// 刷新全图
    /// </summary>
    Regen = 2,

    /// <summary>
    /// 清空选择集
    /// </summary>
    SelectionClean = 4,

    /// <summary>
    /// 视口外
    /// </summary>
    ViewportsFrom = 8,

    /// <summary>
    /// 视口内
    /// </summary>
    ViewportsIn = 16,
}
