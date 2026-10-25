namespace Test;

/// <summary>排序 Beta 的 CAD 适配验收入口；该命令不创建或修改图纸实体。</summary>
public class TestOrderingBeta
{
    /// <summary>在当前实际宿主运行排序、最近点适配和范围结果测试。</summary>
    [CommandMethod(nameof(Test_OrderingBeta))]
    public void Test_OrderingBeta()
    {
        var points = new[] { new Point2d(0, 0), new Point2d(10, 0.75), new Point2d(0, 1.5) };
        AssertOrder(PointOrderingBeta.GetRowOrder(points, 1), 2, 1, 0);
        AssertOrder(PointOrderingBeta.GetRowOrder(points, 1, topToBottom: false), 0, 2, 1);
        if (points[0] != new Point2d(0, 0))
            throw new InvalidOperationException("Point input changed.");

        using var line = new Line(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
        var locations = new[]
        {
            new Point3d(5, 4, 0), new Point3d(-2, 1, 0),
            new Point3d(5, -3, 2), new Point3d(12, 0, 0)
        };
        AssertOrder(line.GetOrderAlongCurveBeta(locations), 1, 0, 2, 3);
        AssertOrder(line.GetOrderAlongCurveBeta(locations, reverse: true), 3, 0, 2, 1);
        AssertOrder(line.GetOrderAlongCurveBeta(Array.Empty<Point3d>()));
        if (line.StartPoint != Point3d.Origin || line.EndPoint != new Point3d(10, 0, 0))
            throw new InvalidOperationException("Curve input changed.");

        using var polyline = new Polyline();
        polyline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
        polyline.AddVertexAt(1, new Point2d(10, 0), 0, 0, 0);
        polyline.AddVertexAt(2, new Point2d(10, 10), 0, 0, 0);
        polyline.AddVertexAt(3, new Point2d(0, 10), 0, 0, 0);
        polyline.Closed = true;
        AssertOrder(polyline.GetOrderAlongCurveBeta(new[]
        {
            new Point3d(5, 10, 0), new Point3d(5, 0, 0), new Point3d(0, 5, 0), new Point3d(10, 5, 0)
        }), 1, 3, 0, 2);

        using var ray = new Ray();
        try
        {
            ray.GetOrderAlongCurveBeta(Array.Empty<Point3d>());
            throw new InvalidOperationException("Ray was accepted.");
        }
        catch (NotSupportedException) { }

        var groups = ExtentGroupingBeta.GroupByProximity(new[]
        {
            new Extents3d(new Point3d(0, 0, -2), new Point3d(1, 4, 0)),
            new Extents3d(new Point3d(1, 3, 1), new Point3d(4, 4, 3)),
            new Extents3d(new Point3d(3, 0, 0), new Point3d(4, 1, 1))
        });
        if (groups.Count != 2 || groups[0].Bounds.MinPoint != new Point3d(0, 0, -2) ||
            groups[0].Bounds.MaxPoint != new Point3d(4, 4, 3))
            throw new InvalidOperationException("Extent grouping mismatch.");
        AssertOrder(groups[0].SourceIndices, 0, 1);
        AssertOrder(groups[1].SourceIndices, 2);
        Env.Printl("Ordering Beta CAD adapter checks passed.");
    }

    private static void AssertOrder(IReadOnlyList<int> actual, params int[] expected)
    {
        if (!actual.SequenceEqual(expected))
            throw new InvalidOperationException("Ordering Beta result mismatch.");
    }
}
