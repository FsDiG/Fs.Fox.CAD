using System;
using System.ComponentModel;

namespace NFox.ComponentModel
{
    /// <summary>
    /// 提供对属性的封装
    /// </summary>
    internal class XTypedValueDescriptor : PropertyDescriptor
    {
        private XTypedValue _value;

        public XTypedValueDescriptor(XTypedValue value, Attribute[] atts) : base(value.Name, new Attribute[0])
        {
            _value = value;
        }

        public override string Description
        {
            get
            {
                return _value.Description;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return
                    _value.Converter == null ?
                    null : new XTypedValueConverter(_value);
            }
        }

        public override string Category
        {
            get
            {
                return _value.Category;
            }
        }

        public override bool CanResetValue(object component)
        {
            DefaultValueAttribute attribute = (DefaultValueAttribute)this.Attributes[typeof(DefaultValueAttribute)];
            if (attribute == null)
            {
                return false;
            }
            return attribute.Value.Equals(this.GetValue(component));
        }

        public override void ResetValue(object component)
        {
            DefaultValueAttribute attribute = (DefaultValueAttribute)this.Attributes[typeof(DefaultValueAttribute)];
            if (attribute != null)
            {
                this.SetValue(component, attribute.Value);
            }
        }

        public override Type ComponentType
        {
            get { return typeof(XEntityProperties); }
        }

        public override object GetValue(object component)
        {
            return _value.Value;
        }

        public override bool IsReadOnly
        {
            get { return _value.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return _value.Type; }
        }

        public override void SetValue(object component, object value)
        {
            _value.Value = value;
            _value.SetProperty();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}