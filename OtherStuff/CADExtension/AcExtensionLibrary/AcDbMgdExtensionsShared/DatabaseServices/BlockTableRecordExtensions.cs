using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension Methods for BlockTableRecord class
    /// </summary>
    public static class BlockTableRecordExtensions
    {
        /// <summary>
        /// 
        /// Author: Tony Tanzillo
        /// Source: http://www.theswamp.org/index.php?topic=41311.msg464529#msg464529
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> UserDefinedBlocks(this IEnumerable<BlockTableRecord> source)
        {
            return source.Where(btr =>
                !(
                    btr.IsAnonymous ||
                    btr.IsFromExternalReference ||
                    btr.IsFromOverlayReference ||
                    btr.IsLayout ||
                    btr.IsAProxy
                    )
                );
        }

        /// <summary>
        /// User Created Blocks are any blocks that are created by user or from a user block created by a user. It
        /// This includes anonymous blocks
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> UserCreatedBlocks(this IEnumerable<BlockTableRecord> source)
        {
            return source.Where(btr =>
                !(
                    btr.IsFromExternalReference ||
                    btr.IsFromOverlayReference ||
                    btr.IsLayout ||
                    btr.IsAProxy
                    )
                );
        }


        public static IEnumerable<BlockTableRecord> NonLayoutBlocks(this IEnumerable<BlockTableRecord> source)
        {
            return source.Where(btr =>
                !(
                    btr.IsLayout ||
                    btr.IsAProxy
                    )
                );
        }
        /// <summary>
        /// Author: Tony Tanzillo
        /// Source: http://www.theswamp.org/index.php?topic=41311.msg464529#msg464529
        /// Use instead of UserDefinedBlocks when anonymous blocks are needed
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> NonDependent(this IEnumerable<BlockTableRecord> source)
        {
            return source.Where(btr =>
                !(
                    btr.IsFromExternalReference ||
                    btr.IsFromOverlayReference
                    )
                );
        }

        /// <summary>
        /// Returns a enumerable of ObjectsIds contained
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="btr">The BTR.</param>
        /// <returns></returns>
        public static IEnumerable<ObjectId> GetObjectIds<T>(this BlockTableRecord btr) where T : Entity
        {
            IntPtr impobj = RXClass.GetClass(typeof(T)).UnmanagedObject;
            foreach (ObjectId id in btr)
            {
                if (id.ObjectClass.UnmanagedObject == impobj)
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Gets the object ids.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <returns></returns>
        public static IEnumerable<ObjectId> GetObjectIds(this BlockTableRecord btr)
        {
            foreach (ObjectId id in btr)
            {
                yield return id;
            }
        }

        /// <summary>
        /// Author: Tony Tanzillo
        /// A combination of code written by Tony Tanzillo from 2 sources below
        /// Source1: http://www.theswamp.org/index.php?topic=42197.msg474011#msg474011
        /// Source2: http://www.theswamp.org/index.php?topic=41311.msg464457#msg464457
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<Entity> GetEntities(this BlockTableRecord btr, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            foreach (ObjectId id in includingErased ? btr.IncludingErased : btr)
            {
                yield return (Entity)trx.GetObject(id, mode, includingErased, openObjectsOnLockedLayers);
            }
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<Entity> GetEntities(this BlockTableRecord btr, OpenMode mode = OpenMode.ForRead,
            bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            return btr.GetEntities(btr.Database.TransactionManager.TopTransaction, mode, includingErased,
                openObjectsOnLockedLayers);
        }

        /// <summary>
        /// Author: Tony Tanzillo
        /// A combination of code written by Tony Tanzillo from 2 sources below
        /// Source1: http://www.theswamp.org/index.php?topic=42197.msg474011#msg474011
        /// Source2: http://www.theswamp.org/index.php?topic=41311.msg464457#msg464457
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
            where T : Entity
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            IntPtr impObject = RXClass.GetClass(typeof(T)).UnmanagedObject;

            foreach (ObjectId id in includingErased ? btr.IncludingErased : btr)
            {
                if (id.ObjectClass.UnmanagedObject == impObject)
                {
                    yield return (T)trx.GetObject(id, mode, includingErased, openObjectsOnLockedLayers);
                }
            }
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="btr">The BTR.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr, OpenMode mode = OpenMode.ForRead,
            bool includingErased = false, bool openObjectsOnLockedLayers = false) where T : Entity
        {
            return btr.GetEntities<T>(btr.Database.TransactionManager.TopTransaction, mode, includingErased,
                openObjectsOnLockedLayers);
        }

        /// <summary>
        /// Author: Tony Tanzillo
        /// A combination of code written by Tony Tanzillo from 2 sources below
        /// Source1: http://www.theswamp.org/index.php?topic=42197.msg474011#msg474011
        /// Source2: http://www.theswamp.org/index.php?topic=41311.msg464457#msg464457
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<T> GetEntitiesAssignableFrom<T>(this BlockTableRecord btr, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
            where T : Entity
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            RXClass rxclass = RXClass.GetClass(typeof(T));

            foreach (ObjectId id in includingErased ? btr.IncludingErased : btr)
            {
                if (id.ObjectClass.IsDerivedFrom(rxclass))
                {
                    yield return (T)trx.GetObject(id, mode, includingErased, openObjectsOnLockedLayers);
                }
            }
        }

        /// <summary>
        /// Gets the entities assignable from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="btr">The BTR.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEntitiesAssignableFrom<T>(this BlockTableRecord btr,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
            where T : Entity
        {
            return btr.GetEntitiesAssignableFrom<T>(btr.Database.TransactionManager.TopTransaction, mode,
                includingErased, openObjectsOnLockedLayers);
        }

        /// <summary>
        /// Method for getting all blockreference Ids
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="directOnly">if set to <c>true</c> [direct only].</param>
        /// <param name="forceValidity">if set to <c>true</c> [force validity].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ObjectIdCollection GetAllBlockReferenceIds(this BlockTableRecord btr, Transaction trx,
            bool directOnly, bool forceValidity)
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            ObjectIdCollection blockReferenceIds = btr.GetBlockReferenceIds(directOnly, forceValidity);

            if (!btr.IsDynamicBlock) return blockReferenceIds;
            foreach (ObjectId id in btr.GetAnonymousBlockIds())
            {
                BlockTableRecord record = trx.GetObject(id, OpenMode.ForRead) as BlockTableRecord;
                blockReferenceIds.Add(record.GetBlockReferenceIds(directOnly, forceValidity));
            }
            return blockReferenceIds;
        }

        /// <summary>
        /// Gets all block reference ids.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="directOnly">if set to <c>true</c> [direct only].</param>
        /// <param name="forceValidity">if set to <c>true</c> [force validity].</param>
        /// <returns></returns>
        public static ObjectIdCollection GetAllBlockReferenceIds(this BlockTableRecord btr, bool directOnly,
            bool forceValidity)
        {
            return btr.GetAllBlockReferenceIds(btr.Database.TransactionManager.TopTransaction, directOnly, forceValidity);
        }

        /// <summary>
        /// Gets all block references.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="directOnly">if set to <c>true</c> [direct only].</param>
        /// <param name="forceValidity">if set to <c>true</c> [force validity].</param>
        /// <returns></returns>
        public static IEnumerable<BlockReference> GetAllBlockReferences(this BlockTableRecord btr, Transaction trx,
            bool directOnly, bool forceValidity)
        {
            TransactionManager tm = btr.Database.TransactionManager;
            foreach (ObjectId id in GetAllBlockReferenceIds(btr, trx, directOnly, forceValidity))
            {
                yield return (BlockReference)trx.GetObject(id, OpenMode.ForRead, false, false);
            }
        }

        /// <summary>
        /// Gets all block references.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="directOnly">if set to <c>true</c> [direct only].</param>
        /// <param name="forceValidity">if set to <c>true</c> [force validity].</param>
        /// <returns></returns>
        public static IEnumerable<BlockReference> GetAllBlockReferences(this BlockTableRecord btr, bool directOnly,
            bool forceValidity)
        {
            return btr.GetAllBlockReferences(btr.Database.TransactionManager.TopTransaction, directOnly, forceValidity);
        }

        /// <summary>
        /// Gets the attribute definitions.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<AttributeDefinition> GetAttributeDefinitions(this BlockTableRecord btr,
            Transaction trx, OpenMode mode = OpenMode.ForRead, bool includingErased = false,
            bool openObjectsOnLockedLayers = false)
        {
            return btr.GetEntities<AttributeDefinition>(trx, mode, includingErased, openObjectsOnLockedLayers);
        }

        /// <summary>
        /// Gets the attribute definitions.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<AttributeDefinition> GetAttributeDefinitions(this BlockTableRecord btr,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            return btr.GetAttributeDefinitions(btr.Database.TransactionManager.TopTransaction, mode, includingErased,
                openObjectsOnLockedLayers);
        }
    }
}