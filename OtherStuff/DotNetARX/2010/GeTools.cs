using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 几何类
    /// </summary>
    public static partial class GeTools
    {
        /// <summary>
        /// 将弧度值转换为角度值
        /// </summary>
        /// <param name="angle">弧度</param>
        /// <returns>返回角度值</returns>
        public static double RadianToDegree(this double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        /// <summary>
        /// 将角度值转换为弧度值
        /// </summary>
        /// <param name="angle">角度</param>
        /// <returns>返回弧度值</returns>
        public static double DegreeToRadian(this double angle)
        {
            return angle * (Math.PI / 180.0);
        }

        /// <summary>
        /// 获取与给定点指定角度和距离的点
        /// </summary>
        /// <param name="point">给定点</param>
        /// <param name="angle">角度</param>
        /// <param name="dist">距离</param>
        /// <returns>返回与给定点指定角度和距离的点</returns>
        public static Point3d PolarPoint(this Point3d point, double angle, double dist)
        {
            return new Point3d(point.X + dist * Math.Cos(angle), point.Y + dist * Math.Sin(angle), point.Z);
        }

        /// <summary>
        /// 获取两个点之间的中点
        /// </summary>
        /// <param name="pt1">第一点</param>
        /// <param name="pt2">第二点</param>
        /// <returns>返回两个点之间的中点</returns>
        public static Point3d MidPoint(Point3d pt1, Point3d pt2)
        {
            Point3d midPoint = new Point3d((pt1.X + pt2.X) / 2.0,
                                        (pt1.Y + pt2.Y) / 2.0,
                                        (pt1.Z + pt2.Z) / 2.0);
            return midPoint;
        }

        /// <summary>
        /// 计算从第一点到第二点所确定的矢量与X轴正方向的夹角
        /// </summary>
        /// <param name="pt1">第一点</param>
        /// <param name="pt2">第二点</param>
        /// <returns>返回两点所确定的矢量与X轴正方向的夹角</returns>
        public static double AngleFromXAxis(this Point3d pt1, Point3d pt2)
        {
            //构建一个从第一点到第二点所确定的矢量
            Vector2d vector=new Vector2d(pt1.X - pt2.X, pt1.Y - pt2.Y);
            //返回该矢量和X轴正半轴的角度（弧度）
            return vector.Angle;
        }
    }
}
