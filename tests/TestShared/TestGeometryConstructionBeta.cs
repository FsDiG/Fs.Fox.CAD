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
        var rejected = false;
        try { PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, new[] { new[] { 0, 1, 3 } }); }
        catch (ArgumentException) { rejected = true; }
        Require(rejected && model.Cast<ObjectId>().Count() == before, "Invalid index must not append");
        var id = PolyFaceMeshBuilderBeta.Append(database, transaction, modelId, points, new[] { new[] { 0, 1, 2 } });
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
