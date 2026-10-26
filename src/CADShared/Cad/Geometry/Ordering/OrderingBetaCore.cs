using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// 行列排序、标量排序与范围连通分量共用的纯计算核心。
// 不依赖 CAD SDK，使数值规则可以直接由生产源码链接测试，而不是另写一份算法验证自身。
internal static class OrderingBetaCore
{
    // 使用不可变值快照隔离调用方集合；后续比较器不再读取外部索引器。
    internal readonly struct Point
    {
        internal readonly double X;
        internal readonly double Y;

        internal Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    internal readonly struct Box
    {
        internal readonly double MinX;
        internal readonly double MinY;
        internal readonly double MinZ;
        internal readonly double MaxX;
        internal readonly double MaxY;
        internal readonly double MaxZ;

        internal Box(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        internal Box Union(Box other) => new(
            Math.Min(MinX, other.MinX), Math.Min(MinY, other.MinY), Math.Min(MinZ, other.MinZ),
            Math.Max(MaxX, other.MaxX), Math.Max(MaxY, other.MaxY), Math.Max(MaxZ, other.MaxZ));
    }

    internal sealed class Group
    {
        internal Box Bounds { get; }
        internal int[] Indices { get; }

        internal Group(Box bounds, int[] indices)
        {
            Bounds = bounds;
            Indices = indices;
        }
    }

    internal static int[] GetGridOrder(IReadOnlyList<Point> points, double tolerance,
        bool byRows, bool topToBottom, bool leftToRight)
    {
        if (points is null)
            throw new ArgumentNullException(nameof(points));
        ValidateTolerance(tolerance, nameof(tolerance));

        var snapshot = new Point[points.Count];
        var indices = new int[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            snapshot[i] = points[i];
            if (!IsFinite(snapshot[i].X) || !IsFinite(snapshot[i].Y))
                throw new ArgumentException("Point coordinates must be finite.", nameof(points));
            indices[i] = i;
        }

        // 先按精确坐标建立唯一的扫描次序：行从最高 Y、列从最低 X 建立锚点。
        // 这里不能把“相差在容差内”视作相等，否则比较关系不满足传递性。
        // Array.Sort 本身不稳定，最终以原始索引区分相同坐标，保留重复输入及其稳定次序。
        Array.Sort(indices, (a, b) =>
        {
            var primary = byRows
                ? snapshot[b].Y.CompareTo(snapshot[a].Y)
                : snapshot[a].X.CompareTo(snapshot[b].X);
            if (primary != 0)
                return primary;
            var secondary = byRows
                ? snapshot[a].X.CompareTo(snapshot[b].X)
                : snapshot[b].Y.CompareTo(snapshot[a].Y);
            return secondary != 0 ? secondary : a.CompareTo(b);
        });

        var groups = new List<int[]>();
        var start = 0;
        while (start < indices.Length)
        {
            // 整组只与固定锚点比较，不沿相邻点累计容差；改变读取方向不能改变组的成员。
            var anchor = byRows ? snapshot[indices[start]].Y : snapshot[indices[start]].X;
            var end = start + 1;
            while (end < indices.Length)
            {
                var coordinate = byRows ? snapshot[indices[end]].Y : snapshot[indices[end]].X;
                if (Math.Abs(coordinate - anchor) > tolerance)
                    break;
                end++;
            }

            var group = new int[end - start];
            Array.Copy(indices, start, group, 0, group.Length);
            Array.Sort(group, (a, b) =>
            {
                var x = leftToRight
                    ? snapshot[a].X.CompareTo(snapshot[b].X)
                    : snapshot[b].X.CompareTo(snapshot[a].X);
                var y = topToBottom
                    ? snapshot[b].Y.CompareTo(snapshot[a].Y)
                    : snapshot[a].Y.CompareTo(snapshot[b].Y);
                var primary = byRows ? x : y;
                var secondary = byRows ? y : x;
                return primary != 0 ? primary : secondary != 0 ? secondary : a.CompareTo(b);
            });
            groups.Add(group);
            start = end;
        }

        // 方向只重排已经确定的分组，不能反过来影响锚点选取。
        if (byRows ? !topToBottom : !leftToRight)
            groups.Reverse();
        var result = new int[indices.Length];
        var offset = 0;
        foreach (var group in groups)
        {
            Array.Copy(group, 0, result, offset, group.Length);
            offset += group.Length;
        }
        return result;
    }

    internal static int[] GetScalarOrder(IReadOnlyList<double> values, bool descending)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        var snapshot = new double[values.Count];
        var indices = new int[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            snapshot[i] = values[i];
            if (!IsFinite(snapshot[i]))
                throw new ArgumentException("Sort keys must be finite.", nameof(values));
            indices[i] = i;
        }
        // 相同里程仍以输入索引排序；不能扰动里程值来制造唯一键，也不能倒置相等键的次序。
        Array.Sort(indices, (a, b) =>
        {
            var comparison = descending
                ? snapshot[b].CompareTo(snapshot[a])
                : snapshot[a].CompareTo(snapshot[b]);
            return comparison != 0 ? comparison : a.CompareTo(b);
        });
        return indices;
    }

    internal static Group[] GroupExtents(IReadOnlyList<Box> extents, double maximumGap)
    {
        if (extents is null)
            throw new ArgumentNullException(nameof(extents));
        ValidateTolerance(maximumGap, nameof(maximumGap));

        var boxes = new Box[extents.Count];
        var parents = new int[boxes.Length];
        var ranks = new byte[boxes.Length];
        for (var i = 0; i < boxes.Length; i++)
        {
            var box = extents[i];
            if (!IsFinite(box.MinX) || !IsFinite(box.MinY) || !IsFinite(box.MinZ) ||
                !IsFinite(box.MaxX) || !IsFinite(box.MaxY) || !IsFinite(box.MaxZ) ||
                box.MinX > box.MaxX || box.MinY > box.MaxY || box.MinZ > box.MaxZ)
            {
                throw new ArgumentException("Extents must have finite, ordered bounds.", nameof(extents));
            }
            boxes[i] = box;
            parents[i] = i;
        }

        // 原始矩形是图的顶点，间距满足条件时连边。并查集只求连通分量，不改变连边依据。
        // 若拿合并后的外包框继续连边，L 形空隙里的独立对象会被错误吸收。
        for (var i = 0; i < boxes.Length; i++)
        {
            for (var j = i + 1; j < boxes.Length; j++)
            {
                if (!WithinGap(boxes[i], boxes[j], maximumGap))
                    continue;
                var firstRoot = FindRoot(parents, i);
                var secondRoot = FindRoot(parents, j);
                if (firstRoot == secondRoot)
                    continue;
                if (ranks[firstRoot] < ranks[secondRoot])
                    parents[firstRoot] = secondRoot;
                else
                {
                    parents[secondRoot] = firstRoot;
                    if (ranks[firstRoot] == ranks[secondRoot])
                        ranks[firstRoot]++;
                }
            }
        }

        var members = new Dictionary<int, List<int>>();
        for (var i = 0; i < boxes.Length; i++)
        {
            var root = FindRoot(parents, i);
            if (!members.TryGetValue(root, out var group))
            {
                group = new List<int>();
                members.Add(root, group);
            }
            group.Add(i);
        }

        // 成员按输入索引顺序收集，合并框仅在分组完成后用于输出及确定组的次序。
        // 不依赖字典枚举顺序，也不以合并框反向影响邻近关系。
        var result = new List<Group>(members.Count);
        foreach (var member in members.Values)
        {
            var bounds = boxes[member[0]];
            for (var i = 1; i < member.Count; i++)
                bounds = bounds.Union(boxes[member[i]]);
            result.Add(new Group(bounds, member.ToArray()));
        }
        result.Sort((a, b) =>
        {
            var comparison = CompareBoxes(a.Bounds, b.Bounds);
            return comparison != 0 ? comparison : a.Indices[0].CompareTo(b.Indices[0]);
        });
        return result.ToArray();
    }

    internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    private static void ValidateTolerance(double value, string parameterName)
    {
        if (!IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The distance must be finite and non-negative.");
    }

    private static bool WithinGap(Box a, Box b, double gap)
    {
        var x = Math.Max(0, Math.Max(a.MinX - b.MaxX, b.MinX - a.MaxX));
        var y = Math.Max(0, Math.Max(a.MinY - b.MaxY, b.MinY - a.MaxY));
        if (x > gap || y > gap)
            return false;
        if (gap == 0)
            return true;
        // 次正规数先共乘 2^52，把所有非零分量精确升到正常数范围。
        // 否则 x=y=gap=double.Epsilon 的斜距会舍入回 gap，错误合并本来分离的范围。
        const double minimumNormal = 2.2250738585072014e-308;
        const double subnormalScale = 4503599627370496; // 2^52，仅移动二进制指数，不扩大容差。
        if (gap < minimumNormal)
        {
            x *= subnormalScale;
            y *= subnormalScale;
            gap *= subnormalScale;
        }

        // 以最大轴差归一化后求斜距，避免平方溢出。
        // 不改成 (x/gap)^2+(y/gap)^2<=1：5-12-13 的平方和可能舍入到 1 以上，排除合法边界。
        var largest = Math.Max(x, y);
        if (largest == 0)
            return true;
        var ratio = Math.Min(x, y) / largest;
        return largest * Math.Sqrt(1 + ratio * ratio) <= gap;
    }

    private static int FindRoot(int[] parents, int index)
    {
        // 路径减半避免深链反复查找；根编号只用于分组，不决定最终输出次序。
        while (parents[index] != index)
        {
            parents[index] = parents[parents[index]];
            index = parents[index];
        }
        return index;
    }

    private static int CompareBoxes(Box a, Box b)
    {
        var comparison = a.MinX.CompareTo(b.MinX);
        if (comparison == 0) comparison = a.MinY.CompareTo(b.MinY);
        if (comparison == 0) comparison = a.MinZ.CompareTo(b.MinZ);
        if (comparison == 0) comparison = a.MaxX.CompareTo(b.MaxX);
        if (comparison == 0) comparison = a.MaxY.CompareTo(b.MaxY);
        if (comparison == 0) comparison = a.MaxZ.CompareTo(b.MaxZ);
        return comparison;
    }
}
