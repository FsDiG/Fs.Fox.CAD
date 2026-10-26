namespace Test;

/// <summary>多段线 Beta 的宿主验收入口；仅使用临时实体和临时数据库，不写入当前图纸。</summary>
public class TestPolylineBeta
{
    [CommandMethod(nameof(Test_PolylineRegionBeta))]
    public void Test_PolylineRegionBeta()
    {
        using var source = Make((-1, 0, 1, 20, 30), (1, 0, 1, 40, 50));
        source.Closed = true;
        source.Normal = Vector3d.YAxis;
        source.Elevation = 7;
        source.Thickness = 4;
        var before = Snapshot(source);
        var ocsToWorld = Matrix3d.WorldToPlane(source.Normal).Inverse();
        Point3d World(double x, double y, double z = 7) => new Point3d(x, y, z).TransformBy(ocsToWorld);
        True(source.ClassifyPointBeta(source.GetPoint3dAt(0)) == PolylinePointRelationBeta.Boundary,
            "Native WCS vertex anchors non-default OCS boundary independently of the test transform.");
        True(source.ClassifyPointBeta(new Point3d(0, 7, 0)) == PolylinePointRelationBeta.Inside,
            "Literal WCS center independently anchors the Y-normal plane.");
        True(source.ClassifyPointBeta(new Point3d(0, 7.01, 0)) == PolylinePointRelationBeta.OffPlane,
            "Literal WCS offset independently anchors plane-distance rejection.");
        True(source.ClassifyPointBeta(World(0, 0)) == PolylinePointRelationBeta.Inside, "Non-default OCS circle interior.");
        True(source.ClassifyPointBeta(World(0, -1)) == PolylinePointRelationBeta.Boundary, "OCS arc boundary.");
        True(source.ClassifyPointBeta(World(2, 0)) == PolylinePointRelationBeta.Outside, "Widths do not enlarge the region.");
        True(source.ClassifyPointBeta(World(0, 0, 7.01)) == PolylinePointRelationBeta.OffPlane, "WCS point beyond the base-plane tolerance.");
        True(source.ClassifyPointBeta(World(0, 0, 7.0000005)) == PolylinePointRelationBeta.Inside, "Point within plane tolerance is queried in OCS.");
        True(source.ClassifyPointBeta(World(1.0009, 0, 7.0009), .001) == PolylinePointRelationBeta.Boundary,
            "Plane and in-plane boundary tolerance are checked separately.");
        SameSnapshot(source, before);
        source.SetBulgeAt(0, 1e-10);
        var unsupported = Snapshot(source);
        Throws<NotSupportedException>(() => source.ClassifyPointBeta(World(0, 0, 100)));
        SameSnapshot(source, unsupported); // Complete source validation precedes OffPlane.
        source.Closed = false;
        Throws<ArgumentException>(() => source.ClassifyPointBeta(World(0, 0, 100)));

        // 在真实入口检查分数下端点的半开计数，避免直段范围的往返舍入排除下降边。
        using var fractional = Make((0, 1, 0, 0, 0), (1, .3, 0, 0, 0), (2, 1, 0, 0, 0));
        fractional.Closed = true;
        using var fractionalReverse = fractional.CreateReversedBeta();
        foreach (var contour in new[] { fractional, fractionalReverse })
        {
            True(contour.ClassifyPointBeta(new Point3d(-1, .3, 0), 0) == PolylinePointRelationBeta.Outside,
                "A left-side point at the minimum vertex Y remains outside in either direction.");
            True(contour.ClassifyPointBeta(new Point3d(3, .3, 0), 0) == PolylinePointRelationBeta.Outside,
                "A right-side point at the minimum vertex Y remains outside in either direction.");
            True(contour.ClassifyPointBeta(new Point3d(1, .3, 0), 0) == PolylinePointRelationBeta.Boundary,
                "The fractional minimum vertex is still boundary.");
        }

        using var database = new Database(true, true);
        ObjectId id;
        using (var write = database.TransactionManager.StartTransaction())
        {
            var table = (BlockTable)write.GetObject(database.BlockTableId, OpenMode.ForRead);
            var model = (BlockTableRecord)write.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            using var circle = Make((-1, 0, 1, 0, 0), (1, 0, 1, 0, 0));
            circle.Closed = true;
            id = model.AppendEntity(circle);
            write.AddNewlyCreatedDBObject(circle, true);
            write.Commit();
        }
        using var read = database.TransactionManager.StartTransaction();
        var borrowed = (Polyline)read.GetObject(id, OpenMode.ForRead);
        var borrowedBefore = Snapshot(borrowed);
        True(borrowed.ClassifyPointBeta(Point3d.Origin) == PolylinePointRelationBeta.Inside, "Read-only database input.");
        True(!borrowed.IsWriteEnabled, "Point relation does not upgrade input to write mode.");
        SameSnapshot(borrowed, borrowedBefore);
        Env.Printl("Test_PolylineRegionBeta passed (OCS, plane tolerance, arc region, source validation and read-only input).");
    }

