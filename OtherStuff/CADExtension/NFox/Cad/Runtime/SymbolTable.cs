using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace NFox.Cad
{
    /// <summary>
    /// 符号表类
    /// </summary>
    /// <typeparam name="TTable">符号表</typeparam>
    /// <typeparam name="TRecord">符号表记录</typeparam>
    public class SymbolTableCollection<TTable, TRecord> : IEnumerable<ObjectId>
        where TTable : SymbolTable
        where TRecord : SymbolTableRecord, new()
    {
        #region prop

        /// <summary>
        /// 事务管理器
        /// </summary>
        internal DBTransaction Trans
        { get; private set; }

        /// <summary>
        /// 数据库
        /// </summary>
        internal Database Database
        {
            get { return Trans.Database; }
        }

        /// <summary>
        /// 符号表
        /// </summary>
        public TTable AcTable
        { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tr">事务管理器</param>
        /// <param name="tableId">符号表的对象id</param>
        internal SymbolTableCollection(DBTransaction tr, ObjectId tableId)
        {
            Trans = tr;
            AcTable = Trans.GetObject(tableId, OpenMode.ForRead) as TTable;
        }
        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="key">对象名</param>
        /// <returns>对象id</returns>
        public ObjectId this[string key]
        {
            get
            {
                if (Has(key))
                    return AcTable[key];
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 判断是否存在符号表
        /// </summary>
        /// <param name="key">符号表名</param>
        /// <returns>存在返回 <see langword="true"/>, 不存在返回 <see langword="false"/></returns>
        public bool Has(string key)
        {
            return AcTable.Has(key);
        }

        #endregion prop

        #region Add

        /// <summary>
        /// 添加符号表记录
        /// </summary>
        /// <param name="record">符号表记录</param>
        /// <returns>对象id</returns>
        private ObjectId Add(TRecord record)
        {
            using (AcTable.UpgradeOpenAndRun())
            {
                ObjectId id = AcTable.Add(record);
                Trans.Transaction.AddNewlyCreatedDBObject(record, true);
                return id;
            }
        }

        /// <summary>
        /// 添加符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <param name="action">符号表记录处理函数的无返回值委托</param>
        /// <returns>对象id</returns>
        public ObjectId Add(string name, Action<TRecord> action)
        {
            ObjectId id = this[name];
            if (id.IsNull)
            {
                TRecord record = new TRecord();
                record.Name = name;
                id = Add(record);
                if (action != null)
                    action(record);
            }
            return id;
        }

        /// <summary>
        /// 添加符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <returns>符号表记录对象</returns>
        public TRecord Add(string name)
        {
            TRecord record = GetRecord(name);
            if (record == null)
            {
                record = new TRecord();
                record.Name = name;
                Add(record);
            }
            return record;
        }

        #endregion Add

        #region Remove

        /// <summary>
        /// 删除符号表记录
        /// </summary>
        /// <param name="record">符号表记录对象</param>
        private void Remove(TRecord record)
        {
            using (record.UpgradeOpenAndRun())
                record.Erase();
        }

        /// <summary>
        /// 删除符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        public void Remove(string name)
        {
            TRecord record = GetRecord(name);
            if (record != null)
                Remove(record);
        }

        /// <summary>
        /// 删除符号表记录
        /// </summary>
        /// <param name="id">符号表记录对象id</param>
        public void Remove(ObjectId id)
        {
            TRecord record = GetRecord(id);
            if (record != null)
                Remove(record);
        }

        #endregion Remove

        #region GetRecord

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="id">符号表记录的id</param>
        /// <param name="openMode">打开模式</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(ObjectId id, OpenMode openMode)
        {
            if (id.IsNull)
                return null;
            else
                return Trans.GetObject(id, openMode) as TRecord;
        }

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="id">符号表记录的id</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(ObjectId id)
        {
            return GetRecord(id, OpenMode.ForRead);
        }

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <param name="openMode">打开模式</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(string name, OpenMode openMode)
        {
            return GetRecord(this[name], openMode);
        }

        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <param name="name">符号表记录名</param>
        /// <returns>符号表记录</returns>
        public TRecord GetRecord(string name)
        {
            return GetRecord(name, OpenMode.ForRead);
        }

        /// <summary>
        /// 从源数据库拷贝符号表记录
        /// </summary>
        /// <param name="table">符号表</param>
        /// <param name="name">符号表记录名</param>
        /// <param name="over">是否覆盖，<see langword="true"/> 为覆盖，<see langword="false"/> 为不覆盖</param>
        /// <returns>对象id</returns>
        public ObjectId GetRecordFrom(SymbolTableCollection<TTable, TRecord> table, string name, bool over)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            ObjectId rid = this[name];
            bool has = rid != ObjectId.Null;

            if ((has && over) || !has)
            {
                ObjectId id = table[name];
                using (IdMapping idm = new IdMapping())
                {
                    using (ObjectIdCollection ids = new ObjectIdCollection { id })
                    {
                        table.Database.WblockCloneObjects(ids, AcTable.Id, idm, DuplicateRecordCloning.Replace, false);
                    }
                    rid = idm[id].Value;
                }
            }
            return rid;
        }

        /// <summary>
        /// 从文件拷贝符号表记录
        /// </summary>
        /// <param name="tableSelector">符号表过滤器</param>
        /// <param name="fileName">文件名</param>
        /// <param name="name">符号表记录名</param>
        /// <param name="over">是否覆盖，<see langword="true"/> 为覆盖，<see langword="false"/> 为不覆盖</param>
        /// <returns>对象id</returns>
        internal ObjectId GetRecordFrom(Func<DBTransaction, SymbolTableCollection<TTable, TRecord>> tableSelector, string fileName, string name, bool over)
        {
            using (var tr = new DBTransaction(fileName))
            {
                return GetRecordFrom(tableSelector(tr), name, over);
            }
        }

        #endregion GetRecord

        #region IEnumerable<ObjectId> 成员
        /// <summary>
        /// 获取符号表记录
        /// </summary>
        /// <returns>符号表记录迭代器</returns>
        public IEnumerable<TRecord> GetRecords()
        {
            return this.Select(id => GetRecord(id));
        }
        /// <summary>
        /// 获取对象id迭代器
        /// </summary>
        /// <returns>对象id迭代器</returns>
        public IEnumerator<ObjectId> GetEnumerator()
        {
            foreach (var id in AcTable)
                yield return id;
        }

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable 成员

        #endregion IEnumerable<ObjectId> 成员
    }
}