﻿namespace IFoxCAD.Cad;

/// <summary>
/// 工具类
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IFoxUtils
{
#if acad
    /// <summary>
    /// 刷新图层状态，在修改图层的锁定或冻结状态后使用
    /// </summary>
    /// <param name="layerIds">图层id集合</param>
    public static void RegenLayers(IEnumerable<ObjectId> layerIds)
    {
        var type = Acaop.Version.Major >= 21
            ? Assembly.Load("accoremgd")?.GetType("Autodesk.AutoCAD.Internal.CoreLayerUtilities")
            : Assembly.Load("acmgd")?.GetType("Autodesk.AutoCAD.Internal.LayerUtilities");
        var mi = type?.GetMethods().FirstOrDefault(e => e.Name == "RegenLayers");
        var pi = type?.GetProperties().FirstOrDefault(e => e.Name == "RegenPending");
        var regenPending = (int)(pi?.GetValue(null) ?? 0);
        mi?.Invoke(null, [layerIds.ToArray(), regenPending]);
    }

    /// <summary>
    /// 刷新图层状态，在修改图层的锁定或冻结状态后使用
    /// </summary>
    /// <param name="layerIds">图层id集合</param>
    public static void RegenLayers2(IEnumerable<ObjectId> layerIds)
    {
        var isOff = false;
        var layerRxc = RXObject.GetClass(typeof(LayerTableRecord));
        var entityRxc = RXObject.GetClass(typeof(Entity));
        foreach (var group in layerIds.Where(e => e.IsOk() && e.ObjectClass.IsDerivedFrom(layerRxc))
                     .GroupBy(e => e.Database))
        {
            var db = group.Key;
            var doc = Acap.DocumentManager.GetDocument(db);
            if (doc is null)
                continue; // 获取不到文档说明是后台开图，后台开图就不用刷新了
            using var dl = doc.LockDocument();
            using var tr = new DBTrans(db);
            var layerIdSet = group.ToHashSet();
            layerIdSet.Count.Print();
            foreach (var ltr in layerIdSet.Select(id => tr.GetObject(id, OpenMode.ForWrite))
                         .OfType<LayerTableRecord>())
            {
                Volatile.Write(ref isOff, ltr.IsOff);
                ltr.IsOff = Volatile.Read(ref isOff);
            }

            for (var i = db.BlockTableId.Handle.Value; i < db.Handseed.Value; i++)
            {
                if (!db.TryGetObjectId(new Handle(i), out var id) || !id.IsOk() ||
                    !id.ObjectClass.IsDerivedFrom(entityRxc))
                    continue;
                var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, false, true);
                if (!layerIdSet.Contains(ent.LayerId))
                    continue;
                ent.RecordGraphicsModified(true);
            }
        }
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
    public static void ShowBubbleWindow(int second, string title, string text,
        IconType iconType = IconType.None, string? hyperText = null, string? hyperLink = null,
        string? text2 = null)
    {
        TrayItem? trayItem = null;
        const string name = "IFox";
        var num = Acap.StatusBar.TrayItems.Count;
        for (var i = 0; i < num; i++)
        {
            var ti = Acap.StatusBar.TrayItems[i];
            if (ti.ToolTipText != name)
                continue;
            trayItem = ti;
            break;
        }

        if (trayItem == null)
        {
            trayItem = new() { ToolTipText = name, Visible = true, };
            Acap.StatusBar.TrayItems.Add(trayItem);
            Acap.StatusBar.Update();
        }

        if (second <= 0)
            second = 0;
        else if (second % 10 == 0)
            second = 10;
        else
            second %= 10;
        Acaop.SetSystemVariable("TrayTimeOut", second);
        var tibw = new TrayItemBubbleWindow
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