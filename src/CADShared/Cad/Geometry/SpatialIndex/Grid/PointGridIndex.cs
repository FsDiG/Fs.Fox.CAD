namespace Fs.Fox.Cad;

/// <summary>
/// 基于 XY 稀疏网格的三维点索引
/// </summary>
/// <remarks>
/// 索引没有固定空间边界，点索引在调用 <see cref="Clear"/> 前保持稳定。
/// 本类型不是线程安全的；同一实例的读取和写入应由调用方同步。
/// </remarks>
public sealed class PointGridIndex
{
    private readonly Dictionary<(long X, long Y), List<int>> _buckets = [];
    private readonly List<Point3d> _points = [];
    private readonly List<int> _candidateIndices = [];
    private readonly IReadOnlyList<Point3d> _readOnlyPoints;

    /// <summary>
    /// 初始化稀疏点网格索引
    /// </summary>
    /// <param name="cellSize">XY 网格边长，必须为有限正数</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cellSize"/> 不是有限正数</exception>
    public PointGridIndex(double cellSize)
    {
        if (!IsFinite(cellSize) || cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "The cell size must be finite and positive.");

        CellSize = cellSize;
        _readOnlyPoints = _points.AsReadOnly();
    }

    /// <summary>
    /// XY 网格边长
    /// </summary>
    public double CellSize { get; }

    /// <summary>
    /// 索引中的点数
    /// </summary>
    public int Count => _points.Count;

    /// <summary>
    /// 按稳定索引排列的只读点视图
    /// </summary>
    public IReadOnlyList<Point3d> Points => _readOnlyPoints;

    /// <summary>
    /// 按索引获取点
    /// </summary>
    /// <param name="index">点索引</param>
    /// <returns>对应的三维点</returns>
    public Point3d this[int index] => _points[index];

    /// <summary>
    /// 无条件添加一个点
    /// </summary>
    /// <param name="point">要添加的有限三维点</param>
    /// <returns>新点的稳定索引</returns>
    /// <exception cref="ArgumentOutOfRangeException">点包含 NaN、无穷值或无法映射到网格的坐标</exception>
    public int Add(Point3d point)
    {
        ValidatePoint(point, nameof(point));
        return AddValidated(point);
    }

    /// <summary>
    /// 在不存在近似点时添加一个点
    /// </summary>
    /// <remarks>
    /// 近似点必须同时满足：XY 欧氏距离小于或等于 <paramref name="xyTolerance"/>，
    /// 且 Z 差绝对值小于或等于 <paramref name="zTolerance"/>。
    /// </remarks>
    /// <param name="point">要添加的有限三维点</param>
    /// <param name="xyTolerance">XY 欧氏距离容差，必须为有限非负数</param>
    /// <param name="zTolerance">Z 差容差，必须为非负数；允许正无穷</param>
    /// <param name="pointIndex">添加成功时为新索引；存在近似点时为最接近的既有索引</param>
    /// <returns>添加新点时返回 <see langword="true"/>；复用既有点时返回 <see langword="false"/></returns>
    public bool TryAdd(Point3d point, double xyTolerance, double zTolerance, out int pointIndex)
    {
        ValidatePoint(point, nameof(point));
        ValidateTolerance(xyTolerance, nameof(xyTolerance), false);
        ValidateTolerance(zTolerance, nameof(zTolerance), true);

        if (TryFindCore(point, xyTolerance, zTolerance, out pointIndex))
            return false;

        pointIndex = AddValidated(point);
        return true;
    }

    /// <summary>
    /// 查找与指定点近似的既有点
    /// </summary>
    /// <remarks>
    /// 候选点必须同时满足 XY 欧氏距离和 Z 差的闭区间容差；有多个候选时返回三维距离最近者，
    /// 距离相同时返回较早添加的点。
    /// </remarks>
    /// <param name="point">查询点</param>
    /// <param name="xyTolerance">XY 欧氏距离容差，必须为有限非负数</param>
    /// <param name="zTolerance">Z 差容差，必须为非负数；允许正无穷</param>
    /// <param name="pointIndex">成功时为匹配点索引；失败时为 -1</param>
    /// <returns>找到近似点时返回 <see langword="true"/></returns>
    public bool TryFind(Point3d point, double xyTolerance, double zTolerance, out int pointIndex)
    {
        ValidatePoint(point, nameof(point));
        ValidateTolerance(xyTolerance, nameof(xyTolerance), false);
        ValidateTolerance(zTolerance, nameof(zTolerance), true);
        return TryFindCore(point, xyTolerance, zTolerance, out pointIndex);
    }

