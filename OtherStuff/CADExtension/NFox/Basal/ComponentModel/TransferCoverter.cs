using System;
using System.Collections;
using System.ComponentModel;

namespace NFox.ComponentModel
{
    public interface ITransfer
    {
        string[] SortedNames { get; }

        string ConvertToString();

        void ConvertFromString(string value);
    }

    public interface ITransfer<T> : ITransfer
    {
        ITransfer<T> CreateInstance(IDictionary propertyValues);

        T ConvertTo();

        void ConvertFrom(T value);
    }

    public static class Transfer
    {
        private static string _itransferTypeName
            = typeof(ITransfer<>).Name;

        private static Type GetITransfer(Type type)
        {
            return type.GetInterface(_itransferTypeName);
        }

        public static object ConvertTo(Type transferType, object transfer)
        {
            Type t = GetITransfer(transferType);
            if (t != null)
            {
                var mi = t.GetMethod("ConvertTo");
                return mi.Invoke(transfer, new object[0]);
            }
            return transfer;
        }

        public static object ConvertFrom(Type transferType, object value)
        {
            Type t = GetITransfer(transferType);
            if (t != null)
            {
                object transfer = Activator.CreateInstance(transferType);
                var mi = t.GetMethod("ConvertFrom");
                mi.Invoke(transfer, new object[1] { value });
                return transfer;
            }
            return value;
        }

        public static bool TestValue(object value, Type type)
        {
            return value != null && type.IsInstanceOfType(value);
        }
    }

    public class TransferCoverter<TOld, TNew> : TypeConverter where TNew : ITransfer<TOld>, new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                TNew temp = new TNew();
                temp.ConvertFromString((string)value);
                return temp;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                if (destinationType == typeof(string))
                {
                    TNew temp = (TNew)value;
                    return temp.ConvertToString();
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            TNew temp = (TNew)value;
            var pds = TypeDescriptor.GetProperties(temp, attributes);
            if (temp.SortedNames != null)
                pds = pds.Sort(temp.SortedNames);
            return pds;
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues != null)
            {
                try
                {
                    TNew temp = new TNew();
                    return temp.CreateInstance(propertyValues);
                }
                catch
                { }
            }
            return base.CreateInstance(context, propertyValues);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}