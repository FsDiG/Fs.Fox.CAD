namespace Test;

/// <summary>块内曲线求交 Beta 的宿主命令回归；仅使用临时数据库，构建不代表已运行。</summary>
public static class TestBlockIntersectionsBeta
{
    /// <summary>验证嵌套变换、重复实例、三维交点、去重、删除/恢复成员、只读事务和输入身份。</summary>
    [CommandMethod(nameof(Test_BlockIntersectionsBeta))]
    public static void Test_BlockIntersectionsBeta()
    {
        var working = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        var fixture = CreateFixture(database);
        using var transaction = database.TransactionManager.StartTransaction();
        var block = (BlockReference)transaction.GetObject(fixture.Root, OpenMode.ForRead);
        var target = (Line)transaction.GetObject(fixture.Target, OpenMode.ForRead);
        var source = (Line)transaction.GetObject(fixture.LeafLine, OpenMode.ForRead);
        var sourceStart = source.StartPoint;
        var targetStart = target.StartPoint;
        var expected = new[]
        {
            new Point3d(10, 29, 5), new Point3d(10, 23, 5),
            new Point3d(10, 44, 5), new Point3d(10, 41, 5)
        };

        var points = block.GetCurveIntersectionPointsBeta(transaction, target);
        RequirePoints(points, expected, "Parent times child transform, arc and repeated definition instances");
        RequirePoints(block.GetCurveIntersectionPointsBeta(transaction, target, duplicatePointTolerance: 0), expected,
            "Exact duplicates merge at zero tolerance");
        Throws<NotSupportedException>(() => ((IList<Point3d>)points)[0] = Point3d.Origin);

        using (var shortTarget = new Line(new Point3d(10, 24, 5), new Point3d(10, 27, 5)))
        {
            Require(block.GetCurveIntersectionPointsBeta(transaction, shortTarget).Count == 0, "Finite target must not extend");
            RequirePoints(block.GetCurveIntersectionPointsBeta(transaction, shortTarget, Intersect.ExtendArgument), expected,
                "ExtendArgument extends only the WCS target");
            Require(!shortTarget.IsDisposed, "Borrowed transient target must remain alive");
        }
        using (var raisedTarget = new Line(new Point3d(10, 20, 6), new Point3d(10, 50, 6)))
            Require(block.GetCurveIntersectionPointsBeta(transaction, raisedTarget).Count == 0, "True 3D must not project to XY");

        var nearBlock = (BlockReference)transaction.GetObject(fixture.NearRoot, OpenMode.ForRead);
        using (var diagonal = new Line(new Point3d(-1, -1, 0), new Point3d(2, 2, 0)))
        {
            RequirePoints(nearBlock.GetCurveIntersectionPointsBeta(transaction, diagonal, duplicatePointTolerance: 1),
                new[] { Point3d.Origin, new Point3d(0.8, 0.8, 0) }, "Deduplication uses Euclidean distance and retained representatives");
            Require(nearBlock.GetCurveIntersectionPointsBeta(transaction, diagonal, duplicatePointTolerance: 0).Count == 3,
                "Zero tolerance retains distinct nearby points");
        }
        var emptyBlock = (BlockReference)transaction.GetObject(fixture.EmptyRoot, OpenMode.ForRead);
        Require(emptyBlock.GetCurveIntersectionPointsBeta(transaction, target).Count == 0, "Non-Curve leaves do not participate");
        foreach (var distanceCase in new[]
        {
            (fixture.DistanceRoot, new Point3d(5, 12, 0)),
            (fixture.SpatialDistanceRoot, new Point3d(3, 4, 12))
        })
        {
            var distanceBlock = (BlockReference)transaction.GetObject(distanceCase.Item1, OpenMode.ForRead);
            using var distanceTarget = new Line(Point3d.Origin, distanceCase.Item2);
            RequirePoints(distanceBlock.GetCurveIntersectionPointsBeta(transaction, distanceTarget, duplicatePointTolerance: 13),
                new[] { Point3d.Origin }, "Pythagorean distance exactly at the tolerance must merge");
            RequirePoints(distanceBlock.GetCurveIntersectionPointsBeta(transaction, distanceTarget, duplicatePointTolerance: 13 - 1e-10),
                new[] { Point3d.Origin, distanceCase.Item2 }, "A tolerance below the distance must keep both points");
        }
        var polylineBlock = (BlockReference)transaction.GetObject(fixture.PolylineRoot, OpenMode.ForRead);
        var polyline = (Polyline)transaction.GetObject(fixture.Polyline, OpenMode.ForRead);
        using (var polylineTarget = new Line(new Point3d(-1, 5, 2), new Point3d(5, 5, 2)))
            RequirePoints(polylineBlock.GetCurveIntersectionPointsBeta(transaction, polylineTarget),
                new[] { new Point3d(1, 5, 2) }, "Lightweight polyline arc transforms to WCS");
        Require(polyline.NumberOfVertices == 2 && polyline.GetPoint2dAt(0).Equals(new Point2d(1, 0)) &&
                polyline.GetPoint2dAt(1).Equals(new Point2d(-1, 0)) && polyline.GetBulgeAt(0) == 1 &&
                polyline.GetBulgeAt(1) == 0 && polyline.Elevation == 0 && polyline.Normal.IsEqualTo(Vector3d.ZAxis),
            "Transforming a lightweight polyline copy must preserve the source vertices, bulges and plane");

        foreach (var tolerance in new[] { -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Throws<System.ArgumentOutOfRangeException>(() => block.GetCurveIntersectionPointsBeta(transaction, target, duplicatePointTolerance: tolerance));
        Throws<System.ArgumentOutOfRangeException>(() => block.GetCurveIntersectionPointsBeta(transaction, target, Intersect.ExtendBoth));
        Throws<System.ArgumentNullException>(() => BlockReferenceIntersectionExtensionsBeta.GetCurveIntersectionPointsBeta(null!, transaction, target));
        Throws<System.ArgumentNullException>(() => block.GetCurveIntersectionPointsBeta(null!, target));
        Throws<System.ArgumentNullException>(() => block.GetCurveIntersectionPointsBeta(transaction, null!));
        using (var oldPolyline = new Polyline2d())
            Throws<NotSupportedException>(() => block.GetCurveIntersectionPointsBeta(transaction, oldPolyline));
        using (var transientBlock = new BlockReference(Point3d.Origin, fixture.RootDefinition))
            Throws<System.ArgumentException>(() => transientBlock.GetCurveIntersectionPointsBeta(transaction, target));
        var nestedReference = (BlockReference)transaction.GetObject(fixture.NestedReference, OpenMode.ForRead);
        Throws<System.ArgumentException>(() => nestedReference.GetCurveIntersectionPointsBeta(transaction, target));
        Throws<System.ArgumentException>(() => block.GetCurveIntersectionPointsBeta(transaction, source));

        using (var otherDatabase = new Database(true, true))
        using (var otherTransaction = otherDatabase.TransactionManager.StartTransaction())
        {
            var otherModel = (BlockTableRecord)otherTransaction.GetObject(otherDatabase.CurrentSpaceId, OpenMode.ForWrite);
            using var otherTarget = new Line(Point3d.Origin, new Point3d(1, 1, 0));
            otherModel.AppendEntity(otherTarget);
            otherTransaction.AddNewlyCreatedDBObject(otherTarget, true);
            Throws<System.ArgumentException>(() => block.GetCurveIntersectionPointsBeta(otherTransaction, target));
            Throws<System.ArgumentException>(() => block.GetCurveIntersectionPointsBeta(transaction, otherTarget));
            Require(ReferenceEquals(otherTransaction, otherDatabase.TransactionManager.TopTransaction), "Foreign transaction preserved");
        }
        using (var nested = database.TransactionManager.StartTransaction())
        {
            Throws<System.ArgumentException>(() => block.GetCurveIntersectionPointsBeta(transaction, target));
            Require(ReferenceEquals(nested, database.TransactionManager.TopTransaction), "Rejected parent transaction leaves child active");
        }

        RequirePoints(points, expected, "Returned snapshot remains independent of later queries");
        Require(source.StartPoint.Equals(sourceStart) && target.StartPoint.Equals(targetStart), "Source and target geometry preserved");
        foreach (var id in fixture.ReadIds)
            Require(!transaction.GetObject(id, OpenMode.ForRead).IsWriteEnabled, "Query must not upgrade definitions or entities for write");
        Require(!block.IsDisposed && !target.IsDisposed && !transaction.IsDisposed, "Borrowed input lifetimes preserved");
        Require(ReferenceEquals(transaction, database.TransactionManager.TopTransaction), "Caller transaction remains top");
        Require(HostApplicationServices.WorkingDatabase == working, "Working database preserved");
        transaction.Abort();
        VerifyErasedMembers();
        Env.Printl("Block intersections Beta: temporary-database regression passed; current drawing was not changed.");
    }

    private static void VerifyErasedMembers()
    {
        var working = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        ObjectId definitionId, rootId, retainedId, committedErasedId, currentErasedId;
        using (var setup = database.TransactionManager.StartTransaction())
        {
            var table = (BlockTable)setup.GetObject(database.BlockTableId, OpenMode.ForWrite);
            using var definition = new BlockTableRecord { Name = "FoxIntersectionsBetaErasedMembers" };
            definitionId = table.Add(definition);
            setup.AddNewlyCreatedDBObject(definition, true);
            var model = (BlockTableRecord)setup.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            ObjectId Append(BlockTableRecord owner, Entity entity)
            {
                using (entity)
                {
                    entity.SetDatabaseDefaults(database);
                    var id = owner.AppendEntity(entity);
                    setup.AddNewlyCreatedDBObject(entity, true);
                    return id;
                }
            }

            retainedId = Append(definition, new Line(new Point3d(1, -1, 0), new Point3d(1, 1, 0)));
            committedErasedId = Append(definition, new Line(new Point3d(2, -1, 0), new Point3d(2, 1, 0)));
            currentErasedId = Append(definition, new Line(new Point3d(3, -1, 0), new Point3d(3, 1, 0)));
            rootId = Append(model, new BlockReference(Point3d.Origin, definitionId));
            setup.Commit();
        }
        // Delete an already committed member in a separate transaction, rather than discarding a newly created entity.
        using (var deletion = database.TransactionManager.StartTransaction())
        {
            var member = (Line)deletion.GetObject(committedErasedId, OpenMode.ForWrite);
            member.Erase();
            deletion.Commit();
        }

        using var transaction = database.TransactionManager.StartTransaction();
        var block = (BlockReference)transaction.GetObject(rootId, OpenMode.ForRead);
        var owner = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
        var retained = (Line)transaction.GetObject(retainedId, OpenMode.ForRead);
        var committed = (Line)transaction.GetObject(committedErasedId, OpenMode.ForRead, true);
        var current = (Line)transaction.GetObject(currentErasedId, OpenMode.ForRead);
        using var target = new Line(Point3d.Origin, new Point3d(4, 0, 0));
        var retainedStart = retained.StartPoint;
        var retainedEnd = retained.EndPoint;
        var currentStart = current.StartPoint;
        var currentEnd = current.EndPoint;
        var targetStart = target.StartPoint;
        var targetEnd = target.EndPoint;
        var withCommittedDeletion = new[] { new Point3d(1, 0, 0), new Point3d(3, 0, 0) };
        var withBothDeletions = new[] { new Point3d(1, 0, 0) };

        bool[] WriteStates() => new[]
        {
            block.IsWriteEnabled, owner.IsWriteEnabled, retained.IsWriteEnabled,
            committed.IsWriteEnabled, current.IsWriteEnabled
        };
        bool[] ErasedStates() => new[] { retainedId.IsErased, committedErasedId.IsErased, currentErasedId.IsErased };

        IReadOnlyList<Point3d> QueryPreservingState(string message)
        {
            var writeStates = WriteStates();
            var erasedStates = ErasedStates();
            var result = block.GetCurveIntersectionPointsBeta(transaction, target);
            Require(writeStates.SequenceEqual(WriteStates()), message + ": existing read/write states preserved");
            Require(erasedStates.SequenceEqual(ErasedStates()), message + ": existing erased states preserved");
            Require(!block.IsDisposed && !owner.IsDisposed && !retained.IsDisposed && !committed.IsDisposed && !current.IsDisposed &&
                    !target.IsDisposed && !transaction.IsDisposed, message + ": borrowed lifetimes preserved");
            Require(ReferenceEquals(transaction, database.TransactionManager.TopTransaction), message + ": caller transaction remains top");
            Require(retained.StartPoint.Equals(retainedStart) && retained.EndPoint.Equals(retainedEnd) &&
                    target.StartPoint.Equals(targetStart) && target.EndPoint.Equals(targetEnd), message + ": source and target geometry preserved");
            Require(HostApplicationServices.WorkingDatabase == working, message + ": working database preserved");
            return result;
        }

        Require(committedErasedId.IsErased, "Fixture deletion must survive commit");
        var committedSnapshot = QueryPreservingState("Committed erased member");
        RequirePoints(committedSnapshot, withCommittedDeletion, "Committed erased member must not contribute its distinct intersection");

        current.UpgradeOpen();
        current.Erase();
        Require(currentErasedId.IsErased, "Fixture deletion must be visible in the current transaction");
        var erasedSnapshot = QueryPreservingState("Current-transaction erased member");
        RequirePoints(erasedSnapshot, withBothDeletions, "Current-transaction erased member must not contribute its distinct intersection");

        var restored = (Line)transaction.GetObject(currentErasedId, OpenMode.ForWrite, true);
        restored.Erase(false);
        Require(!currentErasedId.IsErased && committedErasedId.IsErased, "Restore only the current-transaction erased member");
        RequirePoints(QueryPreservingState("Restored current-transaction member"), withCommittedDeletion,
            "Restoring a member restores its distinct intersection");
        Require(current.StartPoint.Equals(currentStart) && current.EndPoint.Equals(currentEnd), "Restored member geometry preserved");

        var previouslyCommitted = (Line)transaction.GetObject(committedErasedId, OpenMode.ForWrite, true);
        previouslyCommitted.Erase(false);
        Require(!committedErasedId.IsErased, "Previously committed deletion can be restored explicitly");
        RequirePoints(QueryPreservingState("Restored committed member"),
            new[] { new Point3d(1, 0, 0), new Point3d(2, 0, 0), new Point3d(3, 0, 0) },
            "Restoring the committed member restores all three distinct intersections");
        Require(previouslyCommitted.StartPoint.Equals(new Point3d(2, -1, 0)) &&
                previouslyCommitted.EndPoint.Equals(new Point3d(2, 1, 0)), "Restored committed member geometry preserved");
        RequirePoints(committedSnapshot, withCommittedDeletion, "Earlier committed-deletion snapshot stays independent");
        RequirePoints(erasedSnapshot, withBothDeletions, "Earlier current-deletion snapshot stays independent");
        transaction.Abort();
    }

    private static Fixture CreateFixture(Database database)
    {
        var fixture = new Fixture();
        using var transaction = database.TransactionManager.StartTransaction();
        var table = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
        var modelId = table[BlockTableRecord.ModelSpace];
        fixture.ReadIds.Add(modelId);

        ObjectId Define(string name)
        {
            if (!table.IsWriteEnabled) table.UpgradeOpen();
            using var definition = new BlockTableRecord { Name = name };
            var id = table.Add(definition);
            transaction.AddNewlyCreatedDBObject(definition, true);
            fixture.ReadIds.Add(id);
            return id;
        }
        ObjectId Append(ObjectId owner, Entity entity)
        {
            using (entity)
            {
                var visible = entity.Visible;
                entity.SetDatabaseDefaults(database);
                entity.Visible = visible;
                var definition = (BlockTableRecord)transaction.GetObject(owner, OpenMode.ForWrite);
                var id = definition.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
                fixture.ReadIds.Add(id);
                return id;
            }
        }

        var leaf = Define("FoxIntersectionsBetaLeaf");
        fixture.LeafLine = Append(leaf, new Line(new Point3d(-1, 0, 0), new Point3d(1, 0, 0)));
        Append(leaf, new Line(new Point3d(-1, 0, 0), new Point3d(1, 0, 0)) { Visible = false });
        Append(leaf, new Arc(Point3d.Origin, Vector3d.ZAxis, 1, 0, Math.PI) { Visible = false });
        fixture.RootDefinition = Define("FoxIntersectionsBetaParent");
        fixture.NestedReference = Append(fixture.RootDefinition, new BlockReference(new Point3d(3, 0, 0), leaf)
        {
            Rotation = Math.PI / 2,
            ScaleFactors = new Scale3d(2)
        });
        Append(fixture.RootDefinition, new BlockReference(new Point3d(8, 0, 0), leaf) { Rotation = Math.PI / 2 });
        fixture.Root = Append(modelId, new BlockReference(new Point3d(10, 20, 5), fixture.RootDefinition)
        {
            Rotation = Math.PI / 2,
            ScaleFactors = new Scale3d(3)
        });
        fixture.Target = Append(modelId, new Line(new Point3d(10, 20, 5), new Point3d(10, 50, 5)));

        var near = Define("FoxIntersectionsBetaNear");
        foreach (var y in new[] { 0.0, 0.8, 0.1 })
            Append(near, new Line(new Point3d(-2, y, 0), new Point3d(2, y, 0)));
        fixture.NearRoot = Append(modelId, new BlockReference(Point3d.Origin, near));
        var empty = Define("FoxIntersectionsBetaNonCurve");
        Append(empty, new DBPoint(Point3d.Origin));
        fixture.EmptyRoot = Append(modelId, new BlockReference(Point3d.Origin, empty));
        ObjectId DefineDistance(string name, Point3d endpoint)
        {
            var definition = Define(name);
            // Shared endpoint intersections have distinct coordinates; neither point can be hidden by exact deduplication.
            Append(definition, new Line(Point3d.Origin, new Point3d(-1, 0, 0)));
            Append(definition, new Line(endpoint, endpoint + Vector3d.XAxis));
            return Append(modelId, new BlockReference(Point3d.Origin, definition));
        }
        fixture.DistanceRoot = DefineDistance("FoxIntersectionsBetaDistanceXY", new Point3d(5, 12, 0));
        fixture.SpatialDistanceRoot = DefineDistance("FoxIntersectionsBetaDistanceXYZ", new Point3d(3, 4, 12));
        var polylineDefinition = Define("FoxIntersectionsBetaPolyline");
        var polyline = new Polyline();
        polyline.AddVertexAt(0, new Point2d(1, 0), 1, 0, 0);
        polyline.AddVertexAt(1, new Point2d(-1, 0), 0, 0, 0);
        fixture.Polyline = Append(polylineDefinition, polyline);
        fixture.PolylineRoot = Append(modelId, new BlockReference(new Point3d(3, 5, 2), polylineDefinition)
        {
            Rotation = Math.PI / 2,
            ScaleFactors = new Scale3d(2)
        });
        transaction.Commit();
        return fixture;
    }

    private static void RequirePoints(IReadOnlyList<Point3d> actual, IReadOnlyList<Point3d> expected, string message)
    {
        Require(actual.Count == expected.Count, message + ": point count");
        for (var index = 0; index < expected.Count; index++)
            Require(actual[index].DistanceTo(expected[index]) <= 1e-8, message + ": point " + index);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Throws<TException>(Action action) where TException : System.Exception
    {
        try { action(); }
        catch (TException) { return; }
        throw new InvalidOperationException("Expected " + typeof(TException).Name);
    }

    private sealed class Fixture
    {
        internal ObjectId Root;
        internal ObjectId RootDefinition;
        internal ObjectId Target;
        internal ObjectId NestedReference;
        internal ObjectId LeafLine;
        internal ObjectId NearRoot;
        internal ObjectId EmptyRoot;
        internal ObjectId DistanceRoot;
        internal ObjectId SpatialDistanceRoot;
        internal ObjectId PolylineRoot;
        internal ObjectId Polyline;
        internal readonly List<ObjectId> ReadIds = new();
    }
}
