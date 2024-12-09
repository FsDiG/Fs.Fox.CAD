#if a2024 || zcad
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 点
/// </summary>
public static class PointEx
{
    /// <summary>
    /// 获取点的hash字符串，同时可以作为pt的字符串表示
    /// </summary>
    /// <param name="pt">点</param>
    /// <param name="xyz">指示计算几维坐标的标志，1为计算x，2为计算x，y，其他为计算x，y，z</param>
    /// <param name="decimalRetain">保留的小数位数</param>
    /// <returns>hash字符串</returns>
    public static string GetHashString(this Point3d pt, int xyz = 3, int decimalRetain = 6)
    {
        var de = $"f{decimalRetain}";
        return xyz switch
        {
            1 => $"({pt.X.ToString(de)})",
            2 => $"({pt.X.ToString(de)},{pt.Y.ToString(de)})",
            _ => $"({pt.X.ToString(de)},{pt.Y.ToString(de)},{pt.Z.ToString(de)})"
        };
    }

    // 为了频繁触发所以弄个缓存
    private static Plane? _planeCache;

    /// <summary>
    /// 两点计算弧度范围0到2Pi
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <param name="direction">方向</param>
    /// <returns>弧度值</returns>
    public static double GetAngle(this Point3d startPoint, Point3d endPoint, Vector3d? direction = null)
    {
        if (direction != null)
            _planeCache = new Plane(new Point3d(), direction.Value);
        if (_planeCache == null)
            _planeCache = new Plane(new Point3d(), Vector3d.ZAxis);
        return startPoint.GetVectorTo(endPoint).AngleOnPlane(_planeCache);
    }

    /// <summary>
    /// 两点计算弧度范围0到2Pi
    /// </summary>
    /// <param name="startPoint">起点</param>
    /// <param name="endPoint">终点</param>
    /// <returns>弧度值</returns>
    public static double GetAngle(this Point2d startPoint, Point2d endPoint)
    {
        return startPoint.GetVectorTo(endPoint).Angle;
    }

    /// <summary>
    /// 获取中点
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Point2d GetMidPointTo(this Point2d a, Point2d b)
    {
        // (p1 + p2) / 2; // 溢出风险
        return new Point2d(a.X * 0.5 + b.X * 0.5,
            a.Y * 0.5 + b.Y * 0.5);
    }

    /// <summary>
    /// 获取两个点之间的中点
    /// </summary>
    /// <param name="pt1">第一点</param>
    /// <param name="pt2">第二点</param>
    /// <returns>返回两个点之间的中点</returns>
    public static Point3d GetMidPointTo(this Point3d pt1, Point3d pt2)
    {
        return new(pt1.X * 0.5 + pt2.X * 0.5,
            pt1.Y * 0.5 + pt2.Y * 0.5,
            pt1.Z * 0.5 + pt2.Z * 0.5);
    }

    /// <summary>
    /// Z值归零
    /// </summary>
    /// <param name="point">点</param>
    /// <returns>新点</returns>
    public static Point3d Z20(this Point3d point)
    {
        return new Point3d(point.X, point.Y, 0);
    }

    /// <summary>
    /// 将三维点转换为二维点
    /// </summary>
    /// <param name="pt">三维点</param>
    /// <returns>二维点</returns>
    public static Point2d Point2d(this Point3d pt)
    {
        return new(pt.X, pt.Y);
    }

    /// <summary>
    /// 将三维点集转换为二维点集
    /// </summary>
    /// <param name="pts">三维点集</param>
    /// <returns>二维点集</returns>
    public static IEnumerable<Point2d> Point2d(this IEnumerable<Point3d> pts)
    {
        return pts.Select(pt => pt.Point2d());
    }

    /// <summary>
    /// 将二维点转换为三维点
    /// </summary>
    /// <param name="pt">二维点</param>
    /// <param name="z">Z值</param>
    /// <returns>三维点</returns>
    public static Point3d Point3d(this Point2d pt, double z = 0)
    {
        return new(pt.X, pt.Y, z);
    }

    /// <summary>
    /// 投影到指定Z值
    /// </summary>
    /// <param name="pt">点</param>
    /// <param name="z">新z值</param>
    /// <returns>投影后的坐标</returns>
    public static Point3d OrthoProject(this Point3d pt, double z)
    {
        return new Point3d(pt.X, pt.Y, z);
    }

