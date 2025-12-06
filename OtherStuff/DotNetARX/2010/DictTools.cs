using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 字典操作类，包括扩展字典与有名对象字典
    /// </summary>
    public static class DictTools
    {
        /// <summary>
        /// 添加扩展记录
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录名称</param>
        /// <param name="values">扩展记录的内容</param>
        /// <returns>返回添加的扩展记录的Id</returns>
        public static ObjectId AddXrecord(this ObjectId id, string searchKey, TypedValueList values)
        {
            DBObject obj=id.GetObject(OpenMode.ForRead);//打开对象
            // 判断对象是否已经拥有扩展字典,若无扩展字典，则
            if (obj.ExtensionDictionary.IsNull)
            {
                obj.UpgradeOpen(); // 切换对象为写的状态
                obj.CreateExtensionDictionary();// 为对象创建扩展字典
                obj.DowngradeOpen(); // 为了安全，将对象切换成读的状态
            }
            // 打开对象的扩展字典
            DBDictionary dict=(DBDictionary)obj.ExtensionDictionary.GetObject(OpenMode.ForRead);
            // 如果扩展字典中已包含指定的扩展记录对象，则返回
            if (dict.Contains(searchKey))
                return ObjectId.Null;
            Xrecord xrec=new Xrecord();// 为对象新建一个扩展记录
            xrec.Data = values;// 指定扩展记录的内容
            dict.UpgradeOpen(); // 将扩展字典切换成写的状态
            //在扩展字典中加入新建的扩展记录，并指定它的搜索关键字
            ObjectId idXrec = dict.SetAt(searchKey, xrec);
            id.Database.TransactionManager.AddNewlyCreatedDBObject(xrec, true);
            dict.DowngradeOpen(); // 为了安全，将扩展字典切换成读的状态            
            return idXrec; // 返回添加的扩展记录的的Id
        }

        /// <summary>
        /// 获取扩展记录
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录名称</param>
        /// <returns>返回扩展记录的内容</returns>
        public static TypedValueList GetXrecord(this ObjectId id, string searchKey)
        {
            DBObject obj=id.GetObject(OpenMode.ForRead);
            //获取对象的扩展字典
            ObjectId dictId=obj.ExtensionDictionary;
            if (dictId.IsNull) return null;//若对象没有扩展字典，则返回
            DBDictionary dict=(DBDictionary)dictId.GetObject(OpenMode.ForRead);
            //在扩展字典中搜索指定关键字的扩展记录，如果没找到则返回
            if (!dict.Contains(searchKey)) return null;
            // 获取扩展记录的Id
            ObjectId xrecordId=dict.GetAt(searchKey);
            //打开扩展记录并获取扩展记录的内容
            Xrecord xrecord=(Xrecord)xrecordId.GetObject(OpenMode.ForRead);
            return xrecord.Data; // 返回扩展记录的内容
        }

        /// <summary>
        /// 添加有名对象字典项
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="searchKey">有名对象字典项的名称</param>
        /// <returns>返回添加的有名对象字典项的Id</returns>
        public static ObjectId AddNamedDictionary(this Database db, string searchKey)
        {
            ObjectId id=ObjectId.Null; // 存储添加的命名字典项的Id
            //打开数据库的有名对象字典
            DBDictionary dicts=(DBDictionary)db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead);
            if (!dicts.Contains(searchKey)) // 如果不存在指定关键字的字典项
            {
                DBDictionary dict=new DBDictionary(); //新建字典项 
                dicts.UpgradeOpen(); // 切换有名对象字典为写
                id = dicts.SetAt(searchKey, dict);//设置新建字典项的搜索关键字
                dicts.DowngradeOpen(); // 为了安全，将有名对象字典切换成读的状态
                //将新建的字典项添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(dict, true);
            }
            return id; // 返回添加的字典项的Id
        }
    }
}
