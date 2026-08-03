#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 临时解锁图层，并在释放作用域时恢复原锁定状态。
/// </summary>
/// <remarks>
/// 本作用域不会解冻被冻结的图层。冻结状态受视口和当前图层约束，通用释放器无法可靠恢复。
/// <para>
/// 实现有意使用 <c>TransactionManager.StartTransaction()</c>，而不使用 AutoCAD 的
/// <c>StartOpenCloseTransaction()</c>：ZWCAD 2022 的 <c>TransactionManager</c> 未暴露后一个 API。
/// </para>
/// </remarks>
public sealed class LayerUnlockScope : IDisposable
{
    private readonly Database _database;
    private readonly ObjectId _layerId;
    private readonly bool _wasLocked;
    private bool _disposed;

    /// <summary>
    /// 获取本作用域管理的图层标识。
    /// </summary>
    public ObjectId LayerId => _layerId;

    /// <summary>
    /// 临时解锁 <paramref name="layerId"/> 标识的图层。
    /// </summary>
    /// <param name="layerId">有效的 <see cref="LayerTableRecord"/> 标识。</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="layerId"/> 无效、已删除或不是图层标识。
    /// </exception>
    public LayerUnlockScope(ObjectId layerId)
        : this(GetDatabase(layerId), layerId)
    {
    }

    /// <summary>
    /// 临时解锁指定数据库中的命名图层。
    /// </summary>
    /// <param name="database">图层所属的数据库。</param>
    /// <param name="layerName">图层名。</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="database"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="layerName"/> 为空或仅包含空白字符。</exception>
    /// <exception cref="KeyNotFoundException">数据库中不存在 <paramref name="layerName"/>。</exception>
    public LayerUnlockScope(Database database, string layerName)
        : this(database, GetLayerId(database, layerName))
    {
    }

    private LayerUnlockScope(Database database, ObjectId layerId)
    {
        _database = database;
        _layerId = layerId;
        _wasLocked = SetLockState(false);
    }

    private static Database GetDatabase(ObjectId layerId)
    {
        if (layerId.IsNull || !layerId.IsValid || layerId.IsErased || layerId.IsEffectivelyErased)
            throw new ArgumentException("图层 ObjectId 必须有效且未被删除。", nameof(layerId));

        return layerId.Database;
    }

    private static ObjectId GetLayerId(Database database, string layerName)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(layerName);
        if (string.IsNullOrWhiteSpace(layerName))
            throw new ArgumentException("图层名不能为空或仅包含空白字符。", nameof(layerName));

        using var transaction = StartCompatibleTransaction(database);
        var layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
        if (!layerTable.Has(layerName))
            throw new KeyNotFoundException($"指定数据库中不存在图层“{layerName}”。");

        // 此事务只用于取得稳定的 ObjectId；有意在不 Commit 的情况下释放。
        return layerTable[layerName];
    }

    private bool SetLockState(bool isLocked)
    {
        using var transaction = StartCompatibleTransaction(_database);
        if (transaction.GetObject(_layerId, OpenMode.ForRead) is not LayerTableRecord layer)
            throw new ArgumentException("ObjectId 不是图层标识。", "layerId");

        var previousState = layer.IsLocked;
        if (previousState != isLocked)
        {
            // 复用仓库的提权作用域，兼容对象已经写打开或处于 Notify 打开状态。
            using (layer.ForWrite())
                layer.IsLocked = isLocked;
        }

        transaction.Commit();
        return previousState;
    }

    private static Transaction StartCompatibleTransaction(Database database)
    {
        // ZWCAD 2022 的 TransactionManager 未暴露 StartOpenCloseTransaction；这里统一使用普通事务，
        // 以保持 AutoCAD、ZWCAD 和 GstarCAD 共享源码可编译。
        return database.TransactionManager.StartTransaction();
    }

    /// <summary>
    /// 恢复构造函数捕获的锁定状态；成功恢复后重复调用不会产生影响。
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        SetLockState(_wasLocked);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
