namespace Fs.Fox.Cad;

/// <summary>创建轻量多段线几何副本的 Beta 操作；真实 CAD 宿主验收单独记录。</summary>
/// <remarks>
/// 本类始终借用输入，不修改、释放或擦除输入，也不启动事务或自动落库。
/// 返回对象只包含顶点、凸度、段宽、Normal、Elevation、Thickness 和 Closed；
/// 不复制图层、颜色、线型、XData、扩展字典或其他非几何数据，不等同于 DBObject.Clone。
/// 结果由调用方负责 Dispose 或交给目标数据库的事务管理。CAD 宿主行为仍需按版本验收。
/// </remarks>
public static class PolylineExBeta
{
    /// <summary>创建行进方向相反的独立多段线，反转圆弧凸度并交换每段的起止宽度。</summary>
    /// <param name="source">借用的轻量多段线，至少含两个顶点及一条非零长度段。</param>
    /// <returns>调用方拥有的内存多段线；闭合线保持原首顶点。</returns>
    /// <exception cref="ArgumentNullException">输入为 null。</exception>
    /// <exception cref="ArgumentException">顶点不足，或包含非有限坐标/凸度、非法宽度。</exception>
    /// <exception cref="InvalidOperationException">全部段为零长度，或圆弧的两端重合。</exception>
    public static Polyline CreateReversedBeta(this Polyline source)
    {
        var vertices = ReadVertices(source);
        return CreateGeometry(source, PolylineCoreBeta.Reverse(vertices, source.Closed), source.Closed);
    }

    /// <summary>创建删除精确零长度直线段并保守合并等宽连续直线段的独立多段线。</summary>
    /// <remarks>
    /// 圆弧及其端点、宽度变化和局部转向达到 90 度的折点均保留。直线合并采用固定参考线半容差带，
    /// 保证被替换的每个原顶点到最终直线段的距离不超过 tolerance；不累计逐点简化误差。
    /// 仅精确重合顶点之间的直线段可作为零长度段删除，近距离短段不会单独作为零段删除。
    /// 非零直段长度处于 double 次正规范围时保留原段，只执行安全的重合顶点清理。
    /// 闭合线保留首顶点，不跨该首点合并。没有可整理内容时仍返回独立对象。
    /// </remarks>
    /// <param name="source">借用的轻量多段线。</param>
    /// <param name="tolerance">OCS 平面内的最大简化距离误差，必须为有限非负数。</param>
    /// <returns>调用方拥有的内存多段线。</returns>
    /// <exception cref="ArgumentNullException">输入为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">容差不是有限非负数。</exception>
    /// <exception cref="ArgumentException">顶点不足，或包含非有限坐标/凸度、非法宽度。</exception>
    /// <exception cref="InvalidOperationException">全部段为零长度，或圆弧的两端重合。</exception>
    public static Polyline CreateCleanedBeta(this Polyline source, double tolerance = 1e-6)
    {
        PolylineCoreBeta.ValidateTolerance(tolerance);
        var vertices = ReadVertices(source);
        return CreateGeometry(source, PolylineCoreBeta.Clean(vertices, source.Closed, tolerance), source.Closed);
    }

    /// <summary>按指定半径圆角化全部非共线角，创建独立的等宽多段线副本。</summary>
    /// <remarks>
    /// 仅支持实际段全部为直线、全线同一恒定宽度的输入；变宽、跨段宽度不连续或含弧输入均拒绝。
    /// 开放线保留首末点；闭合线处理包括首末角的全部角，首角需要圆角时以其入切点为结果首点。
    /// 同向精确共线顶点原样保留；180 度折返、容差内短段及不可表示的切点拒绝。
    /// 先检查全部角的回退预算；每条原边扣除两端回退后必须余下严格大于 tolerance 的直段，
    /// 因此相邻圆角切点恰好相接也拒绝。半径不自动缩小，失败不返回部分结果。
    /// 距离容差不用于角度分类；零容差仍允许计算中的双精度舍入误差。
    /// 不检查不同原边或圆角之间的全局自交，也不检查等宽条带的偏移边界。
    /// </remarks>
    /// <param name="source">借用的轻量多段线；闭合线至少三个顶点，开放线至少两个顶点。</param>
    /// <param name="radius">OCS 平面中的圆角半径，必须为有限正数。</param>
    /// <param name="tolerance">有限非负的 OCS 距离容差，允许零；默认 1e-6。</param>
    /// <returns>调用方拥有的几何副本；保留 Normal、Elevation、Thickness、Closed 和恒定段宽。</returns>
    /// <exception cref="ArgumentNullException">输入为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">半径或容差非法。</exception>
    /// <exception cref="ArgumentException">顶点不足，或包含非有限坐标/凸度、非法宽度。</exception>
    /// <exception cref="NotSupportedException">实际段含圆弧，或没有全线一致的恒定宽度。</exception>
    /// <exception cref="InvalidOperationException">输入退化、存在折返，或半径/切点/保留直段无法满足约定。</exception>
    public static Polyline CreateFilletedBeta(this Polyline source, double radius, double tolerance = 1e-6)
    {
        var vertices = ReadVertices(source);
        return CreateGeometry(source, PolylineCoreBeta.Fillet(vertices, source.Closed, radius, tolerance), source.Closed);
    }

