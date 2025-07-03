using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 文档操作类
    /// </summary>
    public static class DocumentTools
    {
        /// <summary>
        /// 确定文档中是否有未保存的修改。
        /// </summary>
        /// <param name="doc">文档对象</param>
        /// <returns>如果有未保存的修改，则返回true，否则返回false。</returns>
        public static bool Saved(this Document doc)
        {
            //获取DBMOD系统变量，它用来指示图形的修改状态
            object dbmod=Application.GetSystemVariable("DBMOD");
            //若DBMOD不为0，则表示图形已被修改但还未被保存
            if (Convert.ToInt16(dbmod) != 0) return true;
            else return false;//图形没有未保存的修改
        }

        /// <summary>
        /// 保存已有文档
        /// </summary>
        /// <param name="doc">文档对象</param>
        public static void Save(this Document doc)
        {
            doc.Database.SaveAs(doc.Name, DwgVersion.Current);
        }
    }
}
