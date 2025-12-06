using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 简化圆操作类
    /// </summary>
    public static class CircleTools
    {
        /// <summary>
        /// 两点创建圆
        /// </summary>
        /// <param name="circle">圆对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        public static void CreateCircle(this Circle circle, Point3d startPoint, Point3d endPoint)
        {
            circle.Center = GeTools.MidPoint(startPoint, endPoint);
            circle.Radius = startPoint.DistanceTo(endPoint) / 2;
        }
        
        /// <summary>
        /// 三点法创建圆
        /// </summary>
        /// <param name="circle">圆对象</param>
        /// <param name="pt1">第一点</param>
        /// <param name="pt2">第二点</param>
        /// <param name="pt3">第三点</param>
        /// <returns>若成功创建圆，则返回true，否则返回false</returns>
        public static bool CreateCircle(this Circle circle, Point3d pt1, Point3d pt2, Point3d pt3)
        {
            //先判断三点是否共线,得到pt1点指向pt2、pt2点的矢量
            Vector3d va = pt1.GetVectorTo(pt2);
            Vector3d vb = pt1.GetVectorTo(pt3);
            //如两矢量夹角为0或180度（π弧度),则三点共线.
            if (va.GetAngleTo(vb) == 0 | va.GetAngleTo(vb) == Math.PI)
            {
                return false;
            }
            else
            {
                //创建一个几何类的圆弧对象
                CircularArc3d geArc = new CircularArc3d(pt1, pt2, pt3);
                //将圆弧对象的圆心和半径赋值给圆
                circle.Center = geArc.Center;
                circle.Radius = geArc.Radius;
                return true;
            }
        }
    }
}
