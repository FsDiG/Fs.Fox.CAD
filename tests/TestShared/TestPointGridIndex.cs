namespace Test;

public class TestPointGridIndex
{
    private const double Tolerance = 1e-6;

    [CommandMethod(nameof(Test_PointGridIndex))]
    public void Test_PointGridIndex()
    {
        AssertThrowsArgumentOutOfRange(() => _ = new PointGridIndex(0), "Zero cell size");
        AssertThrowsArgumentOutOfRange(() => _ = new PointGridIndex(double.PositiveInfinity),
            "Infinite cell size");

        var index = new PointGridIndex(1);
        var firstIndex = index.Add(new Point3d(0.95, 0, 0));
        AssertTrue(firstIndex == 0, "First point index");

        var added = index.TryAdd(new Point3d(1.05, 0, 0.05), 0.11, 0.1, out var nearIndex);
        AssertFalse(added, "Cross-cell approximate duplicate");
        AssertTrue(nearIndex == firstIndex, "Approximate duplicate index");

        added = index.TryAdd(new Point3d(1.05, 0, 2), 0.11, 0.5, out var elevatedIndex);
        AssertTrue(added, "Z-separated point addition");
        AssertTrue(elevatedIndex == 1, "Z-separated point index");

        var negativeIndex = index.Add(new Point3d(-1, -1, 0));
        var farIndex = index.Add(new Point3d(3, 3, 0));
        AssertTrue(index.Count == 4 && index.Points.Count == 4, "Point count and read-only view");
        AssertPoint(index[firstIndex], new Point3d(0.95, 0, 0), "Point indexer");

        AssertTrue(index.TryFind(new Point3d(1.04, 0, 2.1), 0.1, 0.2, out var foundIndex),
            "Approximate find");
        AssertTrue(foundIndex == elevatedIndex, "Approximate find index");
        AssertFalse(index.TryFind(new Point3d(1.04, 0, 2.1), 0.1, 0.05, out _),
            "Approximate find Z boundary");

        var rangeIndices = index.QueryIndices(new Point2d(-1, -1), new Point2d(1, 1));
        AssertTrue(rangeIndices.Count == 2, "Inclusive XY range count");
        AssertTrue(rangeIndices[0] == firstIndex && rangeIndices[1] == negativeIndex,
            "Range results use insertion order");
        AssertFalse(rangeIndices.Contains(elevatedIndex) || rangeIndices.Contains(farIndex),
            "Range excludes outside points");
        var boundaryIndices = index.QueryIndices(new Point2d(-1, -1), new Point2d(-1, -1));
        AssertTrue(boundaryIndices.Count == 1 && boundaryIndices[0] == negativeIndex,
            "Range includes exact boundary point");
        var extremeRangeIndices = index.QueryIndices(
            new Point2d(-double.MaxValue, -double.MaxValue),
            new Point2d(double.MaxValue, double.MaxValue));
        AssertTrue(extremeRangeIndices.Count == index.Count &&
                   extremeRangeIndices[0] == firstIndex &&
                   extremeRangeIndices[extremeRangeIndices.Count - 1] == farIndex,
            "Unmappable finite range falls back to exact point filtering");
        AssertThrowsArgumentOutOfRange(
            () => index.QueryIndices(new Point2d(1, 0), new Point2d(0, 1)),
            "Reversed range");

        AssertTrue(index.TryGetNearest(new Point3d(1, 0, 0), 0.2, 0.1,
                out var nearestIndex, out var distance),
            "Nearest point query");
        AssertTrue(nearestIndex == firstIndex, "Nearest point index");
        AssertClose(distance, 0.05, "Nearest point distance");
        AssertFalse(index.TryGetNearest(new Point3d(1, 0, 0), 0.01, double.PositiveInfinity,
                out _, out _),
            "Nearest point maximum distance");
        AssertThrowsArgumentOutOfRange(
            () => index.TryFind(Point3d.Origin, -1, 0, out _),
            "Negative XY tolerance");

        var tieIndex = new PointGridIndex(1);
        tieIndex.Add(new Point3d(-1, 0, 0));
        tieIndex.Add(new Point3d(1, 0, 0));
        AssertTrue(tieIndex.TryGetNearest(Point3d.Origin, 1, 0, out var tieResult, out _),
            "Nearest point tie query");
        AssertTrue(tieResult == 0, "Nearest point tie uses insertion order");

        index.Clear();
        AssertTrue(index.Count == 0 && index.Points.Count == 0, "Clear");
        AssertTrue(index.Add(Point3d.Origin) == 0, "Index restarts after clear");

        Env.Printl("Test_PointGridIndex passed.");
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
