namespace Fs.Fox.Cad;

/// <summary>
/// 多段线扩展类
/// </summary>
public static class PolylineEx
{
    #region 获取多段线端点

    /// <summary>
    /// 获取二维多段线的端点坐标
    /// </summary>
    /// <param name="pl2d">二维多段线</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline2d pl2d)
    {
        var tr = DBTrans.GetTopTransaction(pl2d.Database);
        foreach (ObjectId id in pl2d)
        {
            if (tr.GetObject(id) is Vertex2d vertex)
            {
                yield return vertex.Position;
            }
        }
    }

    /// <summary>
    /// 获取三维多段线的端点坐标
    /// </summary>
    /// <param name="pl3d">三维多段线</param>
    /// <returns>端点坐标集合</returns>
    public static IEnumerable<Point3d> GetPoints(this Polyline3d pl3d)
    {
        var tr = DBTrans.GetTopTransaction(pl3d.Database);
        foreach (ObjectId id in pl3d)
        {
            if (tr.GetObject(id) is PolylineVertex3d vertex)
                yield return vertex.Position;
        }
    }

    /// <summary>
    /// 获取多段线的端点坐标
    /// </summary>
    /// <param name="pl">多段线</param>
    /// <returns>端点坐标集合</returns>
    public static List<Point3d> GetPoints(this Polyline pl)
    {
        return
            Enumerable
                .Range(0, pl.NumberOfVertices)
                .Select(pl.GetPoint3dAt)
                .ToList();
    }

    /// <summary>
    /// 获取轻量多段线的顶点、凸度和宽度快照
    /// </summary>
    /// <param name="pl">轻量多段线</param>
    /// <returns>按顶点索引排列的独立数据对象；修改返回值不会修改原多段线</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pl"/> 为 <see langword="null"/></exception>
    public static IReadOnlyList<BulgeVertexWidth> GetVertexData(this Polyline pl)
    {
        if (pl is null)
            throw new ArgumentNullException(nameof(pl));

        var vertices = new List<BulgeVertexWidth>(pl.NumberOfVertices);
        for (var index = 0; index < pl.NumberOfVertices; index++)
            vertices.Add(new BulgeVertexWidth(pl, index));

        return vertices;
    }

