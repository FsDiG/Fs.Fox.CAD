using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// Database operation helpers.
    /// </summary>
    public static class DbHelper
    {
        #region symbol tables & dictionaries

        /// <summary>
        /// Gets all records of a symbol table.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <returns>The record IDs.</returns>
        public static ObjectId[] GetSymbolTableRecords(ObjectId symbolTableId)
        {
            using (var trans = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                var table = (SymbolTable)trans.GetObject(symbolTableId, OpenMode.ForRead);
                return table.Cast<ObjectId>().ToArray();
            }
        }

        /// <summary>
        /// Gets all record names of a symbol table.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <returns>The record names.</returns>
        public static string[] GetSymbolTableRecordNames(ObjectId symbolTableId)
        {
            return DbHelper
                .GetSymbolTableRecords(symbolTableId)
                .QOpenForRead<SymbolTableRecord>()
                .Select(record => record.Name)
                .ToArray();
        }

        /// <summary>
        /// Gets a symbol table record by name.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <param name="name">The record name.</param>
        /// <param name="defaultValue">The default value if not found.</param>
        /// <param name="create">The factory method if not found.</param>
        /// <returns>The record ID.</returns>
        public static ObjectId GetSymbolTableRecord(ObjectId symbolTableId, string name, ObjectId? defaultValue = null, Func<SymbolTableRecord> create = null)
        {
            using (var trans = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                var table = (SymbolTable)trans.GetObject(symbolTableId, OpenMode.ForRead);
                if (table.Has(name))
                {
                    return table[name];
                }

                if (create != null)
                {
                    var record = create();
                    table.UpgradeOpen();
                    var result = table.Add(record);
                    trans.AddNewlyCreatedDBObject(record, true);
                    trans.Commit();
                    return result;
                }
            }

            return defaultValue.Value;
        }

        /// <summary>
        /// Gets layer ID by name. Creates new if not found.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The layer ID.</returns>
        public static ObjectId GetLayerId(string layerName, Database db = null)
        {
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: (db ?? HostApplicationServices.WorkingDatabase).LayerTableId,
                name: layerName,
                create: () => new LayerTableRecord { Name = layerName });
        }

        /// <summary>
        /// Gets all layer IDs.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The layer IDs.</returns>
        public static ObjectId[] GetAllLayerIds(Database db = null)
        {
            return DbHelper.GetSymbolTableRecords((db ?? HostApplicationServices.WorkingDatabase).LayerTableId);
        }

        /// <summary>
        /// Gets all layer names.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The layer names.</returns>
        public static string[] GetAllLayerNames(Database db = null)
        {
            return DbHelper.GetSymbolTableRecordNames((db ?? HostApplicationServices.WorkingDatabase).LayerTableId);
        }

        /// <summary>
        /// Ensures a layer is visible.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        public static void EnsureLayerOn(string layerName)
        {
            var id = DbHelper.GetLayerId(layerName);
            id.QOpenForWrite<LayerTableRecord>(layer =>
            {
                layer.IsFrozen = false;
                layer.IsHidden = false;
                layer.IsOff = false;
            });
        }

        /// <summary>
        /// Gets block table record ID by block name.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The block table ID.</returns>
        public static ObjectId GetBlockId(string blockName, Database db = null)
        {
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: (db ?? HostApplicationServices.WorkingDatabase).BlockTableId,
                name: blockName,
                defaultValue: ObjectId.Null);
        }

        /// <summary>
        /// Gets all block table record IDs.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The object ID array.</returns>
        public static ObjectId[] GetAllBlockIds(Database db = null)
        {
            return DbHelper.GetSymbolTableRecords((db ?? HostApplicationServices.WorkingDatabase).BlockTableId);
        }

        /// <summary>
        /// Gets all block names.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The block name array.</returns>
        public static string[] GetAllBlockNames(Database db = null)
        {
            return DbHelper.GetSymbolTableRecordNames((db ?? HostApplicationServices.WorkingDatabase).BlockTableId);
        }

        /// <summary>
        /// Gets linetype ID by name. Returns the continuous linetype as default if not found.
        /// </summary>
        /// <param name="linetypeName">The linetype name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The linetype ID.</returns>
        public static ObjectId GetLinetypeId(string linetypeName, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.LinetypeTableId,
                name: linetypeName,
                defaultValue: db.ContinuousLinetype);
        }

        /// <summary>
        /// Gets text style ID by name. Returns the current TEXTSTYLE as default if not found.
        /// </summary>
        /// <param name="textStyleName">The text style name.</param>
        /// <param name="createIfNotFound">Whether to create new if not found.</param>
        /// <param name="db">The database.</param>
        /// <returns>The text style ID.</returns>
        public static ObjectId GetTextStyleId(string textStyleName, bool createIfNotFound = false, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.TextStyleTableId,
                name: textStyleName,
                create: () => new TextStyleTableRecord { Name = textStyleName },
                defaultValue: db.Textstyle);
        }

        /// <summary>
        /// Gets dimension style ID by name. Returns the current DIMSTYLE as default if not found.
        /// </summary>
        /// <param name="dimStyleName">The dimension style name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The dimension style ID.</returns>
        public static ObjectId GetDimstyleId(string dimStyleName, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.DimStyleTableId,
                name: dimStyleName,
                defaultValue: db.Dimstyle);
        }

        /// <summary>
        /// Gets a dictionary object.
        /// </summary>
        /// <param name="dictionaryId">The dictionary ID.</param>
        /// <param name="name">The entry name.</param>
        /// <param name="defaultValue">The default value if not found.</param>
        /// <param name="create">The factory method if not found.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId GetDictionaryObject(ObjectId dictionaryId, string name, ObjectId? defaultValue = null, Func<DBObject> create = null)
        {
            using (var trans = dictionaryId.Database.TransactionManager.StartTransaction())
            {
                var dictionary = (DBDictionary)trans.GetObject(dictionaryId, OpenMode.ForRead);
                if (dictionary.Contains(name))
                {
                    return dictionary.GetAt(name);
                }

                if (create != null)
                {
                    var dictObject = create();
                    dictionary.UpgradeOpen();
                    var result = dictionary.SetAt(name, dictObject);
                    trans.AddNewlyCreatedDBObject(dictObject, true);
                    trans.Commit();
                    return result;
                }
            }

            return defaultValue.Value;
        }

        /// <summary>
        /// Gets group ID by name.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId GetGroupId(string groupName, Database db = null)
        {
            return DbHelper.GetDictionaryObject(
                dictionaryId: (db ?? HostApplicationServices.WorkingDatabase).GroupDictionaryId,
                name: groupName,
                defaultValue: ObjectId.Null);
        }

        /// <summary>
        /// Gets group ID by entity ID.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId GetGroupId(ObjectId entityId)
        {
            var groupDict = entityId.Database.GroupDictionaryId.QOpenForRead<DBDictionary>();
            var entity = entityId.QOpenForRead<Entity>();
            try
            {
                return groupDict
                    .Cast<DBDictionaryEntry>()
                    .First(entry => entry.Value.QOpenForRead<Group>().Has(entity))
                    .Value;
            }
            catch
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// Gets all entity IDs in a group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The entity IDs.</returns>
        public static IEnumerable<ObjectId> GetEntityIdsInGroup(ObjectId groupId)
        {
            var group = groupId.QOpenForRead<Group>();
            if (group != null)
            {
                return group.GetAllEntityIds();
            }

            return new ObjectId[0];
        }

        #endregion

        internal static Database GetDatabase(IEnumerable<ObjectId> objectIds)
        {
            return objectIds.Select(id => id.Database).Distinct().Single();
        }
    }
}
