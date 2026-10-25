namespace Fs.Fox.Cad;

/// <summary>含直线及凸度圆弧的闭合轻量多段线区域查询；Beta：真实宿主验收单独记录。</summary>
public static class PolylineRegionExBeta
{
    /// <summary>查询 WCS 点位于闭合多段线区域的内部、外部、边界或基准平面外。</summary>
    /// <remarks>
    /// 先完整快照并验证源几何，再按 Normal/Elevation 把 WCS 点转换到 OCS；平面距离大于 tolerance
    /// 返回 OffPlane，其余点按 OCS 投影查询。平面距离与平面内边界距离分别使用同一容差，不合并为三维距离。
    /// 宽度及 Thickness 不参与区域；自交和重合轮廓采用偶奇填充，源直线/圆弧边界优先返回 Boundary。
    /// 重复顶点之间的零直段忽略；全部零段和零弦圆弧拒绝。共线/折返轮廓可能没有内部，按偶奇规则查询。
    /// 非零绝对 bulge 仅支持闭区间 [1e-8,1e8]；超界明确拒绝，不按端点直连替代。支持正负、主弧和双半圆。
    /// 无法可靠表示的极端坐标拒绝；零容差仍允许以局部几何运算为基准的双精度舍入误差。
    /// 此方法仅借用源，不修改、释放或擦除输入，不启动事务或创建数据库实体。
    /// </remarks>
    /// <param name="source">读取生命周期有效且 Closed 为 true 的轻量多段线，至少两个顶点。</param>
    /// <param name="point">有限坐标的 WCS 查询点。</param>
    /// <param name="tolerance">有限非负距离容差，允许零；默认 1e-6。</param>
    /// <returns>独立的关系枚举值。</returns>
    /// <exception cref="ArgumentNullException">源为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">容差非法。</exception>
    /// <exception cref="ArgumentException">源未闭合、顶点不足或坐标/凸度/平面/查询点非法。</exception>
    /// <exception cref="NotSupportedException">非零绝对 bulge 超出 [1e-8,1e8]。</exception>
    /// <exception cref="InvalidOperationException">全部零段、零弦圆弧，或源/查询的数值尺度无法可靠表示。</exception>
    public static PolylinePointRelationBeta GetPointRelationBeta(this Polyline source, Point3d point,
        double tolerance = 1e-6)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        PolylineContainmentCoreBeta.ValidateTolerance(tolerance);
        if (!source.Closed) throw new ArgumentException("The source polyline must be closed.", nameof(source));
        if (!Finite(point) || !PolylineContainmentCoreBeta.Finite(source.Elevation) ||
            !PolylineContainmentCoreBeta.Finite(source.Normal.X) || !PolylineContainmentCoreBeta.Finite(source.Normal.Y) ||
            !PolylineContainmentCoreBeta.Finite(source.Normal.Z) || source.Normal == Vector3d.Zero)
            throw new ArgumentException("The point and polyline plane must be finite and valid.");
        var vertices = new PolylineVertexDataBeta[source.NumberOfVertices];
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = source.GetPoint2dAt(i);
            vertices[i] = new PolylineVertexDataBeta(vertex.X, vertex.Y, source.GetBulgeAt(i));
        }
        var loop = PolylineContainmentCoreBeta.Prepare(vertices);
        var ocs = point.TransformBy(Matrix3d.WorldToPlane(source.Normal));
        if (!Finite(ocs)) throw new InvalidOperationException("The query cannot be represented in OCS.");
        if (Math.Abs(ocs.Z - source.Elevation) > tolerance)
            return PolylinePointRelationBeta.OffPlane;
        return loop.Classify(ocs.X, ocs.Y, tolerance);
    }

    private static bool Finite(Point3d point) => PolylineContainmentCoreBeta.Finite(point.X) &&
        PolylineContainmentCoreBeta.Finite(point.Y) && PolylineContainmentCoreBeta.Finite(point.Z);
}
