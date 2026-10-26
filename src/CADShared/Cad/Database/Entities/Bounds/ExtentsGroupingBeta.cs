namespace Fs.Fox.Cad;

/// <summary>按原始 XY 包围框的邻近关系组合范围。Beta：实际 CAD 宿主行为尚待验收。</summary>
public static class ExtentsGroupingBeta
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
    public static IReadOnlyList<ExtentsGroupBeta> GroupByXYProximity(IReadOnlyList<Extents3d> extents,
        double maximumGap = 0)
    {
        if (extents is null)
            throw new ArgumentNullException(nameof(extents));
        // 只快照原始范围，不预合并；否则会创造不存在的邻近关系并丢失原始成员身份。
        var boxes = new OrderingAndGroupingCoreBeta.Box[extents.Count];
        for (var i = 0; i < boxes.Length; i++)
        {
            // 范围是值对象，先读一次再取两端，保持同一个输入值的坐标一致性。
            var extent = extents[i];
            var min = extent.MinPoint;
            var max = extent.MaxPoint;
            boxes[i] = new OrderingAndGroupingCoreBeta.Box(min.X, min.Y, min.Z, max.X, max.Y, max.Z);
        }
        var groups = OrderingAndGroupingCoreBeta.GroupExtentsByXYProximity(boxes, maximumGap);
        var result = new ExtentsGroupBeta[groups.Length];
        for (var i = 0; i < groups.Length; i++)
            result[i] = new ExtentsGroupBeta(groups[i]);
        return Array.AsReadOnly(result);
    }
}
