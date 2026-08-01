namespace Fs.Fox.Cad;

/// <summary>
/// 逻辑与类
/// </summary>
public class OpAnd : OpList
{
    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "And";

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="value">过滤器对象</param>
    public override void Add(OpFilter value)
    {
        if (value is OpAnd opand)
        {
            foreach (var item in opand)
                Lst.Add(item);
        }
        else
        {
            Lst.Add(value);
        }
    }
}
