using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.Collections
{
    /// <summary>
    /// 序列化操作
    /// </summary>
    /// <typeparam name="TInfo">节点名</typeparam>
    /// <typeparam name="TValue">节点值</typeparam>
    [Serializable]
    public class SerialList<TInfo, TValue> : SerialList<TValue>
    {
        /// <summary>
        /// 节点名
        /// </summary>
        public TInfo Info;
        /// <summary>
        /// 是否按节点名保存
        /// </summary>
        public bool IsInfoSaved = true;
        /// <summary>
        /// 初始化序列化类
        /// </summary>
        public SerialList()
            : base()
        { }
        /// <summary>
        /// 根据  初始化序列化类
        /// </summary>
        /// <param name="lst">迭代器</param>
        public SerialList(IEnumerable<TValue> lst)
            : base(lst)
        { }

        #region Xml
        /// <summary>
        /// 写xml
        /// </summary>
        /// <param name="writer">xml写入器对象</param>
        protected override void WriteXml(XmlWriter writer)
        {
            XmlSerializer xs;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            writer.WriteStartElement("Info");
            writer.WriteAttributeString("Saved", IsInfoSaved.ToString());
            if (IsInfoSaved)
            {
                xs = new XmlSerializer(typeof(TInfo));
                xs.Serialize(writer, Info, ns);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Values");
            xs = new XmlSerializer(typeof(TValue));
            foreach (TValue item in this)
            {
                xs.Serialize(writer, item, ns);
            }
            writer.WriteEndElement();
        }
        /// <summary>
        /// 读xml
        /// </summary>
        /// <param name="reader">xml访问器对象</param>
        protected override void ReadXml(XmlReader reader)
        {
            while (reader.Name != "Info")
            {
                reader.Read();
            }

            IsInfoSaved = Convert.ToBoolean(reader.GetAttribute("Saved"));
            reader.Read();

            XmlSerializer xs;
            if (IsInfoSaved)
            {
                if (Info != null && Info is ISerializableCollection)
                {
                    ((ISerializableCollection)Info).ReadXml(reader);
                }
                else
                {
                    xs = new XmlSerializer(typeof(TInfo));
                    Info = (TInfo)xs.Deserialize(reader);
                }
            }

            while (reader.Name != "Values")
            {
                reader.Read();
            }
            reader.Read();

            xs = new XmlSerializer(typeof(TValue));
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                TValue item = (TValue)xs.Deserialize(reader);
                if (item != null)
                {
                    Add(true, item);
                }
            };

            reader.Read();
            reader.Read();
            reader.Read();
        }

        #endregion Xml

        #region Bin
        /// <summary>
        /// 从文件读取序列化数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public override ISerializableCollection ReadFrom(string path)
        {
            SerialList<TInfo, TValue> lst =
                (SerialList<TInfo, TValue>)base.ReadFrom(path);

            if (lst != null)
            {
                Info = lst.Info;
            }
            return lst;
        }

        #endregion Bin
    }
}