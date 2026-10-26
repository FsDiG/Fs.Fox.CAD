namespace Fs.Fox.Cad;

/// <summary>三个只读三维顶点的三角形计算。Beta：各 CAD 宿主的实际验收单独记录。</summary>
/// <remarks>
/// 不读写数据库，不持有或释放 CAD 实体。坐标轴按调用方提供的坐标系解释。
/// 三维重心保留全部坐标分量；坡度与坡向按单值地形面 z(x,y) 的语义计算。
/// </remarks>
public sealed class Triangle3dBeta
{
    /// <summary>第一个顶点。</summary>
    public Point3d VertexA { get; }

    /// <summary>第二个顶点。</summary>
    public Point3d VertexB { get; }

    /// <summary>第三个顶点。</summary>
    public Point3d VertexC { get; }

    /// <summary>建立三角形数据；允许共线或重复顶点，以便查询其重心。</summary>
    /// <param name="vertexA">第一个有限坐标顶点。</param>
    /// <param name="vertexB">第二个有限坐标顶点。</param>
    /// <param name="vertexC">第三个有限坐标顶点。</param>
    /// <exception cref="ArgumentException">任一坐标为 NaN 或无穷大。</exception>
    public Triangle3dBeta(Point3d vertexA, Point3d vertexB, Point3d vertexC)
    {
        TriangleMathCoreBeta.Validate(Data(vertexA), nameof(vertexA));
        TriangleMathCoreBeta.Validate(Data(vertexB), nameof(vertexB));
        TriangleMathCoreBeta.Validate(Data(vertexC), nameof(vertexC));
        VertexA = vertexA;
        VertexB = vertexB;
        VertexC = vertexC;
    }

    /// <summary>三个顶点的三维算术平均；不会把 Z 投影为零。</summary>
    public Point3d Centroid
    {
        get
        {
            var value = TriangleMathCoreBeta.GetCentroid(Data(VertexA), Data(VertexB), Data(VertexC));
            return new Point3d(value.X, value.Y, value.Z);
        }
    }

    /// <summary>计算相对 XY 平面的坡度角和下坡方向；顶点顺序不影响结果。</summary>
    /// <param name="slopeDegrees">成功时为 [0,90) 度的坡度角；失败时为 NaN。</param>
    /// <param name="aspectDegrees">下坡方向从 +Y 起顺时针计角，范围 [0,360)；水平面或失败时为 NaN。</param>
    /// <param name="relativeTolerance">无量纲相对容差，必须在 0 与 1 之间；退化判断使用两倍面积除以最长边平方，近水平/垂直使用正弦阈值。</param>
    /// <returns>非退化且非垂直的地形平面返回 true；水平面也返回 true，但没有坡向。</returns>
    /// <remarks>
    /// +X 视为东、+Y 视为北、+Z 向上。面积及法向量使用共同缩放，模长计算避免小分量平方下溢。
    /// 该处理不能恢复输入坐标中已经丢失的差值；坐标范围极端且精度不足时也可能返回 false。
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">容差非法或不是有限数值。</exception>
    public bool TryGetSlopeAndAspectDegrees(out double slopeDegrees, out double aspectDegrees,
        double relativeTolerance = 1e-12) => TriangleMathCoreBeta.TryGetSlopeAndAspectDegrees(
        Data(VertexA), Data(VertexB), Data(VertexC), relativeTolerance, out slopeDegrees, out aspectDegrees);

    private static TrianglePointDataBeta Data(Point3d point) => new(point.X, point.Y, point.Z);
}
