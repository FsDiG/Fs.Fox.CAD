namespace IFoxCAD.Basal;

/// <summary>
/// PInvokeUser32
/// </summary>
public static class PInvokeUser32
{
    #region Win32

    /// <summary>
    /// 查找窗口
    /// </summary>
    /// <param name="lpClassName">类名</param>
    /// <param name="lpWindowName">窗口名</param>
    /// <returns>窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    /// <summary>
    /// 发送指定消息给一个或多个接收对象，常用于窗口或控件的消息处理
    /// </summary>
    /// <param name="hWnd">目标窗口的句柄如果为0，表示发送给所有顶级窗口</param>
    /// <param name="msg">要发送的消息标识符</param>
    /// <param name="wParam">附加的消息特定信息，通常用于传递额外的参数</param>
    /// <param name="lParam">附加的消息特定信息，通常用于传递额外的参数</param>
    /// <returns>返回值取决于发送的消息类型，通常表示操作的结果或状态</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    #region user32

    /// <summary>
    /// 获取窗口客户区的大小,客户区为窗口中除标题栏,菜单栏之外的地方
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="lpRect"></param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetClientRect")]
    public static extern bool GetClientRect(IntPtr hwnd, out WindowsAPI.IntRect lpRect);

    /// <summary>
    /// 查找主线程<br/>
    /// 代替<see cref="AppDomain.GetCurrentThreadId()"/><br/>
    /// 托管线程和他们不一样: <see>
    ///     <cref>System.Threading.Thread.CurrentThread.ManagedThreadId</cref>
    /// </see>
    /// </summary>
    /// <param name="hWnd">主窗口</param>
    /// <param name="lpdwProcessId">进程ID</param>
    /// <returns>线程ID</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>
    /// 设置焦点
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    /// <summary>
    /// 获取当前窗口
    /// </summary>
    /// <returns>当前窗口标识符</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// 将一个消息的组成部分合成一个消息并放入对应线程消息队列的方法
    /// </summary>
    /// <param name="hhwnd">控件句柄</param>
    /// <param name="msg">消息是什么键盘按键、鼠标点击还是其他</param>
    /// <param name="wparam"></param>
    /// <param name="lparam"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hhwnd, int msg, IntPtr wparam, IntPtr lparam);

