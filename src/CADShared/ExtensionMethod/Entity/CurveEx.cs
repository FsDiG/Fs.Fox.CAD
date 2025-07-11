﻿// ReSharper disable ForCanBeConvertedToForeach

#if a2024 || zcad
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 实体类曲线扩展类
/// </summary>
public static class CurveEx
{
    /// <summary>
    /// 获得曲线长度,通过获取曲线的起始参数和结束参数计算
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>长度</returns>
    public static double GetCurveLength(this Curve curve)
    {
        return curve.GetDistanceAtParameter(curve.EndParam);
    }

    /*/// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="pars">打断参数表</param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<double> pars)
    {
        //if (pars is null)
        //    throw new ArgumentNullException(nameof(pars));
        ArgumentNullEx.ThrowIfNull(pars);
        return curve.GetSplitCurves(new DoubleCollection(pars.ToArray())).Cast<Curve>();
    }*/

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="pars">打断参数表</param>
    /// <param name="isOrder">对参数表是否进行排序
    /// <para>
    /// <see langword="true"/>按参数值升序排序<br/>
    /// <see langword="false"/>不排序,默认值
    /// </para>
    /// </param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(Curve curve, IEnumerable<double> pars,
        bool isOrder = false)
    {
        //if (pars is null)
        //    throw new ArgumentNullException(nameof(pars));
        ArgumentNullException.ThrowIfNull(pars);
        if (isOrder)
            pars = pars.OrderBy(x => x);

        return curve.GetSplitCurves(new DoubleCollection(pars.ToArray())).Cast<Curve>();
    }

    /*
    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="points">打断点表</param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<Point3d> points)
    {
        //if (points is null)
        //    throw new ArgumentNullException(nameof(points));
        ArgumentNullEx.ThrowIfNull(points);
        using var pts = new Point3dCollection(points.ToArray());
        return curve.GetSplitCurves(pts).Cast<Curve>();
    }*/

    /// <summary>
    /// 获取分割曲线集合
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <param name="points">打断点表</param>
    /// <param name="isOrder">对点表是否进行排序
    /// <para>
    /// <see langword="true"/>按参数值升序排序<br/>
    /// <see langword="false"/>不排序,默认值
    /// </para>
    /// </param>
    /// <returns>打断后曲线的集合</returns>
    public static IEnumerable<Curve> GetSplitCurves(this Curve curve, IEnumerable<Point3d> points,
        bool isOrder = false)
    {
        //if (points is null)
        //    throw new ArgumentNullException(nameof(points));
        ArgumentNullException.ThrowIfNull(points);
        if (isOrder)
            points = points.OrderBy(point =>
            {
                var pt = curve.GetClosestPointTo(point, false);
                return curve.GetParameterAtPoint(pt);
            });

        using Point3dCollection pts = new(points.ToArray());
        return curve.GetSplitCurves(pts).Cast<Curve>();
    }

    /// <summary>
    /// 曲线打断
    /// </summary>
    /// <param name="curves">曲线列表</param>
    /// <returns>打断后的曲线列表</returns>
    public static List<Curve> BreakCurve(this List<Curve> curves)
    {
        ArgumentNullException.ThrowIfNull(curves);

        var tol = new Tolerance(0.01, 0.01);

        List<CompositeCurve3d> geCurves = []; // 存储曲线转换后的复合曲线
        List<List<double>> paramsList = []; // 存储每个曲线的交点参数值

        for (var i = 0; i < curves.Count; i++)
        {
            var cc3d = curves[i].ToCompositeCurve3d();
            if (cc3d is not null)
            {
                geCurves.Add(cc3d);
                paramsList.Add([]);
            }
        }

        // List<Curve> oldCurves = [];
        List<Curve> newCurves = [];
        var cci3d = new CurveCurveIntersector3d();

        for (var i = 0; i < curves.Count; i++)
        {
            var gc1 = geCurves[i];
            var pars1 = paramsList[i]; // 引用
            for (var j = i; j < curves.Count; j++)
            {
                var gc2 = geCurves[j];
                var pars2 = paramsList[j]; // 引用

                cci3d.Set(gc1, gc2, Vector3d.ZAxis, tol);

                for (var k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]); // 引用修改会同步到源对象
                    pars2.Add(pars[1]); // 引用修改会同步到源对象
                }
            }

            if (pars1.Count > 0)
            {
                var c3ds = gc1.GetSplitCurves(pars1);
                if (c3ds is not null && c3ds.Count > 1)
                {
                    foreach (var c3d in c3ds)
                    {
                        var c3dCur = c3d.ToCurve();
                        if (c3dCur is not null)
                        {
                            c3dCur.SetPropertiesFrom(curves[i]);
                            newCurves.Add(c3dCur);
                        }
                    }
                    // oldCurves.Add(curves[i]);
                }
            }
        }

        return newCurves;
    }

    /// <summary>
    /// 在z法向量平面打断曲线
    /// </summary>
    /// <param name="curves">曲线列表</param>
    /// <returns>打断后的曲线列表</returns>
    /// <exception cref="System.ArgumentNullException">传入的曲线列表错误</exception>
    public static List<Curve> BreakCurveOnZPlane(this List<Curve> curves)
    {
        if (curves is null)
            throw new System.ArgumentNullException(nameof(curves));
        var zPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
        var curvesTemp = curves.Select(c => c.GetProjectedCurve(zPlane, Vector3d.ZAxis)).ToList();
        List<CompositeCurve3d> geCurves = []; // 存储曲线转换后的复合曲线
        List<HashSet<double>> paramsList = []; // 存储每个曲线的交点参数值

        for (var i = 0; i < curvesTemp.Count; i++)
        {
            paramsList.Add([]);
            var cc3d = curvesTemp[i].ToCompositeCurve3d();
            if (cc3d is not null)
            {
                geCurves.Add(cc3d);
            }
        }

        List<Curve> newCurves = [];
        var cci3d = new CurveCurveIntersector3d();

        for (var i = 0; i < curvesTemp.Count; i++)
        {
            var gc1 = geCurves[i];
            var pars1 = paramsList[i]; // 引用
            for (var j = i; j < curvesTemp.Count; j++)
            {
                var gc2 = geCurves[j];
                var pars2 = paramsList[j]; // 引用

                cci3d.Set(gc1, gc2, Vector3d.ZAxis);

                for (var k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    pars1.Add(pars[0]); // 引用修改会同步到源对象
                    pars2.Add(pars[1]); // 引用修改会同步到源对象
                }
            }

            var curNow = curvesTemp[i];
            var length = curNow.GetCurveLength();
            var np = pars1.Where(p => p >= 0 && p <= length)
                .Select(curNow.GetParameterAtDistance)
                .Where(p =>
                    !(Math.Abs(p - curNow.StartParam) < 1e-6 ||
                      Math.Abs(p - curNow.EndParam) < 1e-6))
                .ToList();
            if (np.Count > 0)
            {
                var splitCurs = GetSplitCurves(curNow, np, true).ToList();
                if (splitCurs.Count > 1)
                {
                    newCurves.AddRange(splitCurs);
                }
                else
                {
                    newCurves.Add(curNow.CloneEx());
                }
            }
            else
            {
                newCurves.Add(curNow.CloneEx());
            }
        }

        return newCurves;
    }
