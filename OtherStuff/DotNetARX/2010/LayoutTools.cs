using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
namespace DotNetARX
{
    /// <summary>
    /// 布局操作类
    /// </summary>
    public static class LayoutTools
    {
        /// <summary>
        /// 获取数据库的所有布局
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回所有布局</returns>
        public static List<Layout> GetAllLayouts(this Database db)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            using (OpenCloseTransaction trans = doc.TransactionManager.StartOpenCloseTransaction())
            {
                List<Layout> layouts = new List<Layout>();
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId id in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(id, OpenMode.ForRead);
                    if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                    {
                        Layout layout = (Layout)trans.GetObject(btr.LayoutId, OpenMode.ForRead);
                        layouts.Add(layout);
                    }
                }
                return layouts.OrderBy(layout => layout.TabOrder).ToList();
            }

            #region 原先老版本的写法，新版本不支持id.getobject
            //List<Layout> layouts = new List<Layout>();
            //BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            //foreach (ObjectId id in bt)
            //{
            //    BlockTableRecord btr = (BlockTableRecord)id.GetObject(OpenMode.ForRead);
            //    if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
            //    {
            //        Layout layout = (Layout)btr.LayoutId.GetObject(OpenMode.ForRead);
            //        layouts.Add(layout);
            //    }
            //}
            //return layouts.OrderBy(layout => layout.TabOrder).ToList(); 
            #endregion
        }

        /// <summary>
        /// 用于获得指定布局上的所有实体
        /// </summary>
        /// <param name="layout">布局对象</param>
        /// <param name="bIncludeFirstViewport">是否将第一个视口包含在内</param>
        /// <returns>返回所有实体的Id集合</returns>
        public static ObjectIdCollection GetEntsInLayout(this Layout layout, bool bIncludeFirstViewport)
        {
            ObjectIdCollection entIds = new ObjectIdCollection();
            ObjectId blkDefId = layout.BlockTableRecordId;
            BlockTableRecord btr = blkDefId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            if (btr == null) return null;
            bool bFirstViewport = true;
            foreach (ObjectId entId in btr)
            {
                Viewport vp = entId.GetObject(OpenMode.ForRead) as Viewport;
                if (vp != null && bFirstViewport)
                {
                    if (bIncludeFirstViewport) entIds.Add(entId);
                    bFirstViewport = false;
                }
                else entIds.Add(entId);
                ObjectId dictId = vp.ExtensionDictionary;
                if (dictId.IsValid)
                {
                    DBDictionary dict = (DBDictionary)dictId.GetObject(OpenMode.ForWrite);
                    dict.TreatElementsAsHard = true;
                }
            }
            return entIds;
        }

        /// <summary>
        /// 在指定的图形数据库中寻找特定名称的布局，并且输出该布局中包含的所有实体
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="name">布局名称</param>
        /// <param name="entIds">布局中包含的所有实体的Id集合</param>
        /// <returns>返回布局的Id</returns>
        public static ObjectId GetLayoutId(this Database db, string name, ref ObjectIdCollection entIds)
        {
            ObjectId layoutId = new ObjectId();
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            foreach (ObjectId btrId in bt)
            {
                BlockTableRecord btr = (BlockTableRecord)btrId.GetObject(OpenMode.ForRead);
                if (btr.IsLayout)
                {
                    Layout layout = (Layout)btr.LayoutId.GetObject(OpenMode.ForRead);
                    if (layout.LayoutName.CompareTo(name) == 0)
                    {
                        layoutId = btr.LayoutId;
                        // 获取布局中的所有实体
                        entIds = layout.GetEntsInLayout(true);
                        break;
                    }
                }
            }
            return layoutId;
        }

        /// <summary>
        /// 确保布局中的图纸显示在布局中的中间，而不需要使用缩放命令来显示
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layoutName">布局的名称</param>
        public static void CenterLayoutViewport(this Database db, string layoutName)
        {
            Extents3d ext = db.GetAllEntsExtent();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = btrId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    if (btr.IsLayout)
                    {
                        Layout layout = btr.LayoutId.GetObject(OpenMode.ForRead) as Layout;
                        if (layout.LayoutName.CompareTo(layoutName) == 0)
                        {
                            int vpIndex = 0;
                            ObjectId firstViewportId = new ObjectId();
                            ObjectId secondViewportId = new ObjectId();
                            foreach (ObjectId entId in btr)
                            {
                                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                                if (ent is Viewport)
                                {
                                    if (vpIndex == 0)
                                    {
                                        firstViewportId = entId;
                                        vpIndex++;
                                    }
                                    else if (vpIndex == 1)
                                    {
                                        secondViewportId = entId;
                                    }
                                }
                            }
                            // 布局复制过来之后得到了两个视口，第一个视口与模型空间关联，第二个视口则是在正确的位置上
                            if (firstViewportId.IsValid && secondViewportId.IsValid)
                            {
                                Viewport secondVP = secondViewportId.GetObject(OpenMode.ForWrite) as Viewport;
                                secondVP.ColorIndex = 1;
                                secondVP.Erase();
                                Viewport firstVP = firstViewportId.GetObject(OpenMode.ForWrite) as Viewport;
                                firstVP.CenterPoint = secondVP.CenterPoint;
                                firstVP.Height = secondVP.Height;
                                firstVP.Width = secondVP.Width;
                                firstVP.ColorIndex = 5;
                                Point3d midPt = GeTools.MidPoint(ext.MinPoint, ext.MaxPoint);
                                firstVP.ViewCenter = new Point2d(midPt.X, midPt.Y); ;
                                double xScale = secondVP.Width / ((ext.MaxPoint.X - ext.MinPoint.X) * 1.1);
                                double yScale = secondVP.Height / ((ext.MaxPoint.Y - ext.MinPoint.Y) * 1.1);
                                firstVP.CustomScale = Math.Min(xScale, yScale);
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }
    }
}
