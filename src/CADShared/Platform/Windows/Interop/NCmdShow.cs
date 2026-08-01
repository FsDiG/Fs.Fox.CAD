#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#if true
namespace Fs.Fox.Basal;

[Flags]
public enum NCmdShow : uint
{
    /// <summary>
    /// 隐藏窗口并激活其他窗口。nCmdShow
    /// </summary>
    SW_HIDE = 0,
    /// <summary>
    /// 正常态的窗口(非最大化和非最小化)
    /// 激活并显示一个窗口。如果窗口被最小化或最大化,系统将其恢复到原来的尺寸和大小。应用程序在第一次显示窗口的时候应该指定此标志
    /// </summary>
    SW_SHOWNORMAL = 1,
    /// <summary>
    /// 激活窗口并将其最小化
    /// </summary>
    SW_SHOWMINIMIZED = 2,
    /// <summary>
    /// 激活窗口并将其最大化
    /// </summary>
    SW_SHOWMAXIMIZED = 3,
    /// <summary>
    /// 最大化指定的窗口
    /// </summary>
    SW_MAXIMIZE = 3,
    /// <summary>
    /// 以窗口最近一次的大小和状态显示窗口。激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWNOACTIVATE = 4,
    /// <summary>
    /// 在窗口原来的位置以原来的尺寸激活和显示窗口
    /// </summary>
    SW_SHOW = 5,
    /// <summary>
    /// 最小化指定的窗口并且激活在Z序中的下一个顶层窗口
    /// </summary>
    SW_MINIMIZE = 6,
    /// <summary>
    /// 窗口最小化,激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWMINNOACTIVE = 7,
    /// <summary>
    /// 以窗口原来的状态显示窗口。激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWNA = 8,
    /// <summary>
    /// 激活并显示窗口。如果窗口最小化或最大化,则系统将窗口恢复到原来的尺寸和位置。在恢复最小化窗口时,应用程序应该指定这个标志
    /// </summary>
    SW_RESTORE = 9,
    /// <summary>
    /// 依据在STARTUPINFO结构中指定的SW_FLAG标志设定显示状态,STARTUPINFO 结构是由启动应用程序的程序传递给CreateProcess函数的
    /// </summary>
    SW_SHOWDEFAULT = 10,
    /// <summary>
    /// 在WindowNT5.0中最小化窗口,即使拥有窗口的线程被挂起也会最小化。在从其他线程最小化窗口时才使用这个参数
    /// </summary>
    SW_FORCEMINIMIZE = 11,
}
#endif
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
