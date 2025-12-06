using System;
using System.Collections.Generic;
using System.Linq;

namespace NFox.Collections
{
    /// <summary>
    /// 深度优先遍历模式
    /// </summary>
    public enum DepthSearchModeType
    {
        /// <summary>
        /// 前序遍历
        /// </summary>
        Preorder,

        /// <summary>
        /// 中序遍历
        /// </summary>
        ///Inorder,

        /// <summary>
        /// 后序遍历
        /// </summary>
        Postorder
    }
    /// <summary>
    /// 树
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    public class Tree<T> : List<Tree<T>>
    {
        /// <summary>
        /// 元素
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// 父树
        /// </summary>
        public Tree<T> Parent
        { private set; get; }
        /// <summary>
        /// 是否为根节点
        /// </summary>
        public bool IsRoot
        {
            get { return Parent == null; }
        }
        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        public bool IsLeaf
        {
            get { return Count == 0; }
        }
        /// <summary>
        /// 根节点
        /// </summary>
        public Tree<T> Root
        {
            get
            {
                var node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }
                return node;
            }
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Tree()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">节点数据</param>
        public Tree(T value)
        {
            Value = value;
        }
        /// <summary>
        /// 是否为树的父树
        /// </summary>
        /// <param name="other">树</param>
        /// <returns></returns>
        public bool IsAncestorOf(Tree<T> other)
        {
            Tree<T> node = other;
            while (node.Parent != null)
            {
                node = node.Parent;
                if (this == node)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 是否为树的子树
        /// </summary>
        /// <param name="other">树</param>
        /// <returns></returns>
        public bool IsChildOf(Tree<T> other)
        {
            return other.IsAncestorOf(this);
        }
        /// <summary>
        /// 路径
        /// </summary>
        public List<Tree<T>> Path
        {
            get
            {
                List<Tree<T>> lst = new List<Tree<T>> { this };
                Tree<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    lst.Insert(0, node);
                }
                return lst;
            }
        }
        /// <summary>
        /// 树的深度
        /// </summary>
        public int Depth
        {
            get { return Path.Count - 1; }
        }
        /// <summary>
        /// 插入树
        /// </summary>
        /// <param name="index">位置</param>
        /// <param name="node">树</param>
        public new void Insert(int index, Tree<T> node)
        {
            node.Parent = this;
            base.Insert(index, node);
        }
        /// <summary>
        /// 在节点前插入树
        /// </summary>
        /// <param name="node">树</param>
        /// <param name="value">节点</param>
        public void InsertBefore(Tree<T> node, T value)
        {
            Insert(IndexOf(node), new Tree<T>(value));
        }
        /// <summary>
        /// 在节点后插入树
        /// </summary>
        /// <param name="node">树</param>
        /// <param name="value">节点</param>
        public void InsertAfter(Tree<T> node, T value)
        {
            Insert(IndexOf(node) + 1, new Tree<T>(value));
        }
        /// <summary>
        /// 添加树
        /// </summary>
        /// <param name="node">树</param>
        public new void Add(Tree<T> node)
        {
            node.Parent = this;
            base.Add(node);
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="value">节点</param>
        public void Add(T value)
        {
            Add(new Tree<T>(value));
        }
        /// <summary>
        /// 添加树群
        /// </summary>
        /// <param name="collection">树群</param>
        public new void AddRange(IEnumerable<Tree<T>> collection)
        {
            foreach (var tree in collection)
                tree.Parent = this;

            base.AddRange(collection);
        }
        /// <summary>
        /// 添加节点群
        /// </summary>
        /// <param name="collection">节点群</param>
        public void AddRange(IEnumerable<T> collection)
        {
            AddRange(collection.Select(value => new Tree<T>(value)));
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="match">匹配规则</param>
        public void RemoveAll(Predicate<T> match)
        {
            base.RemoveAll(tree => match(tree.Value));
        }

        /// <summary>
        /// 深度优先
        /// </summary>
        /// <param name="mode">深度优先遍历模式</param>
        /// <param name="action">要执行的函数</param>
        public void SearchByDepthFirst(DepthSearchModeType mode, Action<T> action)
        {
            if (mode == DepthSearchModeType.Preorder)
                action(this.Value);

            foreach (Tree<T> node in this)
                node.SearchByDepthFirst(mode, action);

            if (mode == DepthSearchModeType.Postorder)
                action(this.Value);
        }

        /// <summary>
        /// 广度优先
        /// </summary>
        /// <param name="action">要执行的函数</param>
        public void SearchByBreadthFirst(Action<T> action)
        {
            Queue<Tree<T>> q = new Queue<Tree<T>>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                Tree<T> node = q.Dequeue();
                action(node.Value);
                node.ForEach(child => q.Enqueue(child));
            }
        }
    }
}