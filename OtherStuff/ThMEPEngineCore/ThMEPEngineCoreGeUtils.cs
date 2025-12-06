using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;

namespace ThMEPEngineCore
{
    public class ThMEPEngineCoreGeUtils
    {
        /// <summary>
        /// 计算最大最小点
        /// </summary>
        /// <returns></returns>
        public static List<Point3d> CalBoundingBox(List<Point3d> points)
        {
            double maxX = points.Max(x => x.X);
            double minX = points.Min(x => x.X);
            double maxY = points.Max(x => x.Y);
            double minY = points.Min(x => x.Y);

            return new List<Point3d>(){
                new Point3d(maxX, maxY, 0), //p2(max point)
                new Point3d(maxX, minY, 0), //p3
                new Point3d(minX, maxY, 0), //p1
                new Point3d(minX, minY, 0), //p4(min point)
            };
        }

        /// <summary>
        /// 计算轴网坐标系
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static Matrix3d GetGridMatrix(Vector3d xDir)
        {
            Vector3d longDir = xDir;
            Vector3d shortDir = Vector3d.ZAxis.CrossProduct(longDir);

            Matrix3d matrix = new Matrix3d(new double[]{
                    longDir.X, shortDir.X, Vector3d.ZAxis.X, 0,
                    longDir.Y, shortDir.Y, Vector3d.ZAxis.Y, 0,
                    longDir.Z, shortDir.Z, Vector3d.ZAxis.Z, 0,
                    0.0, 0.0, 0.0, 1.0});
            return matrix;
        }
    }
}
