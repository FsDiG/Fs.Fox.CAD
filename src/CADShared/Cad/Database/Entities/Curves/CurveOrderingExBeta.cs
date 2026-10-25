namespace Fs.Fox.Cad;

/// <summary>沿有限曲线计算点的顺序。Beta：实际 CAD 宿主行为尚待验收。</summary>
public static class CurveOrderingExBeta
{
    /// <summary>将 WCS 点投影到曲线上的最近位置，按从起点计算的距离返回原始索引。</summary>
    /// <param name="curve">参数域和长度有限、长度大于零的曲线。调用方负责对象的有效生命周期。</param>
    /// <param name="points">WCS 坐标点，三个坐标分量必须是有限数。</param>
    /// <param name="reverse">是否按曲线距离从大到小排列；距离相同时仍保留输入顺序。</param>
    /// <returns>包含每个输入索引一次的只读快照；重复点和重复距离不丢失。</returns>
    /// <remarks>
    /// 最近点查询不延长曲线，使用宿主 SDK 的三维最近点语义。闭合曲线以其现有起点为里程零点，
    /// 自交或多解最近点沿用 SDK 的选择结果。本方法不修改曲线、不打开或提交事务、不释放输入对象。
    /// 相等距离按原始索引稳定排序，不添加或减去人为偏移；SDK 几何计算异常原样传播。
    /// </remarks>
    /// <exception cref="ArgumentNullException">曲线或点集合为 null。</exception>
    /// <exception cref="ArgumentException">输入点包含非有限坐标。</exception>
    /// <exception cref="NotSupportedException">曲线为 Ray 或 Xline。</exception>
    /// <exception cref="InvalidOperationException">曲线没有有限的有序参数域或正长度，或宿主返回非有限距离。</exception>
    public static IReadOnlyList<int> GetOrderAlongCurveBeta(this Curve curve,
        IReadOnlyList<Point3d> points, bool reverse = false)
    {
        if (curve is null)
            throw new ArgumentNullException(nameof(curve));
        if (points is null)
            throw new ArgumentNullException(nameof(points));
        if (curve is Ray || curve is Xline)
            throw new NotSupportedException("An unbounded curve cannot define a finite ordering path.");

        var startParameter = curve.StartParam;
        var endParameter = curve.EndParam;
        if (!OrderingBetaCore.IsFinite(startParameter) || !OrderingBetaCore.IsFinite(endParameter) ||
            endParameter <= startParameter)
            throw new InvalidOperationException("The curve must have a finite, ordered parameter domain.");
        var startDistance = curve.GetDistanceAtParameter(startParameter);
        var endDistance = curve.GetDistanceAtParameter(endParameter);
        var length = endDistance - startDistance;
        if (!OrderingBetaCore.IsFinite(startDistance) || !OrderingBetaCore.IsFinite(endDistance) ||
            !OrderingBetaCore.IsFinite(length) || length <= 0)
            throw new InvalidOperationException("The curve must have a finite, positive length.");

        // Validate the complete snapshot before the first native closest-point query.
        var snapshot = new Point3d[points.Count];
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            if (!OrderingBetaCore.IsFinite(point.X) || !OrderingBetaCore.IsFinite(point.Y) ||
                !OrderingBetaCore.IsFinite(point.Z))
                throw new ArgumentException("Point coordinates must be finite.", nameof(points));
            snapshot[i] = point;
        }

        // Keep true station values; the original index resolves ties without perturbing geometry.
        var distances = new double[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            var closest = curve.GetClosestPointTo(snapshot[i], false);
            distances[i] = curve.GetDistAtPoint(closest) - startDistance;
            if (!OrderingBetaCore.IsFinite(distances[i]))
                throw new InvalidOperationException("The host returned a non-finite curve distance.");
        }
        return Array.AsReadOnly(OrderingBetaCore.GetScalarOrder(distances, reverse));
    }
}
