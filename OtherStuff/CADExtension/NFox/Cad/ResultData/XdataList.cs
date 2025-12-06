using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;


namespace NFox.Cad
{
    /// <summary>
    /// 扩展数据类型类，
    /// 这是一个TypedValue类型的列表类
    /// </summary>
    public class XdataList : List<TypedValue>
    {
        /// <summary>
        /// 默认初始化
        /// </summary>
        public XdataList()
        {
        }
        /// <summary>
        /// 采用TypedValue迭代器初始化
        /// </summary>
        /// <param name="values">TypedValue迭代器</param>
        public XdataList(IEnumerable<TypedValue> values)
            : base(values)
        { }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public void Add(int code, object obj)
        {
            base.Add(new TypedValue(code, obj));
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public void Add(LispDataType code, object obj)
        {
            base.Add(new TypedValue((int)code, obj));
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public void Add(DxfCode code, object obj)
        {
            base.Add(new TypedValue((int)code, obj));
        }

        #region Convert

        /// <summary>
        /// 从 ResultBuffer 生成 XdataList
        /// </summary>
        /// <param name="rb">ResultBuffer 类型变量</param>
        /// <returns>XdataList对象</returns>
        public static XdataList FromBuffer(ResultBuffer rb)
        {
            var lst = new XdataList();
            lst.AddRange(rb.Cast<TypedValue>());
            return lst;
        }

        /// <summary>
        /// 转换为 ResultBuffer
        /// </summary>
        /// <returns>ResultBuffer对象</returns>
        public ResultBuffer ToBuffer()
        {
            var rb = new ResultBuffer();
            foreach (var value in this)
                rb.Add(value);
            return rb;
        }

        /// <summary>
        /// 隐式转换为 TypedValue 数组
        /// </summary>
        /// <param name="rlst">扩展数据</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator TypedValue[] (XdataList rlst)
        {
            return rlst.ToArray();
        }

        /// <summary>
        /// 隐式转换为 ResultBuffer
        /// </summary>
        /// <param name="rlst">扩展数据</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ResultBuffer(XdataList rlst)
        {
            return rlst.ToBuffer();
        }

        /// <summary>
        /// 隐式转换为 SelectionFilter
        /// </summary>
        /// <param name="rlst">扩展数据</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SelectionFilter(XdataList rlst)
        {
            return new SelectionFilter(rlst.ToArray());
        }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return ToBuffer().ToString();
        }

        #endregion Convert
    }
}