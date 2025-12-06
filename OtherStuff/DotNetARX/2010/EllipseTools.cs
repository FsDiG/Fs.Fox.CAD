using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 椭圆操作类
    /// </summary>
    public static class EllipseTools
    {
        /// <summary>
        /// 根据外接矩形创建椭圆
        /// </summary>
        /// <param name="ellipse">椭圆对象</param>
        /// <param name="pt1">外接矩形的角点</param>
        /// <param name="pt2">外接矩形的角点</param>
        public static void CreateEllipse(this Ellipse ellipse, Point3d pt1, Point3d pt2)
        {
            //椭圆的中心点
            Point3d center=GeTools.MidPoint(pt1, pt2);
            Vector3d normal=Vector3d.ZAxis;//法向量
            //椭圆1/2长轴的矢量，矢量的起点为椭圆的中心，终点为椭圆长轴的一个端点
            Vector3d majorAxis=new Vector3d(Math.Abs(pt1.X - pt2.X) / 2, 0, 0);
            //椭圆短轴与长轴的长度比例
            double ratio=Math.Abs((pt1.Y - pt2.Y) / (pt1.X - pt2.X));
            //设置椭圆的参数
            ellipse.Set(center, normal, majorAxis, ratio, 0, 2 * Math.PI);
        }
    }
}
