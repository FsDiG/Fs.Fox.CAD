namespace Fs.Fox.Cad;

/// <summary>
/// 加载时自动执行接口
/// </summary>
public interface IFoxAutoGo
{
    /// <summary>
    /// 控制加载顺序
    /// </summary>
    /// <returns></returns>
    Sequence SequenceId();

    /// <summary>
    /// 关闭cad的时候会自动执行
    /// </summary>
    void Terminate();

    /// <summary>
    /// 打开cad的时候会自动执行
    /// </summary>
    void Initialize();
}
