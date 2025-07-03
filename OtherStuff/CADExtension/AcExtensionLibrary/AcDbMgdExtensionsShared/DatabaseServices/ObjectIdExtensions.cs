using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class ObjectIdExtensions
    {
        /// <summary>
        /// Gets the database object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        /// <exception cref="Exception"></exception>
        public static T GetDBObject<T>(this ObjectId id, Transaction trx, OpenMode mode = OpenMode.ForRead,
            bool openErased = false) where T : DBObject
        {
            if (id.IsNull)
            {
                throw new ArgumentNullException("id is null");
            }
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (T)trx.GetObject(id, mode, openErased, false);
        }

        /// <summary>
        /// Gets the database object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        public static T GetDBObject<T>(this ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false)
            where T : DBObject
        {
            return id.GetDBObject<T>(id.Database.TransactionManager.TopTransaction, mode, openErased);
        }

        /// <summary>
        /// Gets the database object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        public static DBObject GetDBObject(this ObjectId id, Transaction trx, OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
        {
            return id.GetDBObject<DBObject>(trx, mode, openErased);
        }

        /// <summary>
        /// Gets the database object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        public static DBObject GetDBObject(this ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            return id.GetDBObject<DBObject>(id.Database.TransactionManager.TopTransaction, mode, openErased);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        /// <exception cref="Exception"></exception>
        public static T GetEntity<T>(this ObjectId id, Transaction trx, OpenMode mode = OpenMode.ForRead,
            bool openErased = false, bool forceOpenOnLockedLayer = false) where T : Entity
        {
            if (id.IsNull)
            {
                throw new ArgumentNullException("id is null");
            }
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (T)trx.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        public static T GetEntity<T>(this ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false,
            bool forceOpenOnLockedLayer = false) where T : Entity
        {
            return id.GetEntity<T>(id.Database.TransactionManager.TopTransaction, mode, openErased,
                forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        public static Entity GetEntity(this ObjectId id, Transaction trx, OpenMode mode = OpenMode.ForRead,
            bool openErased = false, bool forceOpenOnLockedLayer = false)
        {
            return id.GetEntity<Entity>(trx, mode, openErased, forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        public static Entity GetEntity(this ObjectId id, OpenMode mode = OpenMode.ForRead, bool openErased = false,
            bool forceOpenOnLockedLayer = false)
        {
            return id.GetEntity<Entity>(id.Database.TransactionManager.TopTransaction, mode, openErased,
                forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">ids is null</exception>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<T> GetEntities<T>(this IEnumerable<ObjectId> ids, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool openErased = false, bool forceOpenOnLockedLayer = false)
            where T : Entity
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids is null");
            }
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            foreach (ObjectId id in ids)
            {
                yield return (T)trx.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
            }
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        public static IEnumerable<T> GetEntities<T>(this IEnumerable<ObjectId> ids, OpenMode mode = OpenMode.ForRead,
            bool openErased = false, bool forceOpenOnLockedLayer = false) where T : Entity
        {
            ObjectId id = ids.First();
            if (id == null)
            {
                throw new ArgumentNullException("id is null");
            }
            return ids.GetEntities<T>(id.Database.TransactionManager.TopTransaction, mode, openErased,
                forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        public static IEnumerable<Entity> GetEntities(this IEnumerable<ObjectId> ids, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool openErased = false, bool forceOpenOnLockedLayer = false)
        {
            return ids.GetEntities<Entity>(trx, mode, openErased, forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <param name="forceOpenOnLockedLayer">if set to <c>true</c> [force open on locked layer].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        public static IEnumerable<Entity> GetEntities(this IEnumerable<ObjectId> ids, OpenMode mode = OpenMode.ForRead,
            bool openErased = false, bool forceOpenOnLockedLayer = false)
        {
            ObjectId id = ids.First();
            if (id == null)
            {
                throw new ArgumentNullException("id is null");
            }
            return ids.GetEntities<Entity>(id.Database.TransactionManager.TopTransaction, mode, openErased,
                forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Gets the database objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">ids is null</exception>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<T> GetDBObjects<T>(this IEnumerable<ObjectId> ids, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool openErased = false) where T : DBObject
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids is null");
            }
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            foreach (ObjectId id in ids)
            {
                yield return (T)trx.GetObject(id, mode, openErased, false);
            }
        }

        /// <summary>
        /// Gets the database objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        public static IEnumerable<T> GetDBObjects<T>(this IEnumerable<ObjectId> ids, OpenMode mode = OpenMode.ForRead,
            bool openErased = false) where T : DBObject
        {
            ObjectId id = ids.First();
            if (id == null)
            {
                throw new ArgumentNullException("id is null");
            }
            return ids.GetDBObjects<T>(id.Database.TransactionManager.TopTransaction, mode, openErased);
        }

        /// <summary>
        /// Gets the database objects.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        public static IEnumerable<DBObject> GetDBObjects(this IEnumerable<ObjectId> ids, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            return ids.GetDBObjects<DBObject>(trx, mode, openErased);
        }

        /// <summary>
        /// Gets the database objects.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="openErased">if set to <c>true</c> [open erased].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id is null</exception>
        public static IEnumerable<DBObject> GetDBObjects(this IEnumerable<ObjectId> ids,
            OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            ObjectId id = ids.First();
            if (id == null)
            {
                throw new ArgumentNullException("id is null");
            }
            return ids.GetDBObjects<DBObject>(id.Database.TransactionManager.TopTransaction, mode, openErased);
        }
    }
}