#if !gcad
    /// <summary>
    /// 打段曲线2维By四叉树
    /// <code>
    /// 目前对xLine,ray的支持存在错误
    /// 需要更多的测试
    /// </code>
    /// </summary>
    /// <param name="sourceCurveList">曲线列表</param>
    /// <param name="tol">容差</param>
    /// <returns>打断后的曲线列表</returns>
    public static List<Curve> BreakCurve2dByQuadTree(this List<Curve> sourceCurveList,
        double tol = 1e-6)
    {
        //var tolerance = new Tolerance(tol, tol);
        var zPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
        var curves = sourceCurveList.Select(c => c.GetOrthoProjectedCurve(zPlane)).ToList();
        List<BreakCurveInfo> geCurves = [];
        List<Curve> xLines = [];
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        // 遍历每条曲线，计算出四叉树对象，加到四叉树曲线对象列表和四叉树容器中
        for (var i = 0; i < curves.Count; i++)
        {
            var curTemp = curves[i];
            if (curTemp is Ray || curTemp is Xline)
            {
                xLines.Add(curTemp);
            }
            else
            {
                var cc3d = curTemp.ToCompositeCurve3d();
                if (cc3d is not null)
                {
                    var e3d = curTemp.GeometricExtents;
                    var rect = new Rect(e3d.MinPoint.Point2d(), e3d.MaxPoint.Point2d());
                    var cit = new BreakCurveInfo(rect, curTemp, cc3d);
                    if (rect.Left < minX) minX = rect.Left;
                    if (rect.Right > maxX) maxX = rect.Right;
                    if (rect.Bottom < minY) minY = rect.Bottom;
                    if (rect.Top > maxY) maxY = rect.Top;
                    geCurves.Add(cit);
                }
            }
        }

        // 建四叉树容器
        var maxBox = new Rect(minX - 10, minY - 10, maxX + 10, maxY + 10);
        xLines.ForEach(xl =>
        {
            var cc3d = new CompositeCurve3d([xl.GetGeCurve()]);
            var bci = new BreakCurveInfo(maxBox, xl, cc3d);
            geCurves.Add(bci);
        });
        List<Curve> newCurves = [];
        var quadTree = new QuadTree<BreakCurveInfo>(maxBox);
        foreach (var bci in geCurves)
        {
            quadTree.Insert(bci);
        }

        var cci3d = new CurveCurveIntersector3d();

        foreach (var gc1 in geCurves)
        {
            var parsList = new HashSet<double>();
            var cts = quadTree.Query(new Rect(gc1.Left - tol, gc1.Bottom - tol, gc1.Right + tol,
                gc1.Top + tol));
            cts.Remove(gc1);
            foreach (var gc2 in cts)
            {
                cci3d.Set(gc1.Cc3d, gc2.Cc3d, Vector3d.ZAxis, Tolerance.Global);
                for (var k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    parsList.Add(pars[0]);
                }

                if (!gc2.Curve.Closed)
                {
                    var cpt1 = gc1.Cc3d.GetClosestPointTo(gc2.Cc3d.StartPoint);
                    var cpt2 = gc1.Cc3d.GetClosestPointTo(gc2.Cc3d.EndPoint);
                    if (cpt1.Point.Distance2dTo(gc2.Cc3d.StartPoint) < tol &&
                        cpt1.Point.Distance2dTo(gc1.Cc3d.StartPoint) >= tol)
                    {
                        parsList.Add(cpt1.Parameter);
                    }

                    if (cpt2.Point.Distance2dTo(gc2.Cc3d.EndPoint) < tol &&
                        cpt2.Point.Distance2dTo(gc1.Cc3d.EndPoint) >= tol)
                    {
                        parsList.Add(cpt2.Parameter);
                    }
                }
            }

            if (gc1.Curve is Polyline || gc1.Curve is Spline)
            {
                cci3d.Set(gc1.Cc3d, gc1.Cc3d, Vector3d.ZAxis);
                for (var k = 0; k < cci3d.NumberOfIntersectionPoints; k++)
                {
                    var pars = cci3d.GetIntersectionParameters(k);
                    if (Math.Abs(pars[0] - pars[1]) < 1e-6)
                        continue;
                    parsList.Add(pars[0]);
                    parsList.Add(pars[1]);
                }
            }

            var parsNew = parsList.OrderBy(d => d).ToList();

            var cur = gc1.Curve;
            if (parsNew.Count > 0)
            {
                var c3ds = gc1.Cc3d.GetSplitCurves(parsNew);
                if (c3ds is not null && c3ds.Count > 1)
                {
                    if (cur is Arc || (cur is Ellipse { Closed: false }))
                    {
                        foreach (var c3d in c3ds)
                        {
                            var c3dCur = Curve.CreateFromGeCurve(c3d);

                            if (c3dCur is null || c3dCur.Closed || c3dCur is Circle ||
                                c3dCur.GetCurveLength() < tol)
                                continue;
                            c3dCur.SetPropertiesFrom(cur);
                            newCurves.Add(c3dCur);
                        }

                        continue;
                    }

                    foreach (var c3d in c3ds)
                    {
                        var c3dCur = Curve.CreateFromGeCurve(c3d);
                        if (c3dCur is not null)
                        {
                            c3dCur.SetPropertiesFrom(cur);
                            newCurves.Add(c3dCur);
                        }
                    }
                }
                else
                {
                    newCurves.Add(cur.CloneEx());
                }
            }
            else
            {
                newCurves.Add(cur.CloneEx());
            }
        }

        return newCurves;
    }

    private class BreakCurveInfo : QuadEntity
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Rect Rect { get; }
        public Curve Curve { get; }
        public CompositeCurve3d Cc3d { get; }

        public BreakCurveInfo(Rect rect, Curve curve, CompositeCurve3d cc3d) : base(rect)
        {
            Curve = curve;
            Cc3d = cc3d;
            Rect = rect;
        }
    }
