namespace Fs.Fox.Cad;

/// <summary>
/// 参照绑定
/// </summary>
public enum XrefModes : byte
{
    /// <summary>
    /// 卸载
    /// </summary>
    Unload,
    /// <summary>
    /// 重载
    /// </summary>
    Reload,
    /// <summary>
    /// 拆离
    /// </summary>
    Detach,
    /// <summary>
    /// 绑定
    /// </summary>
    Bind,
}
