namespace Fs.Fox.Cad;

/// <summary>
/// 逻辑非类
/// </summary>
public class OpNot : OpLogi
{
    private OpFilter Value { get; }

    /// <summary>
    /// 逻辑非类构造函数
    /// </summary>
    /// <param name="value">OpFilter数据</param>
    public OpNot(OpFilter value)
    {
        Value = value;
    }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Not";

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>OpFilter迭代器</returns>
    [DebuggerStepThrough]
    public override IEnumerator<OpFilter> GetEnumerator()
    {
        yield return Value;
    }
}