    /// <summary>
    /// 获取轻量多段线指定子段的实际长度
    /// </summary>
    /// <remarks>直线段返回直线长度，圆弧段返回沿弧长度。</remarks>
    /// <param name="pl">轻量多段线</param>
    /// <param name="segmentIndex">从零开始的子段索引</param>
    /// <returns>指定子段的长度</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pl"/> 为 <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segmentIndex"/> 不是有效子段索引</exception>
    public static double GetSegmentLength(this Polyline pl, int segmentIndex)
    {
        if (pl is null)
            throw new ArgumentNullException(nameof(pl));

        var segmentCount = GetSegmentCount(pl);
        if (segmentIndex < 0 || segmentIndex >= segmentCount)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex,
                "The polyline does not contain the requested segment.");
        }

        return pl.GetDistanceAtParameter(segmentIndex + 1) - pl.GetDistanceAtParameter(segmentIndex);
    }

    /// <summary>
    /// 按沿折线的最大间距获取采样点
    /// </summary>
    /// <remarks>
    /// 每个原始子段独立等距细分，因此所有原始顶点都会保留。开放折线的结果从首顶点到末顶点；
    /// 闭合折线会在结果末尾再次包含首顶点，以完整表示闭合路径。返回值是独立的点快照，不修改原折线。
    /// </remarks>
    /// <param name="pl">轻量多段线</param>
    /// <param name="maximumSpacing">相邻采样点沿折线的最大距离，必须是有限正数</param>
    /// <returns>按折线行进方向排列的采样点快照</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pl"/> 为 <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumSpacing"/> 不是有限正数</exception>
    /// <exception cref="InvalidOperationException">折线距离域无效，或阈值要求的细分数量无法表示</exception>
    public static IReadOnlyList<Point3d> GetSamplePointsByDistance(this Polyline pl, double maximumSpacing)
    {
        if (pl is null)
            throw new ArgumentNullException(nameof(pl));
        ValidatePositiveFinite(maximumSpacing, nameof(maximumSpacing));

        return GetSegmentSamplePoints(pl, (_, segmentLength) =>
            GetSubdivisionCount(segmentLength / maximumSpacing));
    }

    /// <summary>
    /// 按圆弧段的最大弦偏差获取采样点
    /// </summary>
    /// <remarks>
    /// 直线段只保留端点；圆弧段按弓高公式等弧长细分，使每个采样子弧的弦偏差不超过指定值。
    /// 所有原始顶点都会保留。闭合折线会在结果末尾再次包含首顶点。返回值是独立的点快照，不修改原折线。
    /// </remarks>
    /// <param name="pl">轻量多段线</param>
    /// <param name="maximumChordDeviation">圆弧采样子段允许的最大弦偏差，必须是有限正数</param>
    /// <returns>按折线行进方向排列的采样点快照</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pl"/> 为 <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumChordDeviation"/> 不是有限正数</exception>
    /// <exception cref="InvalidOperationException">圆弧或折线距离域无效，或阈值要求的细分数量无法表示</exception>
    public static IReadOnlyList<Point3d> GetSamplePointsByChordDeviation(this Polyline pl,
        double maximumChordDeviation)
    {
        if (pl is null)
            throw new ArgumentNullException(nameof(pl));
        ValidatePositiveFinite(maximumChordDeviation, nameof(maximumChordDeviation));

        return GetSegmentSamplePoints(pl, (segmentIndex, segmentLength) =>
        {
            if (segmentLength == 0 || pl.GetSegmentType(segmentIndex) != SegmentType.Arc)
                return 1;

            using var arc = pl.GetArcSegment2dAt(segmentIndex);
            var radius = arc.Radius;
            if (double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0)
                throw new InvalidOperationException("The polyline contains an arc with an invalid radius.");

            var deviationRatio = maximumChordDeviation / radius * 0.5;
            var maximumAngle = deviationRatio >= 1
                ? Math.PI * 2
                : 4 * Math.Asin(Math.Sqrt(deviationRatio));
            if (double.IsNaN(maximumAngle) || double.IsInfinity(maximumAngle) || maximumAngle <= 0)
            {
                throw new InvalidOperationException(
                    "The chord deviation requires a subdivision count that cannot be represented.");
            }

            var includedAngle = segmentLength / radius;
            return GetSubdivisionCount(includedAngle / maximumAngle);
        });
    }

    private static IReadOnlyList<Point3d> GetSegmentSamplePoints(Polyline pl,
        Func<int, double, int> getSubdivisionCount)
    {
        var vertexCount = pl.NumberOfVertices;
        if (vertexCount == 0)
            return Array.Empty<Point3d>();

        var points = new List<Point3d> { pl.GetPoint3dAt(0) };
        var segmentCount = GetSegmentCount(pl);
        for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            var startDistance = pl.GetDistanceAtParameter(segmentIndex);
            var endDistance = pl.GetDistanceAtParameter(segmentIndex + 1);
            if (double.IsNaN(startDistance) || double.IsInfinity(startDistance) || startDistance < 0 ||
                double.IsNaN(endDistance) || double.IsInfinity(endDistance) || endDistance < startDistance)
            {
                throw new InvalidOperationException(
                    "The polyline does not have a finite, non-negative, ordered distance range.");
            }

            var segmentLength = endDistance - startDistance;
            var subdivisionCount = getSubdivisionCount(segmentIndex, segmentLength);
            if (subdivisionCount < 1)
                throw new InvalidOperationException("The subdivision count must be positive.");

            for (var sampleIndex = 1; sampleIndex < subdivisionCount; sampleIndex++)
            {
                var fraction = (double)sampleIndex / subdivisionCount;
                var distance = startDistance * (1 - fraction) + endDistance * fraction;
                points.Add(pl.GetPointAtDist(distance));
            }

            points.Add(pl.GetPoint3dAt((segmentIndex + 1) % vertexCount));
        }

        return points;
    }

    private static int GetSegmentCount(Polyline pl)
    {
        return pl.NumberOfVertices < 2
            ? 0
            : pl.Closed ? pl.NumberOfVertices : pl.NumberOfVertices - 1;
    }

    private static int GetSubdivisionCount(double requiredCount)
    {
        var subdivisionCount = Math.Ceiling(requiredCount);
        if (double.IsNaN(subdivisionCount) || double.IsInfinity(subdivisionCount) ||
            subdivisionCount > int.MaxValue)
        {
            throw new InvalidOperationException(
                "The sampling threshold requires a subdivision count that cannot be represented.");
        }

        return Math.Max(1, (int)subdivisionCount);
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be finite and greater than zero.");
    }

    #endregion

    #region 创建多段线

    /// <summary>
    /// 根据点集创建多段线<br/>
    /// 此多段线无默认全局宽度0，无圆弧段
    /// </summary>
    /// <param name="points">点集</param>
    /// <param name="action">多段线属性设置委托</param>
    /// <returns>多段线对象</returns>
    public static Polyline CreatePolyline(this IEnumerable<Point3d> points, Action<Polyline>? action = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        points.ForEach((index, pt) => { pl.AddVertexAt(index, pt.Point2d(), 0, 0, 0); });
        action?.Invoke(pl);
        return pl;
    }

    /// <summary>
    /// 根据点集创建多段线
    /// </summary>
    /// <param name="pts">端点表,利用元组(Point3d pt, double bulge, double startWidth, double endWidth)</param>
    /// <param name="action">轻多段线属性设置委托</param>
    /// <returns>轻多段线对象</returns>
    public static Polyline CreatePolyline(
        this IEnumerable<(Point3d pt, double bulge, double startWidth, double endWidth)> pts,
        Action<Polyline>? action = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();

        pts.ForEach((index, vertex) =>
        {
            pl.AddVertexAt(index, vertex.pt.Point2d(), vertex.bulge, vertex.startWidth, vertex.endWidth);
        });
        action?.Invoke(pl);
        return pl;
    }

    /// <summary>
    /// 根据Extents3d创建多段线<br/>
    /// 此多段线无默认全局宽度0，无圆弧段，标高为0
    /// </summary>
    /// <param name="points">Extents3d</param>
    /// <param name="action">多段线属性设置委托</param>
    /// <returns>多段线对象</returns>
    public static Polyline CreatePolyline(this Extents3d points, Action<Polyline>? action = null)
    {
        List<Point2d> pts = 
        [
            points.MinPoint.Point2d(),
            new(points.MinPoint.X, points.MaxPoint.Y),
            points.MaxPoint.Point2d(),
            new(points.MaxPoint.X, points.MinPoint.Y)
        ];
        
        Polyline pl = new() { Closed = true };
        pl.SetDatabaseDefaults();
        pts.ForEach((index, pt) => { pl.AddVertexAt(index, pt, 0, 0, 0); });
        action?.Invoke(pl);
        return pl;
    }
    
    
    /// <summary>
    /// 点表生成多段线
    /// </summary>
    /// <param name="pointList">点表</param>
    /// <param name="plineWidth">线宽</param>
    /// <param name="closed">是否闭合</param>
    /// <returns>Polyline</returns>
    public static Polyline ToPolyline(this IEnumerable<Point2d> pointList, double plineWidth = 0, bool closed = false)
    {
        var pl = new Polyline();
        var enumerable = pointList.ToList();
        for (var i = 0; i < enumerable.Count; i++)
        {
            pl.AddVertexAt(i, enumerable.ElementAt(i), 0, plineWidth, plineWidth);
        }

        pl.Closed = closed;
        return pl;
    }

    /// <summary>
    /// 点表生成多段线
    /// </summary>
    /// <param name="pointList">点表</param>
    /// <param name="plineWidth">线宽</param>
    /// <param name="closed">是否闭合</param>
    /// <returns>Polyline</returns>
    public static Polyline ToPolyline(this IEnumerable<Point3d> pointList, double plineWidth = 0, bool closed = false)
    {
        var pl = new Polyline();
        var enumerable = pointList.ToList();
        for (var i = 0; i < enumerable.Count; i++)
        {
            pl.AddVertexAt(i, enumerable.ElementAt(i).Point2d(), 0, plineWidth, plineWidth);
        }

        pl.Closed = closed;
        return pl;
    }

    #endregion
}
