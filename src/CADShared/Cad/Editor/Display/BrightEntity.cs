// ReSharper disable InconsistentNaming

namespace Fs.Fox.Cad;

/// <summary>
/// 亮显模式
/// </summary>
[Flags]
public enum BrightEntity
{
    /// <summary>
    /// 块更新
    /// </summary>
    RecordGraphicsModified = 1,

    /// <summary>
    /// 标注更新
    /// </summary>
    RecomputeDimensionBlock = 2,

    /// <summary>
    /// 重画
    /// </summary>
    Draw = 4,

    /// <summary>
    /// 亮显
    /// </summary>
    Highlight = 8,

    /// <summary>
    /// 亮显取消
    /// </summary>
    Unhighlight = 16,

    /// <summary>
    /// 显示图元
    /// </summary>
    VisibleTrue = 32,

    /// <summary>
    /// 隐藏图元
    /// </summary>
    VisibleFalse = 64,

    /// <summary>
    /// 平移更新,可以令ctrl+z撤回时候保证刷新
    /// </summary>
    MoveZero = 128,
}
