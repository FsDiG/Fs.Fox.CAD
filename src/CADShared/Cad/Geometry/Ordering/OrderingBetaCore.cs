using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// Capability source: ZFGK ObjectARX/Entity/BlockUtil.cs (Sort) and
// ObjectARX/ArxOthers/ExtentUtil.cs (GetLocalCombinedExtent), source header:
// Author: TianTeng; Copyright © 北京智帆高科科技有限公司 2015.
// The algorithms below are reimplemented with explicit, deterministic contracts.
// This file deliberately has no CAD SDK dependency; tests link this production file.
internal static class OrderingBetaCore
{
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
        var indices = new int[points.Count];
        for (var i = 0; i < points.Count; i++)
        {
            snapshot[i] = points[i];
            if (!IsFinite(snapshot[i].X) || !IsFinite(snapshot[i].Y))
                throw new ArgumentException("Point coordinates must be finite.", nameof(points));
            indices[i] = i;
        }

        // Canonical anchors are independent of requested reading direction.
        // Rows: highest remaining Y. Columns: lowest remaining X.
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
        var indices = new int[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            snapshot[i] = values[i];
            if (!IsFinite(snapshot[i]))
                throw new ArgumentException("Sort keys must be finite.", nameof(values));
            indices[i] = i;
        }
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
        var parents = new int[extents.Count];
        var ranks = new byte[extents.Count];
        for (var i = 0; i < extents.Count; i++)
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

        // Edges are based only on original boxes. A union envelope never creates
        // new edges across empty space. Connected components supply transitivity.
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
        // A scaled hypotenuse avoids overflow in x*x + y*y. If the final
        // multiplication overflows, the distance also exceeds every finite gap.
        var largest = Math.Max(x, y);
        if (largest == 0)
            return true;
        var ratio = Math.Min(x, y) / largest;
        return largest * Math.Sqrt(1 + ratio * ratio) <= gap;
    }

    private static int FindRoot(int[] parents, int index)
    {
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