    /// <summary>尝试将两条开多段线连接为新的开多段线，保留第一条线的行进方向。</summary>
    /// <remarks>
    /// 两个输入必须具有完全相同的 Normal 分量、Elevation 和 Thickness，且恰有一组端点在容差内。
    /// 不转换 OCS，不把闭合线拆开，也不自动闭合结果。精确重合端点合并；非零间隙通过直线段桥接，
    /// 桥接宽度从前段终宽线性变化到后段起宽，因此不会移动原始圆弧端点。
    /// 两个端点均匹配等歧义情况返回 false，不猜测连接顺序。
    /// </remarks>
    /// <param name="first">第一条借用的开多段线。</param>
    /// <param name="second">第二条借用的开多段线。</param>
    /// <param name="result">成功时返回调用方拥有的内存多段线；无法连接时为 null。</param>
    /// <param name="tolerance">OCS 平面内端点最大连接距离，必须为有限非负数。</param>
    /// <returns>仅在端点关系和几何平面满足上述约定时返回 true。</returns>
    /// <exception cref="ArgumentNullException">任一输入为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">容差不是有限非负数。</exception>
    /// <exception cref="ArgumentException">顶点不足，或包含非有限坐标/凸度、非法宽度。</exception>
    /// <exception cref="InvalidOperationException">输入全部段为零长度，或圆弧的两端重合。</exception>
    public static bool TryCreateJoinedBeta(this Polyline first, Polyline second,
        out Polyline? result, double tolerance = 1e-6)
    {
        result = null;
        if (first is null)
            throw new ArgumentNullException(nameof(first));
        if (second is null)
            throw new ArgumentNullException(nameof(second));
        PolylineCoreBeta.ValidateTolerance(tolerance);
        if (ReferenceEquals(first, second) || first.Closed || second.Closed || !SamePlaneAndThickness(first, second))
            return false;

        var firstVertices = ReadVertices(first);
        var secondVertices = ReadVertices(second);
        if (!PolylineCoreBeta.TryJoin(firstVertices, secondVertices, tolerance, out var joined))
            return false;
        result = CreateGeometry(first, joined!, false);
        return true;
    }

    private static PolylineVertexDataBeta[] ReadVertices(Polyline source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (!PolylineCoreBeta.IsFinite(source.Elevation) || !PolylineCoreBeta.IsFinite(source.Thickness) ||
            !PolylineCoreBeta.IsFinite(source.Normal.X) || !PolylineCoreBeta.IsFinite(source.Normal.Y) ||
            !PolylineCoreBeta.IsFinite(source.Normal.Z))
            throw new ArgumentException("The polyline plane and thickness must be finite.", nameof(source));

        // 先读取独立数值快照，计算核心无需延长借用对象的生命周期，也不能接触数据库。
        var result = new PolylineVertexDataBeta[source.NumberOfVertices];
        for (var i = 0; i < result.Length; i++)
        {
            var point = source.GetPoint2dAt(i);
            result[i] = new PolylineVertexDataBeta(point.X, point.Y, source.GetBulgeAt(i),
                source.GetStartWidthAt(i), source.GetEndWidthAt(i));
        }

        return result;
    }

    // 必须精确相等：此入口不会在两个 OCS 平面之间转换顶点，也不会调整厚度。
    private static bool SamePlaneAndThickness(Polyline first, Polyline second) =>
        first.Normal.X == second.Normal.X && first.Normal.Y == second.Normal.Y &&
        first.Normal.Z == second.Normal.Z && first.Elevation == second.Elevation &&
        first.Thickness == second.Thickness;

    private static Polyline CreateGeometry(Polyline source, IReadOnlyList<PolylineVertexDataBeta> vertices, bool closed)
    {
        // 只有全部数值计算完成后才创建宿主实体；创建途中失败时由本方法释放半成品。
        // 成功返回即转交调用方，本方法不追加到任何数据库。
        var result = new Polyline(vertices.Count);
        try
        {
            result.Normal = source.Normal;
            result.Elevation = source.Elevation;
            result.Thickness = source.Thickness;
            for (var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                result.AddVertexAt(i, new Point2d(vertex.X, vertex.Y), vertex.Bulge,
                    vertex.StartWidth, vertex.EndWidth);
            }
            result.Closed = closed;
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }
}
