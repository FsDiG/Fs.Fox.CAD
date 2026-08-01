namespace Fs.Fox.Cad;

/// <summary>
/// 逻辑异或类
/// </summary>
public class OpXor : OpLogi
{
    /// <summary>
    /// 左操作数
    /// </summary>
    public OpFilter Left { get; }

    /// <summary>
    /// 右操作数
    /// </summary>
    public OpFilter Right { get; }

    /// <summary>
    /// 逻辑异或类构造函数
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    public OpXor(OpFilter left, OpFilter right)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Xor";

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>选择集过滤器类型迭代器</returns>
    [DebuggerStepThrough]
    public override IEnumerator<OpFilter> GetEnumerator()
    {
        yield return Left;
        yield return Right;
    }
}
