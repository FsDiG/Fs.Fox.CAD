using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
namespace DotNetARX
{
    /// <summary>
    /// 组操作类
    /// </summary>
    public static class GroupTools
    {
        /// <summary>
        /// 创建组
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="groupName">组名</param>
        /// <param name="ids">要加入实体的ObjectId集合</param>
        /// <returns>返回组的Id</returns>
        public static ObjectId CreateGroup(this Database db, string groupName, ObjectIdList ids)
        {
            //打开当前数据库的组字典对象
            DBDictionary groupDict=(DBDictionary)db.GroupDictionaryId.GetObject(OpenMode.ForRead);
            //如果已经存在指定名称的组，则返回
            if (groupDict.Contains(groupName)) return ObjectId.Null;
            //新建一个组对象
            Group group=new Group(groupName, true);
            groupDict.UpgradeOpen(); //切换组字典为写的状态
            //在组字典中加入新创建的组对象，并指定它的搜索关键字为groupName
            groupDict.SetAt(groupName, group);
            //通知事务处理完成组对象的加入
            db.TransactionManager.AddNewlyCreatedDBObject(group, true);
            group.Append(ids); // 在组对象中加入实体对象
            groupDict.DowngradeOpen(); //为了安全，将组字典切换成写
            return group.ObjectId; //返回组的Id
        }

        /// <summary>
        /// 向已有组中添加对象
        /// </summary>
        /// <param name="groupId">组对象的Id</param>
        /// <param name="entIds">要加入到组中的实体的ObjectId集合</param>
        public static void AppendEntityToGroup(this ObjectId groupId, ObjectIdList entIds)
        {
            //打开组对象
            Group group=groupId.GetObject(OpenMode.ForRead) as Group;
            if (group == null) return; //如果不是组对象，则返回
            group.UpgradeOpen(); // 切换组为写状态
            group.Append(entIds); // 在组中添加实体对象
            group.DowngradeOpen(); //为了安全，将组切换成写
        }

        /// <summary>
        /// 获取实体对象所在的组
        /// </summary>
        /// <param name="entId">实体的Id</param>
        /// <returns>返回实体所在的组</returns>
        public static IEnumerable<ObjectId> GetGroups(this ObjectId entId)
        {
            DBObject obj=entId.GetObject(OpenMode.ForRead);//打开实体
            //获取实体对象所拥有的永久反应器（组也属性永久反应器之一）
            ObjectIdCollection ids=obj.GetPersistentReactorIds();
            if (ids != null && ids.Count > 0)
            {
                //对实体的永久反应器进行筛选，只返回组
                var groupIds=from ObjectId id in ids
                             //获取永久反应器对象
                             let reactor = id.GetObject(OpenMode.ForRead)
                             //筛选条件设置为Group类
                             where reactor is Group
                             select id;
                if (groupIds.Count() > 0)//如果实体属于组
                    return groupIds;//返回实体所在组的ObjectId
            }
            return null;//没有组，则返回空值
        }

        /// <summary>
        /// 删除组
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="groupName">组名</param>
        public static void RemoveGroup(this Database db, string groupName)
        {
            //获取数据库的组字典对象
            DBDictionary groupDict=(DBDictionary)db.GroupDictionaryId.GetObject(OpenMode.ForRead);
            //在组字典中搜索关键字为groupName的组对象
            if (groupDict.Contains(groupName)) //如果找到名为groupName的组
            {
                groupDict.UpgradeOpen();
                groupDict.Remove(groupName);  //从组字典中删除组
                groupDict.DowngradeOpen();
            }
        }

    }
}