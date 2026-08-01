// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Fs.Fox.Cad;

/// <summary>
/// 关键字错误
/// </summary>
public class KeywordException : Exception
{
    /// <summary>
    /// 关键字错误
    /// </summary>
    /// <param name="input">关键字</param>
    public KeywordException(string input)
    {
        Input = input;
    }

    /// <summary>
    /// 关键字
    /// </summary>
    public string Input { get; }
}