#endif
    /// <summary>
    /// 获取非等比转换的曲线（旋转投影法）
    /// </summary>
    /// <param name="cur">转换前的曲线</param>
    /// <param name="pt">基点</param>
    /// <param name="x">x方向比例</param>
    /// <param name="y">y方向比例</param>
    /// <returns>转换后的曲线</returns>
    public static Curve GetScaleCurve(this Curve cur, Point3d pt, double x, double y)
    {
        // 先做个z平面
        using var zPlane = new Plane(pt, Vector3d.ZAxis);
        // 克隆一个，防止修改到原来的
        using var cur2 = cur.CloneEx();

        // 因为旋转投影后只能比原来小，所以遇到先放大
        while (Math.Abs(x) > 1 || Math.Abs(y) > 1)
        {
            cur2.TransformBy(Matrix3d.Scaling(2, pt));
            x /= 2;
            y /= 2;
        }

        // 旋转投影
        var xA = Math.Acos(x);
        cur2.TransformBy(Matrix3d.Rotation(xA, Vector3d.YAxis, pt));

        using var cur3 = cur2.GetOrthoProjectedCurve(zPlane);

        // 再次旋转投影
        var yA = Math.Acos(y);
        cur3.TransformBy(Matrix3d.Rotation(yA, Vector3d.XAxis, pt));
        var cur4 = cur3.GetOrthoProjectedCurve(zPlane);

        //设置属性
        cur4.SetPropertiesFrom(cur);
        return cur4;
    }

    // 转换DBCurve为GeCurved

    #region Curve

    /// <summary>
    /// 将曲线转换为ge曲线，此函数将在未来淘汰，二惊加油
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>ge曲线</returns>
    public static Curve3d? ToCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => ToCurve3d(li),
            Circle ci => ToCurve3d(ci),
            Arc arc => ToCurve3d(arc),
            Ellipse el => ToCurve3d(el),
            Polyline pl => ToCurve3d(pl),
            Polyline2d pl2 => ToCurve3d(pl2),
            Polyline3d pl3 => ToCurve3d(pl3),
            Spline sp => ToCurve3d(sp),
            _ => null
        };
    }

    /// <summary>
    /// 将曲线转换为复合曲线
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>复合曲线</returns>
    public static CompositeCurve3d? ToCompositeCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => new CompositeCurve3d([ToCurve3d(li)]),
            Circle ci => new CompositeCurve3d([ToCurve3d(ci)]),
            Arc arc => new CompositeCurve3d([ToCurve3d(arc)]),
            Ellipse el => new CompositeCurve3d([ToCurve3d(el)]),
            Polyline pl => new CompositeCurve3d([ToCurve3d(pl)]),

            Polyline2d pl2 => new CompositeCurve3d([ToCurve3d(pl2)!]),
            Polyline3d pl3 => new CompositeCurve3d([ToCurve3d(pl3)]),
            Spline sp => new CompositeCurve3d([ToCurve3d(sp)]),
            _ => null
        };
    }

    /// <summary>
    /// 将曲线转换为Nurb曲线
    /// </summary>
    /// <param name="curve">曲线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Curve curve)
    {
        return curve switch
        {
            Line li => ToNurbCurve3d(li),
            Circle ci => ToNurbCurve3d(ci),
            Arc arc => ToNurbCurve3d(arc),
            Ellipse el => ToNurbCurve3d(el),
            Polyline pl => ToNurbCurve3d(pl),
            Polyline2d pl2 => ToNurbCurve3d(pl2),
            Polyline3d pl3 => ToNurbCurve3d(pl3),
            Spline sp => ToNurbCurve3d(sp),
            _ => null
        };
    }

    #region Line

    /// <summary>
    /// 将直线转换为ge直线
    /// </summary>
    /// <param name="line">直线</param>
    /// <returns>ge直线</returns>
    public static LineSegment3d ToCurve3d(this Line line)
    {
        return new LineSegment3d(line.StartPoint, line.EndPoint);
    }

    /// <summary>
    /// 将直线转换为Nurb曲线
    /// </summary>
    /// <param name="line">直线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Line line)
    {
        return new NurbCurve3d(ToCurve3d(line));
    }

    #endregion Line

    #region Circle

    /// <summary>
    /// 将圆转换为ge圆弧曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>ge圆弧曲线</returns>
    public static CircularArc3d ToCurve3d(this Circle cir)
    {
        return new CircularArc3d(cir.Center, cir.Normal, cir.Radius);
    }

    /// <summary>
    /// 将圆转换为ge椭圆曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>ge椭圆曲线</returns>
    public static EllipticalArc3d ToEllipticalArc3d(this Circle cir)
    {
        return ToCurve3d(cir).ToEllipticalArc3d();
    }

    /// <summary>
    /// 将圆转换为Nurb曲线
    /// </summary>
    /// <param name="cir">圆</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Circle cir)
    {
        return new NurbCurve3d(ToEllipticalArc3d(cir));
    }

    #endregion Circle

    #region Arc

    /// <summary>
    /// 将圆弧转换为ge圆弧曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>ge圆弧曲线</returns>
    public static CircularArc3d ToCurve3d(this Arc arc)
    {
        Plane plane = new(arc.Center, arc.Normal);

        return new CircularArc3d(arc.Center, arc.Normal, plane.GetCoordinateSystem().Xaxis,
            arc.Radius, arc.StartAngle, arc.EndAngle);
    }

    /// <summary>
    /// 将圆弧转换为ge椭圆曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>ge椭圆曲线</returns>
    public static EllipticalArc3d ToEllipticalArc3d(this Arc arc)
    {
        return ToCurve3d(arc).ToEllipticalArc3d();
    }

    /// <summary>
    /// 将圆弧转换为三维Nurb曲线
    /// </summary>
    /// <param name="arc">圆弧</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Arc arc)
    {
        return new NurbCurve3d(ToEllipticalArc3d(arc));
    }

    #endregion Arc

    #region Ellipse

    /// <summary>
    /// 将椭圆转换为三维ge椭圆曲线
    /// </summary>
    /// <param name="ell">椭圆</param>
    /// <returns>三维ge椭圆曲线</returns>
    public static EllipticalArc3d ToCurve3d(this Ellipse ell)
    {
        return new EllipticalArc3d(ell.Center, ell.MajorAxis.GetNormal(), ell.MinorAxis.GetNormal(),
            ell.MajorRadius, ell.MinorRadius, ell.StartParam, ell.EndParam);
    }

    /// <summary>
    /// 将椭圆转换为三维Nurb曲线
    /// </summary>
    /// <param name="ell">椭圆</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Ellipse ell)
    {
        return new NurbCurve3d(ToCurve3d(ell));
    }

    #endregion Ellipse

    #region Spline

    /// <summary>
    /// 将样条曲线转换为三维Nurb曲线
    /// </summary>
    /// <param name="spl">样条曲线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToCurve3d(this Spline spl)
    {
        NurbCurve3d nc3d;
        var nData = spl.NurbsData;
        KnotCollection knots = [.. nData.GetKnots()];

        if (nData.Rational)
        {
            nc3d = new NurbCurve3d(nData.Degree, knots, nData.GetControlPoints(),
                nData.GetWeights(), nData.Periodic);
        }
        else
        {
            nc3d = new NurbCurve3d(nData.Degree, knots, nData.GetControlPoints(), nData.Periodic);
        }

        if (spl.HasFitData)
        {
            var fData = spl.FitData;
            var vec = new Vector3d();
            if (fData.TangentsExist && (fData.StartTangent != vec || fData.EndTangent != vec))
                nc3d.SetFitData(fData.GetFitPoints(), fData.StartTangent, fData.EndTangent);
        }

        return nc3d;
    }

    #endregion Spline

    #region Polyline2d

    /// <summary>
    /// 将二维多段线转换为三维ge曲线
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>三维ge曲线</returns>
    public static Curve3d? ToCurve3d(this Polyline2d pl2d)
    {
        switch (pl2d.PolyType)
        {
            case Poly2dType.SimplePoly:
            case Poly2dType.FitCurvePoly:
                Polyline pl = new();
                pl.SetDatabaseDefaults();
                pl.ConvertFrom(pl2d, false);
                return ToCurve3d(pl);
            default:
                return ToNurbCurve3d(pl2d);
        }

        // Polyline pl = new Polyline();
        // pl.ConvertFrom(pl2d, false);
        // return ToCurve3d(pl);
    }

    /// <summary>
    /// 将二维多段线转换为三维Nurb曲线
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Polyline2d pl2d)
    {
        switch (pl2d.PolyType)
        {
            case Poly2dType.SimplePoly:
            case Poly2dType.FitCurvePoly:
                Polyline pl = new();
                pl.SetDatabaseDefaults();
                pl.ConvertFrom(pl2d, false);
                return ToNurbCurve3d(pl);

            default:
                return ToCurve3d(pl2d.Spline);
        }
    }

    /// <summary>
    /// 将二维多段线转换为三维ge多段线
    /// </summary>
    /// <param name="pl">二维多段线</param>
    /// <returns>三维ge多段线</returns>
    public static PolylineCurve3d ToPolylineCurve3d(this Polyline2d pl)
    {
        using Point3dCollection p3dc = new();
        foreach (Vertex2d ver in pl)
            p3dc.Add(ver.Position);
        return new PolylineCurve3d(p3dc);
    }

    #endregion Polyline2d

    #region Polyline3d

    /// <summary>
    /// 将三维多段线转换为三维曲线
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>三维曲线</returns>
    public static Curve3d ToCurve3d(this Polyline3d pl3d)
    {
        return pl3d.PolyType switch
        {
            Poly3dType.SimplePoly => ToPolylineCurve3d(pl3d),
            _ => ToNurbCurve3d(pl3d),
        };
    }

    /// <summary>
    /// 将三维多段线转换为三维Nurb曲线
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>三维Nurb曲线</returns>
    public static NurbCurve3d ToNurbCurve3d(this Polyline3d pl3d)
    {
        return ToCurve3d(pl3d.Spline);
    }

    /// <summary>
    /// 将三维多段线转换为三维ge多段线
    /// </summary>
    /// <param name="pl">三维多段线</param>
    /// <returns>三维ge多段线</returns>
    public static PolylineCurve3d ToPolylineCurve3d(this Polyline3d pl)
    {
        using Point3dCollection p3dc = new();
        foreach (ObjectId id in pl)
        {
            if (id.GetObject(OpenMode.ForRead) is PolylineVertex3d ver)
                p3dc.Add(ver.Position);
        }

        return new PolylineCurve3d(p3dc);
    }

    #endregion Polyline3d

    #region Polyline

    /// <summary>
    /// 多段线转换为复合曲线
    /// </summary>
    /// <param name="pl">多段线对象</param>
    /// <returns>复合曲线对象</returns>
    public static CompositeCurve3d ToCurve3d(this Polyline pl)
    {
        List<Curve3d> c3ds = [];

        for (var i = 0; i < pl.NumberOfVertices; i++)
        {
            switch (pl.GetSegmentType(i))
            {
                case SegmentType.Line:
                    c3ds.Add(pl.GetLineSegmentAt(i));
                    break;

                case SegmentType.Arc:
                    c3ds.Add(pl.GetArcSegmentAt(i));
                    break;
            }
        }

        return new CompositeCurve3d([.. c3ds]);
    }

    /// <summary>
    /// 多段线转换为Nurb曲线
    /// </summary>
    /// <param name="pl">多段线</param>
    /// <returns>Nurb曲线</returns>
    public static NurbCurve3d? ToNurbCurve3d(this Polyline pl)
    {
        NurbCurve3d? nc3d = null;
        for (var i = 0; i < pl.NumberOfVertices; i++)
        {
            NurbCurve3d? nc3dTemp = null;
            switch (pl.GetSegmentType(i))
            {
                case SegmentType.Line:
                    nc3dTemp = new NurbCurve3d(pl.GetLineSegmentAt(i));
                    break;

                case SegmentType.Arc:
                    nc3dTemp = pl.GetArcSegmentAt(i).ToNurbCurve3d();
                    break;
            }

            if (nc3d is null)
                nc3d = nc3dTemp;
            else if (nc3dTemp is not null)
                nc3d.JoinWith(nc3dTemp);
        }

        return nc3d;
    }

    /// <summary>
    /// 为优化多段线倒角
    /// </summary>
    /// <param name="polyline">优化多段线</param>
    /// <param name="index">顶点索引号</param>
    /// <param name="radius">倒角半径</param>
    /// <param name="isFillet">倒角类型</param>
    public static void ChamferAt(this Polyline polyline, int index, double radius, bool isFillet)
    {
        if (index < 1 || index > polyline.NumberOfVertices - 2)
            throw new Exception("错误的索引号");

        if (SegmentType.Line != polyline.GetSegmentType(index - 1) ||
            SegmentType.Line != polyline.GetSegmentType(index))
            throw new Exception("非直线段不能倒角");

        // 获取当前索引号的前后两段直线,并组合为Ge复合曲线
        Curve3d[] c3ds =
        [
            polyline.GetLineSegmentAt(index - 1),
            polyline.GetLineSegmentAt(index)
        ];
        CompositeCurve3d cc3d = new(c3ds);

        // 试倒直角
        // 子曲线的个数有三种情况:
        // 1、=3时倒角方向正确
        // 2、=2时倒角方向相反
        // 3、=0或为直线时失败
        c3ds = cc3d.GetTrimmedOffset(radius, Vector3d.ZAxis, OffsetCurveExtensionType.Chamfer);

        if (c3ds.Length > 0 && c3ds[0] is CompositeCurve3d)
        {
            var newCc3d = c3ds[0] as CompositeCurve3d;
            c3ds = newCc3d!.GetCurves();
            if (c3ds.Length == 3)
            {
                c3ds = cc3d.GetTrimmedOffset(-radius, Vector3d.ZAxis,
                    OffsetCurveExtensionType.Chamfer);
                if (c3ds.Length == 0 || c3ds[0] is LineSegment3d)
                    throw new Exception("倒角半径过大");
            }
            else if (c3ds.Length == 2)
            {
                radius = -radius;
            }
        }
        else
        {
            throw new Exception("倒角半径过大");
        }

        // GetTrimmedOffset会生成倒角+偏移，故先反方向倒角,再倒回
        c3ds = cc3d.GetTrimmedOffset(-radius, Vector3d.ZAxis, OffsetCurveExtensionType.Extend);
        var type = isFillet ? OffsetCurveExtensionType.Fillet : OffsetCurveExtensionType.Chamfer;
        c3ds = c3ds[0].GetTrimmedOffset(radius, Vector3d.ZAxis, type);

        // 将结果Ge曲线转为Db曲线,并将相关的数值反映到原曲线
        if (c3ds[0].ToCurve() is not Polyline plTemp)
            return;
        polyline.RemoveVertexAt(index);
        polyline.AddVertexAt(index, plTemp.GetPoint2dAt(1), plTemp.GetBulgeAt(1), 0, 0);
        polyline.AddVertexAt(index + 1, plTemp.GetPoint2dAt(2), 0, 0, 0);
    }

    #endregion Polyline

    #endregion
}