    [CommandMethod(nameof(Test_PolylineBeta))]
    public void Test_PolylineBeta()
    {
        CheckReversalAndOwnership();
        CheckCleanup();
        CheckJoining();
        CheckFilleting();
        CheckReadOnlyDatabaseInput();
        Env.Printl("Test_PolylineBeta passed (geometry, ownership, OCS, widths and read-only input).");
    }

    private static void CheckReversalAndOwnership()
    {
        using var source = Make((0, 0, 1, 2, 3), (10, 0, -.5, 4, 5), (10, 10, 0, 0, 0));
        source.Normal = Vector3d.YAxis;
        source.Elevation = 7;
        source.Thickness = 2.5;
        source.ColorIndex = 3;
        var before = Snapshot(source);
        using (var reversed = source.CreateReversedBeta())
        {
            SamePlane(source, reversed);
            Close(reversed.GetBulgeAt(0), .5, "Reverse first arc");
            Close(reversed.GetStartWidthAt(0), 5, "Reverse start width");
            Close(reversed.GetEndWidthAt(0), 4, "Reverse end width");
            Close(reversed.GetBulgeAt(1), -1, "Reverse second arc");
            Point(reversed.GetPoint3dAt(0), source.GetPoint3dAt(2), "Reverse WCS endpoint");
            Point(reversed.GetPointAtParameter(1.5), source.GetPointAtParameter(.5), "Reverse WCS arc midpoint");
            Close(reversed.Length, source.Length, "Reverse length");
            True(reversed.ObjectId.IsNull, "Result is not inserted into a database.");
            using var restored = reversed.CreateReversedBeta();
            SameSnapshot(restored, before);
        }
        SameSnapshot(source, before); // Input remains usable after disposing the result.
        True(source.ColorIndex == 3, "Input appearance is unchanged.");
        source.Closed = true;
        source.SetBulgeAt(2, .25);
        using var closedReverse = source.CreateReversedBeta();
        True(closedReverse.Closed, "Closed state is preserved.");
        Point(closedReverse.GetPoint3dAt(0), source.GetPoint3dAt(0), "Closed first vertex is anchored.");
        Close(closedReverse.GetBulgeAt(0), -.25, "Closing arc reversal");
        using var closedRestored = closedReverse.CreateReversedBeta();
        SameSnapshot(closedRestored, Snapshot(source));
    }

    private static void CheckCleanup()
    {
        using var source = Make((0, 0, 0, 0, 0), (0, 0, 1, 2, 3),
            (10, 0, 0, 4, 4), (15, 0, 0, 4, 4), (20, 0, 0, 0, 0));
        var before = Snapshot(source);
        using var clean = source.CreateSimplifiedBeta(0);
        True(clean.NumberOfVertices == 3, "Exact duplicate and straight middle vertex are removed.");
        Close(clean.GetBulgeAt(0), 1, "Cleanup retains arc");
        Close(clean.GetStartWidthAt(0), 2, "Cleanup retains arc start width");
        Close(clean.GetEndWidthAt(0), 3, "Cleanup retains arc end width");
        Close(clean.GetStartWidthAt(1), 4, "Cleanup retains straight width");
        Close(clean.Length, Math.PI * 5 + 10, "Hand-calculated semicircle plus line length");
        SameSnapshot(source, before);

        using var uniform = Make((0, 0, 0, 0, 0), (5, 0, 0, 0, 0), (10, 0, 0, 0, 0));
        uniform.ConstantWidth = 6;
        using var uniformClean = uniform.CreateSimplifiedBeta(0);
        True(uniformClean.NumberOfVertices == 2, "Constant-width straight run merges.");
        Close(uniformClean.GetStartWidthAt(0), 6, "Constant width is read as segment start width");
        Close(uniformClean.GetEndWidthAt(0), 6, "Constant width is read as segment end width");

        using var single = Make((0, 0, 0, 0, 0));
        Throws<ArgumentException>(() => { using var unused = single.CreateSimplifiedBeta(); });
        True(single.NumberOfVertices == 1, "Failure does not consume the singleton.");
        using var allZero = Make((0, 0, 0, 0, 0), (0, 0, 0, 0, 0));
        Throws<InvalidOperationException>(() => { using var unused = allZero.CreateReversedBeta(); });
        True(allZero.NumberOfVertices == 2, "Failure does not consume the degenerate input.");
    }

