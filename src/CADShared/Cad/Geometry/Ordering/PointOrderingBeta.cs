namespace Fs.Fox.Cad;

/// <summary>按明确的行列分组规则获取二维点的读取顺序。Beta：实际 CAD 宿主行为尚待验收。</summary>
/// <remarks>返回输入位置的索引快照；不修改输入，也不删除相同坐标的点。</remarks>
public static class PointOrderingBeta
{
    /// <summary>先按行分组，再返回各行中点的原始索引。</summary>
    /// <param name="points">需要排序的二维点。坐标必须为有限数。</param>
    /// <param name="rowTolerance">行内 Y 坐标距离首个锚点的最大距离，允许零，必须为有限非负数。</param>
    /// <param name="topToBottom">是否从最高行向最低行排列。</param>
    /// <param name="leftToRight">是否在各行中从左向右排列。</param>
    /// <returns>包含每个输入索引一次的只读快照；空输入返回空结果。</returns>
    /// <remarks>
    /// 始终从最高的尚未分组 Y 建立固定锚点；距锚点不超过容差的连续点属于该行，
    /// 不通过相邻点链式扩大行范围。输出方向不会改变分组成员。
    /// 行内先按 X，再按 Y 排列；相同坐标保持原输入顺序。比较器本身不使用模糊相等。
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="points"/> 为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">容差非法。</exception>
    /// <exception cref="ArgumentException">存在非有限坐标。</exception>
    public static IReadOnlyList<int> GetRowOrder(IReadOnlyList<Point2d> points, double rowTolerance,
        bool topToBottom = true, bool leftToRight = true)
    {
        return Array.AsReadOnly(OrderingBetaCore.GetGridOrder(Snapshot(points), rowTolerance,
            true, topToBottom, leftToRight));
    }

    /// <summary>先按列分组，再返回各列中点的原始索引。</summary>
    /// <param name="points">需要排序的二维点。坐标必须为有限数。</param>
    /// <param name="columnTolerance">列内 X 坐标距离首个锚点的最大距离，允许零，必须为有限非负数。</param>
    /// <param name="leftToRight">是否从最左列向最右列排列。</param>
    /// <param name="topToBottom">是否在各列中从上向下排列。</param>
    /// <returns>包含每个输入索引一次的只读快照；空输入返回空结果。</returns>
    /// <remarks>
    /// 始终从最低的尚未分组 X 建立固定锚点，输出方向不会改变分组成员。
    /// 列内先按 Y，再按 X 排列；相同坐标保持原输入顺序。列分组不采用相邻点的链式容差。
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="points"/> 为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">容差非法。</exception>
    /// <exception cref="ArgumentException">存在非有限坐标。</exception>
    public static IReadOnlyList<int> GetColumnOrder(IReadOnlyList<Point2d> points, double columnTolerance,
        bool leftToRight = true, bool topToBottom = true)
    {
        return Array.AsReadOnly(OrderingBetaCore.GetGridOrder(Snapshot(points), columnTolerance,
            false, topToBottom, leftToRight));
    }

    private static OrderingBetaCore.Point[] Snapshot(IReadOnlyList<Point2d> points)
    {
        if (points is null)
            throw new ArgumentNullException(nameof(points));
        var result = new OrderingBetaCore.Point[points.Count];
        for (var i = 0; i < points.Count; i++)
            result[i] = new OrderingBetaCore.Point(points[i].X, points[i].Y);
        return result;
    }
}