    /// <summary>
    /// 根据世界坐标计算用户坐标
    /// </summary>
    /// <param name="basePt">基点世界坐标</param>
    /// <param name="userPt">基点用户坐标</param>
    /// <param name="transPt">目标世界坐标</param>
    /// <param name="ang">坐标网旋转角，按x轴正向逆时针弧度</param>
    /// <returns>目标用户坐标</returns>
    public static Point3d TransPoint(this Point3d basePt, Point3d userPt, Point3d transPt, double ang)
    {
        var transMat = Matrix3d.Displacement(userPt - basePt);
        var roMat = Matrix3d.Rotation(-ang, Vector3d.ZAxis, userPt);
        return transPt.TransformBy(roMat * transMat);
    }

    /// <summary>
    /// 计算指定距离和角度的点
    /// </summary>
    /// <remarks>本函数仅适用于x-y平面</remarks>
    /// <param name="pt">基点</param>
    /// <param name="ang">角度，x轴正向逆时针弧度</param>
    /// <param name="len">距离</param>
    /// <returns>目标点</returns>
    public static Point3d Polar(this Point3d pt, double ang, double len)
    {
        return pt + Vector3d.XAxis.RotateBy(ang, Vector3d.ZAxis) * len;
    }

    /// <summary>
    /// 计算指定距离和角度的点
    /// </summary>
    /// <remarks>本函数仅适用于x-y平面</remarks>
    /// <param name="pt">基点</param>
    /// <param name="ang">角度，x轴正向逆时针弧度</param>
    /// <param name="len">距离</param>
    /// <returns>目标点</returns>
    public static Point2d Polar(this Point2d pt, double ang, double len)
    {
        return pt + Vector2d.XAxis.RotateBy(ang) * len;
    }

    /// http://www.lee-mac.com/bulgeconversion.html
    /// <summary>
    /// 求凸度,判断三点是否一条直线上
    /// </summary>
    /// <param name="arc1">圆弧起点</param>
    /// <param name="arc2">圆弧腰点</param>
    /// <param name="arc3">圆弧尾点</param>
    /// <param name="tol">容差</param>
    /// <returns>逆时针为正,顺时针为负</returns>
    public static double GetArcBulge(this Point2d arc1, Point2d arc2, Point2d arc3, double tol = 1e-10)
    {
        var dStartAngle = arc2.GetAngle(arc1);
        var dEndAngle = arc2.GetAngle(arc3);
        // 求的P1P2与P1P3夹角
        var talAngle = (Math.PI - dStartAngle + dEndAngle) / 2;
        // 凸度==拱高/半弦长==拱高比值/半弦长比值
        // 有了比值就不需要拿到拱高值和半弦长值了,因为接下来是相除得凸度
        var bulge = Math.Sin(talAngle) / Math.Cos(talAngle);

        switch (bulge)
        {
            // 处理精度
            case > 0.9999 and < 1.0001:
                bulge = 1;
                break;
            case < -0.9999 and > -1.0001:
                bulge = -1;
                break;
            default:
            {
                if (Math.Abs(bulge) < tol)
                    bulge = 0;
                break;
            }
        }

        return bulge;
    }

    /// <summary>
    /// 求两点在Z平面的距离
    /// </summary>
    /// <param name="pt1">点1</param>
    /// <param name="pt2">点2</param>
    /// <returns>距离</returns>
    public static double Distance2dTo(this Point3d pt1, Point3d pt2)
    {
        return new Vector2d(pt2.X - pt1.X, pt2.Y - pt1.Y).Length;
    }

    #region 首尾相连

    /// <summary>
    /// 首尾相连
    /// </summary>
    [DebuggerStepThrough]
    public static void End2End(this Point2dCollection ptCollection)
    {
        ArgumentNullException.ThrowIfNull(ptCollection);

        if (ptCollection.Count == 0 || ptCollection[0].Equals(ptCollection[^1])) // 首尾相同直接返回
            return;

        // 首尾不同,去加一个到最后
        var lst = new Point2d[ptCollection.Count + 1];
        for (var i = 0; i < ptCollection.Count; i++)
            lst[i] = ptCollection[i];
        lst[^1] = lst[0];

        ptCollection.Clear();
        ptCollection.AddRange(lst);
    }

