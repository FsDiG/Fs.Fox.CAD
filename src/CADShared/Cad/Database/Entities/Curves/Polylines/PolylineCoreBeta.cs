using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// Capability source: ZFGK.AutoCAD PolylineUtil (author Zhang Fan), source snapshot
// a882003436fb4cc31b24c90f6779177c13a60abe. Reimplemented with explicit ownership,
// complete segment data and a CAD-independent numerical core.
internal readonly struct PolylineVertexDataBeta
{
    // Bulge and widths belong to the segment leaving this vertex. The last open
    // vertex has no outgoing segment, but its unused fields still round-trip.
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

            // Reversed outgoing data comes from the original incoming segment,
            // not from the data stored on this point itself.
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
                    // every original vertex to the final chord by tolerance, in O(n):
                    // both the vertex and final endpoint are within half a tolerance
                    // of the reference. Increasing reference projection places the
                    // comparison point interpolated between the final segment endpoints.
                    // The local dot product separately protects folds inside that band.
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

        // Match codes: start/start, start/end, end/start, end/end. Move or reverse
        // only the second input's placement; the first keeps its original direction.
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

    internal static PolylineVertexDataBeta[] Fillet(IReadOnlyList<PolylineVertexDataBeta> vertices,
        bool closed, double radius, double tolerance)
    {
        ValidateTolerance(tolerance);
        if (!IsFinite(radius) || radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");
        Validate(vertices, closed);
        if (closed && vertices.Count < 3)
            throw new ArgumentException("A closed straight polyline needs at least three vertices.", nameof(vertices));

        var segmentCount = closed ? vertices.Count : vertices.Count - 1;
        var lengths = new double[segmentCount];
        var directionsX = new double[segmentCount];
        var directionsY = new double[segmentCount];
        var width = vertices[0].StartWidth;
        for (var i = 0; i < segmentCount; i++)
        {
            var start = vertices[i];
            if (start.Bulge != 0)
                throw new NotSupportedException("Filleting only supports straight segments.");
            // A single constant width makes every new line/arc junction continuous.
            // Reject tapered or discontinuous widths instead of silently discarding them.
            if (start.StartWidth != width || start.EndWidth != width)
                throw new NotSupportedException("Filleting requires one constant width on all actual segments.");
            var end = vertices[(i + 1) % vertices.Count];
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var length = Hypot(dx, dy);
            if (!IsFinite(length) || length <= tolerance)
                throw new InvalidOperationException("Every segment must have a finite length greater than tolerance.");
            lengths[i] = length;
            // Scale by the largest component before classifying collinearity.
            // Unit-normalizing proportional edges first can introduce a false turn.
            var scale = Math.Max(Math.Abs(dx), Math.Abs(dy));
            directionsX[i] = dx / scale;
            directionsY[i] = dy / scale;
            // Underflow of a nonzero component can erase a real corner. A finite
            // length alone does not make its direction or tangent cuts representable.
            if ((dx != 0 && directionsX[i] == 0) || (dy != 0 && directionsY[i] == 0))
                throw new InvalidOperationException("A nonzero segment direction component cannot be represented at this scale.");
        }

        var retreats = new double[vertices.Count];
        var bulges = new double[vertices.Count];
        var firstCorner = closed ? 0 : 1;
        var endCorner = closed ? vertices.Count : vertices.Count - 1;
        for (var i = firstCorner; i < endCorner; i++)
        {
            var previous = (i + segmentCount - 1) % segmentCount;
            var cross = directionsX[previous] * directionsY[i] - directionsY[previous] * directionsX[i];
            var dot = directionsX[previous] * directionsX[i] + directionsY[previous] * directionsY[i];
            // Direction is independent of the caller's distance tolerance. Preserve
            // exactly forward-collinear corners and reject exact 180-degree folds.
            if (cross == 0)
            {
                if (dot <= 0)
                    throw new InvalidOperationException("A reversing collinear corner cannot be filleted.");
                continue;
            }
            // d = r*tan(|turn|/2). Choose the half-angle identity whose denominator
            // does not cancel near a reversing corner; radius is never reduced.
            var absoluteCross = Math.Abs(cross);
            var angularScale = Hypot(cross, dot);
            var tangentHalf = dot >= 0 ? absoluteCross / (angularScale + dot) : (angularScale - dot) / absoluteCross;
            var retreat = radius * tangentHalf;
            var bulge = Math.Tan(Math.Atan2(cross, dot) * 0.25);
            if (!IsFinite(retreat) || retreat <= tolerance || bulge == 0 || !IsFinite(bulge))
                throw new InvalidOperationException("The corner's tangent points cannot be represented at this tolerance.");
            retreats[i] = retreat;
            bulges[i] = bulge;
        }

        // Plan every corner before emitting geometry: two individually valid cuts
        // can overlap on a shared edge. Even exactly touching cuts are rejected;
        // every remaining straight segment must be strictly longer than tolerance.
        for (var i = 0; i < segmentCount; i++)
        {
            var remaining = lengths[i] - retreats[i] - retreats[(i + 1) % vertices.Count];
            if (!IsFinite(remaining) || remaining <= tolerance)
                throw new InvalidOperationException("The requested radius exhausts a shared segment.");
        }

        var entries = new PolylineVertexDataBeta[vertices.Count];
        var exits = new PolylineVertexDataBeta[vertices.Count];
        for (var i = 0; i < vertices.Count; i++)
        {
            if (bulges[i] == 0)
            {
                entries[i] = exits[i] = vertices[i];
                continue;
            }
            var previous = (i + segmentCount - 1) % segmentCount;
            var entry = FilletTangent(vertices[i], -directionsX[previous], -directionsY[previous],
                retreats[i], bulges[i], width, tolerance);
            var exit = FilletTangent(vertices[i], directionsX[i], directionsY[i],
                retreats[i], 0, width, tolerance);
            var chord = Hypot(exit.X - entry.X, exit.Y - entry.Y);
            var absoluteBulge = Math.Abs(bulges[i]);
            var actualRadius = (chord * 0.25 / absoluteBulge) * (1 + absoluteBulge * absoluteBulge);
            // Large shared offsets can erase a cut even when d is representable.
            // Do not return a different radius caused by rounded tangent coordinates.
            if (!IsFinite(chord) || chord <= tolerance || !IsFinite(actualRadius) ||
                Math.Abs(actualRadius - radius) > FilletPrecision(radius, tolerance))
                throw new InvalidOperationException("The requested fillet radius cannot be represented at these coordinates.");
            entries[i] = entry;
            exits[i] = exit;
        }
        // Recheck represented cuts, not only their theoretical budgets: rounding at
        // a large offset must not collapse a remaining edge or reverse its direction.
        for (var i = 0; i < segmentCount; i++)
        {
            var next = (i + 1) % vertices.Count;
            var dx = entries[next].X - exits[i].X;
            var dy = entries[next].Y - exits[i].Y;
            var length = Hypot(dx, dy);
            if (!IsFinite(length) || length <= tolerance ||
                dx / length * directionsX[i] + dy / length * directionsY[i] <= 0)
                throw new InvalidOperationException("A remaining straight segment cannot be represented at these coordinates.");
        }
        var result = new List<PolylineVertexDataBeta>(checked(vertices.Count * 2));
        for (var i = 0; i < vertices.Count; i++)
        {
            result.Add(entries[i]);
            if (bulges[i] != 0)
                result.Add(exits[i]);
        }
        Validate(result, closed);
        return result.ToArray();
    }

    private static PolylineVertexDataBeta FilletTangent(PolylineVertexDataBeta corner,
        double directionX, double directionY, double retreat, double bulge, double width, double tolerance)
    {
        var directionLength = Hypot(directionX, directionY);
        var dx = directionX / directionLength * retreat;
        var dy = directionY / directionLength * retreat;
        var point = new PolylineVertexDataBeta(corner.X + dx, corner.Y + dy, bulge, width, width);
        var error = Hypot(point.X - corner.X - dx, point.Y - corner.Y - dy);
        if (!IsFinite(point.X) || !IsFinite(point.Y) || SamePoint(point, corner) ||
            !IsFinite(error) || error > FilletPrecision(retreat, tolerance))
            throw new InvalidOperationException("A tangent point cannot be represented at these coordinates.");
        return point;
    }

    // Keep zero distance tolerance usable for small geometry without demanding
    // exact real arithmetic from double. This is a round-off floor, not a cut budget.
    private static double FilletPrecision(double length, double tolerance) =>
        Math.Max(tolerance, length * (64 * 2.2204460492503131e-16));

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
