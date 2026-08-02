namespace Fs.Fox.Cad;

/// <summary>
/// 加载时自动执行特性
/// </summary>
/// <remarks>
/// 用于初始化和结束回收
/// </remarks>
/// <param name="sequence">优先级</param>
/// <param name="isInitialize"><see langword="true"/>用于初始化;<see langword="false"/>用于结束回收</param>
[AttributeUsage(AttributeTargets.Method)]
// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class IFoxInitializeAttribute(Sequence sequence = Sequence.Last, bool isInitialize = true) : Attribute
{
    /// <summary>
    /// 优先级
    /// </summary>
    internal readonly Sequence SequenceId = sequence;

    /// <summary>
    /// <see langword="true"/>用于初始化;<see langword="false"/>用于结束回收
    /// </summary>
    internal readonly bool IsInitialize = isInitialize;
}