    /// <summary>
    /// 查询 XY 矩形闭区间内的点索引
    /// </summary>
    /// <param name="minPoint">矩形最小 XY 坐标</param>
    /// <param name="maxPoint">矩形最大 XY 坐标</param>
    /// <returns>按添加顺序排列的点索引</returns>
    /// <exception cref="ArgumentOutOfRangeException">坐标不是有限数或最小坐标大于最大坐标</exception>
    public IReadOnlyList<int> QueryIndices(Point2d minPoint, Point2d maxPoint)
    {
        ValidatePoint(minPoint, nameof(minPoint));
        ValidatePoint(maxPoint, nameof(maxPoint));
        if (minPoint.X > maxPoint.X || minPoint.Y > maxPoint.Y)
            throw new ArgumentOutOfRangeException(nameof(maxPoint), "The maximum point must not precede the minimum point.");

        var results = new List<int>();
        foreach (var index in GetCandidateIndices(minPoint.X, minPoint.Y, maxPoint.X, maxPoint.Y))
        {
            var point = _points[index];
            if (point.X >= minPoint.X && point.X <= maxPoint.X &&
                point.Y >= minPoint.Y && point.Y <= maxPoint.Y)
            {
                results.Add(index);
            }
        }

        results.Sort();
        return results;
    }

    /// <summary>
    /// 在最大三维距离内查找最近点
    /// </summary>
    /// <remarks>
    /// 候选点还必须满足 Z 差绝对值小于或等于 <paramref name="zTolerance"/>。
    /// 距离边界为闭区间；距离相同时返回较早添加的点。
    /// </remarks>
    /// <param name="point">查询点</param>
    /// <param name="maxDistance">允许的最大三维距离，必须为有限非负数</param>
    /// <param name="zTolerance">Z 差容差，必须为非负数；允许正无穷</param>
    /// <param name="pointIndex">成功时为最近点索引；失败时为 -1</param>
    /// <param name="distance">成功时为三维距离；失败时为正无穷</param>
    /// <returns>在约束范围内找到点时返回 <see langword="true"/></returns>
    public bool TryGetNearest(Point3d point, double maxDistance, double zTolerance,
        out int pointIndex, out double distance)
    {
        ValidatePoint(point, nameof(point));
        ValidateTolerance(maxDistance, nameof(maxDistance), false);
        ValidateTolerance(zTolerance, nameof(zTolerance), true);

        pointIndex = -1;
        distance = double.PositiveInfinity;
        var bestDistance = double.PositiveInfinity;

        foreach (var index in GetCandidateIndices(
                     point.X - maxDistance,
                     point.Y - maxDistance,
                     point.X + maxDistance,
                     point.Y + maxDistance))
        {
            var candidate = _points[index];
            var zDifference = Math.Abs(candidate.Z - point.Z);
            if (zDifference > zTolerance)
                continue;

            var xDifference = candidate.X - point.X;
            var yDifference = candidate.Y - point.Y;
            var candidateDistance = GetLength(xDifference, yDifference, zDifference);
            if (candidateDistance > maxDistance)
                continue;

            if (pointIndex < 0 || candidateDistance < bestDistance ||
                candidateDistance == bestDistance && index < pointIndex)
            {
                pointIndex = index;
                bestDistance = candidateDistance;
            }
        }

        if (pointIndex < 0)
            return false;

        distance = bestDistance;
        return true;
    }

    /// <summary>
    /// 清空所有点和网格桶
    /// </summary>
    /// <remarks>清空后新点索引重新从零开始。</remarks>
    public void Clear()
    {
        _points.Clear();
        _buckets.Clear();
        _candidateIndices.Clear();
    }

    private int AddValidated(Point3d point)
    {
        var cell = GetCell(point.X, point.Y);
        var pointIndex = _points.Count;
        _points.Add(point);

        if (!_buckets.TryGetValue(cell, out var bucket))
        {
            bucket = [];
            _buckets.Add(cell, bucket);
        }

        bucket.Add(pointIndex);
        return pointIndex;
    }

    private bool TryFindCore(Point3d point, double xyTolerance, double zTolerance, out int pointIndex)
    {
        pointIndex = -1;
        var bestDistance = double.PositiveInfinity;

        foreach (var index in GetCandidateIndices(
                     point.X - xyTolerance,
                     point.Y - xyTolerance,
                     point.X + xyTolerance,
                     point.Y + xyTolerance))
        {
            var candidate = _points[index];
            var xDifference = candidate.X - point.X;
            var yDifference = candidate.Y - point.Y;
            var planarDistance = GetLength(xDifference, yDifference);
            if (planarDistance > xyTolerance)
                continue;

            var zDifference = Math.Abs(candidate.Z - point.Z);
            if (zDifference > zTolerance)
                continue;

            var distance = GetLength(xDifference, yDifference, zDifference);
            if (pointIndex < 0 || distance < bestDistance ||
                distance == bestDistance && index < pointIndex)
            {
                pointIndex = index;
                bestDistance = distance;
            }
        }

        return pointIndex >= 0;
    }

