using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 视图操作类
    /// </summary>
    public static class ViewTableTools
    {
        /// <summary>
        /// 获取WCS到DCS的转换矩阵
        /// </summary>
        /// <param name="vtr">视图（ViewTableRecord）或视口（ViewportTableRecord）</param>
        /// <returns></returns>
        public static Matrix3d Wcs2Dcs(this AbstractViewTableRecord vtr)
        {
            //获取视图或视口平面到世界坐标系转换矩阵
            Matrix3d mat = Matrix3d.PlaneToWorld(vtr.ViewDirection);
            //将平移过的视口或视图回复到原始状态所需要的矩阵
            mat = Matrix3d.Displacement(vtr.Target - Point3d.Origin) * mat;
            //将旋转过的视口或视图回复到原始状态所需要的矩阵
            mat = Matrix3d.Rotation(-vtr.ViewTwist, vtr.ViewDirection, vtr.Target) * mat;
            return mat.Inverse();//将矩阵进行转置
        }

        /// <summary>
        /// 视图缩放
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="ptMin">要显示区域的左下角点</param>
        /// <param name="ptMax">要显示区域的右上角点</param>
        /// <param name="ptCenter">要显示区域的中心点</param>
        /// <param name="factor">缩放比例</param>
        public static void Zoom(this Editor ed, Point3d ptMin, Point3d ptMax, Point3d ptCenter, double factor)
        {
            Extents3d extents;
            Document doc = ed.Document;
            Database db = doc.Database;
            int cvport = Convert.ToInt32(Application.GetSystemVariable("CVPORT"));
            if (db.TileMode == true)
            {
                if (ptMin.Equals(Point3d.Origin) == true && ptMax.Equals(Point3d.Origin) == true)
                {
                    ptMin = db.Extmin;
                    ptMax = db.Extmax;
                }
            }
            else
            {
                if (cvport == 1)
                {
                    if (ptMin.Equals(Point3d.Origin) == true && ptMax.Equals(Point3d.Origin) == true)
                    {
                        ptMin = db.Pextmin;
                        ptMax = db.Pextmax;
                    }
                }
                else
                {
                    if (ptMin.Equals(Point3d.Origin) == true && ptMax.Equals(Point3d.Origin) == true)
                    {
                        ptMin = db.Extmin;
                        ptMax = db.Extmax;
                    }
                }
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ViewTableRecord view = ed.GetCurrentView();
                Matrix3d matWCS2DCS = view.Wcs2Dcs();
                if (ptCenter.DistanceTo(Point3d.Origin) != 0)
                {
                    ptMin = new Point3d(ptCenter.X - view.Width / 2, ptCenter.Y - view.Height / 2, 0);
                    ptMax = new Point3d(ptCenter.X + view.Width / 2, ptCenter.Y + view.Height / 2, 0);
                }
                using (Line line = new Line(ptMin, ptMax))
                {
                    extents = new Extents3d(line.GeometricExtents.MinPoint, line.GeometricExtents.MaxPoint);
                }
                double viewRatio = view.Width / view.Height;
                extents.TransformBy(matWCS2DCS);
                double width, height;
                Point2d newCenter;
                if (ptCenter.DistanceTo(Point3d.Origin) != 0)
                {
                    width = view.Width;
                    height = view.Height;
                    if (factor == 0)
                    {
                        ptCenter = ptCenter.TransformBy(matWCS2DCS);
                    }
                    newCenter = new Point2d(ptCenter.X, ptCenter.Y);
                }
                else
                {
                    width = extents.MaxPoint.X - extents.MinPoint.X;
                    height = extents.MaxPoint.Y - extents.MinPoint.Y;
                    newCenter = new Point2d((extents.MaxPoint.X + extents.MinPoint.X) * 0.5, (extents.MaxPoint.Y + extents.MinPoint.Y) * 0.5);
                }
                if (width > height * viewRatio) height = width / viewRatio;
                if (factor != 0)
                {
                    view.Height = height * factor;
                    view.Width = width * factor;
                }
                view.CenterPoint = newCenter;
                ed.SetCurrentView(view);
                trans.Commit();
            }
        }

        /// <summary>
        /// 比例缩放视图
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="scale">缩放比例</param>
        public static void ZoomScaled(this Editor ed, double scale)
        {
            //得到当前视图
            ViewTableRecord view = ed.GetCurrentView();
            //修改视图的宽度和高度
            view.Width /= scale;
            view.Height /= scale;
            ed.SetCurrentView(view);//更新当前视图
        }

        /// <summary>
        /// 窗口缩放视图
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="pt1">窗口的角点</param>
        /// <param name="pt2">窗口的角点</param>
        public static void ZoomWindow(this Editor ed, Point3d pt1, Point3d pt2)
        {
            //创建一临时的直线用于获取两点表示的范围
            using (Line line = new Line(pt1, pt2))
            {
                //获取两点表示的范围
                Extents3d extents = new Extents3d(line.GeometricExtents.MinPoint, line.GeometricExtents.MaxPoint);
                //获取范围内的最小值点及最大值点
                Point2d minPt = new Point2d(extents.MinPoint.X, extents.MinPoint.Y);
                Point2d maxPt = new Point2d(extents.MaxPoint.X, extents.MaxPoint.Y);
                //得到当前视图
                ViewTableRecord view = ed.GetCurrentView();
                //设置视图的中心点、高度和宽度
                view.CenterPoint = minPt + (maxPt - minPt) / 2;
                view.Height = maxPt.Y - minPt.Y;
                view.Width = maxPt.X - minPt.X;
                ed.SetCurrentView(view);//更新当前视图
            }
        }

        /// <summary>
        /// 根据图形边界显示视图
        /// </summary>
        /// <param name="ed">命令行对象</param>
        public static void ZoomExtents(this Editor ed)
        {
            Database db = ed.Document.Database;
            db.UpdateExt(true);//更新当前模型空间的范围
            //根据当前图形的界限范围对视图进行缩放
            if (db.Extmax.X < db.Extmin.X)
            {
                Plane plane = new Plane();
                Point3d pt1 = new Point3d(plane, db.Limmin);
                Point3d pt2 = new Point3d(plane, db.Limmax);
                ed.ZoomWindow(pt1, pt2);
            }
            else
            {
                ed.ZoomWindow(db.Extmin, db.Extmax);
            }
        }

        /// <summary>
        /// 根据对象的范围显示视图
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="entId">对象的Id</param>
        public static void ZoomObject(this Editor ed, ObjectId entId)
        {
            Database db = ed.Document.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获取实体对象
                Entity ent = trans.GetObject(entId, OpenMode.ForRead) as Entity;
                if (ent == null) return;
                //根据实体的范围对视图进行缩放
                Extents3d ext = ent.GeometricExtents;
                ext.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());
                ed.ZoomWindow(ext.MinPoint, ext.MaxPoint);
                trans.Commit();
            }
        }

        /// <summary>
        /// 居中显示视图
        /// </summary>
        /// <param name="ed">命令行对象</param>
        /// <param name="center">居中的点</param>
        public static void ZoomCenter(this Editor ed, Point3d center)
        {
            ed.Zoom(Point3d.Origin, Point3d.Origin, center, 1);
        }
    }
}
