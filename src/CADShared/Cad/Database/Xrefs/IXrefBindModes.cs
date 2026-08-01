namespace Fs.Fox.Cad;

/// <summary>
/// 参照绑定模式接口
/// </summary>
public interface IXrefBindModes
{
    /// <summary>
    /// 卸载
    /// </summary>
    public void Unload();

    /// <summary>
    /// 重载
    /// </summary>
    public void Reload();

    /// <summary>
    /// 拆离
    /// </summary>
    public void Detach();

    /// <summary>
    /// 绑定
    /// </summary>
    public void Bind();
}
