using System.Drawing;
using System.Windows.Forms;
using Window = System.Windows.Window;

namespace Fs.Fox.Cad;

/// <summary>
/// 窗体扩展
/// </summary>
public static class WindowEx
{
    /// <summary>
    /// 添加Esc退出
    /// </summary>
    /// <param name="window">wpf窗体</param>
    public static void AddEscQuit(this Window window)
    {
        window.KeyDown -= Window_KeyDown_Esc;
        window.KeyDown += Window_KeyDown_Esc;
        window.Closed -= WindowOnClosed;
        window.Closed += WindowOnClosed;
    }

    /// <summary>
    /// 关闭时减掉事件
    /// </summary>
    private static void WindowOnClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
            return;
        window.KeyDown -= Window_KeyDown_Esc;
        window.Closed -= WindowOnClosed;
    }

    private static void Window_KeyDown_Esc(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape
            || sender is not Window { IsLoaded: true, IsActive: true } window)
            return;

        // 判断没有按住ctrl或shift或alt才执行
        var keys = Control.ModifierKeys;
        if ((keys & Keys.Control) != 0
            || (keys & Keys.Shift) != 0
            || (keys & Keys.Alt) != 0)
            return;
        window.Close();
    }

    /// <summary>
    /// 判断wpf是否为模态
    /// </summary>
    /// <param name="window">窗体</param>
    /// <returns>是则返回true</returns>
    public static bool IsModel(this Window window)
    {
        return (bool)(typeof(Window)
            .GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window) ?? false);
    }

    /// <summary>
    /// 获取屏幕分辨率
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>屏幕尺寸</returns>
    public static Size GetScreenResolutionFromWindowHandle(IntPtr windowHandle)
    {
        var screen = Screen.FromHandle(windowHandle);
        return new Size(screen.Bounds.Width, screen.Bounds.Height);
    }

    /// <summary>
    /// 通过分辨率设置面板尺寸
    /// </summary>
    /// <param name="paletteSet">侧栏</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public static void SetSizeByScreenResolution(this PaletteSet paletteSet, int width, int height)
    {
        var size = GetScreenResolutionFromWindowHandle(Acaop.MainWindow.Handle);
        var scale = size.Height * 1d / 1080;
        var newWidth = Convert.ToInt32(width * scale);
        if (newWidth > size.Width)
            newWidth = size.Width;
        var newHeight = Convert.ToInt32(height * scale);
        if (newHeight > size.Height)
        {
            newHeight = size.Height;
        }

#if !ZWCAD2022
// paletteSet.SetSize(new Size(newWidth, newHeight));   // 中望2025 这样调用报错找不到setsize函数
        WindowExtension.SetSize(paletteSet, new Size(newWidth, newHeight)); // 中望2025这样调用没有问题
#else
        Debug.Assert(false, "中望CAD2022未测试!");
#endif
    }
    
    /// <summary>
    /// 获取屏幕比例
    /// </summary>
    /// <returns>比例</returns>
    public static double GetScreenScale()
    {
        var scale = Graphics.FromHwnd(IntPtr.Zero).DpiX / 96.0f;
        return scale;
    }
}