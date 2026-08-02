namespace Fs.Fox.Cad;

/// <summary>
/// 过滤器逻辑运算符抽象类
/// </summary>
public abstract class OpLogi : OpFilter, IEnumerable<OpFilter>
{
    /// <summary>
    /// 返回-4组码的开始内容
    /// </summary>
    public TypedValue First => new(-4, $"<{Name}");

    /// <summary>
    /// 返回-4组码的结束内容
    /// </summary>
    public TypedValue Last => new(-4, $"{Name}>");

    /// <summary>
    /// 获取过滤条件
    /// </summary>
    /// <returns>TypedValue迭代器</returns>
    //[System.Diagnostics.DebuggerStepThrough]
    public override IEnumerable<TypedValue> GetValues()
    {
        yield return First;
        foreach (var item in this)
        {
            foreach (var value in item.GetValues())
                yield return value;
        }
        yield return Last;
    }

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns>OpFilter迭代器</returns>
    [DebuggerStepThrough]
    public abstract IEnumerator<OpFilter> GetEnumerator();

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
