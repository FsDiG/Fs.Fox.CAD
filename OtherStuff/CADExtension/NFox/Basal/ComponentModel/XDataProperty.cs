using System;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.ComponentModel
{
    //类型,即属性集合
    [Serializable]
    public class XDataProperty : XTypedValueCollection, IXmlSerializable
    {
        //类型名
        protected object _entity;

        public virtual object Entity
        {
            set
            {
                _entity = value;
            }
        }

        public EventHandler<XTypedValue> GridChanged;
        public EventHandler<XTypedValue> EntityChanged;
        public EventHandler<XDataProperty> EntityModified;

        public override void SetProperty(XTypedValue value)
        {
            if (GridChanged != null && _entity != null)
                GridChanged(_entity, value);
        }

        public override void GetProperty()
        {
            if (EntityModified == null)
            {
                if (EntityChanged != null && _entity != null)
                    foreach (var value in this)
                        EntityChanged(_entity, value);
            }
            else
            {
                EntityModified(_entity, this);
            }
        }

        public XDataProperty()
        {
            GridChanged =
                (entity, value) => value.SetProperty(entity);

            EntityChanged =
                (entity, value) => value.GetProperty(entity);
        }

        public XDataProperty(string category)
            : this()
        {
            Category = category;
        }

        #region IXmlSerializable 成员

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        internal void ReadXml(XmlReader reader, bool hasOwner)
        {
            Category = reader.GetAttribute("Category");
            reader.ReadStartElement();
            while (reader.Name == "XTypedValue")
            {
                XTypedValue value = new XTypedValue();
                value.ReadXml(reader, hasOwner);
                Add(value);
            }
            reader.ReadEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            ReadXml(reader, false);
        }

        internal void WriteXml(XmlWriter writer, bool hasOwner)
        {
            writer.WriteStartAttribute("Category");
            writer.WriteString(Category);
            writer.WriteEndAttribute();

            foreach (XTypedValue value in this)
            {
                writer.WriteStartElement("XTypedValue");
                value.WriteXml(writer, hasOwner);
                writer.WriteEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, false);
        }

        #endregion IXmlSerializable 成员
    }
}