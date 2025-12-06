using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NFox.ComponentModel
{
    /// <summary>
    /// 为属性提供的特殊转换器
    /// 添加可选值
    /// </summary>
    internal class XTypedValueConverter : TypeConverter
    {
        private TypeConverter _converter;
        private List<string> _standardValues;

        public XTypedValueConverter(XTypedValue value)
        {
            _converter = value.Converter;
            _standardValues = value.StandardValues;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return _converter.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return _converter.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return _converter.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return _converter.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return _standardValues != null || _converter.GetStandardValuesSupported(context);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (_standardValues != null)
                return new StandardValuesCollection(_standardValues.Select(s => _converter.ConvertFrom(s)).ToList());
            return _converter.GetStandardValues(context);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return _converter.GetPropertiesSupported(context);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return _converter.GetProperties(context, value, attributes);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return _converter.CreateInstance(context, propertyValues);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return _converter.GetCreateInstanceSupported(context);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return _converter.IsValid(context, value);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return _converter.GetStandardValuesExclusive(context);
        }
    }
}