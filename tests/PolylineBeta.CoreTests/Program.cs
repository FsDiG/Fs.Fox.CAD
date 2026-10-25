using Fs.Fox.Cad;

// These tests compile the production core directly. No CAD SDK or algorithm copy is used.
internal static class Program
{
    private static int _assertions;

    private static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("open reversal: hand-calculated arcs, tapered widths and terminal data", OpenReverse),
            ("closed reversal: anchored start and closing arc", ClosedReverse),
            ("reversal: randomized involution and independent result", ReverseRoundTrips),
            ("cleanup: exact duplicates transfer outgoing data", CleanDuplicates),
            ("cleanup: arcs, width transitions and backtracking remain", CleanProtectedSegments),
            ("cleanup: fixed-reference error bound and closed anchor", CleanToleranceAndClosed),
            ("joining: four hand-calculated endpoint pairings", JoinFourPairings),
            ("joining: gap preserves arcs and uses endpoint widths", JoinBridge),
            ("joining: ambiguity and nonmatches leave null and inputs unchanged", JoinFailures),
            ("validation: degenerate, nonfinite and invalid tolerance inputs", Validation)
        };
        try
        {
            foreach (var test in tests)
            {
                test.Run();
                Console.WriteLine("PASS " + test.Name);
            }
            Console.WriteLine($"{tests.Length}/{tests.Length} cases passed; {_assertions} assertions.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static PolylineVertexDataBeta V(double x, double y = 0, double bulge = 0,
        double start = 0, double end = 0) => new(x, y, bulge, start, end);

    private static void OpenReverse()
    {
        var source = new[] { V(0, 0, 1, 2, 3), V(10, 0, -.5, 4, 5), V(10, 10, .125, 7, 8) };
        var snapshot = source.ToArray();
        var reversed = PolylineCoreBeta.Reverse(source, false);
        Equal(reversed, new[] { V(10, 10, .5, 5, 4), V(10, 0, -1, 3, 2), V(0, 0, .125, 7, 8) });
        Equal(PolylineCoreBeta.Reverse(reversed, false), source);
        Equal(source, snapshot);
        True(!ReferenceEquals(reversed, source), "Reversal returns a separate array.");
    }

    private static void ClosedReverse()
    {
        var source = new[] { V(0, 0, .5, 1, 2), V(10, 0, -.25, 3, 4), V(0, 10, 1, 5, 6) };
        var reversed = PolylineCoreBeta.Reverse(source, true);
        Equal(reversed, new[] { V(0, 0, -1, 6, 5), V(0, 10, .25, 4, 3), V(10, 0, -.5, 2, 1) });
        Equal(PolylineCoreBeta.Reverse(reversed, true), source);
        // Two semicircles are a valid closed two-vertex curve; no polygon-only assumption.
        Equal(PolylineCoreBeta.Reverse(new[] { V(0, 0, 1), V(10, 0, 1) }, true),
            new[] { V(0, 0, -1), V(10, 0, -1) });
    }

    private static void ReverseRoundTrips()
    {
        var random = new Random(4917);
        for (var trial = 0; trial < 100; trial++)
        {
            var source = Enumerable.Range(0, 2 + random.Next(20))
                .Select(i => V(i, random.NextDouble(), random.NextDouble() * 2 - 1,
                    random.NextDouble() * 10, random.NextDouble() * 10)).ToArray();
            foreach (var closed in new[] { false, true })
                Equal(PolylineCoreBeta.Reverse(PolylineCoreBeta.Reverse(source, closed), closed), source);
        }
    }

    private static void CleanDuplicates()
    {
        var source = new[] { V(0, 0, 0, 9, 8), V(0, 0, 1, 2, 3), V(10, 0, 0, 4, 4),
            V(20, 0, 0, 7, 7), V(20, 0, 0, 5, 5), V(30, 0, .2, 11, 12) };
        var snapshot = source.ToArray();
        Equal(PolylineCoreBeta.Clean(source, false, 0),
            new[] { V(0, 0, 1, 2, 3), V(10, 0, 0, 4, 4), V(20, 0, 0, 5, 5), V(30, 0, .2, 11, 12) });
        Equal(source, snapshot);
        // A zero-length final straight segment is removed; its unused terminal fields remain terminal.
        Equal(PolylineCoreBeta.Clean(new[] { V(0), V(10), V(10, 0, .25, 2, 3) }, false, 0),
            new[] { V(0), V(10, 0, .25, 2, 3) });
    }

    private static void CleanProtectedSegments()
    {
        Equal(PolylineCoreBeta.Clean(new[] { V(0, 0, 0, 2, 2), V(5, 0, 0, 2, 2), V(10) }, false, 0),
            new[] { V(0, 0, 0, 2, 2), V(10) });
        var curved = new[] { V(0), V(5, 0, 1, 2, 3), V(10), V(15) };
        Equal(PolylineCoreBeta.Clean(curved, false, 100), curved);
        var tapered = new[] { V(0, 0, 0, 2, 3), V(5, 0, 0, 3, 3), V(10) };
        Equal(PolylineCoreBeta.Clean(tapered, false, 0), tapered);
        var widths = new[] { V(0, 0, 0, 2, 2), V(5, 0, 0, 3, 3), V(10) };
        Equal(PolylineCoreBeta.Clean(widths, false, 0), widths);
        var backtrack = new[] { V(0), V(10), V(5), V(20) };
        Equal(PolylineCoreBeta.Clean(backtrack, false, 0), backtrack);
        var perpendicularBacktrack = new[] { V(0), V(10), V(10, .04), V(10, -.04), V(20) };
        Equal(PolylineCoreBeta.Clean(perpendicularBacktrack, false, .1), perpendicularBacktrack);
        var obliqueBacktrack = new[] { V(0), V(10), V(10.01, .04), V(10.02, -.04), V(20) };
        Equal(PolylineCoreBeta.Clean(obliqueBacktrack, false, .1),
            new[] { V(0), V(10.01, .04), V(10.02, -.04), V(20) });
        var smallCorner = new[] { V(0), V(1e-9), V(1e-9, 1) };
        Equal(PolylineCoreBeta.Clean(smallCorner, false, 0), smallCorner);
    }

    private static void CleanToleranceAndClosed()
    {
        var source = new[] { V(0), V(1), V(2, .04), V(3, -.04), V(4, .049), V(5, -.049) };
        var cleaned = PolylineCoreBeta.Clean(source, false, .1);
        Equal(cleaned, new[] { V(0), V(5, -.049) });
        foreach (var vertex in source)
            True(DistanceToSegment(vertex, cleaned[0], cleaned[1]) <= .1, "Whole-run deviation stays within tolerance.");
        var drift = Enumerable.Range(0, 30).Select(i => V(i, .01 * i * i)).ToArray();
        var driftClean = PolylineCoreBeta.Clean(drift, false, .1);
        foreach (var vertex in drift)
        {
            var distance = Enumerable.Range(0, driftClean.Length - 1)
                .Select(i => DistanceToSegment(vertex, driftClean[i], driftClean[i + 1])).Min();
            True(distance <= .1 + 1e-12, "Many near-collinear removals cannot accumulate drift.");
        }
        var closed = new[] { V(0, 0), V(5, 0), V(10, 0), V(10, 10), V(0, 10), V(0, 5), V(0, 0) };
        Equal(PolylineCoreBeta.Clean(closed, true, 0), new[] { V(0, 0), V(10, 0), V(10, 10), V(0, 10) });
        // The first point remains even when it is in the middle of a straight closing run.
        Equal(PolylineCoreBeta.Clean(new[] { V(0, 5), V(0, 0), V(10, 0), V(10, 10), V(0, 10) }, true, 0),
            new[] { V(0, 5), V(0, 0), V(10, 0), V(10, 10), V(0, 10) });
        var alreadyClean = new[] { V(0), V(0, 1), V(1, 1) };
        True(!ReferenceEquals(PolylineCoreBeta.Clean(alreadyClean, false, 0), alreadyClean), "No-op cleanup still returns independent data.");
    }

    private static void JoinFourPairings()
    {
        var first = new[] { V(0, 0, .5, 1, 2), V(10, 0, .7, 7, 8) };
        var cases = new[]
        {
            (Second: new[] { V(0, 0, -.25, 3, 4), V(-10, 0, -.8, 9, 10) },
             Expected: new[] { V(-10, 0, .25, 4, 3), V(0, 0, .5, 1, 2), V(10, 0, .7, 7, 8) }),
            (Second: new[] { V(-10, 0, .25, 3, 4), V(0, 0, -.8, 9, 10) },
             Expected: new[] { V(-10, 0, .25, 3, 4), V(0, 0, .5, 1, 2), V(10, 0, .7, 7, 8) }),
            (Second: new[] { V(10, 0, .25, 3, 4), V(20, 0, -.8, 9, 10) },
             Expected: new[] { V(0, 0, .5, 1, 2), V(10, 0, .25, 3, 4), V(20, 0, -.8, 9, 10) }),
            (Second: new[] { V(20, 0, .25, 3, 4), V(10, 0, -.8, 9, 10) },
             Expected: new[] { V(0, 0, .5, 1, 2), V(10, 0, -.25, 4, 3), V(20, 0, -.8, 9, 10) })
        };
        foreach (var test in cases)
        {
            var firstSnapshot = first.ToArray();
            var secondSnapshot = test.Second.ToArray();
            True(PolylineCoreBeta.TryJoin(first, test.Second, 0, out var result), "Unique exact endpoint joins.");
            Equal(result!, test.Expected);
            Equal(first, firstSnapshot);
            Equal(test.Second, secondSnapshot);
        }
    }

    private static void JoinBridge()
    {
        var first = new[] { V(0, 0, 1, 2, 3), V(10, 0, .9, 99, 99) };
        var second = new[] { V(10.02, 0, -.5, 7, 8), V(20, 0, .3, 11, 12) };
        True(PolylineCoreBeta.TryJoin(first, second, .03, out var joined), "Small gap joins.");
        Equal(joined!, new[] { V(0, 0, 1, 2, 3), V(10, 0, 0, 3, 7), V(10.02, 0, -.5, 7, 8), V(20, 0, .3, 11, 12) });
        True(PolylineCoreBeta.TryJoin(new[] { V(0), V(3) }, new[] { V(3, 4), V(10, 4) }, 4, out var boundary),
            "Distance equal to tolerance joins.");
        True(boundary!.Length == 4, "Boundary gap remains an explicit segment.");
    }

    private static void JoinFailures()
    {
        var first = new[] { V(0), V(10) };
        var snapshot = first.ToArray();
        foreach (var second in new[] { new[] { V(20), V(30) }, new[] { V(0), V(10) }, new[] { V(10), V(0) } })
        {
            var secondSnapshot = second.ToArray();
            True(!PolylineCoreBeta.TryJoin(first, second, 0, out var result), "Nonmatch or two matching pairs fails.");
            True(result is null, "Failed output is null.");
            Equal(second, secondSnapshot);
        }
        True(!PolylineCoreBeta.TryJoin(first, new[] { V(20), V(30) }, 100, out var ambiguous), "Broad tolerance does not choose arbitrarily.");
        True(ambiguous is null, "Ambiguous output is null.");
        PolylineVertexDataBeta[]? invalidResult = new[] { V(99) };
        Throws<ArgumentException>(() => PolylineCoreBeta.TryJoin(first, new[] { V(20) }, 0, out invalidResult));
        True(invalidResult is null, "Throwing failure clears output.");
        Equal(first, snapshot);
        True(!PolylineCoreBeta.TryJoin(new[] { V(-double.MaxValue), V(-1) },
            new[] { V(1), V(double.MaxValue) }, 0, out _), "Extreme finite coordinate difference does not overflow into a match.");
    }

    private static void Validation()
    {
        var valid = new[] { V(0), V(1) };
        Throws<ArgumentNullException>(() => PolylineCoreBeta.Reverse(null!, false));
        Throws<ArgumentException>(() => PolylineCoreBeta.Reverse(Array.Empty<PolylineVertexDataBeta>(), false));
        Throws<ArgumentException>(() => PolylineCoreBeta.Clean(new[] { V(1) }, false, 0));
        Throws<InvalidOperationException>(() => PolylineCoreBeta.Clean(new[] { V(1), V(1), V(1) }, true, 0));
        Throws<InvalidOperationException>(() => PolylineCoreBeta.Reverse(new[] { V(1), V(1) }, false));
        Throws<InvalidOperationException>(() => PolylineCoreBeta.Reverse(new[] { V(0, 0, 1), V(0), V(1) }, false));
        foreach (var vertex in new[] { V(double.NaN), V(0, double.PositiveInfinity), V(0, 0, double.NaN), V(0, 0, 0, -1), V(0, 0, 0, 0, double.PositiveInfinity) })
            Throws<ArgumentException>(() => PolylineCoreBeta.Reverse(new[] { vertex, V(1) }, false));
        foreach (var tolerance in new[] { -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            Throws<ArgumentOutOfRangeException>(() => PolylineCoreBeta.Clean(valid, false, tolerance));
            Throws<ArgumentOutOfRangeException>(() => PolylineCoreBeta.TryJoin(valid, valid, tolerance, out _));
        }
    }

    // Independent geometric oracle used only to check the published cleanup error bound.
    private static double DistanceToSegment(PolylineVertexDataBeta p, PolylineVertexDataBeta a, PolylineVertexDataBeta b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy), 0, 1);
        return Math.Sqrt(Math.Pow(p.X - a.X - t * dx, 2) + Math.Pow(p.Y - a.Y - t * dy, 2));
    }

    private static void Equal(IReadOnlyList<PolylineVertexDataBeta> actual, IReadOnlyList<PolylineVertexDataBeta> expected)
    {
        True(actual.Count == expected.Count, $"Vertex count: expected {expected.Count}, got {actual.Count}.");
        for (var i = 0; i < actual.Count; i++)
        {
            True(actual[i].X == expected[i].X && actual[i].Y == expected[i].Y && actual[i].Bulge == expected[i].Bulge &&
                actual[i].StartWidth == expected[i].StartWidth && actual[i].EndWidth == expected[i].EndWidth,
                $"Vertex {i}: expected ({expected[i].X},{expected[i].Y}; {expected[i].Bulge}; {expected[i].StartWidth},{expected[i].EndWidth}), " +
                $"got ({actual[i].X},{actual[i].Y}; {actual[i].Bulge}; {actual[i].StartWidth},{actual[i].EndWidth}).");
        }
    }

    private static void True(bool condition, string message)
    {
        _assertions++;
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        _assertions++;
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ".");
    }
}
