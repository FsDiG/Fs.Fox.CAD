using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace DotNetARX
{
    /// <summary>
    /// 面域操作类
    /// </summary>
    public static class RegionTools
    {
        #region 面域特性
#if ACAD2012
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?getAreaProp@AcDbRegion@@UEBA?AW4ErrorStatus@Acad@@AEBVAcGePoint3d@@AEBVAcGeVector3d@@1AEAN2AEAVAcGePoint2d@@QEAN24QEAVAcGeVector2d@@433@Z")]
#elif ACAD2014
        [DllImport("acdb19.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?getAreaProp@AcDbRegion@@UEBA?AW4ErrorStatus@Acad@@AEBVAcGePoint3d@@AEBVAcGeVector3d@@1AEAN2AEAVAcGePoint2d@@QEAN24QEAVAcGeVector2d@@433@Z")]
#elif ACAD2016
        [DllImport("acdb20.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?getAreaProp@AcDbRegion@@UEBA?AW4ErrorStatus@Acad@@AEBVAcGePoint3d@@AEBVAcGeVector3d@@1AEAN2AEAVAcGePoint2d@@QEAN24QEAVAcGeVector2d@@433@Z")]
#elif ACAD2018
        [DllImport("acdb22.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?getAreaProp@AcDbRegion@@UEBA?AW4ErrorStatus@Acad@@AEBVAcGePoint3d@@AEBVAcGeVector3d@@1AEAN2AEAVAcGePoint2d@@QEAN24QEAVAcGeVector2d@@433@Z")]
#endif
        private static extern ErrorStatus getAreaProp(IntPtr region,
                                                ref Point3d origin,
                                                ref Vector3d xAxis,
                                                ref Vector3d yAxis,
                                                out double perimeter,
                                                out double area,
                                                out Point2d centroid,
                                                double[] momInertia,
                                                out double prodInertia,
                                                double[] prinMoments,
                                                Vector2d[] prinAxes,
                                                double[] radiiGyration,
                                                out Point2d extentsLow,
                                                out Point2d extentsHigh);
        static double perimeter;
        static double area;
        static Point2d centroid;
        static double[] momInertia=new double[2];
        static double prodInertia;
        static double[] prinMoments=new double[2];
        static Vector2d[] prinAxes=new Vector2d[2];
        static double[] radiiGyration=new double[2];
        static Point2d extentsLow;
        static Point2d extentsHigh;
        /// <summary>
        /// 获取面域的质心
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回质心的坐标</returns>
        public static Point2d GetCentroid(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return centroid;
        }

        public static Point2d GetWCSCCentroid(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord = ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin = Point3d.Origin;
            Vector3d xAxis = Vector3d.XAxis;
            Vector3d yAxis = Vector3d.YAxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return centroid;
        }
        /// <summary>
        /// 获取面域的惯性矩
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回惯性矩</returns>
        public static double[] GetMomInertia(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return momInertia;
        }
        /// <summary>
        /// 获取面域的惯性积
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回惯性积</returns>
        public static double GetProdInertia(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return prodInertia;
        }
        /// <summary>
        /// 获取面域的主力矩
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回主力矩</returns>
        public static double[] GetPrinMoments(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return prinMoments;
        }
        /// <summary>
        /// 获取面域的主方向
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回主方向</returns>
        public static Vector2d[] GetPrinAxes(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return prinAxes;
        }
        /// <summary>
        /// 获取面域的旋转半径
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回旋转半径</returns>
        public static double[] GetRadiiGyration(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return radiiGyration;
        }
        /// <summary>
        /// 获取面域边界框的下限
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回边界框的下限</returns>
        public static Point2d GetExtentsLow(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return extentsLow;
        }
        /// <summary>
        /// 获取面域边界框的上限
        /// </summary>
        /// <param name="region">面域对象</param>
        /// <returns>返回边界框的上限</returns>
        public static Point2d GetExtentsHigh(this Region region)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d coord= ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Point3d origin=coord.Origin;
            Vector3d xAxis=coord.Xaxis;
            Vector3d yAxis=coord.Yaxis;
            getAreaProp(region.UnmanagedObject, ref origin, ref xAxis, ref yAxis, out perimeter, out area, out centroid,
                       momInertia, out prodInertia, prinMoments, prinAxes, radiiGyration, out extentsLow, out extentsHigh);
            return extentsHigh;
        } 
#endregion

        /// <summary>
        /// 根据曲线创建面域
        /// </summary>
        /// <param name="curves">曲线</param>
        /// <returns>返回创建的面域列表</returns>
        public static List<Region> CreateRegion(params Curve[] curves)
        {
            //新建面域列表，存储创建的面域
            List<Region> regionList=new List<Region>();
            //将可变数组转化为集合类，用于面域的创建
            DBObjectCollection curveCollection=new DBObjectCollection();
            foreach (Curve curve in curves)//遍历曲线
            {
                //如果曲线已经被加入到数据库且为写的状态，则返回
                if (!curve.IsNewObject && curve.IsWriteEnabled)
                    return null;
                curveCollection.Add(curve);//将曲线添加到集合中
            }
            try
            {
                //根据曲线集合，在内存中创建面域对象集合
                DBObjectCollection regionObjs=Region.CreateFromCurves(curveCollection);
                //将面域对象集合复制到面域列表
                foreach (Region region in regionObjs)
                {
                    regionList.Add(region);
                }
                return regionList;
            }
            catch //面域创建失败
            {
                regionList.Clear();//清空面域列表
                return regionList;//返回空的面域列表
            }
        }
    }
}
