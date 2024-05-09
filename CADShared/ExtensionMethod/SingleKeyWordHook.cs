using Keys = System.Windows.Forms.Keys;

namespace IFoxCAD.Cad;

/// <summary>
/// 关键字不需要空格钩子
/// By DYH 20230508
/// </summary>
public sealed class SingleKeyWordHook : IDisposable
{
    #region 静态字段

    private static readonly string _enterStr = Convert.ToChar(Keys.Enter).ToString();
    private static readonly string _backStr = Convert.ToChar(Keys.Back).ToString();

    #endregion

    #region 私有字段

    /// <summary>
    /// 关键字合集
    /// </summary>
    private readonly HashSet<Keys> _keyWords;

    private bool _isResponsed;
    private bool _working;
    private Keys _key;
    private readonly SingleKeyWordWorkType _workType;

    #endregion

    #region 公共属性

    /// <summary>
    /// 上一个触发的关键字
    /// </summary>
    public Keys Key => _key;

    /// <summary>
    /// 上一个触发的关键字字符串
    /// </summary>
    public string StringResult => _key.ToString().ToUpper();

    /// <summary>
    /// 是否响应了
    /// </summary>
    public bool IsResponsed => _isResponsed && _working;

    #endregion

    #region 构造

    /// <summary>
    /// 单字母关键字免输回车钩子
    /// </summary>
    /// <param name="workType">使用esc(填false则使用回车)</param>
    public SingleKeyWordHook(SingleKeyWordWorkType workType = SingleKeyWordWorkType.ESCAPE)
    {
        IsDisposed = false;
        _isResponsed = false;
        _keyWords = new HashSet<Keys>();
        _key = Keys.None;
        _working = true;
        _workType = workType;
        Acap.PreTranslateMessage -= Acap_PreTranslateMessage;
        Acap.PreTranslateMessage += Acap_PreTranslateMessage;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void AddKeys(params Keys[] values) => values.ForEach(value => _keyWords.Add(value));

    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="keywordCollection">关键字集合</param>
    public void AddKeys(KeywordCollection keywordCollection)
    {
        foreach (Keyword item in keywordCollection)
        {
            if (item.LocalName.Length == 1)
            {
                var k = (Keys)item.LocalName[0];
                _keyWords.Add(k);
            }
        }
    }

    /// <summary>
    /// 移除Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void Remove(params Keys[] values) => values.ForEach(value => _keyWords.Remove(value));

    /// <summary>
    /// 清空Keys
    /// </summary>
    public void Clear() => _keyWords.Clear();

    /// <summary>
    /// 复位响应状态，每个循环开始时使用
    /// </summary>
    public void Reset()
    {
        _isResponsed = false;
    }

    /// <summary>
    /// 暂停工作
    /// </summary>
    public void Pause()
    {
        _working = false;
    }

    /// <summary>
    /// 开始工作
    /// </summary>
    public void Working()
    {
        _working = true;
    }

    #endregion

    #region 事件

    private void Acap_PreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
    {
        if (!_working || e.Message.message != 256) return;
        var tempKey = IntPtr.Size == 4 ? (Keys)e.Message.wParam.ToInt32() : (Keys)e.Message.wParam.ToInt64();
        bool contains = _keyWords.Contains(tempKey);
        if (contains || tempKey == Keys.ProcessKey)
        {
            // 标记为true，表示此按键已经被处理，Windows不会再进行处理
            if (_workType != SingleKeyWordWorkType.ENTER)
            {
                e.Handled = true;
            }

            if (contains)
                _key = tempKey;
            if (!_isResponsed)
            {
                // 此bool是防止按键被长按时出错
                _isResponsed = true;
                switch (_workType)
                {
                    case SingleKeyWordWorkType.ESCAPE:
                        // ESC稳妥一些，但是要判断promptResult的顺序
                        KeyBoardSendKey(Keys.Escape);
                        break;
                    case SingleKeyWordWorkType.ENTER:
                        KeyBoardSendKey(Keys.Enter);
                        break;
                    case SingleKeyWordWorkType.WRITE_LINE:
                        Utils.WriteToCommandLine(Convert.ToChar(_key) + _enterStr);
                        break;
                }
            }
        }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// 已经销毁
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        Acap.PreTranslateMessage -= Acap_PreTranslateMessage;
        if (disposing)
        {
            _keyWords.Clear();
        }

        IsDisposed = true;
    }

    /// <summary>
    /// 析够里把事件拆了
    /// </summary>
    ~SingleKeyWordHook()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 静态方法

    /// <summary>
    /// 发送按键
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bScan"></param>
    /// <param name="dwFlags"></param>
    /// <param name="dwExtraInfo"></param>
    private static void KeyBoardSendKey(Keys key, byte bScan = 0, uint dwFlags = 0, uint dwExtraInfo = 0)
    {
        keybd_event(key, bScan, dwFlags, dwExtraInfo);
        keybd_event(key, bScan, 2, dwExtraInfo);
    }

    [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
    private static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    #endregion
}

public static class SingleKeywordHookEx
{
    /// <summary>
    /// 钩取单文本关键字
    /// </summary>
    /// <param name="keywords">关键字集合</param>
    /// <param name="workType">工作模式</param>
    /// <returns>单文本关键字类(需要using)</returns>
    public static SingleKeyWordHook HookSingleKeyword(this KeywordCollection keywords, SingleKeyWordWorkType workType = SingleKeyWordWorkType.WRITE_LINE)
    {
        var singleKeyWordHook = new SingleKeyWordHook(workType);
        singleKeyWordHook.AddKeys(keywords);
        return singleKeyWordHook;
    }
}

public enum SingleKeyWordWorkType : byte
{
    ESCAPE,
    ENTER,
    WRITE_LINE,
}