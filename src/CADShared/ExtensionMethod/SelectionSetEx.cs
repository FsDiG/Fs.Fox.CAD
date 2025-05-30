﻿#if a2024 || zcad
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 选择集扩展类
/// </summary>
public static class SelectionSetEx
{
    #region 获取对象id

    /// <summary>
    /// 从选择集中获取对象id
    /// </summary>
    /// <typeparam name="T">图元类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <returns>已选择的对象id集合</returns>
    [DebuggerStepThrough]
    public static IEnumerable<ObjectId> GetObjectIds<T>(this SelectionSet ss) where T : Entity
    {
        var rxc = RXObject.GetClass(typeof(T));

        return ss.GetObjectIds()
            .Where(id => id.ObjectClass.IsDerivedFrom(rxc));
    }

    /// <summary>
    /// 将选择集的对象按类型分组
    /// </summary>
    /// <param name="ss">选择集</param>
    /// <returns>分组后的类型/对象id集合</returns>
    [DebuggerStepThrough]
    public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIdGroup(this SelectionSet ss)
    {
        return ss.GetObjectIds()
            .GroupBy(id => id.ObjectClass.DxfName);
    }

    #endregion

    #region 获取实体对象

    /// <summary>
    /// 获取指定类型图元
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>图元集合</returns>
    [DebuggerStepThrough]
    public static IEnumerable<T> GetEntities<T>(this SelectionSet? ss,
        OpenMode openMode = OpenMode.ForRead,
        bool openErased = false,
        bool openLockedLayer = false) where T : Entity
    {
        return ss?.GetObjectIds()
            .Select(id => id.GetObject<T>(openMode, openErased, openLockedLayer))
            .OfType<T>() ?? [];
    }

    #endregion

    #region ForEach

    /// <summary>
    /// 遍历选择集
    /// </summary>
    /// <typeparam name="T">指定图元类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <param name="action">处理函数;(图元)</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    [DebuggerStepThrough]
    public static void ForEach<T>(this SelectionSet ss,
        Action<T?> action,
        OpenMode openMode = OpenMode.ForRead,
        bool openErased = false,
        bool openLockedLayer = false) where T : Entity
    {
        ForEach<T>(ss, (ent, _) => { action.Invoke(ent); }, openMode, openErased, openLockedLayer);
    }

    /// <summary>
    /// 遍历选择集
    /// </summary>
    /// <typeparam name="T">指定图元类型</typeparam>
    /// <param name="ss">选择集</param>
    /// <param name="action">处理函数;(图元,终止方式)</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void ForEach<T>(this SelectionSet ss,
        Action<T, LoopState> action,
        OpenMode openMode = OpenMode.ForRead,
        bool openErased = false,
        bool openLockedLayer = false) where T : Entity
    {
        ArgumentNullException.ThrowIfNull(action);

        LoopState state = new();
        var ents = ss.GetEntities<T>(openMode, openErased, openLockedLayer);
        foreach (var ent in ents)
        {
            action.Invoke(ent, state);
            if (!state.IsRun)
                break;
        }
    }

    #endregion
}