    private static void CheckJoining()
    {
        using var first = Make((0, 0, 1, 2, 3), (10, 0, 0, 0, 0));
        using var second = Make((10.02, 0, -.5, 7, 8), (20, 0, 0, 0, 0));
        first.Normal = second.Normal = Vector3d.YAxis;
        first.Elevation = second.Elevation = 12;
        first.Thickness = second.Thickness = 3;
        var firstBefore = Snapshot(first);
        var secondBefore = Snapshot(second);
        True(first.TryCreateJoinedBeta(second, out var result, .03), "One nearby endpoint joins.");
        using (result)
        {
            True(result is not null && result.NumberOfVertices == 4, "Gap remains an explicit straight segment.");
            SamePlane(first, result!);
            Close(result!.GetBulgeAt(0), 1, "Original first arc retained");
            Close(result.GetBulgeAt(1), 0, "Bridge is straight");
            Close(result.GetStartWidthAt(1), 3, "Bridge begins at preceding end width");
            Close(result.GetEndWidthAt(1), 7, "Bridge ends at next start width");
            Close(result.GetBulgeAt(2), -.5, "Original second arc retained");
            Point(result.GetPoint3dAt(1), first.GetPoint3dAt(1), "First arc endpoint is not snapped.");
            Point(result.GetPoint3dAt(2), second.GetPoint3dAt(0), "Second arc endpoint is not snapped.");
            True(!result.Closed && result.ObjectId.IsNull, "Joined result is open and not inserted.");
        }
        SameSnapshot(first, firstBefore);
        SameSnapshot(second, secondBefore);

        second.Elevation = 13;
        True(!first.TryCreateJoinedBeta(second, out var mismatch, .03) && mismatch is null, "Different OCS elevation is rejected.");
        second.Elevation = 12;
        second.Thickness = 4;
        True(!first.TryCreateJoinedBeta(second, out mismatch, .03) && mismatch is null, "Different thickness is rejected.");
        second.Thickness = 3;
        second.Normal = Vector3d.ZAxis;
        True(!first.TryCreateJoinedBeta(second, out mismatch, .03) && mismatch is null, "Different normal is rejected.");
        second.Normal = Vector3d.YAxis;
        second.Closed = true;
        True(!first.TryCreateJoinedBeta(second, out mismatch, .03) && mismatch is null, "Closed input is rejected.");
        second.Closed = false;
        True(!first.TryCreateJoinedBeta(first, out mismatch) && mismatch is null, "Self-join is rejected.");
        True(!first.TryCreateJoinedBeta(second, out mismatch, 100) && mismatch is null, "Ambiguous endpoint matches are rejected.");
        SameSnapshot(first, firstBefore);
        SameSnapshot(second, secondBefore);
    }

    private static void CheckFilleting()
    {
        using var source = Make((0, 0, 0, 2, 2), (10, 0, 0, 2, 2), (10, 10, 0, 2, 2), (0, 10, 0, 2, 2));
        source.Closed = true;
        source.Normal = Vector3d.YAxis;
        source.Elevation = 7;
        source.Thickness = 2.5;
        var before = Snapshot(source);
        using (var rounded = source.CreateFilletedBeta(2))
        {
            SamePlane(source, rounded);
            True(rounded.Closed && rounded.NumberOfVertices == 8, "Every closed corner, including the first and last, is filleted.");
            True(rounded.ObjectId.IsNull, "Fillet result is not inserted into a database.");
            Close(rounded.Length, 24 + 4 * Math.PI, "Fillet length in a non-default OCS");
            Point(rounded.GetPoint3dAt(0), source.GetPointAtDist(source.Length - 2), "First corner incoming tangent is on the original closing edge.");
            for (var i = 0; i < rounded.NumberOfVertices; i++)
            {
                Close(rounded.GetStartWidthAt(i), 2, "Fillet start width");
                Close(rounded.GetEndWidthAt(i), 2, "Fillet end width");
                if (i % 2 == 0)
                {
                    using var arc = rounded.GetArcSegmentAt(i);
                    Close(arc.Radius, 2, "Fillet requested radius");
                }
            }
        }
        SameSnapshot(source, before);

        using var open = Make((0, 0, 0, 0, 0), (10, 0, 0, 0, 0), (10, -10, 0, 0, 0));
        using var openResult = open.CreateFilletedBeta(2);
        True(!openResult.Closed && openResult.GetBulgeAt(1) < 0, "Open right turn remains open and clockwise.");
        Point(openResult.StartPoint, open.StartPoint, "Open start stays unchanged.");
        Point(openResult.EndPoint, open.EndPoint, "Open end stays unchanged.");
        using var conflict = Make((0, 0, 0, 0, 0), (1, 0, 0, 0, 0), (1, 1, 0, 0, 0), (2, 1, 0, 0, 0));
        var conflictBefore = Snapshot(conflict);
        Throws<InvalidOperationException>(() => { using var unused = conflict.CreateFilletedBeta(.75); });
        SameSnapshot(conflict, conflictBefore);
        open.SetStartWidthAt(0, 1);
        var unsupportedBefore = Snapshot(open);
        Throws<NotSupportedException>(() => { using var unused = open.CreateFilletedBeta(2); });
        SameSnapshot(open, unsupportedBefore);
    }

