using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

/*
 * NFox.Cad命名空间，主要为跟cad相关的类。*/
namespace NFox.Cad
{
    /// <summary>
    /// 事务管理类
    /// </summary>
    public class DBTransaction : IDisposable
    {
        private bool _commit = false;

        //属性

#region Properties

        /// <summary>
        /// 文档对象是否存在
        /// </summary>
        public bool HasDocument
        { get; private set; }

        /// <summary>
        /// 当前文档对象
        /// </summary>
        public Document Document
        { get; private set; }

        /// <summary>
        /// 当前编辑对象
        /// </summary>
        public Editor Editor
        { get; private set; }

        /// <summary>
        /// 当前数据库
        /// </summary>
        public Database Database
        { get; private set; }

#endregion Properties

        //事务相关

#region Trans

        private Transaction _tr;

        /// <summary>
        /// 事务,默认Transaction
        /// </summary>
        internal Transaction Transaction
        {
            get { return _tr ?? (_tr = Database.TransactionManager.StartTransaction()); }
        }

        /// <summary>
        /// 开始一个Transaction
        /// </summary>
        public void Start()
        {
            if (_tr == null)
                _tr = Database.TransactionManager.StartTransaction();
        }

        /// <summary>
        /// 开始一个OpenCloseTransaction
        /// </summary>
        public void StartOpenClose()
        {
            if (_tr == null)
                _tr = Database.TransactionManager.StartOpenCloseTransaction();
        }

        /// <summary>
        /// 事务初始化,并只读方式打开块表
        /// </summary>
        private void Initialize(bool hasDocument)
        {
            if (HasDocument = hasDocument)
            {
                Document = Application.DocumentManager.GetDocument(Database);
                Editor = Document.Editor;
            }
        }

        /// <summary>
        /// 创建当前活动文档的事务(默认提交)
        /// </summary>
        public DBTransaction()
        {
            Database = HostApplicationServices.WorkingDatabase;
            Initialize(true);
        }

        /// <summary>
        /// 创建当前活动文档的事务
        /// </summary>
        /// <param name="commit">是否提交,<see langword="true"/>为提交，<see langword="false"/>为不提交</param>
        public DBTransaction(bool commit)
        {
            _commit = !commit;
            Database = HostApplicationServices.WorkingDatabase;
            Initialize(true);
        }

        /// <summary>
        /// 创建指定数据库的事务,一般用于临时数据库(默认提交)
        /// </summary>
        /// <param name="database">数据库</param>
        public DBTransaction(Database database)
        {
            Database = database;
            Initialize(false);
        }

        /// <summary>
        /// 创建指定数据库的事务
        /// </summary>
        /// <param name="database">数据库</param>
        /// <param name="commit">是否提交,<see langword="true"/>为提交，<see langword="false"/>为不提交</param>
        public DBTransaction(Database database, bool commit)
        {
            _commit = !commit;
            Database = database;
            Initialize(false);
        }

        /// <summary>
        /// 创建临时数据库的事务,并读入指定的文档(默认提交)
        /// </summary>
        /// <param name="fileName">文件路径</param>
        public DBTransaction(string fileName)
        {
            Database = new Database(false, true);
            Database.ReadDwgFile(fileName, FileShare.Read, true, null);

            Database.CloseInput(true);
            Initialize(false);
        }

