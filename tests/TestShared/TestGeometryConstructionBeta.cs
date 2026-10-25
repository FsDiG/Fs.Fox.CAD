namespace Test;

/// <summary>Beta 三角形及多面网格的宿主内验收，使用临时数据库并中止事务。</summary>
public class TestGeometryConstructionBeta
{
    /// <summary>验证三维重心、网格索引、显式数据库归属和失败不写入。</summary>
    [CommandMethod(nameof(Test_GeometryConstructionBeta))]
    public void Test_GeometryConstructionBeta()
    {
        var triangle = new Triangle3dBeta(Point3d.Origin, new Point3d(3, 0, 3), new Point3d(0, 3, 0));
        Require(triangle.Centroid.IsEqualTo(new Point3d(1, 1, 1)), "Triangle centroid");
        Require(triangle.TryGetSlopeAndAspect(out var slope, out var aspect) &&
                Math.Abs(slope - 45) < 1e-10 && Math.Abs(aspect - 270) < 1e-10, "Triangle slope/aspect");
        var working = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        using var transaction = database.TransactionManager.StartTransaction();
        var table = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
        var modelId = table[BlockTableRecord.ModelSpace];
        var model = (BlockTableRecord)transaction.GetObject(modelId, OpenMode.ForRead);
        var before = model.Cast<ObjectId>().Count();
        var points = new[] { Point3d.Origin, new Point3d(10, 0, 0), new Point3d(0, 10, 0) };
        var faces = new[] { new[] { 0, 1, 2 } };
        void RequireRejectedWithoutWrite(Action append, string message)
        {
            var top = database.TransactionManager.TopTransaction;
            var rejected = false;
            try { append(); }
            catch (System.ArgumentException) { rejected = true; }
            Require(rejected && model.Cast<ObjectId>().Count() == before, message);
            Require(ReferenceEquals(top, database.TransactionManager.TopTransaction), message + ": Top transaction preserved");
        }

        RequireRejectedWithoutWrite(
            () => PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, new[] { new[] { 0, 1, 3 } }),
            "Invalid index must not append");
        using (var otherDatabase = new Database(true, true))
        using (var otherTransaction = otherDatabase.TransactionManager.StartTransaction())
        {
            var otherModel = (BlockTableRecord)otherTransaction.GetObject(otherDatabase.CurrentSpaceId, OpenMode.ForRead);
            var otherBefore = otherModel.Cast<ObjectId>().Count();
            void RequireOtherUnchanged() => Require(
                ReferenceEquals(otherTransaction, otherDatabase.TransactionManager.TopTransaction) &&
                otherModel.Cast<ObjectId>().Count() == otherBefore, "Foreign database and transaction preserved");

            RequireRejectedWithoutWrite(
                () => PolyFaceMeshBuilderBeta.Append(database, otherTransaction, modelId, points, faces),
                "Foreign transaction must not append");
            RequireOtherUnchanged();
            RequireRejectedWithoutWrite(
                () => PolyFaceMeshBuilderBeta.Append(database, transaction, otherModel.ObjectId, points, faces),
                "Foreign space must not append");
            RequireOtherUnchanged();
        }
        using (var nested = database.TransactionManager.StartTransaction())
        {
            RequireRejectedWithoutWrite(
                () => PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, faces),
                "Parent transaction must be rejected while a child is active");
            Require(!nested.IsDisposed, "Rejected append must not end the child transaction");
        }
        Require(ReferenceEquals(transaction, database.TransactionManager.TopTransaction), "Parent transaction restored after child disposal");

        var enumerationFailure = new InvalidOperationException("Deliberate face enumeration failure");
        IEnumerable<IEnumerable<int>> FailingFaces()
        {
            yield return faces[0];
            throw enumerationFailure;
        }
        var originalFailurePropagated = false;
        try { PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, FailingFaces()); }
        catch (InvalidOperationException exception) { originalFailurePropagated = ReferenceEquals(exception, enumerationFailure); }
        Require(originalFailurePropagated && model.Cast<ObjectId>().Count() == before, "Face enumeration failure must propagate unchanged without appending");
        Require(ReferenceEquals(transaction, database.TransactionManager.TopTransaction), "Face enumeration failure must preserve the caller transaction");

        var id = PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, faces);
        Require(id.Database == database && model.Cast<ObjectId>().Count() == before + 1, "Explicit target database");
        var mesh = (PolyFaceMesh)transaction.GetObject(id, OpenMode.ForRead);
        var faceCount = 0;
        foreach (ObjectId childId in mesh)
        {
            if (transaction.GetObject(childId, OpenMode.ForRead) is FaceRecord face)
            {
                faceCount++;
                Require(face.GetVertexAt(3) == 0, "Triangle fourth index");
            }
        }
        Require(faceCount == 1, "One face expected");
        Require(ReferenceEquals(working, HostApplicationServices.WorkingDatabase), "Working database preserved");
        transaction.Abort();
        CadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nGeometry construction Beta: passed; temporary database rolled back.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
