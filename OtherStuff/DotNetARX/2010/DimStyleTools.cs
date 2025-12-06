using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 标注样式操作类
    /// </summary>
    public static class DimStyleTools
    {
        /// <summary>
        /// 创建一个新的标注样式
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="styleName">标注样式名</param>
        /// <returns>返回新建的标注样式的Id</returns>
        public static ObjectId AddDimStyle(this Database db, string styleName)
        {
            //打开标注样式表
            DimStyleTable dst=(DimStyleTable)db.DimStyleTableId.GetObject(OpenMode.ForRead);
            if (!dst.Has(styleName))//如果不存在名为styleName的标注样式，则新建一个标注样式
            {
                //定义一个新的标注样式表记录
                DimStyleTableRecord dstr=new DimStyleTableRecord();
                dstr.Name = styleName;//设置标注样式名
                dst.UpgradeOpen();//切换标注样式表的状态为写以添加新的标注样式
                dst.Add(dstr);//将标注样式表记录的信息添加到标注样式表中
                //把标注式表记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(dstr, true);
                dst.DowngradeOpen();//为了安全，将标注样式表的状态切换为读
            }
            return dst[styleName];//返回新添加的标注样式表记录的ObjectId
        }
    }
}
