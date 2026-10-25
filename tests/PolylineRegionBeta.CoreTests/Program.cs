using Fs.Fox.Cad;
using Relation = Fs.Fox.Cad.PolylinePointRelationBeta;

// Direct production-source links; the numerical implementation is not copied here.
internal static class Program
{
    private static int _assertions;
    private static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("straight concave contours and boundary tolerance", StraightContours),
            ("two semicircles, both directions and horizontal tangencies", FullCircles),
            ("minor/major positive and negative arcs and selected-arc boundary", ArcCaps),
            ("major-arc side extent and complement circle points", MajorArcSides),
            ("bulge support thresholds and shallow sagittas", BulgeThresholds),
            ("independent disk/cut oracle for arc caps", DiskOracle),
            ("translation, uniform scale and reversal invariance", TransformInvariance),
            ("repeated vertices, degenerate edges and even-odd filling", RepeatsAndEvenOdd),
            ("snapshot ownership and ignored widths", SnapshotAndWidths),
            ("complete validation and unrepresentable geometry", Validation)
        };
        try
        {
            foreach (var test in tests) { test.Run(); Console.WriteLine("PASS " + test.Name); }
            Console.WriteLine($"{tests.Length}/{tests.Length} cases passed; {_assertions} assertions.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static PolylineVertexDataBeta V(double x, double y, double bulge = 0) => new(x, y, bulge);
    private static PolylineVertexDataBeta[] Cap(double bulge) => new[] { V(-1, 0, bulge), V(1, 0) };
    private static PolylineVertexDataBeta[] Circle(double bulge = 1) => new[] { V(-1, 0, bulge), V(1, 0, bulge) };
    private static void Is(IReadOnlyList<PolylineVertexDataBeta> vertices, double x, double y, Relation expected, double tolerance = 0)
    {
        var actual = PolylineContainmentCoreBeta.GetPointRelation(vertices, x, y, tolerance);
        True(actual == expected, $"({x:R},{y:R}), b={vertices[0].Bulge:R}: expected {expected}, got {actual}.");
    }

    private static void StraightContours()
    {
        var square = new[] { V(0, 0), V(10, 0), V(10, 10), V(0, 10) };
        Is(square, 5, 5, Relation.Inside);
        Is(square, 0, 0, Relation.Boundary);
        Is(square, 10, 5, Relation.Boundary);
        Is(square, -.001, 5, Relation.Boundary, .001);
        Is(square, -.001, 5, Relation.Outside, .00099);
        Is(square, .0005, 5, Relation.Boundary, .001);
        Is(square, -1e200, 5, Relation.Outside);
        Is(square, -1e308, 0, Relation.Outside);
        var concave = new[] { V(0, 0), V(4, 0), V(4, 4), V(2, 4), V(2, 2), V(0, 2) };
        Is(concave, 3, 3, Relation.Inside);
        Is(concave, 1, 3, Relation.Outside);
        Is(concave, 2, 3, Relation.Boundary);
    }

    private static void FullCircles()
    {
        foreach (var sign in new[] { 1, -1 })
        {
            var circle = Circle(sign);
            Is(circle, 0, 0, Relation.Inside);
            Is(circle, -.999, 0, Relation.Inside);
            Is(circle, -1, 0, Relation.Boundary);
            Is(circle, 0, 1, Relation.Boundary);
            Is(circle, 0, -1, Relation.Boundary);
            Is(circle, -2, 0, Relation.Outside);
            Is(circle, -2, 1, Relation.Outside);
            Is(circle, -2, -1, Relation.Outside);
            Is(circle, 0, -1.0005, Relation.Boundary, .001);
            Is(circle, 0, -1.002, Relation.Outside, .001);
            Is(circle, -1e200, -.5, Relation.Outside);
        }
        var rotated = new[] { V(-1, -1, 1), V(1, 1, 1) };
        Is(rotated, 0, 0, Relation.Inside);
        Is(rotated, -10, Math.Sqrt(2), Relation.Outside);
        Is(rotated, -10, -Math.Sqrt(2), Relation.Outside);
        Is(rotated, 0, Math.Sqrt(2), Relation.Boundary, 1e-12);
    }

    private static void ArcCaps()
    {
        foreach (var sign in new[] { 1, -1 })
        {
            Is(Cap(.5 * sign), 0, -.25 * sign, Relation.Inside);
            Is(Cap(.5 * sign), 0, -.75 * sign, Relation.Outside);
            Is(Cap(.5 * sign), 0, -.5 * sign, Relation.Boundary);
            Is(Cap(.5 * sign), 0, 2 * sign, Relation.Outside); // Same supporting circle, excluded arc.
            Is(Cap(2 * sign), 0, -sign, Relation.Inside);
            Is(Cap(2 * sign), 0, .25 * sign, Relation.Outside);
            Is(Cap(2 * sign), 0, -2 * sign, Relation.Boundary);
            Is(Cap(sign), -1e200, -.5 * sign, Relation.Outside);
            Is(Cap(sign), -1e308, -.5 * sign, Relation.Outside);
        }
    }

    private static void MajorArcSides()
    {
        foreach (var sign in new[] { 1, -1 })
        {
            var cap = Cap(2 * sign);
            Is(cap, 1.1, -.75 * sign, Relation.Inside);
            Is(cap, 1.25, -.75 * sign, Relation.Boundary);
            Is(cap, -1.1, -.75 * sign, Relation.Inside);
            Is(cap, 0, .5 * sign, Relation.Outside);
            Is(cap, -2, -2 * sign, Relation.Outside); // Ray touches the bottom extremum.
        }
        var vertical = new[] { V(0, -1, 2), V(0, 1) }; // Contains both Y extrema: three monotone pieces.
        Is(vertical, 1, 0, Relation.Inside);
        Is(vertical, -.25, 0, Relation.Outside);
        Is(vertical, 2, 0, Relation.Boundary);
        Is(vertical, .75, 1.1, Relation.Inside);
        Is(vertical, .75, 1.25, Relation.Boundary);
        Is(vertical, -10, 1.25, Relation.Outside);
        Is(vertical, -10, -1.25, Relation.Outside);
    }

    private static void BulgeThresholds()
    {
        foreach (var sign in new[] { 1, -1 })
        foreach (var magnitude in new[] { 1e-8, 1e-8 * (1 + 1e-12), 1e8 * (1 - 1e-12), 1e8 })
        {
            var b = sign * magnitude;
            Is(Cap(b), 0, -b, Relation.Boundary);
            Is(Cap(b), 0, -b * .5, Relation.Inside);
            Is(Cap(b), 0, b * .25, Relation.Outside);
        }
        foreach (var sign in new[] { 1, -1 })
        {
            Is(Cap(sign * 1e-8), 0, -sign * 8e-9, Relation.Inside);
            Is(Cap(sign * 1e-8), 0, -sign * 12e-9, Relation.Outside);
            Is(Cap(sign * 1e8), 1.1, -sign * 1e-8, Relation.Inside);
            Is(Cap(sign * 1e8), 1.1, 0, Relation.Outside);
            foreach (var magnitude in new[] { 1e-8 * (1 - 1e-12), 1e8 * (1 + 1e-12), 1e-16, 1e-10 })
                Throws<NotSupportedException>(() => PolylineContainmentCoreBeta.Prepare(Cap(sign * magnitude)));
        }
        var shiftedShallow = new[] { V(1e16 - 10, 0, 1e-8), V(1e16 + 10, 0) };
        Is(shiftedShallow, 1e16, -5e-8, Relation.Inside);
        Is(shiftedShallow, 1e16, -1e-7, Relation.Boundary);
        foreach (var bulge in new[] { 1e-8, -1e-8, 1e8, -1e8 })
        foreach (var angle in new[] { .37, Math.PI * .5, -.62 })
        {
            var cosine = Math.Cos(angle); var sine = Math.Sin(angle);
            var rotated = Cap(bulge).Select(v => V(v.X * cosine, v.X * sine, v.Bulge)).ToArray();
            var reversed = PolylineCoreBeta.Reverse(rotated, true);
            foreach (var contour in new[] { rotated, reversed })
            {
                Is(contour, bulge * sine, -bulge * cosine, Relation.Boundary);
                Is(contour, bulge * .8 * sine, -bulge * .8 * cosine, Relation.Inside);
                Is(contour, -bulge * .25 * sine, bulge * .25 * cosine, Relation.Outside);
            }
        }
    }

    private static void DiskOracle()
    {
        // A single positive arc plus its straight chord bounds exactly the portion
        // of its disk below Y=0. This oracle uses distances, not ray roots or arc pieces.
        foreach (var b in new[] { .25, .5, 1d, 2d, 4d })
        foreach (var sign in new[] { 1, -1 })
        {
            var source = Cap(b * sign);
            var centerY = (1 - b * b) / (2 * b);
            var radius = (1 + b * b) / (2 * b);
            var rotations = new[] { 0d, .37, Math.PI * .5 }.Select(angle =>
                (Cos: Math.Cos(angle), Sin: Math.Sin(angle), Vertices: source.Select(v =>
                    V(v.X * Math.Cos(angle) - v.Y * Math.Sin(angle), v.X * Math.Sin(angle) + v.Y * Math.Cos(angle), v.Bulge)).ToArray())).ToArray();
            for (var ix = -30; ix <= 30; ix++)
            for (var iy = -35; iy <= 8; iy++)
            {
                var x = ix * .125; var y = iy * .125;
                var distance = Math.Sqrt(x * x + Math.Pow(y - centerY, 2));
                var boundary = (y == 0 && x >= -1 && x <= 1) || (y <= 0 && Math.Abs(distance - radius) <= 1e-10);
                var expected = boundary ? Relation.Boundary : y < 0 && distance < radius ? Relation.Inside : Relation.Outside;
                foreach (var rotation in rotations)
                    Is(rotation.Vertices, x * rotation.Cos - y * sign * rotation.Sin,
                        x * rotation.Sin + y * sign * rotation.Cos, expected, 1e-10);
            }
        }
    }

    private static void TransformInvariance()
    {
        foreach (var b in new[] { .5, 1d, 2d, -.5, -2d })
        foreach (var scale in new[] { 1e-100, 1d, 1e100 })
        {
            var original = Cap(b);
            var scaled = original.Select(v => V(v.X * scale + 3 * scale, v.Y * scale - 5 * scale, v.Bulge)).ToArray();
            var reversed = PolylineCoreBeta.Reverse(scaled, true);
            foreach (var point in new[] { (X: 0d, Y: -.25), (X: .25, Y: -.1), (X: 1.1, Y: -.75), (X: 0d, Y: 1d), (X: -2d, Y: 0d) })
            {
                var expected = PolylineContainmentCoreBeta.GetPointRelation(original, point.X, point.Y, 1e-12);
                Is(scaled, point.X * scale + 3 * scale, point.Y * scale - 5 * scale, expected, scale * 1e-12);
                Is(reversed, point.X * scale + 3 * scale, point.Y * scale - 5 * scale, expected, scale * 1e-12);
            }
        }
    }

    private static void RepeatsAndEvenOdd()
    {
        var repeats = new[] { V(0, 0), V(0, 0), V(2, 0), V(2, 2), V(0, 2), V(0, 0) };
        Is(repeats, 1, 1, Relation.Inside);
        Is(repeats, 0, 0, Relation.Boundary);
        var line = new[] { V(0, 0), V(2, 0), V(1, 0) };
        Is(line, 1, 0, Relation.Boundary);
        Is(line, 1, 1, Relation.Outside);
        var bowtie = new[] { V(0, 0), V(2, 2), V(0, 2), V(2, 0) };
        Is(bowtie, .5, .25, Relation.Inside);
        Is(bowtie, 1, 1.75, Relation.Inside);
        Is(bowtie, .25, 1, Relation.Outside);
        Is(bowtie, 1, 1, Relation.Boundary);
        var twice = new[] { V(0, 0), V(2, 0), V(2, 2), V(0, 2), V(0, 0), V(2, 0), V(2, 2), V(0, 2) };
        Is(twice, 1, 1, Relation.Outside);
        Is(twice, 1, 0, Relation.Boundary);
        var twiceCircle = new[] { V(-1, 0, 1), V(1, 0, 1), V(-1, 0, 1), V(1, 0, 1) };
        Is(twiceCircle, 0, 0, Relation.Outside);
        Is(twiceCircle, 0, -1, Relation.Boundary);
        Is(new[] { V(-1, 0, 1), V(1, 0, -1) }, 0, -.5, Relation.Outside);
    }

    private static void SnapshotAndWidths()
    {
        var vertices = new[] { new PolylineVertexDataBeta(-1, 0, 1, double.NaN, -5), new PolylineVertexDataBeta(1, 0, 1, 100, 200) };
        var before = vertices.ToArray();
        var loop = PolylineContainmentCoreBeta.Prepare(vertices);
        True(loop.Classify(0, 0, 0) == Relation.Inside, "Widths do not participate in region geometry.");
        True(vertices.SequenceEqual(before), "The production core does not modify input data.");
        vertices[0] = V(100, 100);
        True(loop.Classify(0, 0, 0) == Relation.Inside, "Prepared loop retains independent numeric snapshots.");
    }

    private static void Validation()
    {
        Throws<ArgumentNullException>(() => PolylineContainmentCoreBeta.Prepare(null!));
        Throws<ArgumentException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0) }));
        Throws<ArgumentException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0), V(double.NaN, 1) }));
        Throws<ArgumentException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0, double.PositiveInfinity), V(1, 0) }));
        Throws<InvalidOperationException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0), V(0, 0) }));
        Throws<InvalidOperationException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0, 1), V(0, 0), V(1, 0) }));
        Throws<NotSupportedException>(() => PolylineContainmentCoreBeta.GetPointRelation(new[] { V(0, 0), V(1, 0, 1e-10) }, 0, 0, 0));
        Throws<InvalidOperationException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(-1e308, 0), V(1e308, 0) }));
        Throws<InvalidOperationException>(() => PolylineContainmentCoreBeta.Prepare(new[] { V(0, 0), V(1e308, 1e-308) }));
        foreach (var tolerance in new[] { -1d, double.NaN, double.PositiveInfinity })
            Throws<ArgumentOutOfRangeException>(() => PolylineContainmentCoreBeta.GetPointRelation(Circle(), 0, 0, tolerance));
        Throws<ArgumentException>(() => PolylineContainmentCoreBeta.GetPointRelation(Circle(), double.NaN, 0, 0));
    }

    private static void True(bool condition, string message)
    { _assertions++; if (!condition) throw new InvalidOperationException(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        _assertions++;
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ".");
    }
}
