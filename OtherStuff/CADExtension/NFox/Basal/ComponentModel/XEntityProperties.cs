using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.ComponentModel
{
    /// <summary>
    /// 类型集合
    /// </summary>
    [Serializable]
    public class XEntityProperties : IEnumerable<XDataProperty>, ICustomTypeDescriptor, IXmlSerializable
    {
        /// <summary>
        /// 扩展类型字典
        /// </summary>
        private Dictionary<string, XDataProperty> _xdataProperties =
            new Dictionary<string, XDataProperty>();

        private PropertyGrid _grid;

        private TypeManager _typeManager = new TypeManager();

        public XEntityProperties()
        { }

        public XEntityProperties(PropertyGrid grid)
        {
            _grid = grid;
        }

        private bool _isWritting;

        /// <summary>
        /// 从实体获取数据
        /// </summary>
        public void RefreshToGrid()
        {
            if (!_isWritting)
            {
                foreach (XDataProperty xdp in this)
                {
                    xdp.GetProperty();
                }
                if (_grid.SelectedObject == null)
                    _grid.SelectedObject = this;
                else
                    _grid.Refresh();
            }
        }

        /// <summary>
        /// 刷新实体数据
        /// </summary>
        public void RefreshToEntity()
        {
            _isWritting = true;
            if (_grid.SelectedObject == null)
                _grid.SelectedObject = this;

            foreach (XDataProperty xdp in this)
            {
                foreach (var value in xdp)
                    xdp.SetProperty(value);
            }
            _isWritting = false;
        }

        public XDataProperty this[string category]
        {
            get
            {
                return _xdataProperties[category];
            }
        }

        public void Add(XDataProperty values)
        {
            string key = values.Category;
            if (_xdataProperties.ContainsKey(key))
                _xdataProperties[key].AddRange(values);
            else
                _xdataProperties.Add(key, values);

            foreach (var value in values)
                value.BuildIdFromType(_typeManager);
        }

        public void Clear()
        {
            _xdataProperties.Clear();
        }

        #region ICustomTypeDescriptor 成员

        System.ComponentModel.AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var pds = new List<XTypedValueDescriptor>();
            foreach (var lst in _xdataProperties.Values)
            {
                foreach (var value in lst)
                {
                    var pd = new XTypedValueDescriptor(value, attributes);
                    pds.Add(pd);
                }
            }
            return new PropertyDescriptorCollection(pds.ToArray());
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion ICustomTypeDescriptor 成员

        #region Converters

        /// <summary>
        /// 自定义类型的转换器字典
        /// </summary>
        private static Dictionary<Type, TypeConverter> _typeConverters =
            new Dictionary<Type, TypeConverter>();

        /// <summary>
        /// 为特定的类型指定转换器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeConverter"></param>
        public static void AddConverter(Type type, TypeConverter typeConverter)
        {
            _typeConverters.Add(type, typeConverter);
        }

        /// <summary>
        /// 获取指定类型的转换器
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>TypeConverter对象</returns>
        public static TypeConverter GetConverter(Type type)
        {
            if (_typeConverters.ContainsKey(type))
                return _typeConverters[type];
            return TypeDescriptor.GetConverter(type);
        }

        public static object GetEditor(Type componentType, Type type)
        {
            return TypeDescriptor.GetEditor(componentType, type);
        }

        #endregion Converters

        #region IEnumerable<XDataProperty> 成员

        public IEnumerator<XDataProperty> GetEnumerator()
        {
            foreach (var lst in _xdataProperties.Values)
                yield return lst;
        }

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable 成员

        #endregion IEnumerable<XDataProperty> 成员

        #region IXmlSerializable 成员

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();

            _typeManager = new TypeManager();
            _typeManager.ReadXml(reader);

            while (reader.Name == "XDataProperty")
            {
                XDataProperty values = new XDataProperty();
                values.ReadXml(reader, true);
                foreach (var value in values)
                    value.BuildTypeFromId(_typeManager);

                string key = values.Category;
                if (_xdataProperties.ContainsKey(key))
                    _xdataProperties[key].AddRange(values);
                else
                    _xdataProperties.Add(key, values);
            }
            reader.ReadEndElement();
        }

        public void ReadXml(string path)
        {
            using (XmlTextReader reader = new XmlTextReader(path))
            {
                XmlSerializer xs = new XmlSerializer(typeof(XEntityProperties));
                XEntityProperties xdata = xs.Deserialize(reader) as XEntityProperties;
                this._typeManager = xdata._typeManager;
                this._xdataProperties = xdata._xdataProperties;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("TypeManager");
            _typeManager.WriteXml(writer);
            writer.WriteEndElement();
            foreach (XDataProperty values in _xdataProperties.Values)
            {
                writer.WriteStartElement("XDataProperty");
                values.WriteXml(writer, true);
                writer.WriteEndElement();
            }
        }

        public void WriteXml(string path)
        {
            using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
            {
                XmlSerializer xs = new XmlSerializer(typeof(XEntityProperties));
                xs.Serialize(writer, this);
            }
        }

        #endregion IXmlSerializable 成员
    }
}