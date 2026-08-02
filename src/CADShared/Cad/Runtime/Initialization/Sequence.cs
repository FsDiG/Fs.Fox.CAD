namespace Fs.Fox.Cad;

/// <summary>
/// 加载时优先级
/// </summary>
[Flags]
public enum Sequence : byte
{
    /// <summary>
    /// 最先
    /// </summary>
    First,

    /// <summary>
    /// 最后
    /// </summary>
    Last,
}
