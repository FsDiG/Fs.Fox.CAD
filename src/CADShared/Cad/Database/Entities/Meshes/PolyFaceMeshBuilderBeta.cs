namespace Fs.Fox.Cad;

/// <summary>在显式目标空间及调用方事务中创建多面网格。Beta：真实宿主验收尚待执行。</summary>
/// <remarks>提炼自 Zfgk PolyfaceMeshUtil；全部数值、面数和索引验证先于数据库写入。</remarks>
public static class PolyFaceMeshBuilderBeta
{
    /// <summary>将三角面和四边面网格写入指定块表记录，不提交或中止调用方事务。</summary>
    /// <param name="database">目标数据库。</param>
    /// <param name="transaction">该数据库当前最上层的有效事务。</param>
    /// <param name="targetSpaceId">目标数据库中可写的块表记录 ID，可为模型空间、图纸空间或普通块定义。</param>
    /// <param name="vertices">有限 WCS 坐标顶点，数量为 3 至 32767；内部建立独立快照。</param>
    /// <param name="faces">最多 32767 个三角面或四边面，索引从零开始；内部先完整枚举并复制。</param>
    /// <returns>已登记到调用方事务中的新网格 ID。</returns>
    /// <remarks>
    /// 所有边均可见；不执行三角化、自交修复或四边面共面性检查。外观使用目标数据库默认值。
    /// 三角面的第四个原生索引为 0。先追加全部顶点，再追加面记录。
    /// 输入验证失败时尚未写入；若宿主在写入期间抛错，调用方必须中止事务，不能捕获后提交部分结果。
    /// 不改变 WorkingDatabase，不打开文档锁，不释放输入数据库或事务。
    /// </remarks>
    /// <exception cref="ArgumentNullException">数据库、事务、顶点或面集合为 null。</exception>
    /// <exception cref="ArgumentException">坐标、索引、面定义、目标空间或事务归属非法。</exception>
    /// <exception cref="ArgumentOutOfRangeException">顶点数超出支持范围。</exception>
    public static ObjectId Append(Database database, Transaction transaction, ObjectId targetSpaceId,
        IReadOnlyList<Point3d> vertices, IEnumerable<IEnumerable<int>> faces)
    {
        if (database is null)
            throw new ArgumentNullException(nameof(database));
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));
        if (vertices is null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Count < 3 || vertices.Count > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertices), "顶点数必须为 3 至 32767。");
        if (targetSpaceId.IsNull || !targetSpaceId.IsValid || targetSpaceId.IsErased ||
            targetSpaceId.Database != database)
            throw new ArgumentException("目标空间必须属于指定数据库。", nameof(targetSpaceId));
        // A child transaction would change native object registration and the caller's rollback boundary.
        if (!ReferenceEquals(database.TransactionManager.TopTransaction, transaction))
            throw new ArgumentException("必须使用目标数据库当前最上层事务。", nameof(transaction));

        var points = new Point3d[vertices.Count];
        for (var index = 0; index < points.Length; index++)
        {
            var point = vertices[index];
            if (!Finite(point.X) || !Finite(point.Y) || !Finite(point.Z))
                throw new ArgumentException("网格顶点坐标必须为有限数值。", nameof(vertices));
            points[index] = point;
        }
        // Exhaust lazy enumerables before opening the target for write, including exceptions raised mid-enumeration.
        var faceData = PolyFaceMeshDataBeta.ValidateAndCopyFaces(points.Length, faces);
        var target = transaction.GetObject(targetSpaceId, OpenMode.ForRead) as BlockTableRecord;
        if (target is null)
            throw new ArgumentException("目标对象必须为块表记录。", nameof(targetSpaceId));

        using (target.ForWrite())
        using (var mesh = new PolyFaceMesh())
        {
            mesh.SetDatabaseDefaults(database);
            var id = target.AppendEntity(mesh);
            // All native children join this same caller-owned transaction; later write failure requires its Abort.
            transaction.AddNewlyCreatedDBObject(mesh, true);
            foreach (var point in points)
            {
                using var vertex = new PolyFaceMeshVertex(point);
                mesh.AppendVertex(vertex);
                transaction.AddNewlyCreatedDBObject(vertex, true);
            }
            foreach (var indices in faceData)
            {
                using var face = new FaceRecord(indices[0], indices[1], indices[2], indices[3]);
                mesh.AppendFaceRecord(face);
                transaction.AddNewlyCreatedDBObject(face, true);
            }
            return id;
        }
    }

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
