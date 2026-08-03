namespace Test;

public class TestGeometryQuery
{
    private const double Tolerance = 1e-8;

    [CommandMethod(nameof(Test_GeometryQuery))]
    public void Test_GeometryQuery()
    {
        var startPoint = new Point3d(0, 0, 0);
        var endPoint = new Point3d(10, 20, 30);

        AssertPoint(startPoint.InterpolateTo(endPoint, 0.25), new Point3d(2.5, 5, 7.5),
            nameof(PointEx.InterpolateTo));
        AssertTrue(startPoint.TryInterpolateAtElevation(endPoint, 15, out var elevationPoint),
            nameof(PointEx.TryInterpolateAtElevation));
        AssertPoint(elevationPoint, new Point3d(5, 10, 15), nameof(PointEx.TryInterpolateAtElevation));
        AssertFalse(startPoint.TryInterpolateAtElevation(new Point3d(10, 20, 0), 0, out _),
            "Horizontal segment must not return an arbitrary point.");
        AssertFalse(startPoint.TryInterpolateAtElevation(endPoint, 31, out _),
            "Elevation outside the segment must fail.");

        using var line = new Line(startPoint, endPoint);
        AssertPoint(line.GetPointAtDistanceFraction(0.25), new Point3d(2.5, 5, 7.5),
            nameof(CurveEx.GetPointAtDistanceFraction));
        AssertThrowsArgumentOutOfRange(() => line.GetPointAtDistanceFraction(1.01),
            "Curve distance fraction range");
        AssertClose(line.GetMidpointChordDeviation(line.StartParam, line.EndParam), 0,
            nameof(CurveEx.GetMidpointChordDeviation));
        AssertClose(line.GetMidpointChordDeviationByDistance(0, line.Length), 0,
            nameof(CurveEx.GetMidpointChordDeviationByDistance));

        using var arc = new Arc(Point3d.Origin, 10, 0, Math.PI);
        AssertClose(arc.GetMidpointChordDeviation(arc.StartParam, arc.EndParam), 10,
            "Semicircle parameter midpoint deviation");
        AssertClose(arc.GetMidpointChordDeviationByDistance(0, Math.PI * 10), 10,
            "Semicircle distance midpoint deviation");

        using var polyline = new Polyline();
        polyline.AddVertexAt(0, new Point2d(0, 0), 1, 2, 3);
        polyline.AddVertexAt(1, new Point2d(10, 0), 0, 4, 5);
        polyline.AddVertexAt(2, new Point2d(10, 10), 0, 0, 0);

        var vertexData = polyline.GetVertexData();
        AssertTrue(vertexData.Count == 3, "Vertex snapshot count");
        AssertPoint(vertexData[0].Vertex.Point3d(), Point3d.Origin, "Vertex snapshot position");
        AssertClose(vertexData[0].Bulge, 1, "Vertex snapshot bulge");
        AssertClose(vertexData[0].StartWidth, 2, "Vertex snapshot start width");
        AssertClose(vertexData[0].EndWidth, 3, "Vertex snapshot end width");
        vertexData[0].X = 100;
        AssertPoint(polyline.GetPoint3dAt(0), Point3d.Origin, "Vertex snapshot independence");
        AssertClose(polyline.GetSegmentLength(0), Math.PI * 5, "Arc segment length");
        AssertClose(polyline.GetSegmentLength(1), 10, "Line segment length");
        AssertThrowsArgumentOutOfRange(() => polyline.GetSegmentLength(2), "Open polyline segment range");

        polyline.Closed = true;
        AssertClose(polyline.GetSegmentLength(2), Math.Sqrt(200), "Closing segment length");

        using var degeneratePolyline = new Polyline { Closed = true };
        degeneratePolyline.AddVertexAt(0, Point2d.Origin, 0, 0, 0);
        AssertThrowsArgumentOutOfRange(() => degeneratePolyline.GetSegmentLength(0),
            "Degenerate closed polyline segment range");

        Env.Printl("Test_GeometryQuery passed.");
    }

    private static void AssertPoint(Point3d actual, Point3d expected, string name)
    {
        if (actual.DistanceTo(expected) > Tolerance)
            throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}.");
    }

    private static void AssertClose(double actual, double expected, string name)
    {
        if (Math.Abs(actual - expected) > Tolerance)
            throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}.");
    }

    private static void AssertTrue(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException($"{name}: expected true.");
    }

    private static void AssertFalse(bool condition, string name)
    {
        if (condition)
            throw new InvalidOperationException($"{name}: expected false.");
    }

    private static void AssertThrowsArgumentOutOfRange(Action action, string name)
    {
        try
        {
            action();
        }
        catch (ArgumentOutOfRangeException)
        {
            return;
        }

        throw new InvalidOperationException($"{name}: expected ArgumentOutOfRangeException.");
    }
}
