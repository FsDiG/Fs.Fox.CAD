#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// Temporarily unlocks a layer and restores its original lock state when disposed.
/// </summary>
/// <remarks>
/// This scope deliberately does not thaw a frozen layer. Freeze state has viewport and current-layer
/// restrictions that cannot be restored reliably by a generic disposal helper.
/// <para>
/// The implementation intentionally uses <c>TransactionManager.StartTransaction()</c> instead of
/// AutoCAD's <c>StartOpenCloseTransaction()</c>: ZWCAD 2022 does not expose the latter API.
/// </para>
/// </remarks>
public sealed class LayerUnlockScope : IDisposable
{
    private readonly Database _database;
    private readonly ObjectId _layerId;
    private readonly bool _wasLocked;
    private bool _disposed;

    /// <summary>
    /// Gets the layer identifier managed by this scope.
    /// </summary>
    public ObjectId LayerId => _layerId;

    /// <summary>
    /// Temporarily unlocks the layer identified by <paramref name="layerId"/>.
    /// </summary>
    /// <param name="layerId">A valid <see cref="LayerTableRecord"/> identifier.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="layerId"/> is invalid, erased, or does not identify a layer.
    /// </exception>
    public LayerUnlockScope(ObjectId layerId)
        : this(GetDatabase(layerId), layerId)
    {
    }

    /// <summary>
    /// Temporarily unlocks a named layer in the specified database.
    /// </summary>
    /// <param name="database">Database that owns the layer.</param>
    /// <param name="layerName">Layer name.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="database"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="layerName"/> is empty or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The database does not contain <paramref name="layerName"/>.</exception>
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
            throw new ArgumentException("Layer ObjectId must be valid and non-erased.", nameof(layerId));

        return layerId.Database;
    }

    private static ObjectId GetLayerId(Database database, string layerName)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(layerName);
        if (string.IsNullOrWhiteSpace(layerName))
            throw new ArgumentException("Layer name cannot be empty or whitespace.", nameof(layerName));

        using var transaction = StartCompatibleTransaction(database);
        var layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
        if (!layerTable.Has(layerName))
            throw new KeyNotFoundException($"Layer '{layerName}' does not exist in the specified database.");

        // This transaction only resolves the stable ObjectId; disposing it without Commit is intentional.
        return layerTable[layerName];
    }

    private bool SetLockState(bool isLocked)
    {
        using var transaction = StartCompatibleTransaction(_database);
        if (transaction.GetObject(_layerId, OpenMode.ForRead) is not LayerTableRecord layer)
            throw new ArgumentException("ObjectId does not identify a layer.", nameof(_layerId));

        var previousState = layer.IsLocked;
        if (previousState != isLocked)
        {
            layer.UpgradeOpen();
            layer.IsLocked = isLocked;
        }

        transaction.Commit();
        return previousState;
    }

    private static Transaction StartCompatibleTransaction(Database database)
    {
        // Keep this as a normal transaction: ZWCAD 2022 has no StartOpenCloseTransaction API.
        return database.TransactionManager.StartTransaction();
    }

    /// <summary>
    /// Restores the lock state captured by the constructor. Repeated calls after a successful restore
    /// have no effect.
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
