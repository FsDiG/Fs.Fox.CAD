using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace NFox.Cad
{
    /// <summary>
    /// 词典扩展类
    /// </summary>
    public static class DBDictionaryEx
    {
        /// <summary>
        /// 获取词典里的全部对象
        /// </summary>
        /// <typeparam name="T">对象类型的泛型</typeparam>
        /// <param name="dict">词典</param>
        /// <param name="tr">事务</param>
        /// <returns>对象迭代器</returns>
        public static IEnumerable<T> GetAllObjects<T>(this DBDictionary dict, Transaction tr) where T : DBObject
        {
            foreach (DBDictionaryEntry e in dict)
            {
                yield return
                    tr.GetObject(e.Value, OpenMode.ForRead) as T;
            }
        }

        /// <summary>
        /// 获取词典里指定对象
        /// </summary>
        /// <typeparam name="T">对象类型的泛型</typeparam>
        /// <param name="dict">词典</param>
        /// <param name="tr">事务</param>
        /// <param name="key">指定的键值</param>
        /// <returns>T 类型的对象</returns>
        public static T GetAt<T>(this DBDictionary dict, Transaction tr, string key) where T : DBObject
        {
            if (dict.Contains(key))
            {
                ObjectId id = dict.GetAt(key);
                if (!id.IsNull)
                {
                    return tr.GetObject(id, OpenMode.ForRead) as T;
                }
            }
            return null;
        }

        /// <summary>
        /// 设定词典里的键值对
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="dict">词典</param>
        /// <param name="tr">事务</param>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        public static void SetAt<T>(this DBDictionary dict, Transaction tr, string key, T obj) where T : DBObject
        {
            using (dict.UpgradeOpenAndRun())
            {
                dict.SetAt(key, obj);
                tr.AddNewlyCreatedDBObject(obj, true);
            }
        }

        #region GetSubDictionary

        /// <summary>
        /// 获取词典名称，莫名其妙的函数！！！！
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="key"></param>
        /// <returns>词典名列表</returns>
        internal static List<string> GetDictNames(string[] keys, out string key)
        {
            List<string> dictNames = new List<string>(keys);
            if (dictNames.Count > 0)
            {
                int index = dictNames.Count - 1;
                key = dictNames[index];
                dictNames.RemoveAt(index);
            }
            else
            {
                key = "*";
            }
            return dictNames;
        }

        //internal static DBDictionary GetSubDictionary(this DBDictionary dict, bool createSubDictionary, IEnumerable<string> dictNames)
        //{
        //    Database db = dict.Database;

        //    DBDictionary subdict;
        //    if (createSubDictionary)
        //    {
        //        foreach (string name in dictNames)
        //        {
        //            if (dict.Contains(name))
        //            {
        //                subdict = dict.GetAt(name).Open<DBDictionary>();
        //                dict.Dispose();
        //                dict = subdict;
        //            }
        //            else
        //            {
        //                using (dict.UpgradeOpenAndRun())
        //                {
        //                    subdict = new DBDictionary();
        //                    dict.SetAt(name, subdict);
        //                    db.AddDBObject(subdict);
        //                }
        //                dict.Dispose();
        //                dict = subdict;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (string name in dictNames)
        //        {
        //            if (dict.Contains(name))
        //            {
        //                subdict = dict.GetAt(name).Open<DBDictionary>();
        //                dict.Dispose();
        //                dict = subdict;
        //            }
        //            else
        //                return null;
        //        }
        //    }

        //    return dict;
        //}

        //public static DBDictionary GetSubDictionary(this DBDictionary dict, bool createSubDictionary, params string[] dictNames)
        //{
        //    return
        //        GetSubDictionary(
        //            dict,
        //            createSubDictionary,
        //            (IEnumerable<string>)dictNames);
        //}

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="dict">根字典</param>
        /// <param name="tr">事务</param>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">键值列表</param>
        /// <returns>字典</returns>
        internal static DBDictionary GetSubDictionary(this DBDictionary dict, Transaction tr, bool createSubDictionary, IEnumerable<string> dictNames)
        {
            if (createSubDictionary)
            {
                using (dict.UpgradeOpenAndRun())
                    dict.TreatElementsAsHard = true;

                foreach (string name in dictNames)
                {
                    if (dict.Contains(name))
                    {
                        dict = dict.GetAt(name).GetObject<DBDictionary>(tr);
                    }
                    else
                    {
                        DBDictionary subDict = new DBDictionary();
                        dict.SetAt(tr, name, subDict);
                        dict = subDict;
                        dict.TreatElementsAsHard = true;
                    }
                }
            }
            else
            {
                foreach (string name in dictNames)
                {
                    if (dict.Contains(name))
                        dict = dict.GetAt(name).GetObject<DBDictionary>(tr);
                    else
                        return null;
                }
            }

            return dict;
        }

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="dict">根字典</param>
        /// <param name="tr">事务管理器</param>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">键值列表</param>
        /// <returns>字典</returns>
        public static DBDictionary GetSubDictionary(this DBDictionary dict, Transaction tr, bool createSubDictionary, params string[] dictNames)
        {
            if (dictNames.Length == 0)
            {
                throw new System.Exception("空的键值列表");
            }

            return
                GetSubDictionary(
                    dict,
                    tr,
                    createSubDictionary,
                    (IEnumerable<string>)dictNames);
        }

        /// <summary>
        /// 获取扩展词典
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="tr">事务</param>
        /// <returns>字典</returns>
        public static DBDictionary GetXDictionary(this DBObject obj, Transaction tr)
        {
            ObjectId id = obj.ExtensionDictionary;
            if (id.IsNull)
            {
                using (obj.UpgradeOpenAndRun())
                    obj.CreateExtensionDictionary();
                id = obj.ExtensionDictionary;
            }
            return id.GetObject<DBDictionary>(tr);
        }

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="tr">事务</param>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">键值列表</param>
        /// <returns>字典</returns>
        public static DBDictionary GetSubDictionary(this DBObject obj, Transaction tr, bool createSubDictionary, params string[] dictNames)
        {
            return obj.GetXDictionary(tr).GetSubDictionary(tr, createSubDictionary, dictNames);
        }

#endregion GetSubDictionary

        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <param name="colTypes">原数据类型的字典</param>
        /// <param name="content">表元素（二维数组）</param>
        /// <returns>数据表</returns>
        public static DataTable CreateDataTable(Dictionary<string, CellType> colTypes, object[,] content)
        {
            DataTable table = new DataTable();
            foreach (var t in colTypes)
                table.AppendColumn(t.Value, t.Key);
            var ncol = colTypes.Count;
            var nrow = content.GetLength(0);
            var types = new CellType[ncol];
            colTypes.Values.CopyTo(types, 0);
            for (int i = 0; i < nrow; i++)
            {
                DataCellCollection row = new DataCellCollection();
                for (int j = 0; j < ncol; j++)
                {
                    var cell = new DataCell();
                    cell.SetValue(types[j], content[i, j]);
                    row.Add(cell);
                }
                table.AppendRow(row, true);
            }
            return table;
        }

        /// <summary>
        /// 设定单元格数据
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <param name="type">类型</param>
        /// <param name="value">数据</param>
        public static void SetValue(this DataCell cell, CellType type, object value)
        {
            switch (type)
            {
                case CellType.Bool:
                    cell.SetBool((bool)value);
                    break;

                case CellType.CharPtr:
                    cell.SetString((string)value);
                    break;

                case CellType.Integer:
                    cell.SetInteger((int)value);
                    break;

                case CellType.Double:
                    cell.SetDouble((double)value);
                    break;

                case CellType.ObjectId:
                    cell.SetObjectId((ObjectId)value);
                    break;

                case CellType.Point:
                    cell.SetPoint((Point3d)value);
                    break;

                case CellType.Vector:
                    cell.SetVector((Vector3d)value);
                    break;

                case CellType.HardOwnerId:
                    cell.SetHardOwnershipId((ObjectId)value);
                    break;

                case CellType.HardPtrId:
                    cell.SetHardPointerId((ObjectId)value);
                    break;

                case CellType.SoftOwnerId:
                    cell.SetSoftOwnershipId((ObjectId)value);
                    break;

                case CellType.SoftPtrId:
                    cell.SetSoftPointerId((ObjectId)value);
                    break;
            }
        }
        
    }
}