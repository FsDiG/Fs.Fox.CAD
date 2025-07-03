using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace NFox.Cad
{
    /// <summary>
    /// 对象id扩展类
    /// </summary>
    public static class ObjectIdEx
    {
        #region GetObject

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开删除对象</param>
        /// <returns>DBObject对象</returns>
        public static DBObject GetObject(this ObjectId id, Transaction tr, OpenMode mode, bool openErased)
        {
            return tr.GetObject(id, mode, openErased);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>DBObject对象</returns>
        public static DBObject GetObject(this ObjectId id, Transaction tr, OpenMode mode)
        {
            return id.GetObject(tr, mode, false);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <returns>DBObject对象</returns>
        public static DBObject GetObject(this ObjectId id, Transaction tr)
        {
            return id.GetObject(tr, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开删除对象</param>
        /// <returns>指定类型对象</returns>
        public static T GetObject<T>(this ObjectId id, Transaction tr, OpenMode mode, bool openErased) where T : DBObject
        {
            return (T)tr.GetObject(id, mode, openErased);
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>指定类型对象</returns>
        public static T GetObject<T>(this ObjectId id, Transaction tr, OpenMode mode) where T : DBObject
        {
            return id.GetObject<T>(tr, mode, false);
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="id">对象id</param>
        /// <param name="tr">事务</param>
        /// <returns>指定类型对象</returns>
        public static T GetObject<T>(this ObjectId id, Transaction tr) where T : DBObject
        {
            return id.GetObject<T>(tr, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开删除对象</param>
        /// <returns>指定类型对象集合</returns>
        public static IEnumerable<T> GetObject<T>(this IEnumerable<ObjectId> ids, Transaction tr, OpenMode mode, bool openErased) where T : DBObject
        {
            return ids.Select(id => id.GetObject<T>(tr, mode, openErased));
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>指定类型对象集合</returns>
        public static IEnumerable<T> GetObject<T>(this IEnumerable<ObjectId> ids, Transaction tr, OpenMode mode) where T : DBObject
        {
            return ids.Select(id => id.GetObject<T>(tr, mode));
        }

        /// <summary>
        /// 获取指定类型对象
        /// </summary>
        /// <typeparam name="T">指定的泛型</typeparam>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <returns>指定类型对象集合</returns>
        public static IEnumerable<T> GetObject<T>(this IEnumerable<ObjectId> ids, Transaction tr) where T : DBObject
        {
            return ids.Select(id => id.GetObject<T>(tr));
        }

        /// <summary>
        /// 获取DBObject对象
        /// </summary>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开删除对象</param>
        /// <returns>DBObject对象集合</returns>
        public static IEnumerable<DBObject> GetObject(this IEnumerable<ObjectId> ids, Transaction tr, OpenMode mode, bool openErased)
        {
            return ids.Select(id => id.GetObject(tr, mode, openErased));
        }

        /// <summary>
        /// 获取DBObject对象
        /// </summary>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>DBObject对象集合</returns>
        public static IEnumerable<DBObject> GetObject(this IEnumerable<ObjectId> ids, Transaction tr, OpenMode mode)
        {
            return ids.Select(id => id.GetObject(tr, mode));
        }

        /// <summary>
        /// 获取DBObject对象
        /// </summary>
        /// <param name="ids">对象id集合</param>
        /// <param name="tr">事务</param>
        /// <returns>DBObject对象集合</returns>
        public static IEnumerable<DBObject> GetObject(this IEnumerable<ObjectId> ids, Transaction tr)
        {
            return ids.Select(id => id.GetObject(tr));
        }

        #endregion GetObject

        #region Open

        //public static T Open<T>(this ObjectId id, OpenMode mode, bool openErased) where T : DBObject
        //{
        //    return (T)id.Open(mode, openErased);
        //}

        //public static T Open<T>(this ObjectId id, OpenMode mode) where T : DBObject
        //{
        //    return (T)id.Open(mode, false);
        //}

        //public static T Open<T>(this ObjectId id) where T : DBObject
        //{
        //    return (T)id.Open(OpenMode.ForRead, false);
        //}

        #endregion Open

        /// <summary>
        /// 返回符合类型的对象id
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="ids">对象id集合</param>
        /// <returns>对象id集合</returns>
        public static IEnumerable<ObjectId> OfType<T>(this IEnumerable<ObjectId> ids) where T : DBObject
        {
            string dxfName = RXClass.GetClass(typeof(T)).DxfName;
            return
                ids
                .Where(id => id.ObjectClass.DxfName == dxfName);
        }
    }
}