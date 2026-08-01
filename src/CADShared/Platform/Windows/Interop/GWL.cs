#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#if true
namespace Fs.Fox.Basal;

public enum GWL : int
{
    /// <summary>
    /// 获取、设置窗口过程的地址
    /// </summary>
    GWL_WNDPROC = -4,
    /// <summary>
    /// 获取应用程序的实例句柄
    /// </summary>
    GWL_HINSTANCE = -6,
    /// <summary>
    /// 获取父窗口句柄
    /// </summary>
    GWL_HWNDPARENT = -8,
    /// <summary>
    /// 获取窗口标识
    /// </summary>
    GWL_ID = -12,
    /// <summary>
    /// 获取、设置窗口样式
    /// </summary>
    GWL_STYLE = -16,
    /// <summary>
    /// 获取、设置窗口扩展样式
    /// </summary>
    GWL_EXSTYLE = -20,
    /// <summary>
    /// 获取、设置与窗口关联的自定义数据
    /// </summary>
    GWL_USERDATA = -21,
}
#endif
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
