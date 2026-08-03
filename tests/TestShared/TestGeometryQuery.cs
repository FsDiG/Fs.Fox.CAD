namespace Test;

public class TestGeometryQuery
{
    private const double Tolerance = 1e-6;

    [CommandMethod(nameof(Test_GeometryQuery))]
    public void Test_GeometryQuery()
    {
        var startPoint = new Point3d(0, 0, 0);
        var endPoint = new Point3d(10, 20, 30);

        AssertPoint(startPoint.InterpolateTo(endPoint, 0.25), new Point3d(2.5, 5, 7.5),
            nameof(PointEx.InterpolateTo));

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

        using var ray = new Ray();
        ray.BasePoint = Point3d.Origin;
        ray.UnitDir = Vector3d.XAxis;
        AssertThrowsInvalidOperation(() => ray.GetPointAtDistanceFraction(0.5),
            "Ray has no finite total length");
        AssertClose(ray.GetMidpointChordDeviation(0, 1), 0,
            "Ray parameter midpoint deviation");
        AssertClose(ray.GetMidpointChordDeviationByDistance(0, 1), 0,
            "Ray distance midpoint deviation");
        AssertThrowsArgumentOutOfRange(() => ray.GetMidpointChordDeviation(-1, 1),
            "Ray parameter range");
        AssertThrowsArgumentOutOfRange(() => ray.GetMidpointChordDeviationByDistance(-1, 1),
            "Ray distance range");

        using var xline = new Xline();
        xline.BasePoint = Point3d.Origin;
        xline.UnitDir = Vector3d.XAxis;
        AssertThrowsInvalidOperation(() => xline.GetPointAtDistanceFraction(0.5),
            "Xline has no finite total length");
        AssertClose(xline.GetMidpointChordDeviation(-1, 1), 0,
            "Xline parameter midpoint deviation");
        AssertThrowsInvalidOperation(() => xline.GetMidpointChordDeviationByDistance(0, 1),
            "Xline has no distance origin");

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

        var distanceSamples = polyline.GetSamplePointsByDistance(6);
        AssertTrue(distanceSamples.Count == 6, "Distance sampling count");
        AssertPoint(distanceSamples[0], polyline.GetPoint3dAt(0), "Distance sampling start vertex");
        AssertPoint(distanceSamples[3], polyline.GetPoint3dAt(1), "Distance sampling preserved arc vertex");
        AssertPoint(distanceSamples[5], polyline.GetPoint3dAt(2), "Distance sampling end vertex");
        for (var index = 1; index < distanceSamples.Count; index++)
        {
            AssertTrue(distanceSamples[index - 1].DistanceTo(distanceSamples[index]) <= 6 + Tolerance,
                "Distance sampling maximum spacing");
        }

        var chordSamples = polyline.GetSamplePointsByChordDeviation(1.5);
        AssertTrue(chordSamples.Count == 4, "Chord deviation sampling count");
        AssertPoint(chordSamples[0], polyline.GetPoint3dAt(0), "Chord sampling start vertex");
        AssertPoint(chordSamples[2], polyline.GetPoint3dAt(1), "Chord sampling preserved arc vertex");
        AssertPoint(chordSamples[3], polyline.GetPoint3dAt(2), "Chord sampling end vertex");
        AssertClose(chordSamples[1].DistanceTo(new Point3d(5, 0, 0)), 5,
            "Semicircle chord sampling midpoint deviation");
        AssertClose(polyline.GetBulgeAt(0), 1, "Sampling leaves bulge unchanged");
        AssertClose(polyline.GetStartWidthAt(0), 2, "Sampling leaves start width unchanged");
        AssertClose(polyline.GetEndWidthAt(0), 3, "Sampling leaves end width unchanged");

        AssertThrowsArgumentOutOfRange(() => polyline.GetSamplePointsByDistance(0),
            "Distance sampling positive threshold");
        AssertThrowsArgumentOutOfRange(() => polyline.GetSamplePointsByDistance(double.NaN),
            "Distance sampling finite threshold");
        AssertThrowsArgumentOutOfRange(() => polyline.GetSamplePointsByChordDeviation(double.PositiveInfinity),
            "Chord sampling finite threshold");
        AssertThrowsInvalidOperation(() => polyline.GetSamplePointsByDistance(double.Epsilon),
            "Distance sampling representable count");
        AssertThrowsInvalidOperation(() => polyline.GetSamplePointsByChordDeviation(double.Epsilon),
            "Chord sampling representable count");
        AssertThrowsInvalidOperation(() => polyline.GetSamplePointsByDistance(1e-6),
            "Distance sampling total point limit");
        AssertThrowsInvalidOperation(() => polyline.GetSamplePointsByChordDeviation(1e-12),
            "Chord sampling total point limit");

        polyline.Closed = true;
        AssertClose(polyline.GetSegmentLength(2), Math.Sqrt(200), "Closing segment length");
        var closedSamples = polyline.GetSamplePointsByDistance(100);
        AssertTrue(closedSamples.Count == 4, "Closed sampling count");
        AssertPoint(closedSamples[0], closedSamples[closedSamples.Count - 1],
            "Closed sampling repeats first vertex");

        using var orientedPolyline = new Polyline
        {
            Elevation = 7,
            Normal = Vector3d.YAxis
        };
        orientedPolyline.AddVertexAt(0, Point2d.Origin, 0, 0, 0);
        orientedPolyline.AddVertexAt(1, new Point2d(10, 0), 0, 0, 0);
        var orientedSamples = orientedPolyline.GetSamplePointsByDistance(4);
        AssertPoint(orientedSamples[0], orientedPolyline.GetPoint3dAt(0),
            "Oriented sampling preserves start point");
        AssertPoint(orientedSamples[orientedSamples.Count - 1], orientedPolyline.GetPoint3dAt(1),
            "Oriented sampling preserves end point");

        using var degeneratePolyline = new Polyline { Closed = true };
        degeneratePolyline.AddVertexAt(0, Point2d.Origin, 0, 0, 0);
        AssertThrowsArgumentOutOfRange(() => degeneratePolyline.GetSegmentLength(0),
            "Degenerate closed polyline segment range");
        AssertTrue(degeneratePolyline.GetSamplePointsByDistance(1).Count == 1,
            "Degenerate distance sampling");
        AssertTrue(degeneratePolyline.GetSamplePointsByChordDeviation(1).Count == 1,
            "Degenerate chord sampling");

        using var emptyPolyline = new Polyline();
        AssertTrue(emptyPolyline.GetSamplePointsByDistance(1).Count == 0, "Empty distance sampling");
        AssertTrue(emptyPolyline.GetSamplePointsByChordDeviation(1).Count == 0, "Empty chord sampling");

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

    private static void AssertThrowsInvalidOperation(Action action, string name)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        throw new InvalidOperationException($"{name}: expected InvalidOperationException.");
    }
}