    /// <summary>
    /// 首尾相连
    /// </summary>
    [DebuggerStepThrough]
    public static void End2End(this Point3dCollection ptCollection)
    {
        ArgumentNullException.ThrowIfNull(ptCollection);
        if (ptCollection.Count == 0 || ptCollection[0].Equals(ptCollection[^1])) // 首尾相同直接返回
            return;

        // 首尾不同,去加一个到最后
        var lst = new Point3d[ptCollection.Count + 1];
        for (var i = 0; i < ptCollection.Count; i++)
            lst[i] = ptCollection[i];
        lst[^1] = lst[0];

        ptCollection.Clear();
        foreach (var t in lst)
            ptCollection.Add(t);
    }

    #endregion

    #region 包围盒

    /// <summary>
    /// 获取矩形4个3d角点(左下起，正方向)
    /// </summary>
    /// <param name="extents3d">包围盒</param>
    /// <param name="z">z轴坐标</param>
    /// <returns>点表</returns>
    public static List<Point3d> GetRecPoint3ds(this Extents3d extents3d, double z = 0)
    {
        var xMin = extents3d.MinPoint.X;
        var xMax = extents3d.MaxPoint.X;
        var yMin = extents3d.MinPoint.Y;
        var yMax = extents3d.MaxPoint.Y;
        var pt1 = new Point3d(xMin, yMin, z);
        var pt2 = new Point3d(xMax, yMin, z);
        var pt3 = new Point3d(xMax, yMax, z);
        var pt4 = new Point3d(xMin, yMax, z);
        return [pt1, pt2, pt3, pt4];
    }

    /// <summary>
    /// 获取矩形4个2d角点(左下起，正方向)
    /// </summary>
    /// <param name="extents3d">包围盒</param>
    /// <returns>点表</returns>
    public static List<Point2d> GetRecPoint2ds(this Extents3d extents3d)
    {
        var xMin = extents3d.MinPoint.X;
        var xMax = extents3d.MaxPoint.X;
        var yMin = extents3d.MinPoint.Y;
        var yMax = extents3d.MaxPoint.Y;
        var pt1 = new Point2d(xMin, yMin);
        var pt2 = new Point2d(xMax, yMin);
        var pt3 = new Point2d(xMax, yMax);
        var pt4 = new Point2d(xMin, yMax);
        return [pt1, pt2, pt3, pt4];
    }

    /// <summary>
    /// 获取矩形4个角3d点(左下起，正方向)
    /// </summary>
    /// <param name="corner1">角1</param>
    /// <param name="corner2">角2</param>
    /// <param name="z">z轴坐标</param>
    /// <returns>点表</returns>
    public static List<Point3d> GetRecPoint3ds(this Point3d corner1, Point3d corner2, double z = 0)
    {
        var xMin = Math.Min(corner1.X, corner2.X);
        var xMax = Math.Max(corner1.X, corner2.X);
        var yMin = Math.Min(corner1.Y, corner2.Y);
        var yMax = Math.Max(corner1.Y, corner2.Y);
        var pt1 = new Point3d(xMin, yMin, z);
        var pt2 = new Point3d(xMax, yMin, z);
        var pt3 = new Point3d(xMax, yMax, z);
        var pt4 = new Point3d(xMin, yMax, z);
        return [pt1, pt2, pt3, pt4];
    }

    /// <summary>
    /// 获取矩形4个角3d点(左下起，正方向)
    /// </summary>
    /// <param name="corner1">角1</param>
    /// <param name="corner2">角2</param>
    /// <param name="z">z轴坐标</param>
    /// <returns>点表</returns>
    public static List<Point2d> GetRecPoint2ds(this Point2d corner1, Point2d corner2, double z = 0)
    {
        var xMin = Math.Min(corner1.X, corner2.X);
        var xMax = Math.Max(corner1.X, corner2.X);
        var yMin = Math.Min(corner1.Y, corner2.Y);
        var yMax = Math.Max(corner1.Y, corner2.Y);
        var pt1 = new Point2d(xMin, yMin);
        var pt2 = new Point2d(xMax, yMin);
        var pt3 = new Point2d(xMax, yMax);
        var pt4 = new Point2d(xMin, yMax);
        return [pt1, pt2, pt3, pt4];
    }

    #endregion
}