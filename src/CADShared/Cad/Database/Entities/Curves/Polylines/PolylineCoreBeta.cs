using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// Capability source: ZFGK.AutoCAD PolylineUtil (author Zhang Fan), source snapshot
// a882003436fb4cc31b24c90f6779177c13a60abe. Reimplemented with explicit ownership,
// complete segment data and a CAD-independent numerical core.
internal readonly struct PolylineVertexDataBeta
{
    internal PolylineVertexDataBeta(double x, double y, double bulge = 0,
        double startWidth = 0, double endWidth = 0)
    {
        X = x;
        Y = y;
        Bulge = bulge;
        StartWidth = startWidth;
        EndWidth = endWidth;
    }

    internal double X { get; }
    internal double Y { get; }
    internal double Bulge { get; }
    internal double StartWidth { get; }
    internal double EndWidth { get; }
}

internal static class PolylineCoreBeta
{
    internal static void ValidateTolerance(double tolerance)
    {
        if (!IsFinite(tolerance) || tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be finite and nonnegative.");
    }

    internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    internal static void Validate(IReadOnlyList<PolylineVertexDataBeta> vertices, bool closed)
    {
        if (vertices == null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Count < 2)
            throw new ArgumentException("At least two vertices are required.", nameof(vertices));

        var hasLength = false;
        for (var i = 0; i < vertices.Count; i++)
        {
            var vertex = vertices[i];
            if (!IsFinite(vertex.X) || !IsFinite(vertex.Y) || !IsFinite(vertex.Bulge) ||
                !IsFinite(vertex.StartWidth) || !IsFinite(vertex.EndWidth) ||
                vertex.StartWidth < 0 || vertex.EndWidth < 0)
                throw new ArgumentException("Vertices must contain finite coordinates, bulges and nonnegative widths.", nameof(vertices));
            if (!closed && i == vertices.Count - 1)
                continue;
            var next = vertices[(i + 1) % vertices.Count];
            if (SamePoint(vertex, next))
            {
                if (vertex.Bulge != 0)
                    throw new InvalidOperationException("An arc cannot have coincident endpoints.");
            }
            else
                hasLength = true;
        }

        if (!hasLength)
            throw new InvalidOperationException("The polyline has no nonzero segment.");
    }

    internal static PolylineVertexDataBeta[] Reverse(IReadOnlyList<PolylineVertexDataBeta> vertices, bool closed)
    {
        Validate(vertices, closed);
        var result = new PolylineVertexDataBeta[vertices.Count];
        for (var i = 0; i < result.Length; i++)
        {
            var pointIndex = closed ? (i == 0 ? 0 : vertices.Count - i) : vertices.Count - 1 - i;
            var point = vertices[pointIndex];
            if (!closed && i == result.Length - 1)
            {
                // Preserve the unused terminal metadata so reversing twice is lossless.
                var terminal = vertices[vertices.Count - 1];
                result[i] = new PolylineVertexDataBeta(point.X, point.Y,
                    terminal.Bulge, terminal.StartWidth, terminal.EndWidth);
                continue;
            }

            var segment = vertices[(pointIndex + vertices.Count - 1) % vertices.Count];
            result[i] = new PolylineVertexDataBeta(point.X, point.Y, -segment.Bulge,
                segment.EndWidth, segment.StartWidth);
        }

        return result;
    }

    internal static PolylineVertexDataBeta[] Clean(IReadOnlyList<PolylineVertexDataBeta> vertices,
        bool closed, double tolerance)
    {
        ValidateTolerance(tolerance);
        Validate(vertices, closed);
        var compact = new List<PolylineVertexDataBeta>(vertices.Count);
        foreach (var vertex in vertices)
        {
            if (compact.Count != 0 && SamePoint(compact[compact.Count - 1], vertex))
            {
                // Validation has rejected a zero-chord arc. Replace only the outgoing
                // data of the zero-length straight segment, keeping incoming geometry.
                compact[compact.Count - 1] = vertex;
            }
            else
                compact.Add(vertex);
        }

        if (closed && compact.Count > 1 && SamePoint(compact[compact.Count - 1], compact[0]))
            compact.RemoveAt(compact.Count - 1);
        if (compact.Count < 2)
            throw new InvalidOperationException("Cleaning would leave no nonzero segment.");

        // The first closed vertex is an intentional anchor; do not simplify across it.
        if (closed)
            compact.Add(compact[0]);
        var result = new List<PolylineVertexDataBeta> { compact[0] };
        var anchor = 0;
        while (anchor < compact.Count - 1)
        {
            var last = anchor + 1;
            var first = compact[anchor];
            var width = first.StartWidth;
            var dx = compact[last].X - first.X;
            var dy = compact[last].Y - first.Y;
            var length = Hypot(dx, dy);
            if (first.Bulge == 0 && first.EndWidth == width && IsFinite(length) && length > 0)
            {
                var ux = dx / length;
                var uy = dy / length;
                var previousDirectionX = ux;
                var previousDirectionY = uy;
                var previousProjection = length;
                for (var end = anchor + 2; end < compact.Count; end++)
                {
                    var segment = compact[end - 1];
                    if (segment.Bulge != 0 || segment.StartWidth != width || segment.EndWidth != width)
                        break;
                    var px = compact[end].X - first.X;
                    var py = compact[end].Y - first.Y;
                    var stepX = compact[end].X - segment.X;
                    var stepY = compact[end].Y - segment.Y;
                    var stepLength = Hypot(stepX, stepY);
                    if (!IsFinite(stepLength) || stepLength == 0)
                        break;
                    var directionX = stepX / stepLength;
                    var directionY = stepY / stepLength;
                    var forwardTurn = previousDirectionX * directionX + previousDirectionY * directionY;
                    var projection = px * ux + py * uy;
                    var deviation = Math.Abs(px * uy - py * ux);
                    // A fixed reference and half-tolerance band bound the distance of
                    // every original vertex to the final chord by tolerance, in O(n).
                    if (!IsFinite(projection) || !IsFinite(deviation) ||
                        projection <= previousProjection || forwardTurn <= 0 || deviation > tolerance * 0.5)
                        break;
                    previousProjection = projection;
                    previousDirectionX = directionX;
                    previousDirectionY = directionY;
                    last = end;
                }
            }

            result.Add(compact[last]);
            anchor = last;
        }

        if (closed)
            result.RemoveAt(result.Count - 1);
        Validate(result, closed);
        return result.ToArray();
    }

    internal static bool TryJoin(IReadOnlyList<PolylineVertexDataBeta> first,
        IReadOnlyList<PolylineVertexDataBeta> second, double tolerance,
        out PolylineVertexDataBeta[]? result)
    {
        result = null;
        ValidateTolerance(tolerance);
        Validate(first, false);
        Validate(second, false);

        var match = -1;
        var count = 0;
        for (var firstEnd = 0; firstEnd < 2; firstEnd++)
        {
            var a = first[firstEnd == 0 ? 0 : first.Count - 1];
            for (var secondEnd = 0; secondEnd < 2; secondEnd++)
            {
                var b = second[secondEnd == 0 ? 0 : second.Count - 1];
                if (Hypot(a.X - b.X, a.Y - b.Y) <= tolerance)
                {
                    match = firstEnd * 2 + secondEnd;
                    count++;
                }
            }
        }

        if (count != 1)
            return false;

        IReadOnlyList<PolylineVertexDataBeta> head;
        IReadOnlyList<PolylineVertexDataBeta> tail;
        if (match < 2)
        {
            head = match == 0 ? Reverse(second, false) : second;
            tail = first;
        }
        else
        {
            head = first;
            tail = match == 3 ? Reverse(second, false) : second;
        }

        var commonHead = head[head.Count - 1];
        var commonTail = tail[0];
        var joined = new List<PolylineVertexDataBeta>(checked(head.Count + tail.Count));
        for (var i = 0; i < head.Count - 1; i++)
            joined.Add(head[i]);
        if (!SamePoint(commonHead, commonTail))
        {
            // Never snap an arc endpoint: retain original geometry and bridge the gap.
            joined.Add(new PolylineVertexDataBeta(commonHead.X, commonHead.Y, 0,
                head[head.Count - 2].EndWidth, commonTail.StartWidth));
        }
        for (var i = 0; i < tail.Count; i++)
            joined.Add(tail[i]);
        result = joined.ToArray();
        return true;
    }

    private static bool SamePoint(PolylineVertexDataBeta first, PolylineVertexDataBeta second) =>
        first.X == second.X && first.Y == second.Y;

    private static double Hypot(double x, double y)
    {
        x = Math.Abs(x);
        y = Math.Abs(y);
        var maximum = Math.Max(x, y);
        if (maximum == 0 || double.IsInfinity(maximum))
            return maximum;
        x /= maximum;
        y /= maximum;
        return maximum * Math.Sqrt(x * x + y * y);
    }
}
