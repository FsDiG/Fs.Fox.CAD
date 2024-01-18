namespace IFoxCAD.Basal;


/// <summary>
/// A least-recently-used cache stored like a dictionary.
/// </summary>
/// <typeparam name="TKey">
/// The type of the key to the cached item
/// </typeparam>
/// <typeparam name="TValue">
/// The type of the cached item.
/// </typeparam>
/// <remarks>
/// Derived from https://stackoverflow.com/a/3719378/240845
/// https://stackoverflow.com/users/240845/mheyman
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="LinkedHashMap{TKey, TValue}"/>
/// class.
/// </remarks>
/// <param name="capacity">
/// Maximum number of elements to cache.
/// </param>
/// <param name="dispose">
/// When elements cycle out of the cache, disposes them. May be null.
/// </param>
public class LinkedHashMap<TKey, TValue>(int capacity, Action<TValue>? dispose = null)
{
    private readonly Dictionary<TKey, LinkedListNode<MapItem>> _cacheMap = [];

    private readonly LinkedList<MapItem> _lruList = [];

    /// <summary>
    /// Gets the capacity of the cache.
    /// </summary>
    public int Capacity { get; } = capacity;

    /// <summary>Gets the value associated with the specified key.</summary>
    /// <param name="key">
    /// The key of the value to get.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified
    /// key, if the key is found; otherwise, the default value for the type of the 
    /// <paramref name="value" /> parameter. This parameter is passed
    /// uninitialized.
    /// </param>
    /// <returns>
    /// true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> 
    /// contains an element with the specified key; otherwise, false.
    /// </returns>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        lock (_cacheMap)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return true;
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// Looks for a value for the matching <paramref name="key"/>. If not found, 
    /// calls <paramref name="valueGenerator"/> to retrieve the value and add it to
    /// the cache.
    /// </summary>
    /// <param name="key">
    /// The key of the value to look up.
    /// </param>
    /// <param name="valueGenerator">
    /// Generates a value if one isn't found.
    /// </param>
    /// <returns>
    /// The requested value.
    /// </returns>
    public TValue Get(TKey key, Func<TValue> valueGenerator)
    {
        lock (_cacheMap)
        {
            TValue value;
            if (_cacheMap.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
            }
            else
            {
                value = valueGenerator();
                if (_cacheMap.Count >= Capacity)
                {
                    RemoveFirst();
                }

                MapItem cacheItem = new(key, value);
                node = new LinkedListNode<MapItem>(cacheItem);
                _lruList.AddLast(node);
                _cacheMap.Add(key, node);
            }

            return value;
        }
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">
    /// The key of the element to add.
    /// </param>
    /// <param name="value">
    /// The value of the element to add. The value can be null for reference types.
    /// </param>
    public void Add(TKey key, TValue value)
    {
        lock (_cacheMap)
        {
            if (_cacheMap.Count >= Capacity)
            {
                RemoveFirst();
            }

            MapItem cacheItem = new(key, value);
            LinkedListNode<MapItem> node = new(cacheItem);
            _lruList.AddLast(node);
            _cacheMap.Add(key, node);
        }
    }

    private void RemoveFirst()
    {
        // Remove from LRUPriority
        var node = _lruList.First;
        _lruList.RemoveFirst();

        // Remove from cache
        _cacheMap.Remove(node.Value.Key);

        // dispose
        dispose?.Invoke(node.Value.Value);
    }

    private class MapItem(TKey k, TValue v)
    {
        public TKey Key { get; } = k;

        public TValue Value { get; } = v;
    }
}

