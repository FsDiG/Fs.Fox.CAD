namespace IFoxCAD.Basal;

public static class Win32Api
{
    #region Win32

    /// <summary>
    /// 查找窗口
    /// </summary>
    /// <param name="lpClassName"></param>
    /// <param name="lpWindowName"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="hWnd"></param>
    public static void CloseWindow(IntPtr hWnd)
    {
        SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    // 定义 WM_CLOSE 消息
    const UInt32 WM_CLOSE = 0x0010;

    #endregion
}