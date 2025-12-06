using AcHelper.Enumerables;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace AcHelper
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Loops through all entities of the given type in modelspace.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="database"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this Database database, Action<T> action) where T : Entity
        {
            try
            {
                RXClass ent_type = RXClass.GetClass(typeof(T));

                using (Transaction tr = database.TransactionManager.StartOpenCloseTransaction())
                {
                    var blocktable = tr.GetObject<BlockTable>(database.BlockTableId, OpenMode.ForRead);
                    var modelspace = tr.GetObject<BlockTableRecord>(blocktable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    foreach (ObjectId id in modelspace)
                    {
                        if (id.ObjectClass.IsDerivedFrom(ent_type))
                        {
                            T entity = tr.GetObject<T>(id, OpenMode.ForRead);
                            action(entity);
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception) 
            {
                throw;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Loops through all entities of the given type in modelspace.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="doc"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this Document doc, Action<T> action) where T : Entity
        {
            Database database = doc.Database;
            try
            {
                RXClass ent_type = RXClass.GetClass(typeof(T));
                Common.UsingModelSpace(doc, (tr, ms) =>
                {
                    Transaction t = tr.Transaction;
                    // Loop through the entities in modelspace
                    foreach (ObjectId id in ms)
                    {
                        // Look for entities of the correct type
                        if (id.ObjectClass.IsDerivedFrom(ent_type))
                        {
                            T entity = t.GetObject<T>(id, OpenMode.ForRead);
                            action(entity);
                        }
                    }
                });
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                throw;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static IEnumerable<T> OfType<T>(this IEnumerable<ObjectId> enumerable, Transaction tr, OpenMode openMode) where T : DBObject
        {
            return new DbObjectEnumerable<T>(enumerable, tr, openMode);
        }

        /// <summary>
        /// <see cref="AcHelper.Common.UsingModelSpace"/>
        /// </summary>
        /// <param name="document"></param>
        /// <param name="action"></param>
        /// <param name="CommandName"></param>
        [Obsolete("Use Document.StartTransaction() instead. This extension will be removed next version.")]
        public static void UsingModelSpace(this Document document, Action<Wrappers.AcTransaction, BlockTableRecord> action, string CommandName = "")
        {
            Common.UsingModelSpace(document, CommandName, action);
        }
        /// <summary>
        /// <see cref="AcHelper.Common.UsingTransaction"/>
        /// </summary>
        /// <param name="document"></param>
        /// <param name="action"></param>
        /// <param name="commandName"></param>
        [Obsolete("Use Document.StartTransaction() instead. This extension will be removed next version.")]
        public static void UsingTransaction(this Document document, Action<Wrappers.AcTransaction> action, string commandName = "")
        {
            Common.UsingTransaction(document, commandName, action);
        }

        /// <summary>
        /// Gets the DBObject corresponding the given ObjectId.
        /// </summary>
        /// <typeparam name="T">DBObject type</typeparam>
        /// <param name="tr">Database Transaction</param>
        /// <param name="objectId"></param>
        /// <param name="openMode"></param>
        /// <param name="openErased">Open object if erased</param>
        /// <returns>DBObject as T</returns>
        public static T GetObject<T>(this Transaction tr, ObjectId objectId, OpenMode openMode, bool openErased = false) where T : DBObject
        {
            T dbObject = tr.GetObject(objectId, openMode, openErased) as T;
            return dbObject;
        }
    }
}
