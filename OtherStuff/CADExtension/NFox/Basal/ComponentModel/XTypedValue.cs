using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace NFox.ComponentModel
{
    [Serializable]
    public class XTypedValue : EventArgs, IXmlSerializable
    {
        #region Properties

        private object _value;

        /// <summary>
        /// 包含值
        /// </summary>
        public virtual object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == null)
                    throw new NotSupportedException("空属性值");
                else
                {
                    if (value.GetType() == _type)
                        _value = value;
                    else if (_converter.CanConvertFrom(value.GetType()))
                        _value = _converter.ConvertFrom(value);
                    else
                        throw new NotSupportedException("错误的属性类型!");
                }
            }
        }

        private Type _type;

        /// <summary>
        /// 包含值的类型
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        private TypeId _typeId;

        private TypeConverter _converter;
        private bool _isSpecConverter;

        /// <summary>
        /// 类型转换器
        /// </summary>
        public TypeConverter Converter
        {
            get { return _converter; }
        }

        public bool IsSpecConverter
        {
            get { return _isSpecConverter; }
        }

        private TypeId _converterTypeId;

        private List<string> _standardValues = null;

        /// <summary>
        /// 可选值列表
        /// </summary>
        public List<string> StandardValues
        {
            get { return _standardValues; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="values"></param>
        public void AddStandardValues(params string[] values)
        {
            if (_standardValues == null)
                _standardValues = new List<string>();
            _standardValues.AddRange(values);
        }

        /// <summary>
        /// 宿主对象
        /// </summary>
        public XTypedValueCollection Owner
        { private get; set; }

        /// <summary>
        /// 是否可读
        /// </summary>
        public bool IsReadOnly
        { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string Category
        {
            get { return Owner.Category; }
        }

        public string Description
        { get; set; }

        /// <summary>
        /// 显示在属性框中的名称
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName
        { get; set; }

        /// <summary>
        /// 类型代号
        /// </summary>
        public int? TypeCode
        { get; set; }

        #endregion Properties

        public void GetProperty(object entity)
        {
            if (PropertyName != null)
            {
                Type type = entity.GetType();
                var pi = type.GetProperty(PropertyName);
                if (pi != null && pi.CanRead)
                {
                    var value = pi.GetValue(entity, null);
                    _value = Transfer.ConvertFrom(_type, value);
                }
            }
        }

        public void SetProperty(object entity)
        {
            if (PropertyName != null)
            {
                Type type = entity.GetType();
                var pi = type.GetProperty(PropertyName);
                if (pi != null && pi.CanWrite)
                {
                    var value = Transfer.ConvertTo(_type, _value);
                    pi.SetValue(entity, value, null);
                }
            }
        }

        internal void SetProperty()
        {
            if (Owner != null)
                Owner.SetProperty(this);
        }

        public XTypedValue()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="displayName">属性名</param>
        /// <param name="typeCode">类型代号</param>
        /// <param name="value">值,默认为空字符串</param>
        public XTypedValue(string displayName, int typeCode)
        {
            Name = displayName;
            TypeCode = typeCode;
        }

        public XTypedValue(string displayName, string propertyName)
        {
            Name = displayName;
            PropertyName = propertyName;
        }

        internal void BuildTypeFromId(TypeManager tm)
        {
            _type = tm.GetType(_typeId);
            if (_converterTypeId == null)
            {
                _converter = XEntityProperties.GetConverter(_type);
            }
            else
            {
                _converter = Activator.CreateInstance(tm.GetType(_converterTypeId)) as TypeConverter;
                _isSpecConverter = true;
            }
            if (_value is string && _type != typeof(string))
                _value = _converter.ConvertFrom(_value);
        }

        internal void BuildIdFromType(TypeManager tm)
        {
            _typeId = tm.GetTypeId(_type);
            if (_isSpecConverter)
                _converterTypeId = tm.GetTypeId(_converter.GetType());
        }

        public void SetValue<T>(T value)
        {
            _value = value;
            _type = typeof(T);
            _converter = XEntityProperties.GetConverter(_type);
        }

        public void SetValue<T>(T value, TypeConverter converter)
        {
            _value = value;
            _type = typeof(T);
            _isSpecConverter = true;
            _converter = converter;
        }

        public void SetValue(Type type)
        {
            _value = null;
            _type = type;
            _converter = XEntityProperties.GetConverter(_type);
        }

        public void SetValue(Type type, TypeConverter converter)
        {
            _value = null;
            _type = type;
            _isSpecConverter = true;
            _converter = converter;
        }

        public void SetAttributes(bool isReadOnly, string description)
        {
            IsReadOnly = isReadOnly;
            Description = description;
        }

        #region IXmlSerializable 成员

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        internal void ReadXml(XmlReader reader, bool hasOwner)
        {
            if (reader.HasAttributes)
            {
                Name = reader.GetAttribute("Name");
                PropertyName = reader.GetAttribute("Property");
                Description = reader.GetAttribute("Description");
                IsReadOnly = Convert.ToBoolean(reader.GetAttribute("IsReadOnly"));
            }

            reader.ReadStartElement();
            if (reader.HasAttributes)
            {
                if (hasOwner)
                {
                    _typeId = TypeId.Parse(reader.GetAttribute("TypeId"));
                    string s = reader.GetAttribute("CTypeId");
                    if (s != null)
                    {
                        _isSpecConverter = true;
                        _converterTypeId = TypeId.Parse(s);
                    }
                }
                else
                {
                    _type = Type.GetType(reader.GetAttribute("Type"));
                    string typeName = reader.GetAttribute("Converter");
                    if (typeName != null)
                    {
                        _isSpecConverter = true;
                        _converter =
                            Activator.CreateInstance(Type.GetType(typeName))
                            as TypeConverter;
                    }
                    else
                        _converter = XEntityProperties.GetConverter(_type);
                }

                string code = reader.GetAttribute("Code");
                if (code != null)
                    TypeCode = Convert.ToInt32(code);
            }

            reader.ReadStartElement();
            if (reader.HasValue)
            {
                _value = reader.ReadContentAsString();
                if (!hasOwner)
                    _value = _converter.ConvertFrom(_value);
                reader.ReadEndElement();
            }

            if (reader.Name == "StandardValues")
            {
                reader.ReadStartElement();
                if (reader.Name == "Value")
                {
                    while (reader.Name == "Value")
                    {
                        reader.ReadStartElement();
                        AddStandardValues(reader.ReadString());
                        reader.ReadEndElement();
                    }
                    reader.ReadEndElement();
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer, bool hasOwner)
        {
            writer.WriteStartAttribute("Name");
            writer.WriteString(Name);
            writer.WriteEndAttribute();
            if (PropertyName != null)
            {
                writer.WriteStartAttribute("Property");
                writer.WriteString(PropertyName);
                writer.WriteEndAttribute();
            }
            if (Description != null)
            {
                writer.WriteStartAttribute("Description");
                writer.WriteString(Description);
                writer.WriteEndAttribute();
            }
            if (IsReadOnly)
            {
                writer.WriteStartAttribute("IsReadOnly");
                writer.WriteValue(IsReadOnly);
                writer.WriteEndAttribute();
            }

            writer.WriteStartElement("Value");
            if (hasOwner)
            {
                writer.WriteStartAttribute("TypeId");
                writer.WriteString(_typeId.ToString());
                writer.WriteEndAttribute();
                if (_isSpecConverter)
                {
                    writer.WriteStartAttribute("CTypeId");
                    writer.WriteString(_converterTypeId.ToString());
                    writer.WriteEndAttribute();
                }
            }
            else
            {
                writer.WriteStartAttribute("Type");
                writer.WriteString(_type.AssemblyQualifiedName);
                writer.WriteEndAttribute();
                if (_isSpecConverter)
                {
                    writer.WriteStartAttribute("Converter");
                    writer.WriteString(_converter.GetType().AssemblyQualifiedName);
                    writer.WriteEndAttribute();
                }
            }

            if (TypeCode != null)
            {
                writer.WriteStartAttribute("Code");
                writer.WriteValue(TypeCode);
                writer.WriteEndAttribute();
            }

            if (_value != null)
            {
                writer.WriteString((string)_converter.ConvertTo(_value, typeof(string)));
            }
            writer.WriteEndElement();

            if (_standardValues != null)
            {
                writer.WriteStartElement("StandardValues");
                for (int i = 0; i < _standardValues.Count; i++)
                {
                    writer.WriteStartElement("Value");
                    writer.WriteString(_standardValues[i]);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, false);
        }

        public void ReadXml(XmlReader reader)
        {
            ReadXml(reader, false);
        }

        #endregion IXmlSerializable 成员
    }
}