    private List<int> GetCandidateIndices(double minX, double minY, double maxX, double maxY)
    {
        var results = _candidateIndices;
        results.Clear();
        if (_points.Count == 0)
            return results;

        if (double.IsNaN(minX) || double.IsNaN(minY) || double.IsNaN(maxX) || double.IsNaN(maxY))
            throw new ArgumentOutOfRangeException(nameof(minX), "The query bounds must not contain NaN.");

        if (double.IsInfinity(minX) || double.IsInfinity(minY) ||
            double.IsInfinity(maxX) || double.IsInfinity(maxY))
        {
            AddAllPointIndices(results);
            return results;
        }

        var hasMinCell = TryGetCell(minX, minY, out var minCell);
        var hasMaxCell = TryGetCell(maxX, maxY, out var maxCell);
        if (!hasMinCell || !hasMaxCell)
        {
            AddAllPointIndices(results);
            return results;
        }

        var columnCount = (double)maxCell.X - minCell.X + 1;
        var rowCount = (double)maxCell.Y - minCell.Y + 1;
        var gridCellCount = columnCount * rowCount;

        if (gridCellCount <= _buckets.Count)
        {
            for (var row = minCell.Y;; row++)
            {
                for (var column = minCell.X;; column++)
                {
                    if (_buckets.TryGetValue((column, row), out var bucket))
                        results.AddRange(bucket);

                    if (column == maxCell.X)
                        break;
                }

                if (row == maxCell.Y)
                    break;
            }

            return results;
        }

        foreach (var entry in _buckets)
        {
            if (entry.Key.X < minCell.X || entry.Key.X > maxCell.X ||
                entry.Key.Y < minCell.Y || entry.Key.Y > maxCell.Y)
            {
                continue;
            }

            results.AddRange(entry.Value);
        }

        return results;
    }

    private void AddAllPointIndices(List<int> results)
    {
        for (var index = 0; index < _points.Count; index++)
            results.Add(index);
    }

    private (long X, long Y) GetCell(double x, double y)
    {
        return (GetCellCoordinate(x), GetCellCoordinate(y));
    }

    private bool TryGetCell(double x, double y, out (long X, long Y) cell)
    {
        var hasX = TryGetCellCoordinate(x, out var cellX);
        var hasY = TryGetCellCoordinate(y, out var cellY);
        if (!hasX || !hasY)
        {
            cell = default;
            return false;
        }

        cell = (cellX, cellY);
        return true;
    }

    private long GetCellCoordinate(double value)
    {
        if (!TryGetCellCoordinate(value, out var coordinate))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "The coordinate cannot be represented by this grid cell size.");
        }

        return coordinate;
    }

    private bool TryGetCellCoordinate(double value, out long cellCoordinate)
    {
        var coordinate = Math.Floor(value / CellSize);
        if (!IsFinite(coordinate) || coordinate <= long.MinValue || coordinate >= long.MaxValue)
        {
            cellCoordinate = default;
            return false;
        }

        cellCoordinate = (long)coordinate;
        return true;
    }

    private static void ValidatePoint(Point3d point, string parameterName)
    {
        if (!IsFinite(point.X) || !IsFinite(point.Y) || !IsFinite(point.Z))
            throw new ArgumentOutOfRangeException(parameterName, "Point coordinates must be finite.");
    }

    private static void ValidatePoint(Point2d point, string parameterName)
    {
        if (!IsFinite(point.X) || !IsFinite(point.Y))
            throw new ArgumentOutOfRangeException(parameterName, "Point coordinates must be finite.");
    }

    private static void ValidateTolerance(double value, string parameterName, bool allowPositiveInfinity)
    {
        if (double.IsNaN(value) || value < 0 || !allowPositiveInfinity && double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value,
                allowPositiveInfinity
                    ? "The tolerance must be non-negative and must not be NaN."
                    : "The value must be finite and non-negative.");
        }
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static double GetLength(double x, double y, double z = 0)
    {
        x = Math.Abs(x);
        y = Math.Abs(y);
        z = Math.Abs(z);
        var scale = Math.Max(x, Math.Max(y, z));
        if (double.IsInfinity(scale))
            return double.PositiveInfinity;
        if (scale == 0)
            return 0;

        x /= scale;
        y /= scale;
        z /= scale;
        return scale * Math.Sqrt(x * x + y * y + z * z);
    }
}
