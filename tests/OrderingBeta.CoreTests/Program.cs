using Fs.Fox.Cad;
using Point = Fs.Fox.Cad.OrderingBetaCore.Point;
using Box = Fs.Fox.Cad.OrderingBetaCore.Box;

var passed = 0;
var cases = new (string Name, Action Body)[]
{
    ("empty input", () =>
    {
        Equal(OrderingBetaCore.GetGridOrder(Array.Empty<Point>(), 0, true, true, true));
        Equal(OrderingBetaCore.GetScalarOrder(Array.Empty<double>(), false));
        Check(OrderingBetaCore.GroupExtents(Array.Empty<Box>(), 0).Length == 0);
    }),
    ("rows with anchor tolerance", () => Equal(Rows(new[]
    {
        new Point(3, 10), new Point(1, 9.5), new Point(2, 8.8), new Point(0, 8.1)
    }, 1), 1, 0, 3, 2)),
    ("row membership does not chain", () => Equal(Rows(new[]
    {
        new Point(0, 0), new Point(10, 0.75), new Point(0, 1.5)
    }, 1), 2, 1, 0)),
    ("reversing row direction preserves membership", () => Equal(Rows(new[]
    {
        new Point(0, 0), new Point(10, 0.75), new Point(0, 1.5)
    }, 1, false), 0, 2, 1)),
    ("column membership does not chain", () => Equal(Columns(new[]
    {
        new Point(0, 0), new Point(0.75, 10), new Point(1.5, 0)
    }, 1), 1, 0, 2)),
    ("reversing column direction preserves membership", () => Equal(Columns(new[]
    {
        new Point(0, 0), new Point(0.75, 10), new Point(1.5, 0)
    }, 1, false), 2, 1, 0)),
    ("within-row reverse direction", () => Equal(Rows(new[]
    {
        new Point(1, 0), new Point(3, 0), new Point(2, 0)
    }, 0, true, false), 1, 2, 0)),
    ("within-column reverse direction", () => Equal(Columns(new[]
    {
        new Point(0, 1), new Point(0, 3), new Point(0, 2)
    }, 0, true, false), 0, 2, 1)),
    ("identical points remain stable and complete", () => Equal(Rows(new[]
    {
        new Point(2, 3), new Point(2, 3), new Point(1, 3), new Point(2, 3)
    }, 0), 2, 0, 1, 3)),
    ("zero tolerance creates exact rows", () => Equal(Rows(new[]
    {
        new Point(0, 0), new Point(2, 0.1), new Point(1, 0)
    }, 0), 1, 0, 2)),
    ("tolerance boundary included", () => Equal(Rows(new[]
    {
        new Point(0, 0), new Point(2, 1), new Point(1, 2)
    }, 1), 2, 1, 0)),
    ("grid geometry order is independent of input permutation", () =>
    {
        var original = new[] { new Point(2, 9.8), new Point(1, 10), new Point(1, 8), new Point(0, 8.1) };
        var permutation = new[] { 2, 0, 3, 1 };
        var reordered = permutation.Select(i => original[i]).ToArray();
        Equal(Rows(reordered, 0.5).Select(i => permutation[i]).ToArray(), 1, 0, 3, 2);
        Check(original[0].X == 2 && original[0].Y == 9.8);
    }),
    ("scalar keys are not perturbed", () =>
    {
        var keys = new[] { 4.0, 2.0, 2.0, 2.0 + 1e-9, 2.0 };
        Equal(OrderingBetaCore.GetScalarOrder(keys, false), 1, 2, 4, 3, 0);
        Equal(OrderingBetaCore.GetScalarOrder(keys, true), 0, 3, 1, 2, 4);
        Check(keys[2] == 2.0 && keys[3] == 2.0 + 1e-9);
    }),
    ("scalar signed zeros are stable", () => Equal(
        OrderingBetaCore.GetScalarOrder(new[] { -0.0, 0.0, -1.0 }, false), 2, 0, 1)),
    ("null collections rejected", () =>
    {
        Throws<ArgumentNullException>(() => Rows(null!, 0));
        Throws<ArgumentNullException>(() => OrderingBetaCore.GetScalarOrder(null!, false));
        Throws<ArgumentNullException>(() => OrderingBetaCore.GroupExtents(null!, 0));
    }),
    ("invalid grid tolerance rejected even for empty input", () =>
    {
        foreach (var tolerance in new[] { -1.0, double.NaN, double.PositiveInfinity })
            Throws<ArgumentOutOfRangeException>(() => Rows(Array.Empty<Point>(), tolerance));
    }),
    ("nonfinite point and key rejected", () =>
    {
        foreach (var value in new[] { double.NaN, double.NegativeInfinity, double.PositiveInfinity })
        {
            Throws<ArgumentException>(() => Rows(new[] { new Point(value, 0) }, 0));
            Throws<ArgumentException>(() => Rows(new[] { new Point(0, value) }, 0));
            Throws<ArgumentException>(() => OrderingBetaCore.GetScalarOrder(new[] { value }, false));
        }
    }),
    ("extreme finite point coordinates", () => Equal(Rows(new[]
    {
        new Point(0, -double.MaxValue), new Point(0, double.MaxValue)
    }, double.MaxValue), 1, 0)),
    ("touching boxes join at zero gap", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[] { B(0, 0, 1, 1), B(1, 1, 2, 2) }, 0);
        Check(groups.Length == 1);
        Equal(groups[0].Indices, 0, 1);
        Bounds(groups[0].Bounds, 0, 0, 0, 2, 2, 1);
    }),
    ("diagonal uses Euclidean distance", () =>
    {
        var boxes = new[] { B(0, 0, 1, 1), B(2, 2, 3, 3) };
        Check(OrderingBetaCore.GroupExtents(boxes, 1.4).Length == 2);
        Check(OrderingBetaCore.GroupExtents(boxes, 1.5).Length == 1);
    }),
    ("exact three-four-five gap included", () =>
    {
        Check(OrderingBetaCore.GroupExtents(new[] { B(0, 0, 0, 0), B(3, 4, 3, 4) }, 5).Length == 1);
    }),
    ("original adjacency is transitive", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[] { B(0, 0, 1, 1), B(2, 0, 3, 1), B(4, 0, 5, 1) }, 1);
        Check(groups.Length == 1);
        Equal(groups[0].Indices, 0, 1, 2);
        Bounds(groups[0].Bounds, 0, 0, 0, 5, 1, 1);
    }),
    ("union envelope cannot bridge an empty region", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[] { B(0, 0, 1, 4), B(1, 3, 4, 4), B(3, 0, 4, 1) }, 0);
        Check(groups.Length == 2);
        Equal(groups[0].Indices, 0, 1);
        Equal(groups[1].Indices, 2);
    }),
    ("duplicate boxes preserve both source indices", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[] { B(0, 0, 1, 1), B(0, 0, 1, 1) }, 0);
        Check(groups.Length == 1);
        Equal(groups[0].Indices, 0, 1);
    }),
    ("XY grouping retains complete Z range", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[]
        {
            new Box(0, 0, -100, 1, 1, -90), new Box(0, 0, 50, 1, 1, 70)
        }, 0);
        Check(groups.Length == 1);
        Bounds(groups[0].Bounds, 0, 0, -100, 1, 1, 70);
    }),
    ("range grouping independent of input permutation", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[] { B(3, 0, 4, 1), B(1, 3, 4, 4), B(0, 0, 1, 4) }, 0);
        Check(groups.Length == 2);
        Bounds(groups[0].Bounds, 0, 0, 0, 4, 4, 1);
        Equal(groups[0].Indices, 1, 2);
        Equal(groups[1].Indices, 0);
    }),
    ("all input permutations preserve component bounds", () =>
    {
        var boxes = new[] { B(0, 0, 1, 4), B(1, 3, 4, 4), B(3, 0, 4, 1) };
        foreach (var p in new[] { new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } })
        {
            var groups = OrderingBetaCore.GroupExtents(p.Select(i => boxes[i]).ToArray(), 0);
            Check(groups.Length == 2);
            Bounds(groups[0].Bounds, 0, 0, 0, 4, 4, 1);
            Bounds(groups[1].Bounds, 3, 0, 0, 4, 1, 1);
        }
    }),
    ("invalid range and gap rejected", () =>
    {
        foreach (var gap in new[] { -1.0, double.NaN, double.PositiveInfinity })
            Throws<ArgumentOutOfRangeException>(() => OrderingBetaCore.GroupExtents(Array.Empty<Box>(), gap));
        Throws<ArgumentException>(() => OrderingBetaCore.GroupExtents(new[] { B(2, 0, 1, 1) }, 0));
        Throws<ArgumentException>(() => OrderingBetaCore.GroupExtents(new[] { new Box(0, 0, 2, 1, 1, 1) }, 0));
        Throws<ArgumentException>(() => OrderingBetaCore.GroupExtents(new[] { B(0, 0, double.PositiveInfinity, 1) }, 0));
    }),
    ("extreme finite boxes do not overflow adjacency", () =>
    {
        var groups = OrderingBetaCore.GroupExtents(new[]
        {
            B(double.MaxValue, 0, double.MaxValue, 0), B(-double.MaxValue, 0, -double.MaxValue, 0)
        }, double.MaxValue);
        Check(groups.Length == 2);
        Equal(groups[0].Indices, 1);
        Equal(groups[1].Indices, 0);
    })
};