    private static void CheckReadOnlyDatabaseInput()
    {
        // A private temporary database proves read-only DB-resident inputs work; the active document is unused.
        using var database = new Database(true, true);
        ObjectId id;
        ObjectId filletId;
        using (var write = database.TransactionManager.StartTransaction())
        {
            var table = (BlockTable)write.GetObject(database.BlockTableId, OpenMode.ForRead);
            var model = (BlockTableRecord)write.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            using var source = Make((0, 0, .5, 1, 2), (10, 0, 0, 0, 0));
            id = model.AppendEntity(source);
            write.AddNewlyCreatedDBObject(source, true);
            using var straight = Make((0, 0, 0, 1, 1), (10, 0, 0, 1, 1), (10, 10, 0, 0, 0));
            filletId = model.AppendEntity(straight);
            write.AddNewlyCreatedDBObject(straight, true);
            write.Commit();
        }
        using var read = database.TransactionManager.StartTransaction();
        var borrowed = (Polyline)read.GetObject(id, OpenMode.ForRead);
        var before = Snapshot(borrowed);
        using var reversed = borrowed.CreateReversedBeta();
        True(!borrowed.IsWriteEnabled, "Borrowed input stays read-only.");
        True(reversed.ObjectId.IsNull, "Database-resident input does not imply result insertion.");
        SameSnapshot(borrowed, before);
        var borrowedStraight = (Polyline)read.GetObject(filletId, OpenMode.ForRead);
        var straightBefore = Snapshot(borrowedStraight);
        using var filleted = borrowedStraight.CreateFilletedBeta(2);
        True(!borrowedStraight.IsWriteEnabled, "Fillet input stays read-only.");
        True(filleted.ObjectId.IsNull && filleted.NumberOfVertices == 4, "A read-only database polyline can create an independent fillet result.");
        SameSnapshot(borrowedStraight, straightBefore);
    }

    private static Polyline Make(params (double X, double Y, double Bulge, double Start, double End)[] vertices)
    {
        var polyline = new Polyline(vertices.Length);
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            polyline.AddVertexAt(i, new Point2d(vertex.X, vertex.Y), vertex.Bulge, vertex.Start, vertex.End);
        }
        return polyline;
    }

    private static double[] Snapshot(Polyline source)
    {
        var values = new List<double>
        {
            source.Normal.X, source.Normal.Y, source.Normal.Z, source.Elevation, source.Thickness,
            source.Closed ? 1 : 0, source.NumberOfVertices
        };
        for (var i = 0; i < source.NumberOfVertices; i++)
        {
            values.Add(source.GetPoint2dAt(i).X);
            values.Add(source.GetPoint2dAt(i).Y);
            values.Add(source.GetBulgeAt(i));
            values.Add(source.GetStartWidthAt(i));
            values.Add(source.GetEndWidthAt(i));
        }
        return values.ToArray();
    }

    private static void SameSnapshot(Polyline actual, double[] expected)
    {
        var values = Snapshot(actual);
        True(values.Length == expected.Length, "Snapshot vertex count");
        for (var i = 0; i < values.Length; i++)
            Close(values[i], expected[i], "Snapshot field " + i);
    }

    private static void SamePlane(Polyline source, Polyline result)
    {
        True(source.Normal == result.Normal, "Normal is preserved.");
        Close(result.Elevation, source.Elevation, "Elevation is preserved");
        Close(result.Thickness, source.Thickness, "Thickness is preserved");
    }

    private static void Point(Point3d actual, Point3d expected, string message) =>
        True(actual.DistanceTo(expected) <= 1e-8, message);

    private static void Close(double actual, double expected, string message) =>
        True(Math.Abs(actual - expected) <= 1e-8, message);

    private static void True(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException("Polyline Beta: " + message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Polyline Beta: expected " + typeof(T).Name);
    }
}
