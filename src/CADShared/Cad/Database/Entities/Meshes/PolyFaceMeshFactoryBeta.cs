namespace Fs.Fox.Cad;

/// <summary>在显式目标空间及调用方事务中创建多面网格。Beta：真实宿主验收尚待执行。</summary>
/// <remarks>全部数值、面数和索引验证先于数据库写入；网格及子对象登记到同一调用方事务。</remarks>
public static class PolyFaceMeshFactoryBeta
{
    /// <summary>将三角面和四边面网格写入指定块表记录，不提交或中止调用方事务。</summary>
    /// <param name="database">目标数据库。</param>
    /// <param name="transaction">该数据库当前最上层的有效事务。</param>
    /// <param name="targetBlockTableRecordId">目标数据库中可写的块表记录 ID，可为模型空间、图纸空间或普通块定义。</param>
    /// <param name="vertices">目标块表记录存储坐标系中的有限顶点，数量为 3 至 32767；内部建立独立快照。</param>
    /// <param name="faces">最多 32767 个三角面或四边面，索引从零开始；内部先完整枚举并复制。</param>
    /// <returns>已登记到调用方事务中的新网格 ID。</returns>
    /// <remarks>
    /// 所有边均可见；不执行三角化、自交修复或四边面共面性检查。外观使用目标数据库默认值。
    /// 三角面的第四个原生索引为 0。先追加全部顶点，再追加面记录。
    /// 顶点按给定坐标直接写入；普通块定义使用定义局部坐标，本方法不执行 WCS 到定义坐标的转换。
    /// 输入验证失败时尚未写入；若宿主在写入期间抛错，调用方必须中止事务，不能捕获后提交部分结果。
    /// 不改变 WorkingDatabase，不打开文档锁，不释放输入数据库或事务。
    /// </remarks>
    /// <exception cref="ArgumentNullException">数据库、事务、顶点或面集合为 null。</exception>
    /// <exception cref="ArgumentException">坐标、索引、面定义、目标空间或事务归属非法。</exception>
    /// <exception cref="ArgumentOutOfRangeException">顶点数超出支持范围。</exception>
    public static ObjectId CreateAndAppend(Database database, Transaction transaction, ObjectId targetBlockTableRecordId,
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
        if (targetBlockTableRecordId.IsNull || !targetBlockTableRecordId.IsValid || targetBlockTableRecordId.IsErased ||
            targetBlockTableRecordId.Database != database)
            throw new ArgumentException("目标空间必须属于指定数据库。", nameof(targetBlockTableRecordId));
        // 要求当前顶层事务，避免登记到已被子事务挂起的父事务；当前顶层本身可以是调用方的子事务。
        if (!ReferenceEquals(database.TransactionManager.TopTransaction, transaction))
            throw new ArgumentException("必须使用目标数据库当前最上层事务。", nameof(transaction));

        var points = new Point3d[vertices.Count];
        for (var index = 0; index < points.Length; index++)
        {
            var point = vertices[index];
            if (!IsFinite(point.X) || !IsFinite(point.Y) || !IsFinite(point.Z))
                throw new ArgumentException("网格顶点坐标必须为有限数值。", nameof(vertices));
            points[index] = point;
        }
        // 先完整求值惰性面序列；枚举中途抛错时，本次调用尚未打开目标写状态或追加半成品。
        var faceData = PolyFaceMeshValidationCoreBeta.ValidateAndConvertFaceIndices(points.Length, faces);
        var target = transaction.GetObject(targetBlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
        if (target is null)
            throw new ArgumentException("目标对象必须为块表记录。", nameof(targetBlockTableRecordId));

        using (target.ForWrite())
        using (var mesh = new PolyFaceMesh())
        {
            mesh.SetDatabaseDefaults(database);
            var id = target.AppendEntity(mesh);
            // 登记后由调用方事务掌握提交/回滚；局部 using 负责包装器释放，不代替事务的原子性边界。
            transaction.AddNewlyCreatedDBObject(mesh, true);
            foreach (var point in points)
            {
                // 原生面记录引用这些顶点的一基序号，因此全部顶点必须先追加，且保持输入顺序。
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

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
