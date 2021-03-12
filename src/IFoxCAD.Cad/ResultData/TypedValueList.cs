﻿using Autodesk.AutoCAD.DatabaseServices;

using System.Collections.Generic;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 用于集中管理扩展数据/扩展字典/resultbuffer的类
    /// </summary>
    public class TypedValueList : List<TypedValue>
    {
        #region 构造函数
        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public TypedValueList() { }
        /// <summary>
        /// 采用 TypedValue 迭代器构造 TypedValueList
        /// </summary>
        /// <param name="values"></param>
        public TypedValueList(IEnumerable<TypedValue> values) : base(values) { }

        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public virtual void Add(int code, object obj)
        {
            Add(new TypedValue(code, obj));
        }
        ///// <summary>
        ///// 添加数据
        ///// </summary>
        ///// <param name="code">组码</param>
        ///// <param name="obj">组码值</param>
        //public virtual void Add(DxfCode code, object obj)
        //{
        //    Add(new TypedValue((int)code, obj));
        //}

        #endregion

        #region 转换器
        /// <summary>
        /// ResultBuffer 隐式转换到 TypedValueList
        /// </summary>
        /// <param name="buffer">ResultBuffer 实例</param>
        public static implicit operator TypedValueList(ResultBuffer buffer) => new TypedValueList(buffer.AsArray());
        /// <summary>
        /// TypedValueList 隐式转换到 TypedValue 数组
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator TypedValue[](TypedValueList values) => values.ToArray();
        /// <summary>
        /// TypedValueList 隐式转换到 ResultBuffer
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator ResultBuffer(TypedValueList values) => new ResultBuffer(values);
        /// <summary>
        /// TypedValue 数组隐式转换到 TypedValueList
        /// </summary>
        /// <param name="values">TypedValue 数组</param>
        public static implicit operator TypedValueList(TypedValue[] values) => new TypedValueList(values);
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>ResultBuffer 字符串</returns>
        public override string ToString()
        {
            return new ResultBuffer(this).ToString();
        }
        #endregion
    }
}