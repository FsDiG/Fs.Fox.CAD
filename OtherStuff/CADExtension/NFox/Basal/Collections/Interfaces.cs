using System;
using System.Xml.Serialization;

namespace NFox.Collections
{
    /// <summary>
    /// XML序列化操作接口
    /// </summary>
    public interface ISerializableCollection : IXmlSerializable
    {
        /// <summary>
        /// 写入xml
        /// </summary>
        /// <param name="path"></param>
        void WriteXml(string path);
        /// <summary>
        /// 读取XML
        /// </summary>
        /// <param name="path"></param>
        void ReadXml(string path);
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        void WriteTo(string path);
        /// <summary>
        /// 读取窗体
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ISerializableCollection ReadFrom(string path);
    }
    /// <summary>
    /// 对象增加、删除、更改接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItems<T>
    {
        /// <summary>
        /// 增加元素
        /// </summary>
        Action<T> ItemAdded { get; set; }
        /// <summary>
        /// 删除元素
        /// </summary>
        Action<T> ItemRemoving { get; set; }
        /// <summary>
        /// 更改元素
        /// </summary>
        Action<T> ItemChanged { get; set; }
    }
}