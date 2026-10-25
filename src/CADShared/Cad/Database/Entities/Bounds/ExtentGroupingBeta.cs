namespace Fs.Fox.Cad;

/// <summary>原始范围集合中的一个 XY 邻近连通分量。Beta：实际 CAD 宿主行为尚待验收。</summary>
public sealed class ExtentGroupBeta
{
    /// <summary>该组所有输入范围的三维合并包围框。</summary>
    public Extents3d Bounds { get; }

    /// <summary>组内输入范围的原始索引，按输入顺序排列；重复范围仍有各自的索引。</summary>
    public IReadOnlyList<int> SourceIndices { get; }

    internal ExtentGroupBeta(OrderingBetaCore.Group group)
    {
        var box = group.Bounds;
        Bounds = new Extents3d(new Point3d(box.MinX, box.MinY, box.MinZ),
            new Point3d(box.MaxX, box.MaxY, box.MaxZ));
        SourceIndices = Array.AsReadOnly((int[])group.Indices.Clone());
    }
}

/// <summary>按原始 XY 包围框的邻近关系组合范围。Beta：实际 CAD 宿主行为尚待验收。</summary>
public static class ExtentGroupingBeta
{
    /// <summary>将 XY 最短间距不超过给定距离的原始范围归入同一连通分量。</summary>
    /// <param name="extents">输入范围。各坐标必须有限，且每一轴最小值不大于最大值；点范围有效。</param>
    /// <param name="maximumGap">两个原始 XY 矩形间允许的最短欧氏距离；有限非负数，零表示相交或接触。</param>
    /// <returns>
    /// 各连通分量的只读快照；空输入返回空结果。
    /// 按合并框 MinX、MinY、MinZ、MaxX、MaxY、MaxZ 顺序排列，相同框按组内最小原始索引排列。
    /// </returns>
    /// <remarks>
    /// A 邻近 B、B 邻近 C 时三者同组，即使 A 不直接邻近 C。
    /// 邻近关系只取决于原始范围，不使用不断扩大的合并框建立新的关系。
    /// 分组忽略 Z 间距，但结果保留完整 Z 范围；这是包围框分析，不能代表实体真实相交。
    /// 输入不被修改，也不打开数据库。时间复杂度 O(n²)，额外空间 O(n)，适合范围集合的批次分析。
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="extents"/> 为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">距离不是有限非负数。</exception>
    /// <exception cref="ArgumentException">输入范围具有非有限或倒置的坐标。</exception>
    public static IReadOnlyList<ExtentGroupBeta> GroupByProximity(IReadOnlyList<Extents3d> extents,
        double maximumGap = 0)
    {
        if (extents is null)
            throw new ArgumentNullException(nameof(extents));
        var boxes = new OrderingBetaCore.Box[extents.Count];
        for (var i = 0; i < extents.Count; i++)
        {
            var min = extents[i].MinPoint;
            var max = extents[i].MaxPoint;
            boxes[i] = new OrderingBetaCore.Box(min.X, min.Y, min.Z, max.X, max.Y, max.Z);
        }
        var groups = OrderingBetaCore.GroupExtents(boxes, maximumGap);
        var result = new ExtentGroupBeta[groups.Length];
        for (var i = 0; i < groups.Length; i++)
            result[i] = new ExtentGroupBeta(groups[i]);
        return Array.AsReadOnly(result);
    }
}
