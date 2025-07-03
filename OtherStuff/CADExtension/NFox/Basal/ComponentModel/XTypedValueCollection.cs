using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NFox.ComponentModel
{
    public abstract class XTypedValueCollection : EventArgs, IEnumerable<XTypedValue>
    {
        protected List<XTypedValue> _lst = new List<XTypedValue>();

        public abstract void SetProperty(XTypedValue value);

        public abstract void GetProperty();

        //类型名
        public string Category
        { get; protected set; }

        public XTypedValue this[int index]
        {
            get
            {
                return _lst[index];
            }
        }

        public XTypedValue this[string name]
        {
            get
            {
                return this.FirstOrDefault(value => value.Name == name);
            }
        }

        #region Add By TypeCode

        public void Add<T>(string name, int typeCode, T value)
        {
            XTypedValue xvalue = new XTypedValue(name, typeCode);
            xvalue.SetValue(value);
            Add(xvalue);
        }

        public void Add(string name, int typeCode, Type type)
        {
            XTypedValue xvalue = new XTypedValue(name, typeCode);
            xvalue.SetValue(type);
            Add(xvalue);
        }

        public void Add<T>(string name, int typeCode, T value, TypeConverter converter)
        {
            XTypedValue xvalue = new XTypedValue(name, typeCode);
            xvalue.SetValue(value, converter);
            Add(xvalue);
        }

        public void Add(string name, int typeCode, Type type, TypeConverter converter)
        {
            XTypedValue xvalue = new XTypedValue(name, typeCode);
            xvalue.SetValue(type, converter);
            Add(xvalue);
        }

        #endregion Add By TypeCode

        #region Add By PropertyName

        public void Add<T>(string displayName, string propertyName, T value)
        {
            XTypedValue xvalue = new XTypedValue(displayName, propertyName);
            xvalue.SetValue(value);
            Add(xvalue);
        }

        public void Add(string displayName, string propertyName, Type type)
        {
            XTypedValue xvalue = new XTypedValue(displayName, propertyName);
            xvalue.SetValue(type);
            Add(xvalue);
        }

        public void Add<T>(string displayName, string propertyName, T value, TypeConverter converter)
        {
            XTypedValue xvalue = new XTypedValue(displayName, propertyName);
            xvalue.SetValue(value, converter);
            Add(xvalue);
        }

        public void Add(string displayName, string propertyName, Type type, TypeConverter converter)
        {
            XTypedValue xvalue = new XTypedValue(displayName, propertyName);
            xvalue.SetValue(type, converter);
            Add(xvalue);
        }

        #endregion Add By PropertyName

        public virtual void Add(XTypedValue value)
        {
            value.Owner = this;
            _lst.Add(value);
        }

        public void AddRange(IEnumerable<XTypedValue> lst)
        {
            foreach (var value in lst)
                Add(value);
        }

        public void Clear()
        {
            _lst.Clear();
        }

        public virtual IEnumerator<XTypedValue> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable 成员
    }
}