using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 多线样式操作类
    /// </summary>
    public static class MLineTools
    {
        /// <summary>
        /// 创建一个新的多线样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">多线样式名</param>
        /// <param name="elements">加入到多线样式的元素集合</param>
        /// <returns>返回加入的多线样式的Id</returns>
        public static ObjectId CreateMLineStyle(this Database db, string styleName, List<MlineStyleElement> elements)
        {
            //打开当前数据库的多线样式字典对象
            DBDictionary dict=(DBDictionary)db.MLStyleDictionaryId.GetObject(OpenMode.ForRead);
            if (dict.Contains(styleName)) // 如果已经存在指定名称的多线样式
                return (ObjectId)dict[styleName]; // 返回该多线样式的Id
            MlineStyle mStyle=new MlineStyle();// 创建一个多线样式对象
            mStyle.Name = styleName;//设置多线样式的名称
            //为多线样式添加新的元素
            foreach (var element in elements)
            {
                mStyle.Elements.Add(element, true);
            }
            dict.UpgradeOpen(); //切换多线字典为写
            //在多线样式字典中加入新创建的多线样式对象，并指定搜索关键字为styleName
            dict.SetAt(styleName, mStyle);
            //通知事务处理完成多线样式对象的加入
            db.TransactionManager.AddNewlyCreatedDBObject(mStyle, true);
            dict.DowngradeOpen(); //为了安全，将多线样式字典切换成读
            return mStyle.ObjectId; // 返回该多线样式的Id
        }

        /// <summary>
        /// 删除多线样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">多线样式名</param>
        public static void RemoveMLineStyle(this Database db, string styleName)
        {
            //打开当前数据库的多线样式字典对象
            DBDictionary dict=(DBDictionary)db.MLStyleDictionaryId.GetObject(OpenMode.ForRead);
            //在多线样式字典中搜索关键字为styleName的多线样式对象
            if (dict.Contains(styleName))
            {
                dict.UpgradeOpen();
                //如果找到名为styleName的多线样式，则从多线样式字典中删除
                dict.Remove(styleName);
                dict.DowngradeOpen();
            }
        }
    }
}
