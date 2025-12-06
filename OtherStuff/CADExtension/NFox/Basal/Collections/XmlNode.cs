using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NFox.Common.Xml
{
    /// <summary>
    /// xml节点类
    /// </summary>
    public class XmlNode : IEnumerable<XmlNode>
    {
        private XElement _owner;
        private List<XElement> _lst = new List<XElement>();
        /// <summary>
        /// 是否为列表
        /// </summary>
        public bool IsList
        { get { return _lst.Count > 1; } }
        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get { return _lst.Count; }
        }
        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="id">索引</param>
        /// <returns></returns>
        public virtual XmlNode this[int id]
        {
            get { return new XmlNode(_lst[id]); }
        }

        #region Constructor
        /// <summary>
        /// 指定xml元素初始化xml节点
        /// </summary>
        /// <param name="owner">xml元素</param>
        public XmlNode(XElement owner)
        {
            _owner = owner;
            _lst.Add(_owner);
        }
        /// <summary>
        /// 指定xml元素和xml元素迭代器初始化xml节点
        /// </summary>
        /// <param name="owner">xml元素</param>
        /// <param name="items">xml元素迭代器</param>
        public XmlNode(XElement owner, IEnumerable<XElement> items)
        {
            _owner = owner;
            _lst.AddRange(items);
        }
        /// <summary>
        /// 初始化xml节点
        /// </summary>
        public XmlNode()
            : this(new XElement("Null"))
        { }
        /// <summary>
        /// 指定名称初始化xml节点
        /// </summary>
        /// <param name="name">名称</param>
        public XmlNode(string name)
            : this(new XElement(name))
        { }
        /// <summary>
        /// 指定名称和内容初始化xml节点
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="content">内容</param>
        public XmlNode(string name, object content)
            : this(new XElement(name, content))
        {
            _owner = new XElement(name, content);
            _lst.Add(_owner);
        }
        /// <summary>
        /// 按路径载入xml节点
        /// </summary>
        /// <param name="uri">路径</param>
        /// <returns></returns>
        public static XmlNode Load(string uri)
        {
            return new XmlNode(XElement.Load(uri));
        }

        #endregion Constructor

        #region IXmlObject
        /// <summary>
        /// 返回xml元素
        /// </summary>
        /// <returns></returns>
        public XElement GetX()
        {
            return _owner;
        }
        /// <summary>
        /// 设置xml元素值
        /// </summary>
        /// <param name="value">值</param>
        public void SetValue(object value)
        {
            if (!IsList)
                _owner.Value = value.ToString();
        }
        /// <summary>
        /// 获取xml元素值
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            if (!IsList)
                return _owner.Value;
            else
                return null;
        }
        /// <summary>
        /// 设置xml元素名
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            foreach (var e in _lst)
                e.Name = name;
        }
        /// <summary>
        /// 获取xml元素名
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return _lst[0].Name.LocalName;
        }
        /// <summary>
        /// 删除xml节点
        /// </summary>
        public void Remove()
        {
            if (!IsList)
            {
                _owner.Remove();
            }
            else
            {
                foreach (var xe in _lst)
                    xe.Remove();
            }
        }

        #endregion IXmlObject

        #region Member

        private enum ItemsType
        {
            Null,
            Node,
            List
        }

        private ItemsType GetSameItems(string name, out IEnumerable<XElement> items)
        {
            items = _owner.Descendants(XName.Get(name));
            int count = items.Count();
            if (items == null || count == 0)
                return ItemsType.Null;
            else if (count == 1)
                return ItemsType.Node;
            else
                return ItemsType.List;
        }
        /// <summary>
        /// 获取节点属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <returns></returns>
        public string GetAttribute(string name)
        {
            //获取属性

            var att = _owner.Attribute(name);
            if (att == null)
                return null;
            else
                return att.Value;
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="name">节点名</param>
        /// <param name="createNode">如节点不存在，是否新建节点</param>
        /// <returns></returns>
        public XmlNode GetMember(string name, bool createNode)
        {
            //获取子节点
            IEnumerable<XElement> items;
            switch (GetSameItems(name, out items))
            {
                case ItemsType.Null:
                    return createNode ? null : Add(name);

                case ItemsType.Node:
                    return new XmlNode(items.ElementAt(0));

                case ItemsType.List:
                    return new XmlNode(_owner, items);

                default:
                    return null;
            }
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="name">节点名</param>
        /// <returns></returns>
        public XmlNode GetMember(string name)
        {
            return GetMember(name, false);
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        public void SetAttribute(string name, object value)
        {
            var att = _owner.Attribute(name);
            if (att == null)
                _owner.Add(new XAttribute(name, value));
            else
                att.Value = value.ToString();
        }
        /// <summary>
        /// 设置子节点
        /// </summary>
        /// <param name="name">节点名</param>
        /// <param name="value">节点值</param>
        public void SetMember(string name, object value)
        {
            //设置子节点
            IEnumerable<XElement> items;
            switch (GetSameItems(name, out items))
            {
                case ItemsType.Null:
                    _owner.Add(new XElement(name, value));
                    break;

                case ItemsType.Node:
                    items.ElementAt(0).Value = value.ToString();
                    break;

                case ItemsType.List:
                default:
                    break;
            }
        }

        #endregion Member

        #region Add

        /// <summary>
        /// 加入子元素
        /// </summary>
        /// <param name="name">子元素名称</param>
        /// <returns>节点</returns>
        public XmlNode Add(string name)
        {
            var e = new XElement(name);
            _owner.Add(e);
            return new XmlNode(e);
        }

        /// <summary>
        /// 加入子元素
        /// </summary>
        /// <param name="name">子元素名称</param>
        /// <param name="value">节点数据</param>
        /// <returns>节点</returns>
        public XmlNode Add(string name, object value)
        {
            var e = new XElement(name, value);
            _owner.Add(e);
            return new XmlNode(e);
        }

        /// <summary>
        /// 加入子元素
        /// </summary>
        /// <param name="item">节点</param>
        public void Add(XmlNode item)
        {
            Add(item._owner);
        }

        /// <summary>
        /// 加入子元素
        /// </summary>
        /// <param name="item">节点</param>
        public void Add(XElement item)
        {
            var lst = item.Elements();
            var names = lst.Select(e => e.Name).Distinct();

            if (lst.Count() > 1 && names.Count() == 1)
            {
                foreach (var xe in lst)
                {
                    xe.Name = item.Name;
                    Add(xe);
                }
            }
            else
            {
                _owner.Add(item);
            }
        }

        /// <summary>
        /// 序列化对象,并加入子元素
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        public void Add<T>(T obj)
        {
            Add(XmlNode.ConvertFrom(obj));
        }

        /// <summary>
        /// 加入同级元素
        /// </summary>
        /// <param name="item">节点</param>
        public void AddSame(XElement item)
        {
            var name = GetName();
            var parent = _lst[0].Parent;
            if (parent == null)
            {
                XmlNode node = new XmlNode();
                node._owner.Add(_owner);
                parent = node._owner;
            }

            if (IsList)
            {
                XElement e = item;
                e.Name = name;
                _owner.Add(e);
                _lst.Add(e);
            }
            else
            {
                XElement e = _lst[0];
                if (e.IsEmpty)
                {
                    _owner.Remove();
                    parent.Add(item);
                    _owner = item;
                    _owner.Name = name;
                    _lst[0] = _owner;
                }
                else
                {
                    _owner = _owner.Parent;
                    e = item;
                    e.Name = name;
                    _owner.Add(e);
                    _lst.Add(e);
                }
            }
        }

        /// <summary>
        /// 加入同级元素
        /// </summary>
        /// <param name="item">节点</param>
        public void AddSame(XmlNode item)
        {
            AddSame(item._owner);
        }

        /// <summary>
        /// 序列化对象,并加入同级元素
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        public void AddSame<T>(T obj)
        {
            AddSame(XmlNode.ConvertFrom(obj));
        }

        #endregion Add

        #region Convert

        /// <summary>
        /// 反序列化为对象迭代器
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象迭代器</returns>
        public List<T> Cast<T>()
        {
            Type t = typeof(T);
            var oldname = GetName();
            SetName(t.Name);

            var lst = new List<T>();
            foreach (var xe in _lst)
            {
                using (StringReader sr = new StringReader(xe.ToString()))
                {
                    xe.Name = oldname;
                    XmlSerializer xz = new XmlSerializer(t);
                    lst.Add((T)xz.Deserialize(sr));
                }
            }
            return lst;
        }

        /// <summary>
        /// 反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="node">节点</param>
        /// <returns>对象</returns>
        public T ConvertTo<T>(XmlNode node)
        {
            return node.Cast<T>().ElementAt(0);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>节点</returns>
        public static XmlNode ConvertFrom<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlSerializer xz = new XmlSerializer(typeof(T));
                xz.Serialize(sw, obj, ns);
                using (StringReader sr = new StringReader(sw.ToString()))
                    return new XmlNode(XElement.Load(sr));
            }
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="name">节点名称</param>
        /// <returns>节点</returns>
        public static XmlNode ConvertFrom<T>(T obj, string name)
        {
            var node = ConvertFrom(obj);
            node.SetName(name);
            return node;
        }
        /// <summary>
        /// 隐私转换为XElement
        /// </summary>
        /// <param name="node">xml节点</param>
        public static implicit operator XElement(XmlNode node)
        {
            return node._owner;
        }
        /// <summary>
        /// 隐式转换为字符串
        /// </summary>
        /// <param name="node"></param>
        public static implicit operator string(XmlNode node)
        {
            return node._owner.Value;
        }

        #endregion Convert

        #region GetEnumerator
        /// <summary>
        /// 获取节点名称迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetItemNames()
        {
            return
                _owner.Elements()
                .Select(e => e.Name.LocalName)
                .Distinct();
        }
        /// <summary>
        /// 获取子节点迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XmlNode> GetItems()
        {
            foreach (var name in GetItemNames())
                yield return GetMember(name);
        }
        /// <summary>
        /// 获取节点迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<XmlNode> GetEnumerator()
        {
            foreach (var xe in _lst)
                yield return new XmlNode(xe);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion GetEnumerator
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _owner.ToString();
        }
    }
}