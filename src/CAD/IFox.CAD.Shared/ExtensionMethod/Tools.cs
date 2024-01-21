using static IFoxCAD.Basal.Timer;
namespace IFoxCAD.Cad;
/// <summary>
/// 工具类
/// </summary>
public static class Tools
{
    /// <summary>
    /// 计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes2(int count, string message, Action action)
    {
        System.Diagnostics.Stopwatch watch = new();
        watch.Start();  // 开始监视代码运行时间
        for (var i = 0; i < count; i++)
            action.Invoke();// 需要测试的代码
        watch.Stop();  // 停止监视
        var timespan = watch.Elapsed; // 获取当前实例测量得出的总时间
        var time = timespan.TotalMilliseconds;
        var name = "毫秒";
        if (timespan.TotalMilliseconds > 1000)
        {
            time = timespan.TotalSeconds;
            name = "秒";
        }
        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({name})");  // 总毫秒数
    }

    /// <summary>
    /// 计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes3(int count, string message, Action<int> action)
    {
        System.Diagnostics.Stopwatch watch = new();
        watch.Start();  // 开始监视代码运行时间
        for (var i = 0; i < count; i++)
            action.Invoke(i);// 需要测试的代码
        watch.Stop();  // 停止监视
        var timespan = watch.Elapsed; // 获取当前实例测量得出的总时间
        var time = timespan.TotalMilliseconds;
        var name = "毫秒";
        if (timespan.TotalMilliseconds > 1000)
        {
            time = timespan.TotalSeconds;
            name = "秒";
        }
        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({name})");  // 总毫秒数
    }



    /// <summary>
    /// 纳秒计时器
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static void TestTimes(int count, string message, Action action,
        TimeEnum timeEnum = TimeEnum.Millisecond)
    {
        var time = RunTime(() => {
            for (var i = 0; i < count; i++)
                action.Invoke();
        }, timeEnum);

        var timeNameZn = timeEnum switch
        {
            TimeEnum.Millisecond => " 毫秒",
            TimeEnum.Microsecond => " 微秒",
            TimeEnum.Nanosecond => " 纳秒",
            _ => " 秒"
        };

        Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({timeNameZn})");
    }

#if acad
    /// <summary>
    /// 刷新图层状态，在修改图层的锁定或冻结状态后使用
    /// </summary>
    /// <param name="objectIds">图层id集合</param>
    public static void RegenLayers(IEnumerable<ObjectId> objectIds)
    {
        var type = Acaop.Version.Major >= 21
            ? Assembly.Load("accoremgd")?.GetType("Autodesk.AutoCAD.Internal.CoreLayerUtilities")
            : Assembly.Load("acmgd")?.GetType("Autodesk.AutoCAD.Internal.LayerUtilities");
        var mi = type?.GetMethods().FirstOrDefault(e => e.Name == "RegenLayers");
        var pi = type?.GetProperties().FirstOrDefault(e => e.Name == "RegenPending");
        var regenPending = (int)(pi?.GetValue(null) ?? 0);
        mi?.Invoke(null, new object[] { objectIds.ToArray(), regenPending });
    }

    /// <summary>
    /// 发送气泡通知
    /// </summary>
    /// <param name="second">显示的秒数，范围1-10为相应秒数，0为常显</param>
    /// <param name="title">标题</param>
    /// <param name="text">内容1</param>
    /// <param name="iconType">图标样式</param>
    /// <param name="hyperText">链接</param>
    /// <param name="hyperLink">链接地址</param>
    /// <param name="text2">内容2</param>
    public static void ShowBubbleWindow(int second, string title, string text, Autodesk.AutoCAD.Windows.IconType iconType = Autodesk.AutoCAD.Windows.IconType.None, string? hyperText = null, string? hyperLink = null, string? text2 = null)
    {
        Autodesk.AutoCAD.Windows.TrayItem? trayItem = null;
        const string name = "IFox";
        var num = Acap.StatusBar.TrayItems.Count;
        for (var i = 0; i < num; i++)
        {
            var ti = Acap.StatusBar.TrayItems[i];
            if (ti.ToolTipText != name) continue;
            trayItem = ti;
            break;
        }
        if (trayItem == null)
        {
            trayItem = new()
            {
                ToolTipText = name,
                Visible = true,
            };
            Acap.StatusBar.TrayItems.Add(trayItem);
            Acap.StatusBar.Update();
        }
        if (second <= 0) second = 0;
        else if (second % 10 == 0) second = 10;
        else second %= 10;
        Acaop.SetSystemVariable("TrayTimeOut", second);
        var tibw = new Autodesk.AutoCAD.Windows.TrayItemBubbleWindow
        {
            IconType = iconType,
            Title = title,
            Text = text,
            HyperText = hyperText,
            HyperLink = hyperLink,
            Text2 = text2
        };
        Acaop.SetSystemVariable("TRAYICONS", 1);
        Acaop.SetSystemVariable("TRAYNOTIFY", 1);
        trayItem.Visible = true;
        trayItem.ShowBubbleWindow(tibw);
    }

    /// <summary>
    /// 否决双击事件本身的后续操作，在双击事件中使用
    /// </summary>
    public static void VetoMouseDoubleClickEvent()
    {
        const string key = "DBLCLKEDIT";
        var value = Acaop.GetSystemVariable(key);
        Acaop.SetSystemVariable(key, 0);
        IdleAction.Add(() => Acaop.SetSystemVariable(key, value));
    }
    /// <summary>
    /// 获取透明度
    /// </summary>
    /// <param name="value">cad特性栏透明度值，范围0-100</param>
    /// <returns>cad透明度值</returns>
    public static Transparency CreateTransparency(int value)
    {
        return new Transparency(Convert.ToByte(Math.Floor((100 - value) * 2.55)));
    }
#endif
}