using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension Methods for SymbolTable class
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public static class SymbolTableExtensions
    {
        /// <summary>
        /// Gets the object ids of the SymbolTable.
        /// </summary>

        /// <returns>IEnumerable{ObjectId}  the ObjectIds in SymbolTable</returns>
        public static IEnumerable<ObjectId> GetObjectIds(this SymbolTable st)
        {
            foreach (ObjectId id in st)
            {
                yield return id;
            }
            //return st.Cast<ObjectId>();
        }


        /// <summary>
        /// Gets the erased object ids.
        /// </summary>
        /// <returns>IEnumerable{ObjectId}  the ObjectIds with IsErased = <c>true</c> in SymbolTable</returns>
        public static IEnumerable<ObjectId> GetErasedObjectIds(this SymbolTable st)
        {

            foreach (ObjectId id in st.IncludingErased)
            {
                if (id.IsErased)
                {
                    yield return id;
                }
            }

            //return st.IncludingErased.Cast<ObjectId>().Where(id => id.IsErased);
        }

        /// <summary>
        /// Gets the symbol table records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="filterDependecyById">if set to <c>true</c> [filter dependecy by identifier].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static IEnumerable<T> GetSymbolTableRecords<T>(this SymbolTable symbolTbl, Transaction trx,
            OpenMode mode, SymbolTableRecordFilter filter, bool filterDependecyById) where T : SymbolTableRecord
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }

            bool includingErased = filter.IsSet(SymbolTableRecordFilter.IncludedErased);

            if (filter.IsSet(SymbolTableRecordFilter.IncludeDependent))
            {
                foreach (ObjectId id in includingErased ? symbolTbl.IncludingErased : symbolTbl)
                {
                    yield return (T)trx.GetObject(id, mode, includingErased, false);
                }
            }
            else
            {
                if (filterDependecyById)
                {
                    IntPtr dbIntPtr = symbolTbl.Database.UnmanagedObject;
                    foreach (ObjectId id in includingErased ? symbolTbl.IncludingErased : symbolTbl)
                    {
                        if (id.OriginalDatabase.UnmanagedObject == dbIntPtr)
                        {
                            yield return (T)trx.GetObject(id, mode, includingErased, false);
                        }
                    }
                }
                else
                {
                    foreach (ObjectId id in includingErased ? symbolTbl.IncludingErased : symbolTbl)
                    {
                        T current = (T)trx.GetObject(id, mode, includingErased, false);
                        if (!current.IsDependent)
                        {
                            yield return current;
                        }
                    }
                }
            }
        }

        // Author: Gile
        // Source: http://www.theswamp.org/index.php?topic=42539.msg477455#msg477455
        // Use the overloaded Database constructor with explicit arguments (false, true) 
        // which is preferable with ReadDwgfile() method.
        //  https://adndevblog.typepad.com/autocad/2012/07/using-readdwgfile-with-net-attachxref-or-objectarx-acdbattachxref.html

        public static ObjectId ImportSymbolTableRecord<T>(this Database targetDb, string sourceFile, string recordName)
            where T : SymbolTable
        {
            using (Database sourceDb = new Database(false, true))
            {
                sourceDb.ReadDwgFile(sourceFile, System.IO.FileShare.Read, false, "");
                ObjectId sourceTableId, targetTableId;
                switch (typeof(T).Name)
                {
                    case "BlockTable":
                        sourceTableId = sourceDb.BlockTableId;
                        targetTableId = targetDb.BlockTableId;
                        break;
                    case "DimStyleTable":
                        sourceTableId = sourceDb.DimStyleTableId;
                        targetTableId = targetDb.DimStyleTableId;
                        break;
                    case "LayerTable":
                        sourceTableId = sourceDb.LayerTableId;
                        targetTableId = targetDb.LayerTableId;
                        break;
                    case "LinetypeTable":
                        sourceTableId = sourceDb.LinetypeTableId;
                        targetTableId = targetDb.LinetypeTableId;
                        break;
                    case "RegAppTable":
                        sourceTableId = sourceDb.RegAppTableId;
                        targetTableId = targetDb.RegAppTableId;
                        break;
                    case "TextStyleTable":
                        sourceTableId = sourceDb.TextStyleTableId;
                        targetTableId = targetDb.TextStyleTableId;
                        break;
                    case "UcsTable":
                        sourceTableId = sourceDb.UcsTableId;
                        targetTableId = targetDb.UcsTableId;
                        break;
                    case "ViewTable":
                        sourceTableId = sourceDb.ViewportTableId;
                        targetTableId = targetDb.ViewportTableId;
                        break;
                    case "ViewportTable":
                        sourceTableId = sourceDb.ViewportTableId;
                        targetTableId = targetDb.ViewportTableId;
                        break;
                    default:
                        throw new ArgumentException("Requires a concrete type derived from SymbolTable");
                }

                using (Transaction tr = sourceDb.TransactionManager.StartTransaction())
                {
                    T sourceTable = (T)tr.GetObject(sourceTableId, OpenMode.ForRead);
                    if (!sourceTable.Has(recordName))
                        return ObjectId.Null;
                    ObjectIdCollection idCol = new ObjectIdCollection();
                    ObjectId sourceTableRecordId = sourceTable[recordName];
                    idCol.Add(sourceTableRecordId);
                    IdMapping idMap = new IdMapping();
                    sourceDb.WblockCloneObjects(idCol, targetTableId, idMap, DuplicateRecordCloning.Ignore, false);
                    tr.Commit();
                    return idMap[sourceTableRecordId].Value;
                }
            }
        }

        ///// <summary>
        ///// Imports the symbol table record.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="targetDb">The target database.</param>
        ///// <param name="sourceFile">The source file.</param>
        ///// <param name="cloningStyle">The cloning style.</param>
        ///// <param name="recordName">Name of the record.</param>
        ///// <returns></returns>
        ///// <exception cref="System.ArgumentException">Requires a concrete type derived from SymbolTable</exception>
        //public static ObjectId ImportSymbolTableRecord<T>(this Database targetDb, string sourceFile,
        //    DuplicateRecordCloning cloningStyle, string recordName)
        //    where T : SymbolTable
        //{
        //    using (var sourceDb = new Database(false, true))
        //    {
        //        sourceDb.ReadDwgFile(sourceFile, FileShare.ReadWrite, false, "");
        //        ObjectId sourceTableId, targetTableId;
        //        switch (typeof(T).Name)
        //        {
        //            case "BlockTable":
        //                sourceTableId = sourceDb.BlockTableId;
        //                targetTableId = targetDb.BlockTableId;
        //                break;

        //            case "DimStyleTable":
        //                sourceTableId = sourceDb.DimStyleTableId;
        //                targetTableId = targetDb.DimStyleTableId;
        //                break;

        //            case "LayerTable":
        //                sourceTableId = sourceDb.LayerTableId;
        //                targetTableId = targetDb.LayerTableId;
        //                break;

        //            case "LinetypeTable":
        //                sourceTableId = sourceDb.LinetypeTableId;
        //                targetTableId = targetDb.LinetypeTableId;
        //                break;

        //            case "RegAppTable":
        //                sourceTableId = sourceDb.RegAppTableId;
        //                targetTableId = targetDb.RegAppTableId;
        //                break;

        //            case "TextStyleTable":
        //                sourceTableId = sourceDb.TextStyleTableId;
        //                targetTableId = targetDb.TextStyleTableId;
        //                break;

        //            case "UcsTable":
        //                sourceTableId = sourceDb.UcsTableId;
        //                targetTableId = targetDb.UcsTableId;
        //                break;

        //            case "ViewTable":
        //                sourceTableId = sourceDb.ViewportTableId;
        //                targetTableId = targetDb.ViewportTableId;
        //                break;

        //            case "ViewportTable":
        //                sourceTableId = sourceDb.ViewportTableId;
        //                targetTableId = targetDb.ViewportTableId;
        //                break;

        //            default:
        //                throw new ArgumentException("Requires a concrete type derived from SymbolTable");
        //        }

        //        using (Transaction tr = sourceDb.TransactionManager.StartTransaction())
        //        {
        //            T sourceTable = (T)tr.GetObject(sourceTableId, OpenMode.ForRead);
        //            if (!sourceTable.Has(recordName))
        //            {
        //                return ObjectId.Null;
        //            }
        //            var idCol = new ObjectIdCollection();
        //            ObjectId sourceTableRecordId = sourceTable[recordName];
        //            idCol.Add(sourceTableRecordId);
        //            var idMap = new IdMapping();
        //            sourceDb.WblockCloneObjects(idCol, targetTableId, idMap, cloningStyle, false);
        //            tr.Commit();
        //            return idMap[sourceTableRecordId].Value;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Imports the symbol table records.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="targetDb">The target database.</param>
        ///// <param name="sourceFile">The source file.</param>
        ///// <param name="cloningStyle">The cloning style.</param>
        ///// <param name="recordNames">The record names.</param>
        ///// <returns></returns>
        ///// <exception cref="System.ArgumentException">Requires a concrete type derived from SymbolTable</exception>
        //public static bool ImportSymbolTableRecords<T>(this Database targetDb, string sourceFile,
        //    DuplicateRecordCloning cloningStyle, params string[] recordNames)
        //    where T : SymbolTable
        //{
        //    using (var sourceDb = new Database(false, true))
        //    {
        //        sourceDb.ReadDwgFile(sourceFile, FileShare.Read, false, "");
        //        ObjectId sourceTableId, targetTableId;
        //        switch (typeof(T).Name)
        //        {
        //            case "BlockTable":
        //                sourceTableId = sourceDb.BlockTableId;
        //                targetTableId = targetDb.BlockTableId;
        //                break;

        //            case "DimStyleTable":
        //                sourceTableId = sourceDb.DimStyleTableId;
        //                targetTableId = targetDb.DimStyleTableId;
        //                break;

        //            case "LayerTable":
        //                sourceTableId = sourceDb.LayerTableId;
        //                targetTableId = targetDb.LayerTableId;
        //                break;

        //            case "LinetypeTable":
        //                sourceTableId = sourceDb.LinetypeTableId;
        //                targetTableId = targetDb.LinetypeTableId;
        //                break;

        //            case "RegAppTable":
        //                sourceTableId = sourceDb.RegAppTableId;
        //                targetTableId = targetDb.RegAppTableId;
        //                break;

        //            case "TextStyleTable":
        //                sourceTableId = sourceDb.TextStyleTableId;
        //                targetTableId = targetDb.TextStyleTableId;
        //                break;

        //            case "UcsTable":
        //                sourceTableId = sourceDb.UcsTableId;
        //                targetTableId = targetDb.UcsTableId;
        //                break;

        //            case "ViewTable":
        //                sourceTableId = sourceDb.ViewportTableId;
        //                targetTableId = targetDb.ViewportTableId;
        //                break;

        //            case "ViewportTable":
        //                sourceTableId = sourceDb.ViewportTableId;
        //                targetTableId = targetDb.ViewportTableId;
        //                break;

        //            default:
        //                throw new ArgumentException("Requires a concrete type derived from SymbolTable");
        //        }

        //        using (var tr = sourceDb.TransactionManager.StartTransaction())
        //        {
        //            var allImported = true;
        //            var sourceTable = (T)tr.GetObject(sourceTableId, OpenMode.ForRead);
        //            var idCol = new ObjectIdCollection();
        //            foreach (var recordName in recordNames)
        //            {
        //                if (sourceTable.Has(recordName))
        //                {
        //                    idCol.Add(sourceTable[recordName]);
        //                }
        //                else
        //                {
        //                    allImported = false;
        //                }
        //            }

        //            var idMap = new IdMapping();
        //            sourceDb.WblockCloneObjects(idCol, targetTableId, idMap, cloningStyle, false);
        //            tr.Commit();
        //            return allImported;
        //        }
        //    }
        //}
    }
}