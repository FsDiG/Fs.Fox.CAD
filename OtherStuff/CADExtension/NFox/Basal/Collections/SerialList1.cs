using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.Collections
{
    /// <summary>
    /// 序列化操作类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    [Serializable]
    public class SerialList<T> : List<T>, IItems<T>, ISerializableCollection
    {
        /// <summary>
        /// 添加元素
        /// </summary>
        public Action<T> ItemAdded { get; set; }
        /// <summary>
        /// 删除元素
        /// </summary>
        public Action<T> ItemRemoving { get; set; }
        /// <summary>
        /// 更改元素
        /// </summary>
        public Action<T> ItemChanged { get; set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SerialList()
            : base()
        { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lst">元素迭代器</param>
        public SerialList(IEnumerable<T> lst)
            : base(lst)
        { }

        #region IItems<T>
        /// <summary>
        /// 设置事件
        /// </summary>
        /// <param name="itemAdded"></param>
        /// <param name="itemRemoving"></param>
        /// <param name="itemChanged"></param>
        public void SetEvents(Action<T> itemAdded, Action<T> itemRemoving, Action<T> itemChanged)
        {
            ItemAdded = itemAdded;
            ItemRemoving = itemRemoving;
            ItemChanged = itemChanged;
        }
        /// <summary>
        /// 设置事件
        /// </summary>
        /// <param name="itemAdded"></param>
        /// <param name="itemRemoving"></param>
        public void SetEvents(Action<T> itemAdded, Action<T> itemRemoving)
        {
            ItemAdded = itemAdded;
            ItemRemoving = itemRemoving;
            ItemChanged = null;
        }
        /// <summary>
        /// 循环处理器
        /// </summary>
        /// <param name="action"></param>

        public void ForEach(Action<T, int> action)
        {
            int i = 0;
            foreach (T item in this)
            {
                action(item, i++);
            }
        }
        /// <summary>
        /// 更新处理器
        /// </summary>
        /// <param name="item"></param>

        public void Update(T item)
        {
            if (ItemChanged != null)
            {
                ItemChanged(item);
            }
        }
        /// <summary>
        /// 添加处器器
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="item"></param>
        public void Add(bool allowEvents, T item)
        {
            base.Add(item);
            if (allowEvents && ItemAdded != null)
            {
                ItemAdded(item);
            }
        }
        /// <summary>
        /// 添加多个元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="items"></param>
        public void AddRange(bool allowEvents, params T[] items)
        {
            if (allowEvents)
            {
                foreach (T item in items)
                {
                    Add(item);
                }
            }
            else
            {
                base.AddRange(items);
            }
        }
        /// <summary>
        /// 添加多个元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="items"></param>
        public void AddRange(bool allowEvents, IEnumerable<T> items)
        {
            if (allowEvents)
            {
                foreach (T item in items)
                {
                    Add(item);
                }
            }
            else
            {
                base.AddRange(items);
            }
        }
        /// <summary>
        /// 指定位置插入元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>

        public void Insert(bool allowEvents, int index, T item)
        {
            base.Insert(index, item);
            if (allowEvents && ItemAdded != null)
            {
                ItemAdded(item);
            }
        }
        /// <summary>
        /// 指定位置插入多个元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="index"></param>
        /// <param name="lst"></param>
        public void InsertRange(bool allowEvents, int index, IEnumerable<T> lst)
        {
            base.InsertRange(index, lst);
            if (allowEvents && ItemAdded != null)
            {
                foreach (T item in lst)
                {
                    ItemAdded(item);
                }
            }
        }
        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="item"></param>
        /// <returns></returns>

        public bool Remove(bool allowEvents, T item)
        {
            if (allowEvents && ItemRemoving != null && this.Contains(item))
            {
                ItemRemoving(item);
            }
            return base.Remove(item);
        }
        /// <summary>
        /// 删除指定位置元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="index"></param>
        public void RemoveAt(bool allowEvents, int index)
        {
            if (allowEvents && ItemRemoving != null && index > 0 && index < Count)
            {
                ItemRemoving(this[index]);
            }
            base.RemoveAt(index);
        }
        /// <summary>
        /// 删除指定位置元素
        /// </summary>
        /// <param name="allowEvents"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(bool allowEvents, int index, int count)
        {
            if (allowEvents)
            {
                for (int i = index; i < Count && i < index + count; i++)
                {
                    RemoveAt(i);
                }
            }
            else
            {
                base.RemoveRange(index, count);
            }
        }

        #endregion IItems<T>

        #region ISerialCollection

        #region Xml
        /// <summary>
        /// 写入xml
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            writer.WriteStartElement("Info");
            writer.WriteAttributeString("Saved", "false");
            writer.WriteEndElement();

            writer.WriteStartElement("Values");
            XmlSerializer xs = new XmlSerializer(typeof(T));
            foreach (T item in this)
            {
                xs.Serialize(writer, item, ns);
            }
            writer.WriteEndElement();
        }
        /// <summary>
        /// 写入xml
        /// </summary>
        /// <param name="path"></param>
        public virtual void WriteXml(string path)
        {
            using (XmlWriter writer = new XmlTextWriter(path, Encoding.UTF8))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("SerialList");
                WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        /// <summary>
        /// 读取XML
        /// </summary>
        /// <param name="reader"></param>
        protected virtual void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.Name != "Values")
            {
                reader.Read();
            }
            reader.Read();

            XmlSerializer xs = new XmlSerializer(typeof(T));
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                T item = (T)xs.Deserialize(reader);
                if (item != null)
                {
                    Add(item);
                }
            };

            reader.Read();
            reader.Read();
        }
        /// <summary>
        /// 读取XML
        /// </summary>
        /// <param name="path"></param>
        public virtual void ReadXml(string path)
        {
            if (File.Exists(path))
            {
                using (XmlTextReader reader = new XmlTextReader(path))
                {
                    if (reader.NodeType == XmlNodeType.None)
                    {
                        reader.Read();
                    }

                    if (reader.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        reader.Read();
                    }

                    ReadXml(reader);
                }
            }
        }

        #region IXmlSerializable 成员
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        /// <summary>
        /// 读取序列化的xml
        /// </summary>
        /// <param name="reader"></param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.None || reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                return;
            }

            ReadXml(reader);
        }
        /// <summary>
        /// 写入序列化的XML
        /// </summary>
        /// <param name="writer"></param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            WriteXml(writer);
        }

        #endregion IXmlSerializable 成员

        #endregion Xml

        #region Bin
        /// <summary>
        /// 绑定类
        /// </summary>
        private class UBinder : SerializationBinder
        {
            /// <summary>
            /// 绑定类型
            /// </summary>
            /// <param name="assemblyName"></param>
            /// <param name="typeName"></param>
            /// <returns></returns>
            public override Type BindToType(string assemblyName, string typeName)
            {
                try
                {
                    return Type.GetType(typeName);
                }
                catch
                {
                    return Assembly.Load(assemblyName).GetType(typeName);
                }
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        public virtual void WriteTo(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }
        /// <summary>
        /// 从文件读取序列化数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public virtual ISerializableCollection ReadFrom(string path)
        {
            if (File.Exists(path))
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Binder = new UBinder();
                    SerialList<T> lst = (SerialList<T>)bformatter.Deserialize(stream);
                    AddRange(lst);
                    return lst;
                }
            }
            return null;
        }

        #endregion Bin

        #endregion ISerialCollection
    }
}