        /// <summary>
        /// 创建临时数据库的事务,并读入指定的文档
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="commit">是否提交,<see langword="true"/>为提交，<see langword="false"/>为不提交</param>
        public DBTransaction(string fileName, bool commit)
        {
            _commit = !commit;
            Database = new Database(false, true);
            Database.ReadDwgFile(fileName, FileShare.Read, true, null);
            Database.CloseInput(true);
            Initialize(false);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        void IDisposable.Dispose()
        {
            Commit();
            Transaction.Dispose();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            if (!_commit)
            {
                Transaction.Commit();
                _commit = true;
            }
        }

        /// <summary>
        /// 撤销事务
        /// </summary>
        public void Abort()
        {
            Transaction.Abort();
        }
        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="entity">实体对象</param>
        public void Flush(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            entity.RecordGraphicsModified(true);
            Transaction.TransactionManager.QueueForGraphicsFlush();
        }
        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="id">实体id</param>
        public void Flush(ObjectId id)
        {
            Flush(GetObject<Entity>(id));
        }

        /// <summary>
        /// 隐式转换为Transaction
        /// </summary>
        /// <param name="tr">事务管理器</param>
        /// <returns>事务管理器</returns>
        public static implicit operator Transaction(DBTransaction tr)
        {
            return tr ?? tr.Transaction;
        }
        /// <summary>
        /// 转到到事务管理器
        /// </summary>
        /// <returns>事务管理器</returns>
        public Transaction ToTransaction()
        {
            return Transaction;
        }

        #endregion Trans

        //对象获取

        #region GetObject

        private T GetObject<T>(T obj, ObjectId id) where T : DBObject
        {
            //return obj ?? (_ = GetObject<T>(id));
            return obj ?? GetObject<T>(id);
        }

        /// <summary>
        /// 从句柄字符串中获取Id
        /// </summary>
        /// <param name="handleString">句柄字符串</param>
        /// <returns>对象id，ObjectId</returns>
        public ObjectId GetObjectId(string handleString)
        {
            long l = Convert.ToInt64(handleString, 16);
            Handle handle = new Handle(l);
            return Database.GetObjectId(false, handle, 0);
        }

        /// <summary>
        ///  获取对象
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开已删除对象</param>
        /// <returns>DBObject对象</returns>
        public DBObject GetObject(ObjectId id, OpenMode mode, bool openErased)
        {
            return Transaction.GetObject(id, mode, openErased);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="mode">打开模式</param>
        /// <returns>DBObject对象</returns>
        public DBObject GetObject(ObjectId id, OpenMode mode)
        {
            return Transaction.GetObject(id, mode, false);
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns>DBObject对象</returns>
        public DBObject GetObject(ObjectId id)
        {
            return Transaction.GetObject(id, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取对象
        /// <see cref="GetObject{T}(ObjectId)"/>
        /// </summary>
        /// <typeparam name="T">DBObject</typeparam>
        /// <param name="id">ObjectId</param>
        /// <param name="mode">打开模式</param>
        /// <param name="openErased">打开已删除对象</param>
        /// <returns>返回指定的类型的对象</returns>
        public T GetObject<T>(ObjectId id, OpenMode mode, bool openErased) where T : DBObject
        {
            return (T)Transaction.GetObject(id, mode, openErased);
        }

        /// <summary>
        /// <para>获取对象,这里的泛型是为了一次到位的返回所要得到的对象类型。</para>
        /// </summary>
        /// <example> <para>如果原来是需要这么写才能得到Curve：</para>
        /// <code><![CDATA[var longCurve = tr.GetObject<Curve>(ObjectId) as Curve;]]></code>
        /// <para>但是利用这个泛型函数可以直接得到 Curve.</para>
        /// <code><![CDATA[var longCurve = tr.GetObject<Curve>(ObjectId);]]></code></example>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="id">ObjectId</param>
        /// <returns>返回指定的类型的对象</returns>
        public T GetObject<T>(ObjectId id) where T : DBObject
        {
            return (T)Transaction.GetObject(id, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取对象
        /// <see cref="GetObject{T}(ObjectId)"/>
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="id">ObjectId</param>
        /// <param name="mode">打开模式</param>
        /// <returns>返回指定的类型的对象</returns>
        public T GetObject<T>(ObjectId id, OpenMode mode) where T : DBObject
        {
            return (T)Transaction.GetObject(id, mode, false);
        }

#endregion GetObject

        //符号表

#region SymbolTable

        /// <summary>
        /// 获取符号表
        /// </summary>
        /// <typeparam name="TTable">符号表泛型</typeparam>
        /// <typeparam name="TRecord">符号表记录泛型</typeparam>
        /// <param name="table">符号表的引用</param>
        /// <param name="tableId">符号表的id</param>
        /// <returns>符号表</returns>
        private SymbolTableCollection<TTable, TRecord> GetSymbolTable<TTable, TRecord>(ref SymbolTableCollection<TTable, TRecord> table, ObjectId tableId)
            where TTable : SymbolTable
            where TRecord : SymbolTableRecord, new()
        {
            return table ?? (table = new SymbolTableCollection<TTable, TRecord>(this, tableId));
        }

        //块表

#region BlockTable

        private SymbolTableCollection<BlockTable, BlockTableRecord> _bt = null;

        /// <summary>
        /// 块表
        /// </summary>
        public SymbolTableCollection<BlockTable, BlockTableRecord> BlockTable
        {
            get { return GetSymbolTable(ref _bt, Database.BlockTableId); }
        }

#endregion BlockTable

        //层表

#region LayerTable

        private SymbolTableCollection<LayerTable, LayerTableRecord> _lt = null;

        /// <summary>
        /// 层表
        /// </summary>
        public SymbolTableCollection<LayerTable, LayerTableRecord> LayerTable
        {
            get { return GetSymbolTable(ref _lt, Database.LayerTableId); }
        }

#endregion LayerTable

        //文字样式表

#region TextStyleTable

        private SymbolTableCollection<TextStyleTable, TextStyleTableRecord> _tst = null;

        /// <summary>
        /// 文字样式表
        /// </summary>
        public SymbolTableCollection<TextStyleTable, TextStyleTableRecord> TextStyleTable
        {
            get { return GetSymbolTable(ref _tst, Database.TextStyleTableId); }
        }

#endregion TextStyleTable

        //注册应用程序

#region RegAppTable

        private SymbolTableCollection<RegAppTable, RegAppTableRecord> _rat = null;
        /// <summary>
        /// 注册应用程序表
        /// </summary>
        public SymbolTableCollection<RegAppTable, RegAppTableRecord> RegAppTable
        {
            get { return GetSymbolTable(ref _rat, Database.RegAppTableId); }
        }

#endregion RegAppTable

        //标注样式表

#region DimStyleTable

        private SymbolTableCollection<DimStyleTable, DimStyleTableRecord> _dst = null;

        /// <summary>
        /// 标注样式表
        /// </summary>
        public SymbolTableCollection<DimStyleTable, DimStyleTableRecord> DimStyleTable
        {
            get { return GetSymbolTable(ref _dst, Database.DimStyleTableId); }
        }

#endregion DimStyleTable

        //线型表

#region LinetypeTable

        private SymbolTableCollection<LinetypeTable, LinetypeTableRecord> _ltt = null;

        /// <summary>
        /// 线型表
        /// </summary>
        public SymbolTableCollection<LinetypeTable, LinetypeTableRecord> LinetypeTable
        {
            get { return GetSymbolTable(ref _ltt, Database.LinetypeTableId); }
        }
        /// <summary>
        /// 根据线宽创建选择及
        /// </summary>
        /// <param name="lineWeight">线宽</param>
        /// <returns>选择集</returns>
        public SelectionSet SelectByLineWeight(LineWeight lineWeight)
        {
            OpFilter filter = new OpEqual(370, lineWeight);

            var lays =
                LayerTable
                .GetRecords()
                .Where(ltr => ltr.LineWeight == lineWeight)
                .Select(ltr => ltr.Name)
                .ToArray();

            if (lays.Length > 0)
            {
                filter =
                    new OpOr
                    {
                        filter,
                        new OpAnd
                        {
                            { 8, string.Join(",", lays) },
                            { 370, LineWeight.ByLayer }
                        }
                    };
            }

            PromptSelectionResult res = Editor.SelectAll(filter);
            return res.Value;
        }

#endregion LinetypeTable

        //用户坐标系

#region UcsTable

        private SymbolTableCollection<UcsTable, UcsTableRecord> _ut = null;
        /// <summary>
        /// 用户坐标系表
        /// </summary>
        public SymbolTableCollection<UcsTable, UcsTableRecord> UcsTable
        {
            get { return GetSymbolTable(ref _ut, Database.UcsTableId); }
        }

#endregion UcsTable

        //视图

#region ViewTable

        private SymbolTableCollection<ViewTable, ViewTableRecord> _vt = null;
        /// <summary>
        /// 视图表
        /// </summary>
        public SymbolTableCollection<ViewTable, ViewTableRecord> ViewTable
        {
            get { return GetSymbolTable(ref _vt, Database.ViewTableId); }
        }

#endregion ViewTable

        //视口

#region ViewportTable

        private SymbolTableCollection<ViewportTable, ViewportTableRecord> _vpt = null;
        /// <summary>
        /// 视口表
        /// </summary>
        public SymbolTableCollection<ViewportTable, ViewportTableRecord> ViewportTable
        {
            get { return GetSymbolTable(ref _vpt, Database.ViewportTableId); }
        }

#endregion ViewportTable

#endregion SymbolTable

        //字典

#region Dictionary

        private DBDictionary _root;
        /// <summary>
        /// 获取有名对象词典
        /// </summary>
        public DBDictionary RootDictionary
        {
            get
            {
                return _root ?? (_root = GetObject<DBDictionary>(Database.NamedObjectsDictionaryId));
            }
        }

        //保存和获取数据

#region Value

        /// <summary>
        /// 保存数据到字典
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="dict">字典</param>
        /// <param name="key">键值</param>
        public void SetToDictionary(DBDictionary dict, string key, DBObject value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }
            using (dict.UpgradeOpenAndRun())
            {
                if (dict.Contains(key))
                    dict.Remove(key);
                dict.SetAt(Transaction, key, value);
            }
        }

        /// <summary>
        /// 从字典中获取数据
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键值</param>
        /// <returns>DBObject对象</returns>
        public DBObject GetFromDictionary(DBDictionary dict, string key)
        {
            if (dict != null)
            {
                if (dict.Contains(key))
                {
                    DBObject obj = GetObject(dict.GetAt(key));
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">字典名称数组的可变参数</param>
        /// <returns>字典对象</returns>
        public DBDictionary GetSubDictionary(bool createSubDictionary, params string[] dictNames)
        {
            return RootDictionary.GetSubDictionary(Transaction, createSubDictionary, dictNames);
        }

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="dict">字典对象</param>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">字典名称数组的可变参数</param>
        /// <returns>字典对象</returns>
        public DBDictionary GetSubDictionary(DBDictionary dict, bool createSubDictionary, params string[] dictNames)
        {
            return dict.GetSubDictionary(Transaction, createSubDictionary, dictNames);
        }

        /// <summary>
        /// 获取子字典
        /// </summary>
        /// <param name="DBobj">对象</param>
        /// <param name="createSubDictionary">是否创建子字典</param>
        /// <param name="dictNames">字典名称数组的可变参数</param>
        /// <returns>字典对象</returns>
        public DBDictionary GetSubDictionary(DBObject DBobj, bool createSubDictionary, params string[] dictNames)
        {
            return DBobj.GetSubDictionary(Transaction, createSubDictionary, dictNames);
        }

#endregion Value

        //保存和获取扩展数据

#region XRecord

        /// <summary>
        /// 保存扩展数据到字典
        /// </summary>
        /// <param name="rb">扩展数据</param>
        /// <param name="dict">字典</param>
        /// <param name="key">键值</param>
        public void SetXRecord(DBDictionary dict, string key, ResultBuffer rb)
        {
            using (var data = new Xrecord { Data = rb })
            {
                SetToDictionary(dict, key, data);
            } 
            
        }

        /// <summary>
        /// 从字典中获取扩展数据
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键值</param>
        /// <returns>扩展数据</returns>
        public ResultBuffer GetXRecord(DBDictionary dict, string key)
        {
            Xrecord rec = GetFromDictionary(dict, key) as Xrecord;
            if (rec != null)
                return rec.Data;
            return null;
        }

#endregion XRecord

        //编组字典

#region GroupDictionary

        private DBDictionary _groupDictionary = null;

        /// <summary>
        /// 编组字典
        /// </summary>
        public DBDictionary GroupDictionary
        {
            get { return GetObject(_groupDictionary, Database.GroupDictionaryId); }
        }

        /// <summary>
        /// 添加编组
        /// </summary>
        /// <param name="name">组名</param>
        /// <param name="ids">实体Id集合</param>
        /// <returns>编组Id</returns>
        public ObjectId AddGroup(string name, ObjectIdCollection ids)
        {
            if (GroupDictionary.Contains(name))
            {
                return ObjectId.Null;
            }
            else
            {
                using (GroupDictionary.UpgradeOpenAndRun())
                {
                    Group g = new Group();
                    g.Append(ids);
                    GroupDictionary.SetAt(name, g);
                    Transaction.AddNewlyCreatedDBObject(g, true);
                    return g.ObjectId;
                }
            }
        }

        /// <summary>
        /// 添加编组
        /// </summary>
        /// <param name="name">组名</param>
        /// <param name="ids">实体Id集合</param>
        /// <returns>编组Id</returns>
        public ObjectId AddGroup(string name, IEnumerable<ObjectId> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            if (GroupDictionary.Contains(name))
            {
                return ObjectId.Null;
            }
            else
            {
                using (GroupDictionary.UpgradeOpenAndRun())
                {
                    Group g = new Group();
                    foreach (ObjectId id in ids)
                        g.Append(id);
                    GroupDictionary.SetAt(name, g);
                    Transaction.AddNewlyCreatedDBObject(g, true);
                    return g.ObjectId;
                }
            }
        }

        /// <summary>
        /// 按选择条件获取编组集合
        /// </summary>
        /// <param name="func">选择条件，过滤函数</param>
        /// <example><![CDATA[var groups = GetGroups(g => g.NumEntities < 2);]]></example>
        /// <returns>编组集合</returns>
        public IEnumerable<Group> GetGroups(Func<Group, bool> func)
        {
            return
                GroupDictionary
                .GetAllObjects<Group>(Transaction)
                .Where(func);
        }

        /// <summary>
        /// 返回实体的所在编组的集合
        /// </summary>
        /// <param name="ent">图元实体</param>
        /// <returns>编组集合</returns>
        public IEnumerable<Group> GetGroups(Entity ent)
        {
            if (ent == null)
            {
                throw new ArgumentNullException(nameof(ent));
            }
            return
                ent.GetPersistentReactorIds()
                .Cast<ObjectId>()
                .Select(id => GetObject(id))
                .OfType<Group>();
        }

        /// <summary>
        /// 移除所有的空组
        /// </summary>
        /// <returns>被移除编组的名称集合</returns>
        public List<string> RemoveNullGroup()
        {
            var groups = GetGroups(g => g.NumEntities < 2);
            List<string> names = new List<string>();
            foreach (Group g in groups)
            {
                g.UpgradeOpen();
                names.Add(g.Name);
                g.Erase();
            }
            return names;
        }

        /// <summary>
        /// 移除所有空组
        /// </summary>
        /// <param name="func">过滤条件，过滤要删除的组名的规则函数</param>
        /// <example>RemoveNullGroup(g => g.StartsWith("hah"))</example>
        /// <returns>被移除编组的名称集合</returns>
        public List<string> RemoveNullGroup(Func<string, bool> func)
        {
            var groups = GetGroups(g => g.NumEntities < 2);
            List<string> names = new List<string>();
            foreach (Group g in groups)
            {
                if (func(g.Name))
                {
                    names.Add(g.Name);
                    using (g.UpgradeOpenAndRun())
                    {
                        g.Erase();
                    }
                }
            }
            return names;
        }

#endregion GroupDictionary

        //多重引线样式字典

#region MLeaderStyleDictionary

        private DBDictionary _mLeaderStyleDictionary = null;
        /// <summary>
        /// 多重引线样式字典
        /// </summary>
        public DBDictionary MLeaderStyleDictionary
        {
            get { return GetObject(_mLeaderStyleDictionary, Database.MLeaderStyleDictionaryId); }
        }

#endregion MLeaderStyleDictionary

        //多线样式字典Id

#region MLStyleDictionary

        private DBDictionary _mLStyleDictionary = null;
        /// <summary>
        /// 多线样式字典Id
        /// </summary>
        public DBDictionary MLStyleDictionary
        {
            get { return GetObject(_mLStyleDictionary, Database.MLStyleDictionaryId); }
        }

#endregion MLStyleDictionary

        //材质字典Id

#region MaterialDictionary

        private DBDictionary _materialDictionary = null;
        /// <summary>
        /// 材质字典Id
        /// </summary>
        public DBDictionary MaterialDictionary
        {
            get { return GetObject(_materialDictionary, Database.MaterialDictionaryId); }
        }

#endregion MaterialDictionary

        //表格样式字典Id

#region TableStyleDictionary

        private DBDictionary _tableStyleDictionary = null;
        /// <summary>
        /// 表格样式字典
        /// </summary>
        public DBDictionary TableStyleDictionary
        {
            get { return GetObject(_tableStyleDictionary, Database.TableStyleDictionaryId); }
        }

#endregion TableStyleDictionary

#region VisualStyleDictionary

        private DBDictionary _visualStyleDictionary = null;
        /// <summary>
        /// VisualStyleDictionary
        /// </summary>
        public DBDictionary VisualStyleDictionary
        {
            get { return GetObject(_visualStyleDictionary, Database.VisualStyleDictionaryId); }
        }

#endregion VisualStyleDictionary

        //颜色字典Id

#region ColorDictionary

        private DBDictionary _colorDictionary = null;
        /// <summary>
        /// 颜色字典Id
        /// </summary>
        public DBDictionary ColorDictionary
        {
            get { return GetObject(_colorDictionary, Database.ColorDictionaryId); }
        }

#endregion ColorDictionary

        //打印设置字典Id

#region PlotSettingsDictionary

        private DBDictionary _plotSettingsDictionary = null;
        /// <summary>
        /// 打印设置字典Id
        /// </summary>
        public DBDictionary PlotSettingsDictionary
        {
            get { return GetObject(_plotSettingsDictionary, Database.PlotSettingsDictionaryId); }
        }

#endregion PlotSettingsDictionary

        //打印样式表名字典Id

#region PlotStyleNameDictionary

        private DBDictionary _plotStyleNameDictionary = null;
        /// <summary>
        /// 打印样式表名字典Id
        /// </summary>
        public DBDictionary PlotStyleNameDictionary
        {
            get { return GetObject(_plotStyleNameDictionary, Database.PlotStyleNameDictionaryId); }
        }

#endregion PlotStyleNameDictionary

        //布局字典Id

#region LayoutDictionary

        private DBDictionary _layoutDictionary = null;
        /// <summary>
        /// 布局字典Id
        /// </summary>
        public DBDictionary LayoutDictionary
        {
            get { return GetObject(_layoutDictionary, Database.LayoutDictionaryId); }
        }

#endregion LayoutDictionary

#endregion Dictionary

        //块表记录

#region BlockTableRecord

        /// <summary>
        /// 获取块表记录
        /// </summary>
        /// <param name="id">块表记录Id</param>
        /// <param name="openmode">打开模式</param>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenBlockTableRecord(ObjectId id, OpenMode openmode)
        {
            return BlockTable.GetRecord(id, openmode);
        }

        /// <summary>
        /// 获取块表记录，读模式
        /// </summary>
        /// <param name="id">块表记录Id</param>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenBlockTableRecord(ObjectId id)
        {
            return BlockTable.GetRecord(id);
        }

        /// <summary>
        /// 获取块表记录
        /// </summary>
        /// <param name="name">块表记录名</param>
        /// <param name="openmode">打开模式</param>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenBlockTableRecord(string name, OpenMode openmode)
        {
            return BlockTable.GetRecord(name, openmode);
        }

        /// <summary>
        /// 获取块表记录，读模式
        /// </summary>
        /// <param name="name">块表记录名</param>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenBlockTableRecord(string name)
        {
            return BlockTable.GetRecord(name);
        }

        /// <summary>
        /// 获取当前块表记录
        /// </summary>
        /// <param name="openmode">打开模式</param>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenCurrentSpace(OpenMode openmode)
        {
            return BlockTable.GetRecord(Database.CurrentSpaceId, openmode);
        }

        /// <summary>
        /// 获取当前块表记录,读模式
        /// </summary>
        /// <returns>块表记录</returns>
        public BlockTableRecord OpenCurrentSpace()
        {
            return BlockTable.GetRecord(Database.CurrentSpaceId);
        }

        /// <summary>
        /// 获取图纸空间
        /// </summary>
        /// <param name="openmode">打开模式</param>
        /// <returns>图纸空间</returns>
        public BlockTableRecord OpenPaperSpace(OpenMode openmode)
        {
            return
                GetObject<BlockTableRecord>(
                    BlockTable.AcTable[BlockTableRecord.PaperSpace],
                    openmode);
        }

        /// <summary>
        /// 获取图纸空间，读模式
        /// </summary>
        /// <returns>图纸空间</returns>
        public BlockTableRecord OpenPaperSpace()
        {
            return OpenPaperSpace(OpenMode.ForRead);
        }

        /// <summary>
        /// 获取模型空间
        /// </summary>
        /// <param name="openmode">打开模式</param>
        /// <returns>模型空间</returns>
        public BlockTableRecord OpenModelSpace(OpenMode openmode)
        {
            return
                GetObject<BlockTableRecord>(
                    BlockTable.AcTable[BlockTableRecord.ModelSpace],
                    openmode);
        }

        /// <summary>
        /// 获取模型空间，读模式
        /// </summary>
        /// <returns>模型空间</returns>
        public BlockTableRecord OpenModelSpace()
        {
            return OpenModelSpace(OpenMode.ForRead);
        }

#endregion BlockTableRecord

        //添加实体

#region Add Entity

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="entity">图元实体</param>
        /// <returns>实体Id</returns>
        public ObjectId AddEntity(BlockTableRecord btr, Entity entity)
        {
            return btr.AddEntity(Transaction, entity);
        }

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="ents">图元实体集合</param>
        /// <returns>实体Id集合</returns>
        public List<ObjectId> AddEntity(BlockTableRecord btr, DBObjectCollection ents)
        {
            return btr.AddEntity(Transaction, ents);
        }

        /// <summary>添加实体集合</summary>
        /// <typeparam name="T">图元实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="ents">图元实体集合</param>
        /// <returns>实体Id集合</returns>
        public List<ObjectId> AddEntity<T>(BlockTableRecord btr, IEnumerable<T> ents) where T : Entity
        {
            return btr.AddEntity(Transaction, ents);
        }
        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="ents">图元实体数组</param>
        /// <returns>ObjectId对象列表</returns>
        public List<ObjectId> AddEntity(BlockTableRecord btr, params Entity [] ents)
        {
            return btr.AddEntity(Transaction, ents.Cast<Entity>());
        }
#endregion Add Entity

        //删除实体

#region Erase Entity

        /// <summary>
        /// 删除实体集合
        /// </summary>
        /// <param name="ids">实体Id集合</param>
        /// <returns>成功返回true，失败返回false</returns>
        public bool Erase(ObjectIdCollection ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            try
            {
                foreach (ObjectId id in ids)
                {
                    DBObject obj = GetObject(id, OpenMode.ForWrite);
                    obj.Erase(true);
                }
                return true;
            }
            catch (ObjectDisposedException e)
            { 
                Editor.WriteMessage(e.Message); 
            }
            return false;
        }

        /// <summary>
        /// 删除实体集合
        /// </summary>
        /// <param name="ids">实体Id集合</param>
        /// <returns>成功返回true，失败返回false</returns>
        public bool Erase(IEnumerable<ObjectId> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            try
            {
                foreach (ObjectId id in ids)
                {
                    DBObject obj = GetObject(id, OpenMode.ForWrite);
                    obj.Erase(true);
                }
                return true;
            }
            catch (ObjectDisposedException e)
            {
                Editor.WriteMessage(e.Message);
            }
            return false;
        }

        /// <summary>
        /// 删除实体集合
        /// </summary>
        /// <typeparam name="T">DBObject</typeparam>
        /// <param name="ents">实体Id集合</param>
        /// <returns>
        /// 成功返回true，失败返回false
        /// </returns>
        public bool Erase<T>(IEnumerable<T> ents) where T : DBObject
        {
            if (ents == null)
            {
                throw new ArgumentNullException(nameof(ents));
            }
            try
            {
                foreach (T ent in ents)
                {
                    using (ent.UpgradeOpenAndRun())
                    {
                        ent.Erase(true);
                    }
                }
                return true;
            }
            catch (ObjectDisposedException e)
            {
                Editor.WriteMessage(e.Message);
            }
            return false;
        }

        /// <summary>
        /// 删除块表记录里的所有对象
        /// </summary>
        /// <param name="btr">块表记录</param>
        public void Clear(BlockTableRecord btr)
        {
            if (btr == null)
            {
                throw new ArgumentNullException(nameof(btr));
            }
            foreach (ObjectId id in btr)
            {
                DBObject obj = GetObject(id, OpenMode.ForWrite);
                obj.Erase();
            }
        }

#endregion Erase Entity

        //块参照

#region BlockReference

        //定义块
#region AddBlock
        /// <summary>
        /// 定义块
        /// </summary>
        /// <param name="name">块名</param>
        /// <param name="ents">图元列表</param>
        /// <returns>块id</returns>
        public ObjectId AddBlock(string name, params Entity[] ents)
        {
            return BlockTable.Add(name,ents);
        }

        /// <summary>
        /// 定义块
        /// </summary>
        /// <param name="name">块名</param>
        /// <param name="ids">对象id列表</param>
        /// <returns>块id</returns>
        public ObjectId AddBlock(string name, params ObjectId[] ids)
        {
            List<Entity> ents = new List<Entity>();
            foreach (var item in ids)
            {
                ents.Add(GetObject<Entity>(item));
            }
            return BlockTable.Add(name, ents);

            //var btr = BlockTable.Add(name);
            //foreach (var item in ids)
            //{
            //    AddEntity(btr, GetObject<Entity>(item));
            //}

            //return btr.ObjectId;
        }

        /// <summary>
        /// 定义块
        /// </summary>
        /// <param name="name">块名</param>
        /// <param name="collection">图元集合</param>
        /// <returns>块id</returns>
        public ObjectId AddBlock(string name, DBObjectCollection collection)
        {
            //var btr = BlockTable.Add(name);
            //foreach (Entity item in collection)
            //{
            //    AddEntity(btr, item);
            //}
            //return btr.ObjectId;
            return BlockTable.Add(name, collection.Cast<Entity>());
        }

        /// <summary>
        /// 定义块
        /// </summary>
        /// <param name="name">块名</param>
        /// <param name="collection">对象id集合</param>
        /// <returns>块id</returns>
        public ObjectId AddBlock(string name, ObjectIdCollection collection)
        {
            //var btr = BlockTable.Add(name);
            //foreach (ObjectId item in collection)
            //{
            //    AddEntity(btr, GetObject<Entity>(item));
            //}
            //return btr.ObjectId;
            return AddBlock(name, collection.Cast<ObjectId>().ToArray());
        }
        /// <summary>
        /// 定义属性块
        /// </summary>
        /// <param name="name">块名</param>
        /// <param name="ents">图元集合</param>
        /// <param name="attdef">属性集合</param>
        /// <returns>块id</returns>
        public ObjectId AddBlock(string name, IEnumerable<Entity> ents, IEnumerable<AttributeDefinition> attdef)
        {
            return BlockTable.Add(name, ents, attdef);
        }
        #endregion

        //插入块参照

        #region InsertBlock
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockId">块定义id</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="range">旋转角度</param>
        /// <param name="atts">属性标记和值的字典</param>
        /// <returns>块参照id</returns>
        public ObjectId InsertBlock(Point3d position, ObjectId blockId, Scale3d scale = default, double range = default, Dictionary<string,string> atts = default)
        {

            using (var blockref = new BlockReference(position, blockId)
            {
                ScaleFactors = scale,
                Rotation = range
            })
            {
                var objid = AddEntity(OpenCurrentSpace(), blockref);
                if (atts != default)
                {
                    var btr = GetObject<BlockTableRecord>(blockref.BlockTableRecord);
                    if (btr.HasAttributeDefinitions)
                    {
                        var attdefs =
                        btr.GetEntities<AttributeDefinition>(Transaction)
                        .Where(attdef => !(attdef.Constant || attdef.Invisible));
                        foreach (var attdef in attdefs)
                        {
                            using (AttributeReference attref = new AttributeReference())
                            {
                                attref.SetAttributeFromBlock(attdef, blockref.BlockTransform);
                                attref.Position = attdef.Position;
                                attref.AdjustAlignment(Database);
                                if (atts.ContainsKey(attdef.Tag))
                                {
                                    attref.TextString = atts[attdef.Tag];
                                }

                                blockref.AttributeCollection.AppendAttribute(attref);
                                Transaction.AddNewlyCreatedDBObject(attref, true);
                            }

                        }
                    }
                }
                //return AddEntity(OpenCurrentSpace(), blockref);
                return objid;
            }
        }
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="name">块名</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="range">旋转角度</param>
        /// <param name="atts">属性标记和值的字典</param>
        /// <returns>块参照id</returns>
        public ObjectId InsertBlock(Point3d position, string name, Scale3d scale = default, double range = default, Dictionary<string, string> atts = default)
        {
            return InsertBlock(position, BlockTable[name], scale, range, atts);
        }



#endregion




        //添加属性到块参照
#region AppendAttribToBlock

        /// <summary>
        /// 添加属性到块参照
        /// </summary>
        /// <param name="blkrefid">块参照Id</param>
        /// <param name="atts">属性集合</param>
        /// <returns>属性定义和属性参照对照表</returns>
        public Dictionary<AttributeDefinition, AttributeReference>
            AppendAttribToBlock(ObjectId blkrefid, List<string> atts)
        {
            BlockReference blkref = GetObject<BlockReference>(blkrefid, OpenMode.ForWrite);
            return AppendAttribToBlock(blkref, atts);
        }

        /// <summary>
        /// 添加属性到块参照
        /// </summary>
        /// <param name="blkref">块参照</param>
        /// <param name="atts">属性集合</param>
        /// <returns>属性定义和属性参照对照表</returns>
        public Dictionary<AttributeDefinition, AttributeReference>
            AppendAttribToBlock(BlockReference blkref, List<string> atts)
        {
            if (blkref == null)
            {
                throw new ArgumentNullException(nameof(blkref));
            }
            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }
            var blkdef = GetObject<BlockTableRecord>(blkref.BlockTableRecord);

            int i = 0;
            if (blkdef.HasAttributeDefinitions)
            {
                var attribs =
                    new Dictionary<AttributeDefinition, AttributeReference>();

                var attdefs =
                    blkdef.GetEntities<AttributeDefinition>(Transaction)
                    .Where(attdef => !(attdef.Constant || attdef.Invisible));

                foreach (var attdef in attdefs)
                {
                    AttributeReference attref = new AttributeReference();
                    attref.SetAttributeFromBlock(attdef, blkref.BlockTransform);
                    if (i < atts.Count)
                        attref.TextString = atts[i];
                    else
                        attref.TextString = attdef.TextString;
                    i++;
                    blkref.AttributeCollection.AppendAttribute(attref);
                    Transaction.AddNewlyCreatedDBObject(attref, true);
                    attribs.Add(attdef, attref);
                }
                return attribs;
            }
            return null;
        }

        /// <summary>
        /// 添加属性到块参照
        /// </summary>
        /// <param name="blkrefid">块参照Id</param>
        /// <param name="atts">属性集合</param>
        /// <returns>属性定义和属性参照对照表</returns>
        public Dictionary<AttributeDefinition, AttributeReference>
            AppendAttribToBlock(ObjectId blkrefid, List<AttributeDefinition> atts)
        {
            BlockReference blkref = GetObject<BlockReference>(blkrefid, OpenMode.ForWrite);
            return AppendAttribToBlock(blkref, atts);
        }

        /// <summary>
        /// 添加属性到块参照
        /// </summary>
        /// <param name="blkref">块参照</param>
        /// <param name="atts">属性集合</param>
        /// <returns>属性定义和属性参照对照表</returns>
        public Dictionary<AttributeDefinition, AttributeReference>
            AppendAttribToBlock(BlockReference blkref, List<AttributeDefinition> atts)
        {
            if (blkref == null)
            {
                throw new ArgumentNullException(nameof(blkref));
            }
            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }
            var attribs =
                new Dictionary<AttributeDefinition, AttributeReference>();
            for (int i = 0; i < atts.Count; i++)
            {
                AttributeDefinition attdef = atts[i];
                using (AttributeReference attref = new AttributeReference())
                {
                    attref.SetAttributeFromBlock(attdef, blkref.BlockTransform);
                    attref.TextString = attdef.TextString;
                    blkref.AttributeCollection.AppendAttribute(attref);
                    Transaction.AddNewlyCreatedDBObject(attref, true);
                }
            }
            return attribs;
        }

#endregion AppendAttribToBlock

        //动态添加块参照

#region InsertBlockRef
            
        /// <summary>
        /// 块参照拖拽
        /// </summary>
        /// <param name="bref">块参照对象</param>
        /// <param name="atts">属性</param>
        /// <returns>拖拽成功返回 <see langword="true"/>，反之，<see langword="false"/></returns>
        public bool DragBlockRef(BlockReference bref, List<string> atts)
        {
            BlockRefJig jig = new BlockRefJig(Editor, bref, AppendAttribToBlock(bref, atts));
            PromptResult res = jig.DragByMove();
            if (res.Status == PromptStatus.OK)
            {
                res = jig.DragByRotation();
                if (res.Status == PromptStatus.OK)
                    return true;
            }
            bref.Erase();
            return false;
        }
        /// <summary>
        /// 块参照拖拽
        /// </summary>
        /// <param name="bref">块参照对象</param>
        /// <returns>拖拽成功返回 <see langword="true"/>，反之，<see langword="false"/></returns>
        public bool DragBlockRef(BlockReference bref)
        {
            return DragBlockRef(bref, new List<string>());
        }

        /// <summary>
        /// 动态添加块参照
        /// </summary>
        /// <param name="bdefid">块定义Id</param>
        /// <param name="atts">属性集合</param>
        /// <returns>块参照Id</returns>
        public ObjectId InsertBlockRef(ObjectId bdefid, List<string> atts)
        {
            BlockTableRecord btr = OpenCurrentSpace();
            using (BlockReference blkref = new BlockReference(Point3d.Origin, bdefid))
            {
                ObjectId id = AddEntity(btr, blkref);
                return DragBlockRef(blkref, atts) ? id : ObjectId.Null;
            }
        }

        /// <summary>
        /// 动态添加块参照
        /// </summary>
        /// <param name="bdefid">块定义Id</param>
        /// <returns>块参照Id</returns>
        public ObjectId InsertBlockRef(ObjectId bdefid)
        {
            return InsertBlockRef(bdefid, new List<string>());
        }

        /// <summary>
        /// 动态添加块参照
        /// </summary>
        /// <param name="name">块定义名</param>
        /// <param name="attribs">属性集合</param>
        /// <returns>块参照Id</returns>
        public ObjectId InsertBlockRef(string name, List<string> attribs)
        {
            return InsertBlockRef(BlockTable[name], attribs);
        }

        /// <summary>
        /// 动态添加块参照
        /// </summary>
        /// <param name="name">块定义名</param>
        /// <returns>块参照Id</returns>
        public ObjectId InsertBlockRef(string name)
        {
            return InsertBlockRef(name, new List<string>());
        }

#endregion InsertBlockRef

        //裁剪块参照

#region ClipBlockRef

        private const string filterDictName = "ACAD_FILTER";
        private const string spatialName = "SPATIAL";

        /// <summary>
        /// 裁剪块参照
        /// </summary>
        /// <param name="bref">块参照</param>
        /// <param name="pt3ds">裁剪多边形点表</param>
        public void ClipBlockRef(BlockReference bref, IEnumerable<Point3d> pt3ds)
        {
            if (bref == null)
            {
                throw new ArgumentNullException(nameof(bref));
            }
            if (pt3ds == null)
            {
                throw new ArgumentNullException(nameof(pt3ds));
            }
            Matrix3d mat = bref.BlockTransform.Inverse();
            var pts =
                pt3ds
                .Select(p => p.TransformBy(mat))
                .Select(p => new Point2d(p.X, p.Y))
                .ToCollection();

            SpatialFilterDefinition sfd = new SpatialFilterDefinition(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
            using (SpatialFilter sf = new SpatialFilter { Definition = sfd })
            {
                var dict = GetSubDictionary(bref, true, filterDictName);
                SetToDictionary(dict, spatialName, sf);
            }
        }

        /// <summary>
        /// 裁剪块参照
        /// </summary>
        /// <param name="bref">块参照</param>
        /// <param name="pt1">第一角点</param>
        /// <param name="pt2">第二角点</param>
        public void ClipBlockRef(BlockReference bref, Point3d pt1, Point3d pt2)
        {
            if (bref == null)
            {
                throw new ArgumentNullException(nameof(bref));
            }
            Matrix3d mat = bref.BlockTransform.Inverse();
            pt1 = pt1.TransformBy(mat);
            pt2 = pt2.TransformBy(mat);
            Point2dCollection pts =
                new Point2dCollection
                {
                    new Point2d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y)),
                    new Point2d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y))
                };

            SpatialFilterDefinition sfd = new SpatialFilterDefinition(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
            using (SpatialFilter sf = new SpatialFilter { Definition = sfd })
            {
                var dict = GetSubDictionary(bref, true, filterDictName);
                SetToDictionary(dict, spatialName, sf);
            }
        }



        #endregion ClipBlockRef

        #endregion BlockReference
    }
}