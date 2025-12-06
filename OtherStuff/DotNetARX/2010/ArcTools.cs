using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    /// <summary>
    /// 简化圆弧操作类
    /// </summary>
    public static class ArcTools
    {
        /// <summary>
        /// 根据圆心、起点和终点来创建圆弧
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="centerPoint">圆心</param>
        /// <param name="endPoint">终点</param>
        public static void CreateArcSCE(this Arc arc, Point3d startPoint, Point3d centerPoint, Point3d endPoint)
        {
            arc.Center = centerPoint;
            arc.Radius = centerPoint.DistanceTo(startPoint);
            Vector2d startVector=new Vector2d(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            Vector2d endVector=new Vector2d(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
            arc.StartAngle = startVector.Angle;
            arc.EndAngle = endVector.Angle;
        }

        /// <summary>
        /// 根据起点、圆心和圆弧角度创建圆弧
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="centerPoint">圆心</param>
        /// <param name="angle">圆弧角度</param>
        public static void CreateArc(this Arc arc, Point3d startPoint, Point3d centerPoint, double angle)
        {
            arc.Center = centerPoint;
            arc.Radius = centerPoint.DistanceTo(startPoint);
            Vector2d startVector=new Vector2d(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            arc.StartAngle = startVector.Angle;
            arc.EndAngle = startVector.Angle + angle;
        }

        /// <summary>
        /// 三点法创建圆弧
        /// </summary>
        /// <param name="arc">圆弧对象</param>
        /// <param name="startPoint">起点</param>
        /// <param name="pointOnArc">圆弧上的点</param>
        /// <param name="endPoint">终点</param>
        public static void CreateArc(this Arc arc, Point3d startPoint, Point3d pointOnArc, Point3d endPoint)
        {
            //创建一个几何类的圆弧对象
            CircularArc3d geArc=new CircularArc3d(startPoint, pointOnArc, endPoint);
            //将几何类圆弧对象的圆心和半径赋值给圆弧
            Point3d centerPoint=geArc.Center;
            arc.Center = centerPoint;
            arc.Radius = geArc.Radius;
            //计算起始和终止角度
            arc.StartAngle = startPoint.AngleFromXAxis(centerPoint);
            arc.EndAngle = endPoint.AngleFromXAxis(centerPoint);
        }
    }
}
