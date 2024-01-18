namespace IFoxCAD.Basal;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
public sealed class LinkedHashSet<T> : ICollection<T> where T : IComparable
{
    private readonly IDictionary<T, LoopListNode<T>> _mDictionary = new Dictionary<T, LoopListNode<T>>();
    private readonly LoopList<T> _mLinkedList = [];

    public LoopListNode<T>? First => _mLinkedList.First;

    public LoopListNode<T>? Last => _mLinkedList.Last;

    public LoopListNode<T>? MinNode { get; private set; }

    public bool Add(T item)
    {
        if (_mDictionary.ContainsKey(item))
            return false;
        var node = _mLinkedList.AddLast(item);
        _mDictionary.Add(item, node);

        if (MinNode is null)
        {
            MinNode = node;
        }
        else
        {
            if (item.CompareTo(MinNode.Value) < 0)
            {
                MinNode = node;
            }
        }



        return true;
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public LoopListNode<T> AddFirst(T value)
    {
        if (_mDictionary.TryGetValue(value, out var first))
        {
            return first;
        }
        var node = _mLinkedList.AddFirst(value);
        _mDictionary.Add(value, node);
        if (MinNode is null)
        {
            MinNode = node;
        }
        else
        {
            if (value.CompareTo(MinNode.Value) < 0)
            {
                MinNode = node;
            }
        }
        return node;
    }

    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }


    public void Clear()
    {
        _mLinkedList.Clear();
        _mDictionary.Clear();
    }

    public bool Remove(T item)
    {
        var found = _mDictionary.TryGetValue(item, out var node);
        if (!found) return false;
        _mDictionary.Remove(item);
        _mLinkedList.Remove(node!);
        return true;
    }

    public int Count => _mDictionary.Count;

    public void For(LoopListNode<T> from, Action<int, T, T> action)
    {
        var first = from;
        var last = from;

        for (var i = 0; i < Count; i++)
        {
            action.Invoke(i, first!.Value, last!.Value);
            first = first.Next;
            last = last.Previous;
        }
    }

    public List<T> ToList()
    {
        return [.. _mLinkedList];
    }

    [DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        return _mLinkedList.GetEnumerator();
    }

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public bool Contains(T item)
    {
        return _mDictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        // m_LinkedList.CopyTo(array, arrayIndex);
        
    }

    public bool SetFirst(LoopListNode<T> node)
    {
        return _mLinkedList.SetFirst(node);
    }

    public LinkedHashSet<T> Clone()
    {
        var newSet = new LinkedHashSet<T>();
        foreach (var item in this)
        {
            newSet.Add(item);
        }
        return newSet;
    }

    public bool IsReadOnly => _mDictionary.IsReadOnly;

    public override string ToString()
    {
        return _mLinkedList.ToString();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        throw GetNotSupportedDueToSimplification();
    }

    private static Exception GetNotSupportedDueToSimplification()
    {
        return new NotSupportedException("This method is not supported due to simplification of example code.");
    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释