    /// <summary>
    /// 发送击键
    /// </summary>
    /// <param name="bVk"></param>
    /// <param name="bScan"></param>
    /// <param name="dwFlags"></param>
    /// <param name="dwExtraInfo"></param>
    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void KeybdEvent(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    /// <summary>
    /// 获取窗口文字的长度
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <returns>文字长度</returns>
    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    /// <summary>
    /// 获取窗口的标题
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <param name="text">窗口文字</param>
    /// <param name="nMaxCount">文字长度</param>
    /// <returns></returns>
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

    // [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    // internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    /// <summary>
    /// 获取某个线程的输入法布局
    /// </summary>
    /// <param name="threadId">线程ID</param>
    /// <returns>布局码</returns>
    [DllImport("user32.dll")]
    public static extern int GetKeyboardLayout(int threadId);

    /// <summary>
    /// 获取按键的当前状态
    /// </summary>
    /// <param name="nVirtualKey">按键虚拟代码</param>
    /// <returns>表示没按下&gt;0;按下&lt;0</returns>
    [DllImport("user32.dll")]
    public static extern short GetKeyState(int nVirtualKey);

    /// <summary>
    /// 检索指定窗口所属的类的名称
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <param name="lpClassName">存储窗口类名称的字符串缓冲区</param>
    /// <param name="nMaxCount">缓冲区的最大字符数</param>
    /// <returns>如果函数成功，返回值是窗口类名的长度，不包括终止的空字符如果窗口没有类名，返回值为零如果函数失败，返回值为零，并且 GetLastError 返回值提供扩展错误信息</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    /// <summary>
    /// 检索由指定窗口拥有的下一个窗口的句柄
    /// </summary>
    /// <param name="hWnd">要获取其拥有的下一个窗口的句柄的窗口</param>
    /// <param name="uCmd">指定要获取的窗口的类型</param>
    /// <returns>如果函数成功，返回值是指定窗口拥有的下一个窗口的句柄如果没有更多窗口，返回值为<c>null</c></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    /// <summary>
    /// 检索指定窗口的子窗口链表中的第一个窗口
    /// </summary>
    /// <param name="hWnd">要获取其第一个子窗口的父窗口句柄</param>
    /// <returns>如果函数成功，返回值是子窗口的句柄如果没有子窗口，返回值为<c>null</c></returns>
    [DllImport("user32.DLL", CharSet = CharSet.Unicode,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr GetTopWindow(IntPtr hWnd);

    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    /// <param name="idThread">线程</param>
    /// <param name="lpgui"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GuiThreadInfo
    {
        /// <summary>
        /// 结构体的大小
        /// </summary>
        public int cbSize;

        /// <summary>
        /// 状态标志
        /// </summary>
        public int flags;

        /// <summary>
        /// 当前激活的窗口句柄
        /// </summary>
        public IntPtr hwndActive;

        /// <summary>
        /// 当前焦点的窗口句柄
        /// </summary>
        public IntPtr hwndFocus;

        /// <summary>
        /// 当前捕获的窗口句柄
        /// </summary>
        public IntPtr hwndCapture;

        /// <summary>
        /// 当前菜单所有者的窗口句柄
        /// </summary>
        public IntPtr hwndMenuOwner;

        /// <summary>
        /// 当前正在移动或改变大小的窗口句柄
        /// </summary>
        public IntPtr hwndMoveSize;

        /// <summary>
        /// 当前插入符号的窗口句柄
        /// </summary>
        public IntPtr hwndCaret;

        /// <summary>
        /// 插入符号的位置和大小
        /// </summary>
        public System.Drawing.Rectangle rcCaret;

        /// <summary>
        /// 创建GuiThreadInfo实例
        /// </summary>
        /// <param name="windowThreadProcessId">窗口线程的进程ID</param>
        /// <returns>GuiThreadInfo实例</returns>
        public static GuiThreadInfo Create(uint windowThreadProcessId)
        {
            if (windowThreadProcessId == 0)
                throw new ArgumentNullException(nameof(windowThreadProcessId));

            GuiThreadInfo gti = new();
            gti.cbSize = Marshal.SizeOf(gti);
            GetGUIThreadInfo(windowThreadProcessId, ref gti);
            return gti;
        }
    }

    /// <summary>
    /// 获取当前焦点的窗口句柄
    /// </summary>
    /// <returns>窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetFocus();

    /// <summary>
    /// 发送消息到指定窗口
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="msg">消息</param>
    /// <param name="wParam">附加参数</param>
    /// <param name="lParam">附加参数</param>
    /// <returns>消息处理结果</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 获取指定窗口的父窗口句柄
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>父窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    /// <summary>
    /// 将虚拟键代码和扫描码转换为ASCII字符
    /// </summary>
    /// <param name="uVirtKey">虚拟键代码</param>
    /// <param name="uScancode">扫描码</param>
    /// <param name="lpdKeyState">键盘状态</param>
    /// <param name="lpwTransKey">转换后的字符</param>
    /// <param name="fuState">状态标志</param>
    /// <returns>转换的字符数</returns>
    [DllImport("user32.dll")]
    public static extern int ToAscii(int uVirtKey, int uScancode, byte[] lpdKeyState,
        byte[] lpwTransKey, int fuState);

    /// <summary>
    /// 获取当前激活的窗口句柄
    /// </summary>
    /// <returns>窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetActiveWindow();

    /// <summary>
    /// 获取窗口对应的线程和进程ID
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="lpdwProcessId">进程ID</param>
    /// <returns>线程ID</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern long GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);

    /// <summary>
    /// 检查窗口是否最小化
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>是否最小化</returns>
    [DllImport("user32.dll")]
    public static extern bool IsIconic(int hWnd);

    /// <summary>
    /// 检查窗口是否启用
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>是否启用</returns>
    [DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr hWnd);

    #endregion

    #endregion
}

internal class WmMessage
{
    #region Message

    public const uint Close = 0x0010;

    #endregion
}