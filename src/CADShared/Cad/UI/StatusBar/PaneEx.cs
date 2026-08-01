namespace Fs.Fox.Cad;

/// <summary>
/// 托盘类扩展
/// </summary>
public static class PaneEx
{
    /// <summary>
    /// 设置Pane的左右边距
    /// </summary>
    /// <param name="pane">Pane</param>
    /// <param name="leftMarginType">左边距类型</param>
    /// <param name="rightMarginType">右边距类型</param>
    public static void SetMargin(this Pane pane, PaneMarginType leftMarginType, PaneMarginType rightMarginType)
    {
        SetLeftMargin(pane, leftMarginType);
        SetRightMargin(pane, rightMarginType);
    }

    /// <summary>
    /// 设置左侧的边距
    /// </summary>
    /// <param name="pane">pane</param>
    /// <param name="marginType">边距类型</param>
    public static void SetLeftMargin(this Pane pane, PaneMarginType marginType)
    {
        var hasMargin = marginType != PaneMarginType.NONE;
        if (hasMargin)
        {
            var style = (PaneStyles)(marginType == PaneMarginType.LARGE ? 64 : 128);
            while (true)
            {
                CadApp.StatusBar.Update();
                var index = CadApp.StatusBar.Panes.IndexOf(pane);
                if (index == -1 || index == 0)
                    break;
                var left1 = CadApp.StatusBar.Panes[index - 1];
                var left1Style = Convert.ToInt32(left1.Style);

                if (index == 1 && (left1Style == 64 || left1Style == 128))
                {
                    CadApp.StatusBar.Panes.Remove(left1);
                    continue;
                }

                if (left1Style != 64 && left1Style != 128)
                {
                    var leftAdd1 = new Pane() { ToolTipText = pane.ToolTipText, Style = style };
                    CadApp.StatusBar.Panes.Insert(index, leftAdd1);
                    continue;
                }

                left1.Style = style;
                if (index > 1)
                {
                    var left2 = CadApp.StatusBar.Panes[index - 2];
                    var left2Style = Convert.ToInt32(left2.Style);
                    if (left2Style == 64 || left2Style == 128)
                    {
                        CadApp.StatusBar.Panes.Remove(left2);
                        continue;
                    }
                }

                break;
            }
        }
        else
        {
            while (true)
            {
                CadApp.StatusBar.Update();
                var index = CadApp.StatusBar.Panes.IndexOf(pane);
                if (index > 0)
                {
                    var left1 = CadApp.StatusBar.Panes[index - 1];
                    var left1Style = Convert.ToInt32(left1.Style);
                    if (left1Style == 64 || left1Style == 128)
                    {
                        CadApp.StatusBar.Panes.Remove(left1);
                        continue;
                    }
                }

                break;
            }
        }

        CadApp.StatusBar.Update();
    }

    /// <summary>
    /// 设置右侧的边距
    /// </summary>
    /// <param name="pane">pane</param>
    /// <param name="marginType">边距类型</param>
    public static void SetRightMargin(this Pane pane, PaneMarginType marginType)
    {
        var hasMargin = marginType != PaneMarginType.NONE;
        if (hasMargin)
        {
            var style = (PaneStyles)(marginType == PaneMarginType.LARGE ? 64 : 128);
            while (true)
            {
                CadApp.StatusBar.Update();
                var index = CadApp.StatusBar.Panes.IndexOf(pane);
                if (index == -1 || index == CadApp.StatusBar.Panes.Count - 1)
                    break;
                var right1 = CadApp.StatusBar.Panes[index + 1];
                var right1Style = Convert.ToInt32(right1.Style);
                if (right1Style != 64 && right1Style != 128)
                {
                    var rightAdd1 = new Pane() { ToolTipText = pane.ToolTipText, Style = style };
                    CadApp.StatusBar.Panes.Insert(index + 1, rightAdd1);
                    continue;
                }

                right1.Style = style;
                if (index < CadApp.StatusBar.Panes.Count - 2)
                {
                    var right2 = CadApp.StatusBar.Panes[index + 2];
                    var right2Style = Convert.ToInt32(right2.Style);
                    if (right2Style == 64 || right2Style == 128)
                    {
                        CadApp.StatusBar.Panes.Remove(right2);
                        continue;
                    }
                }

                break;
            }
        }
        else
        {
            while (true)
            {
                CadApp.StatusBar.Update();
                var index = CadApp.StatusBar.Panes.IndexOf(pane);
                if (index < CadApp.StatusBar.Panes.Count - 1)
                {
                    var right1 = CadApp.StatusBar.Panes[index + 1];
                    var right1Style = Convert.ToInt32(right1.Style);
                    if (right1Style == 64 || right1Style == 128)
                    {
                        CadApp.StatusBar.Panes.Remove(right1);
                        continue;
                    }
                }

                break;
            }
        }

        CadApp.StatusBar.Update();
    }
}

/// <summary>
/// 托盘边距类型
/// </summary>
public enum PaneMarginType : byte
{
    /// <summary>
    /// 无
    /// </summary>
    NONE,
    /// <summary>
    /// 小边距
    /// </summary>
    SMALL,
    /// <summary>
    /// 大边距
    /// </summary>
    LARGE
}