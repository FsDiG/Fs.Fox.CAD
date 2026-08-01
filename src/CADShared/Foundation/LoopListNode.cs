namespace Fs.Fox.Basal;

#line hidden // 调试的时候跳过它

/// <summary>
/// 环链表节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoopListNode<T>
{
    #region 成员
    /// <summary>
    /// 取值
    /// </summary>
    public T Value;

    /// <summary>
    /// 上一个节点
    /// </summary>
    public LoopListNode<T>? Previous { internal set; get; }

    /// <summary>
    /// 下一个节点
    /// </summary>
    public LoopListNode<T>? Next { internal set; get; }

    /// <summary>
    /// 环链表序列
    /// </summary>
    public LoopList<T>? List { internal set; get; }
    #endregion

    #region 构造
    /// <summary>
    /// 环链表节点构造函数
    /// </summary>
    /// <param name="value">节点值</param>
    /// <param name="ts">环链表</param>
    public LoopListNode(T value, LoopList<T> ts)
    {
        Value = value;
        List = ts;
    }

    /// <summary>
    /// 获取当前节点的临近节点
    /// </summary>
    /// <param name="forward">搜索方向标志,<see langword="true"/>为向前搜索,<see langword="false"/>为向后搜索</param>
    /// <returns></returns>
    public LoopListNode<T>? GetNext(bool forward)
    {
        return forward ? Next : Previous;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 无效化成员
    /// </summary>
    internal void Invalidate()
    {
        List = null;
        Next = null;
        Previous = null;
    }
    #endregion
}

#line default
