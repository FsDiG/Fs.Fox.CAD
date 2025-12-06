using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
namespace DotNetARX
{
    /// <summary>
    /// LINQ操作类
    /// </summary>
    public static partial class LinqToCAD
    {
        /// <summary>
        /// 获取数据库中类型为T的所有实体(对象打开为读）
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <returns>返回数据库中类型为T的实体</returns>
        public static List<T> GetEntsInDatabase<T>(this Database db) where T : Entity
        {
            return GetEntsInDatabase<T>(db, OpenMode.ForRead, false);
        }

        /// <summary>
        ///  获取数据库中类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回数据库中类型为T的实体</returns>
        public static List<T> GetEntsInDatabase<T>(this Database db, OpenMode mode, bool openErased) where T : Entity
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //声明一个List类的变量，用于返回类型为T为的实体列表
            List<T> ents = new List<T>();
            //获取类型T代表的DXF代码名用于构建选择集过滤器
            string dxfname = RXClass.GetClass(typeof(T)).DxfName;
            //构建选择集过滤器        
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname) };
            SelectionFilter filter = new SelectionFilter(values);
            //选择符合条件的所有实体
            PromptSelectionResult entSelected = ed.SelectAll(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                //循环遍历符合条件的实体
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    //将实体强制转化为T类型的对象
                    //不能将实体直接转化成泛型T，必须首先转换成object类
                    T t = (T)(object)id.GetObject(mode, openErased);
                    ents.Add(t);//将实体添加到返回列表中
                }
            }
            return ents;//返回类型为T为的实体列表
        }

        /// <summary>
        /// 获取用户选择的类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <returns>返回类型为T的实体</returns>
        public static List<T> GetSelection<T>(this Database db) where T : Entity
        {
            return GetSelection<T>(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取用户选择的类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体的打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回类型为T的实体</returns>
        public static List<T> GetSelection<T>(this Database db, OpenMode mode, bool openErased) where T : Entity
        {
            var trans = db.TransactionManager;
            string dxfname = RXClass.GetClass(typeof(T)).DxfName;
            List<T> ents = new List<T>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname) };
            var filter = new SelectionFilter(values);
            var entSelected = ed.GetSelection(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    T t = (T)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(t);
                }
            }
            return ents;
        }

        /// <summary>
        /// 选择窗口中及和窗口四条边界相交的实体，实体类型为T
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="pt1">窗口的一个角点</param>
        /// <param name="pt2">窗口的另一个角点</param>
        /// <returns>返回类型为T的实体</returns>
        public static List<T> SelectCrossingWindow<T>(this Database db, Point3d pt1, Point3d pt2) where T : Entity
        {
            return SelectCrossingWindow<T>(db, pt1, pt2, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 选择窗口中及和窗口四条边界相交的实体，实体类型为T
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="pt1">窗口的一个角</param>
        /// <param name="pt2">窗口的另一个角点</param>
        /// <param name="mode">实体的打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回类型为T的实体</returns>
        public static List<T> SelectCrossingWindow<T>(this Database db, Point3d pt1, Point3d pt2, OpenMode mode, bool openErased) where T : Entity
        {
            var trans = db.TransactionManager;
            string dxfname = RXClass.GetClass(typeof(T)).DxfName;
            List<T> ents = new List<T>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname) };
            var filter = new SelectionFilter(values);
            var entSelected = ed.SelectCrossingWindow(pt1, pt2, filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    T t = (T)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(t);
                }
            }
            return ents;
        }

        /// <summary>
        /// 获取模型空间中类型为T的所有实体(对象打开为读）
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <returns>返回模型空间中类型为T的实体</returns>
        public static List<T> GetEntsInModelSpace<T>(this Database db) where T : Entity
        {
            return GetEntsInModelSpace<T>(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取模型空间中类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回模型空间中类型为T的实体</returns>
        public static List<T> GetEntsInModelSpace<T>(this Database db, OpenMode mode, bool openErased) where T : Entity
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //声明一个List类的变量，用于返回类型为T为的实体列表
            List<T> ents = new List<T>();
            //获取类型T代表的DXF代码名用于构建选择集过滤器
            string dxfname = RXClass.GetClass(typeof(T)).DxfName;
            //构建选择集过滤器        
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname),
                                    new TypedValue((int)DxfCode.LayoutName,"Model")};
            SelectionFilter filter = new SelectionFilter(values);
            //选择符合条件的所有实体
            PromptSelectionResult entSelected = ed.SelectAll(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                //循环遍历符合条件的实体
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    //将实体强制转化为T类型的对象
                    //不能将实体直接转化成泛型T，必须首先转换成object类
                    T t = (T)(object)id.GetObject(mode, openErased);
                    ents.Add(t);//将实体添加到返回列表中
                }
            }
            return ents;//返回类型为T为的实体列表
        }

        /// <summary>
        /// 获取图纸空间中类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <returns>返回图纸空间中类型为T的实体</returns>
        public static List<T> GetEntsInPaperSpace<T>(this Database db) where T : Entity
        {
            return GetEntsInPaperSpace<T>(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取图纸空间中类型为T的所有实体
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回图纸空间中类型为T的实体</returns>
        public static List<T> GetEntsInPaperSpace<T>(this Database db, OpenMode mode, bool openErased) where T : Entity
        {
            var trans = db.TransactionManager;
            string dxfname = RXClass.GetClass(typeof(T)).DxfName;
            List<T> ents = new List<T>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname),
                                    new TypedValue((int)DxfCode.ViewportVisibility,1)};
            //TypedValue[] values = { new TypedValue((int)DxfCode.Start, dxfname),
            //                      new TypedValue((int)DxfCode.LayoutName,"~Model")};
            var filter = new SelectionFilter(values);
            var entSelected = ed.SelectAll(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    T t = (T)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(t);
                }
            }
            return ents;
        }

        /// <summary>
        /// 获取数据库中所有的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回数据库中所有的实体</returns>
        public static List<Entity> GetEntsInDatabase(this Database db)
        {
            return GetEntsInDatabase(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取数据库中所有的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回数据库中所有的实体</returns>
        public static List<Entity> GetEntsInDatabase(this Database db, OpenMode mode, bool openErased)
        {
            var trans = db.TransactionManager;
            //声明一个List类的变量，用于返回所有实体
            List<Entity> ents = new List<Entity>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var entSelected = ed.SelectAll();
            if (entSelected.Status == PromptStatus.OK)
            {
                //循环遍历符合条件的实体
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    Entity ent = (Entity)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(ent);
                }
            }
            return ents;
        }

        /// <summary>
        /// 获取用户选择的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回用户选择的实体</returns>
        public static List<Entity> GetSelection(this Database db)
        {
            return GetSelection(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取用户选择的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回用户选择的实体</returns>
        public static List<Entity> GetSelection(this Database db, OpenMode mode, bool openErased)
        {
            var trans = db.TransactionManager;
            List<Entity> ents = new List<Entity>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var entSelected = ed.GetSelection();
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    Entity ent = (Entity)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(ent);
                }
            }
            return ents;
        }

        /// <summary>
        /// 选择窗口中及和窗口四条边界相交的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="pt1">窗口的一个角点</param>
        /// <param name="pt2">窗口的另一个角点</param>
        /// <returns>返回选择的实体</returns>
        public static List<Entity> SelectCrossingWindow(this Database db, Point3d pt1, Point3d pt2)
        {
            return SelectCrossingWindow(db, pt1, pt2, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 选择窗口中及和窗口四条边界相交的实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="pt1">窗口的一个角点</param>
        /// <param name="pt2">窗口的另一个角点</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回选择的实体</returns>
        public static List<Entity> SelectCrossingWindow(this Database db, Point3d pt1, Point3d pt2, OpenMode mode, bool openErased)
        {
            var trans = db.TransactionManager;
            List<Entity> ents = new List<Entity>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var entSelected = ed.SelectCrossingWindow(pt1, pt2);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    Entity ent = (Entity)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(ent);
                }
            }
            return ents;
        }

        /// <summary>
        /// 获取模型空间中的所有实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回模型空间中的所有实体</returns>
        public static List<Entity> GetEntsInModelSpace(this Database db)
        {
            return GetEntsInModelSpace(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取模型空间中的所有实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回模型空间中的所有实体</returns>
        public static List<Entity> GetEntsInModelSpace(this Database db, OpenMode mode, bool openErased)
        {
            var trans = db.TransactionManager;
            List<Entity> ents = new List<Entity>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = { new TypedValue((int)DxfCode.ViewportVisibility, 0) };
            var filter = new SelectionFilter(values);
            var entSelected = ed.SelectAll(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    Entity ent = (Entity)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(ent);
                }
            }
            return ents;
        }

        /// <summary>
        /// 获取图纸空间中的所有实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回图纸空间中的所有实体</returns>
        public static List<Entity> GetEntsInPaperSpace(this Database db)
        {
            return GetEntsInPaperSpace(db, OpenMode.ForRead, false);
        }

        /// <summary>
        /// 获取图纸空间中的所有实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="mode">实体打开方式</param>
        /// <param name="openErased">是否打开已删除的实体</param>
        /// <returns>返回图纸空间中的所有实体</returns>
        public static List<Entity> GetEntsInPaperSpace(this Database db, OpenMode mode, bool openErased)
        {
            var trans = db.TransactionManager;
            List<Entity> ents = new List<Entity>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] values = { new TypedValue((int)DxfCode.ViewportVisibility, 1) };
            var filter = new SelectionFilter(values);
            var entSelected = ed.SelectAll(filter);
            if (entSelected.Status == PromptStatus.OK)
            {
                foreach (var id in entSelected.Value.GetObjectIds())
                {
                    Entity ent = (Entity)(object)trans.GetObject(id, mode, openErased);
                    ents.Add(ent);
                }
            }
            return ents;
        }
    }
}
