using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 多段线操作类
    /// </summary>
    public static class PolylineTools
    {
        /// <summary>
        /// 通过三维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point3dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
        }

        /// <summary>
        /// 通过二维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point2dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, pts[i], 0, 0, 0);
            }
        }

        /// <summary>
        /// 通过不固定的点创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, params Point2d[] pts)
        {
            pline.CreatePolyline(new Point2dCollection(pts));
        }

        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pt1">矩形的角点</param>
        /// <param name="pt2">矩形的角点</param>
        public static void CreateRectangle(this Polyline pline, Point2d pt1, Point2d pt2)
        {
            //设置矩形的4个顶点
            double minX = Math.Min(pt1.X, pt2.X);
            double maxX = Math.Max(pt1.X, pt2.X);
            double minY = Math.Min(pt1.Y, pt2.Y);
            double maxY = Math.Max(pt1.Y, pt2.Y);
            Point2dCollection pts = new Point2dCollection();
            pts.Add(new Point2d(minX, minY));
            pts.Add(new Point2d(minX, maxY));
            pts.Add(new Point2d(maxX, maxY));
            pts.Add(new Point2d(maxX, minY));
            pline.CreatePolyline(pts);
            pline.Closed = true;//闭合多段线以形成矩形
        }

        /// <summary>
        /// 创建多边形
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="centerPoint">多边形中心点</param>
        /// <param name="number">边数</param>
        /// <param name="radius">外接圆半径</param>
        public static void CreatePolygon(this Polyline pline, Point2d centerPoint, int number, double radius)
        {
            Point2dCollection pts = new Point2dCollection(number);
            double angle = 2 * Math.PI / number;//计算每条边对应的角度
            //计算多边形的顶点
            for (int i = 0; i < number; i++)
            {
                Point2d pt = new Point2d(centerPoint.X + radius * Math.Cos(i * angle), centerPoint.Y + radius * Math.Sin(i * angle));
                pts.Add(pt);
            }
            pline.CreatePolyline(pts);
            pline.Closed = true;//闭合多段线以形成多边形
        }

        /// <summary>
        /// 创建多段线形式的圆
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="centerPoint">圆心</param>
        /// <param name="radius">半径</param>
        public static void CreatePolyCircle(this Polyline pline, Point2d centerPoint, double radius)
        {
            //计算多段线的顶点
            Point2d pt1 = new Point2d(centerPoint.X + radius, centerPoint.Y);
            Point2d pt2 = new Point2d(centerPoint.X - radius, centerPoint.Y);
            Point2d pt3 = new Point2d(centerPoint.X + radius, centerPoint.Y);
            Point2dCollection pts = new Point2dCollection();
            //添加多段线的顶点
            pline.AddVertexAt(0, pt1, 1, 0, 0);
            pline.AddVertexAt(1, pt2, 1, 0, 0);
            pline.AddVertexAt(2, pt3, 1, 0, 0);
            pline.Closed = true;//闭合曲线以形成圆
        }

        /// <summary>
        /// 创建多段线形式的圆弧
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="centerPoint">圆弧的圆心</param>
        /// <param name="radius">圆弧的半径</param>
        /// <param name="startAngle">起始角度</param>
        /// <param name="endAngle">终止角度</param>
        public static void CreatePolyArc(this Polyline pline, Point2d centerPoint, double radius, double startAngle, double endAngle)
        {
            //计算多段线的顶点
            Point2d pt1 = new Point2d(centerPoint.X + radius * Math.Cos(startAngle),
                                    centerPoint.Y + radius * Math.Sin(startAngle));
            Point2d pt2 = new Point2d(centerPoint.X + radius * Math.Cos(endAngle),
                                    centerPoint.Y + radius * Math.Sin(endAngle));
            //添加多段线的顶点
            pline.AddVertexAt(0, pt1, Math.Tan((endAngle - startAngle) / 4), 0, 0);
            pline.AddVertexAt(1, pt2, 0, 0, 0);
        }
    }
}
