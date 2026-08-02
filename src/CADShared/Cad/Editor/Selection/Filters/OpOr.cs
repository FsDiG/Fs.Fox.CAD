namespace Fs.Fox.Cad;

/// <summary>
/// 逻辑或类
/// </summary>
public class OpOr : OpList
{
    /// <summary>
    /// 符号名
    /// </summary>
    public override string Name => "Or";

    /// <summary>
    /// 添加过滤条件
    /// </summary>
    /// <param name="value">过滤器对象</param>
    public override void Add(OpFilter value)
    {
        if (value is OpOr opor)
        {
            foreach (var item in opor)
                Lst.Add(item);
        }
        else
        {
            Lst.Add(value);
        }
    }
}
