using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;



namespace NFox.Cad
{
    /// <summary>
    /// lisp列表的类
    /// </summary>
    public class LispListCollection : LispData, IEnumerable<LispData>
    {
        /// <summary>
        /// LispData 列表
        /// </summary>
        protected List<LispData> _lst =
            new List<LispData>();
        /// <summary>
        /// LispList 的父对象
        /// </summary>
        protected LispListCollection _parent;
        /// <summary>
        /// 列表的结尾
        /// </summary>
        protected virtual TypedValue ListEnd
        {
            get { return new TypedValue((int)LispDataType.ListEnd); }
        }
        /// <summary>
        /// 是否为列表
        /// </summary>
        public override bool IsList
        {
            get { return true; }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public LispListCollection()
            : base(LispDataType.ListBegin)
        { }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>LispData对象</returns>
        public LispData this[int index]
        {
            get { return _lst[index]; }
            set { _lst[index] = value; }
        }

        #region AddRemove
        /// <summary>
        /// 列表长度
        /// </summary>
        public int Count
        {
            get { return _lst.Count; }
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">LispData 对象</param>
        public void Add(LispData value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _lst.Add(value);
            if (value.IsList)
                ((LispListCollection)value)._parent = this;
        }

        private void Add(TypedValue value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">布尔值</param>
        public void Add(bool value)
        {
            Add(value ? T : Nil);
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">16位整型数值</param>
        public void Add(short value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">32位整型数值</param>
        public void Add(int value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">64位整型数值</param>
        public void Add(double value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">二维点</param>
        public void Add(Point2d value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">三维点</param>
        public void Add(Point3d value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">对象id</param>
        public void Add(ObjectId value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">字符串</param>
        public void Add(string value)
        {
            Add(new LispData(value));
        }
        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">选择集</param>
        public void Add(SelectionSet value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 删除 index 索引处的值
        /// </summary>
        /// <param name="index">索引</param>
        public void RemoveAt(int index)
        {
            if (index > -1 && index < _lst.Count)
            {
                _lst.RemoveAt(index);
            }
        }

        /// <summary>
        /// 删除值
        /// </summary>
        /// <param name="value">LispData对象</param>
        public void Remove(LispData value)
        {
            _lst.Remove(value);
        }

        /// <summary>
        /// 是否存在值
        /// </summary>
        /// <param name="value">LispData对象</param>
        /// <returns><see langword="true"/>表示存在，<see langword="false"/>表示不存在</returns>
        public bool Contains(LispData value)
        {
            return _lst.Contains(value);
        }

        /// <summary>
        /// 返回值的索引
        /// </summary>
        /// <param name="value">LispData对象</param>
        /// <returns>索引</returns>
        public int IndexOf(LispData value)
        {
            return _lst.IndexOf(value);
        }

        #endregion AddRemove

        #region Convert
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns>LispData迭代器</returns>
        public IEnumerator<LispData> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal override void GetValues(ResultBuffer rb)
        {
            rb.Add(Value);
            _lst.ForEach(d => d.GetValues(rb));
            rb.Add(ListEnd);
        }
        /// <summary>
        /// 获取lisplist列表的值
        /// </summary>
        /// <returns>lisplist列表的值</returns>
        public override object GetValue()
        {
            return ToBuffer();
        }
        /// <summary>
        /// 设置lisplist列表的值
        /// </summary>
        /// <param name="value">lisplist列表的值</param>
        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置lisplist列表的值
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="value">值</param>
        public override void SetValue(int code, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 转换为 ResultBuffer
        /// </summary>
        /// <returns>ResultBuffer对象</returns>
        public ResultBuffer ToBuffer()
        {
            ResultBuffer rb = new ResultBuffer();
            GetValues(rb);
            return rb;
        }

        /// <summary>
        /// 从 ResultBuffer 转换为 list
        /// </summary>
        /// <param name="rb">ResultBuffer对象</param>
        /// <returns>LispList对象</returns>
        public static LispListCollection FromBuffer(ResultBuffer rb)
        {
            LispListCollection lst = new LispListCollection();
            if (rb != null)
            {
                LispListCollection clst = lst;
                foreach (TypedValue value in rb)
                {
                    switch ((LispDataType)value.TypeCode)
                    {
                        case LispDataType.ListBegin:
                            var slst = new LispListCollection();
                            clst.Add(slst);
                            clst = slst;
                            break;

                        case LispDataType.ListEnd:
                            clst = clst._parent;
                            break;

                        case LispDataType.DottedPair:
                            var plst = clst._parent;
                            plst[plst.IndexOf(clst)] =
                                new LispDottedPairCollection
                                {
                                    _lst = clst._lst,
                                    _parent = clst._parent
                                };
                            clst = plst;
                            break;

                        default:
                            clst.Add(value);
                            break;
                    }
                }
            }
            return lst;
        }

        #endregion Convert
    }

    /// <summary>
    /// lisp 点对表
    /// </summary>
    public class LispDottedPairCollection : LispListCollection
    {
        /// <summary>
        /// 列表结尾
        /// </summary>
        protected override TypedValue ListEnd
        {
            get { return new TypedValue((int)LispDataType.DottedPair); }
        }
    }
}