using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class ObjectIdCollectionExtensions
    {
        /// <summary>
        /// Adds the specified ids.
        /// </summary>
        /// <param name="thisIds">The this ids.</param>
        /// <param name="ids">The ids.</param>
        public static void Add(this ObjectIdCollection thisIds, ObjectIdCollection ids)
        {
            foreach (ObjectId id in ids)
            {
                thisIds.Add(id);
            }
        }

        /// <summary>
        /// Adds the specified ids.
        /// </summary>
        /// <param name="thisIds">The this ids.</param>
        /// <param name="ids">The ids.</param>
        public static void Add(this ObjectIdCollection thisIds, IEnumerable<ObjectId> ids)
        {
            foreach (ObjectId id in ids)
            {
                thisIds.Add(id);
            }
        }

        /// <summary>
        /// To the array.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public static ObjectId[] ToArray(this ObjectIdCollection ids)
        {
            ObjectId[] idsArray = new ObjectId[ids.Count];
            ids.CopyTo(idsArray, 0);
            return idsArray;
        }

        /// <summary>
        /// Wheres the specified TRX.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// predicate
        /// </exception>
        public static IEnumerable<ObjectId> Where<T>(this ObjectIdCollection source, Transaction trx,
            Func<T, bool> predicate) where T : DBObject
        {
            if (source.IsNull())
            {
                throw new ArgumentNullException("source");
            }
            if (predicate.IsNull())
            {
                throw new ArgumentNullException("predicate");
            }
            return WhereImpl<T>(source, trx, predicate);
        }

        /// <summary>
        /// Wheres the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static IEnumerable<ObjectId> Where<T>(this ObjectIdCollection source, Func<T, bool> predicate)
            where T : DBObject
        {
            return Where<T>(source, HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction, predicate);
        }

        /// <summary>
        /// Wheres the implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        private static IEnumerable<ObjectId> WhereImpl<T>(this ObjectIdCollection source, Transaction trx,
            Func<T, bool> predicate) where T : DBObject
        {
            foreach (ObjectId item in source)
            {
                T dbo = (T)trx.GetObject(item, OpenMode.ForRead, false, false);
                if (predicate(dbo))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// To the object identifier collection.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ObjectIdCollection ToObjectIdCollection(this IEnumerable<ObjectId> source)
        {
            return new ObjectIdCollection(source.ToArray());
        }

        /// <summary>
        /// To the object identifier collection.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ObjectIdCollection ToObjectIdCollection(this IEnumerable<DBObject> source)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            foreach (var dbObject in source)
            {
                ids.Add(dbObject.ObjectId);
            }
            return ids;
        }
    }
}