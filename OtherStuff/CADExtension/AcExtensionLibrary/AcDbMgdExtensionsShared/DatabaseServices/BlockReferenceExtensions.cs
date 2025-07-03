using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Runtime;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// 
    /// </summary>
    public static class BlockReferenceExtensions
    {
        /// <summary>
        /// Gets the effective block table record.
        /// </summary>
        /// <param name="blockref">The blockref.</param>
        /// <param name="trx">The TRX.</param>
        /// <returns>The BlockTableRecord used to create </returns>
        public static BlockTableRecord GetEffectiveBlockTableRecord(this BlockReference blockref, Transaction trx)
        {
            if (blockref.IsDynamicBlock)
            {
                return blockref.DynamicBlockTableRecord.GetDBObject<BlockTableRecord>(trx, OpenMode.ForRead, false);
            }
            return blockref.BlockTableRecord.GetDBObject<BlockTableRecord>(trx, OpenMode.ForRead, false);
        }

        /// <summary>
        /// Gets the effective block table record.
        /// </summary>
        /// <param name="blockref">The blockref.</param>
        /// <returns></returns>
        public static BlockTableRecord GetEffectiveBlockTableRecord(this BlockReference blockref)
        {
            return blockref.GetEffectiveBlockTableRecord(blockref.Database.TransactionManager.TopTransaction);
        }

        /// <summary>
        /// Gets the name of the effective.
        /// </summary>
        /// <param name="blockref">The blockref.</param>
        /// <param name="trx">The TRX.</param>
        /// <returns></returns>
        public static string GetEffectiveName(this BlockReference blockref, Transaction trx)
        {
            if (blockref.IsDynamicBlock)
            {
                return blockref.DynamicBlockTableRecord.GetDBObject<BlockTableRecord>(trx, OpenMode.ForRead, false).Name;
            }
            return blockref.Name;
        }

        /// <summary>
        /// Gets the name of the effective.
        /// </summary>
        /// <param name="blockref">The blockref.</param>
        /// <returns></returns>
        public static string GetEffectiveName(this BlockReference blockref)
        {
            return blockref.GetEffectiveName(blockref.Database.TransactionManager.TopTransaction);
        }

        /// <summary>
        /// Gets the attribute references.
        /// </summary>
        /// <param name="bref">The bref.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception"></exception>
        public static IEnumerable<AttributeReference> GetAttributeReferences(this BlockReference bref, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            if (includingErased)
            {
                foreach (ObjectId id in bref.AttributeCollection)
                {
                    yield return (AttributeReference) trx.GetObject(id, mode, true, openObjectsOnLockedLayers);
                }
            }
            else
            {
                foreach (ObjectId id in bref.AttributeCollection)
                {
                    if (!id.IsErased)
                    {
                        yield return (AttributeReference) trx.GetObject(id, mode, false, openObjectsOnLockedLayers);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the attribute references.
        /// </summary>
        /// <param name="bref">The bref.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static IEnumerable<AttributeReference> GetAttributeReferences(this BlockReference bref,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            return bref.GetAttributeReferences(bref.Database.TransactionManager.TopTransaction, mode, includingErased,
                openObjectsOnLockedLayers);
        }

        /// <summary>
        /// Gets the attribute reference dictionary.
        /// </summary>
        /// <param name="bref">The bref.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception"></exception>
        public static Dictionary<string, AttributeReference> GetAttributeReferenceDictionary(this BlockReference bref,
            Transaction trx, OpenMode mode = OpenMode.ForRead, bool includingErased = false,
            bool openObjectsOnLockedLayers = false)
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            return
                bref.GetAttributeReferences(trx, mode, includingErased, openObjectsOnLockedLayers)
                    .ToDictionary(a => a.Tag);
        }

        /// <summary>
        /// Gets the attribute reference dictionary.
        /// </summary>
        /// <param name="bref">The bref.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <param name="openObjectsOnLockedLayers">if set to <c>true</c> [open objects on locked layers].</param>
        /// <returns></returns>
        public static Dictionary<string, AttributeReference> GetAttributeReferenceDictionary(this BlockReference bref,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false, bool openObjectsOnLockedLayers = false)
        {
            return bref.GetAttributeReferenceDictionary(bref.Database.TransactionManager.TopTransaction, mode,
                includingErased, openObjectsOnLockedLayers);
        }
    }
}