foreach (var (name, body) in cases)
{
    try { body(); passed++; }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"FAIL {name}: {exception}");
        return 1;
    }
}
Console.WriteLine($"OrderingBeta production-core checks: {passed}/{cases.Length} passed.");
return 0;

static int[] Rows(IReadOnlyList<Point> points, double tolerance, bool topToBottom = true, bool leftToRight = true)
    => OrderingBetaCore.GetGridOrder(points, tolerance, true, topToBottom, leftToRight);

static int[] Columns(IReadOnlyList<Point> points, double tolerance, bool leftToRight = true, bool topToBottom = true)
    => OrderingBetaCore.GetGridOrder(points, tolerance, false, topToBottom, leftToRight);

static Box B(double minX, double minY, double maxX, double maxY) => new(minX, minY, 0, maxX, maxY, 1);

static void Equal(IReadOnlyList<int> actual, params int[] expected)
{
    if (!actual.SequenceEqual(expected))
        throw new InvalidOperationException($"Expected [{string.Join(',', expected)}], actual [{string.Join(',', actual)}].");
}

static void Bounds(Box box, double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
{
    Check(box.MinX == minX && box.MinY == minY && box.MinZ == minZ &&
        box.MaxX == maxX && box.MaxY == maxY && box.MaxZ == maxZ);
}

static void Check(bool condition)
{
    if (!condition) throw new InvalidOperationException("Assertion failed.");
}

static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
