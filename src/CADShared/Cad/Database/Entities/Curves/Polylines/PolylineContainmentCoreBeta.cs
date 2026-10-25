using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

/// <summary>点与闭合轻量多段线区域的关系；Beta 数值及宿主边界见查询入口。</summary>
public enum PolylinePointRelationBeta
{
    /// <summary>位于偶奇填充区域外。</summary>
    Outside,
    /// <summary>位于偶奇填充区域内。</summary>
    Inside,
    /// <summary>在输入直线/圆弧边界的距离容差内；边界优先于填充规则。</summary>
    Boundary,
    /// <summary>到多段线基准平面的距离大于平面容差。</summary>
    OffPlane
}

// Reworked capability: Zfgk PolylineUtil.PtRelationToPoly. This production core
// uses finite analytic crossings, without SDK intersections or retrying rays.
internal static class PolylineContainmentCoreBeta
{
    private const double Epsilon = 2.2204460492503131e-16;
    private const double TwoPi = 2 * Math.PI;
    internal const double MinimumAbsoluteBulge = 1e-8;
    internal const double MaximumAbsoluteBulge = 1e8;

    internal static Loop Prepare(IReadOnlyList<PolylineVertexDataBeta> vertices)
    {
        if (vertices == null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Count < 2)
            throw new ArgumentException("A closed contour needs at least two vertices.", nameof(vertices));
        // Validate every field used by the region before any classification, including OffPlane.
        // Widths intentionally do not participate in centerline-region geometry.
        for (var i = 0; i < vertices.Count; i++)
            if (!Finite(vertices[i].X) || !Finite(vertices[i].Y) || !Finite(vertices[i].Bulge))
                throw new ArgumentException("Coordinates and bulges must be finite.", nameof(vertices));
        var segments = new List<Segment>(vertices.Count);
        for (var i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            if (a.X == b.X && a.Y == b.Y)
            {
                if (a.Bulge != 0)
                    throw new InvalidOperationException("An arc cannot have coincident endpoints.");
                continue; // Repeated straight vertices contribute no area or crossings.
            }
            segments.Add(new Segment(a, b));
        }
        if (segments.Count == 0)
            throw new InvalidOperationException("The contour has no nonzero segment.");
        return new Loop(segments.ToArray());
    }

    internal static PolylinePointRelationBeta GetPointRelation(IReadOnlyList<PolylineVertexDataBeta> vertices,
        double x, double y, double tolerance) => Prepare(vertices).Classify(x, y, tolerance);

    internal sealed class Loop
    {
        private readonly Segment[] _segments;
        internal Loop(Segment[] segments) => _segments = segments;

        internal PolylinePointRelationBeta Classify(double x, double y, double tolerance)
        {
            ValidateTolerance(tolerance);
            if (!Finite(x) || !Finite(y))
                throw new ArgumentException("Query coordinates must be finite.");
            var inside = false;
            foreach (var segment in _segments)
            {
                if (segment.Query(x, y, tolerance, out var crossings))
                    return PolylinePointRelationBeta.Boundary;
                if ((crossings & 1) != 0)
                    inside = !inside;
            }
            return inside ? PolylinePointRelationBeta.Inside : PolylinePointRelationBeta.Outside;
        }
    }

    internal sealed class Segment
    {
        private readonly double _ax, _ay, _bx, _by, _length, _ux, _uy, _bulge;
        private readonly double _cx, _cy, _radius, _k;
        private readonly double _minX, _maxX, _minY, _maxY;
        private readonly ArcPart[] _parts;

        internal Segment(PolylineVertexDataBeta a, PolylineVertexDataBeta b)
        {
            _ax = a.X; _ay = a.Y; _bx = b.X; _by = b.Y; _bulge = a.Bulge;
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            _length = Hypot(dx, dy);
            if (!Finite(_length) || _length == 0)
                throw new InvalidOperationException("The chord length cannot be represented.");
            _ux = dx / _length; _uy = dy / _length;
            if ((dx != 0 && _ux == 0) || (dy != 0 && _uy == 0))
                throw new InvalidOperationException("A chord direction component underflows.");
            _parts = Array.Empty<ArcPart>();
            double lowX = Math.Min(0, _ux), highX = Math.Max(0, _ux);
            double lowY = Math.Min(0, _uy), highY = Math.Max(0, _uy);
            if (_bulge != 0)
            {
                var absolute = Math.Abs(_bulge);
                if (absolute < MinimumAbsoluteBulge || absolute > MaximumAbsoluteBulge)
                    throw new NotSupportedException("Nonzero absolute bulge must lie in [1e-8, 1e8] for this Beta query.");
                _k = (1 / _bulge - _bulge) * .25;
                _cx = .5 * _ux - _k * _uy;
                _cy = .5 * _uy + _k * _ux;
                _radius = Hypot(_cx, _cy);
                _parts = CreateParts();
                // Loose full-circle bounds only skip certainly distant queries. They are
                // padded locally, so cancellation in a shallow circle cannot remove its arc.
                var padding = 64 * Epsilon * (_radius + Math.Abs(_cx) + Math.Abs(_cy) + 1);
                lowX = _cx - _radius - padding; highX = _cx + _radius + padding;
                lowY = _cy - _radius - padding; highY = _cy + _radius + padding;
            }
            _minX = _ax + _length * lowX; _maxX = _ax + _length * highX;
            _minY = _ay + _length * lowY; _maxY = _ay + _length * highY;
            if (!Finite(_minX) || !Finite(_maxX) || !Finite(_minY) || !Finite(_maxY))
                throw new InvalidOperationException("The contour's coordinate range cannot be represented.");
        }

        internal bool Query(double x, double y, double tolerance, out int crossings)
        {
            crossings = 0;
            if (Hypot(x - _ax, y - _ay) <= tolerance || Hypot(x - _bx, y - _by) <= tolerance)
                return true;
            var canCheckBoundary = !((x < _minX && _minX - x > tolerance) || (x > _maxX && x - _maxX > tolerance) ||
                (y < _minY && _minY - y > tolerance) || (y > _maxY && y - _maxY > tolerance));
            // A far-left point still sees crossings, but needs no local X conversion
            // or boundary arithmetic. This avoids overflowing a perfectly ordinary outside query.
            var canCross = y >= _minY && y <= _maxY && x <= _maxX;
            if (!canCheckBoundary && !canCross) return false;
            var dx = x - _ax; var dy = y - _ay;
            var qx = canCheckBoundary ? dx / _length : double.NegativeInfinity;
            var qy = dy / _length;
            var localTolerance = tolerance / _length;
            if (!Finite(qy) || (dy != 0 && qy == 0) || (canCheckBoundary &&
                (!Finite(qx) || !Finite(localTolerance) || (dx != 0 && qx == 0))))
                throw new InvalidOperationException("The query cannot be resolved in the chord's coordinate scale.");
            if (_bulge == 0)
            {
                if (canCheckBoundary)
                {
                    var along = qx * _ux + qy * _uy;
                    var normal = qy * _ux - qx * _uy;
                    var error = 32 * Epsilon * (Math.Abs(qx * _uy) + Math.Abs(qy * _ux));
                    if (along >= 0 && along <= 1 && Math.Abs(normal) <= localTolerance + error)
                        return true;
                }
                if (canCross && (0 > qy) != (_uy > qy) && qy / _uy * _ux > qx)
                    crossings = 1;
                return false;
            }
            // In chord coordinates the circle equation is
            // t(t-1) + n(n+b/2) - n/(2b) = 0. Keep the b and 1/b terms separate:
            // forming center/radius first loses shallow sagittas and near-full-circle detail.
            if (canCheckBoundary)
            {
                var along = qx * _ux + qy * _uy;
                var normal = qy * _ux - qx * _uy;
                var f = along * (along - 1) + normal * (normal + _bulge * .5) - normal * (.5 / _bulge);
                var errorF = 32 * Epsilon * (Math.Abs(along) * (Math.Abs(along) + 1) +
                    Math.Abs(normal) * (Math.Abs(normal) + Math.Abs(_bulge) * .5 + .5 / Math.Abs(_bulge)));
                // A rotated shallow arc amplifies error in its normal coordinate by 1/(2b).
                // Propagate local projection round-off as well as equation arithmetic;
                // otherwise its midpoint can miss Boundary even when the same axis-aligned arc succeeds.
                var errorAlong = 32 * Epsilon * (Math.Abs(qx * _ux) + Math.Abs(qy * _uy));
                var errorNormal = 32 * Epsilon * (Math.Abs(qy * _ux) + Math.Abs(qx * _uy));
                errorF += errorAlong * (2 * Math.Abs(along) + 1) +
                    errorNormal * (2 * Math.Abs(normal) + Math.Abs(_bulge) * .5 + .5 / Math.Abs(_bulge));
                var radialDenominator = Hypot(qx - _cx, qy - _cy) + _radius;
                if (!Finite(f) || !Finite(errorF) || !Finite(radialDenominator))
                    throw new InvalidOperationException("The boundary distance cannot be resolved numerically.");
                var firstCross = Math.Sign(_bulge) * (_k * along - .5 * normal);
                var secondCross = Math.Sign(_bulge) * (_k * (1 - along) - .5 * normal);
                var onSelectedArc = Math.Abs(_bulge) <= 1
                    ? firstCross >= 0 && secondCross >= 0
                    : firstCross >= 0 || secondCross >= 0;
                if (onSelectedArc && Math.Abs(f) <= localTolerance * radialDenominator + errorF)
                    return true;
            }
            if (!canCross) return false;
            foreach (var part in _parts)
            {
                // Half-open Y intervals apply equally to original vertices and extrema.
                // A tangent minimum is counted twice, a maximum zero times: neither flips parity.
                if ((part.StartY > qy) == (part.EndY > qy))
                    continue;
                var constant = qy * (qy - _uy + _bulge * .5 * _ux) - qy * (.5 / _bulge) * _ux;
                var discriminant = _cx * _cx - constant;
                var errorD = 64 * Epsilon * (_cx * _cx + Math.Abs(constant));
                if (!Finite(discriminant) || discriminant < -errorD)
                    throw new InvalidOperationException("An arc crossing cannot be resolved numerically.");
                var root = Math.Sqrt(Math.Max(0, discriminant));
                // Stable quadratic pair: use the product of roots instead of subtracting
                // nearly equal numbers when one intersection is close to the chord origin.
                var far = _cx >= 0 ? _cx + root : _cx - root;
                var near = far == 0 ? 0 : constant / far;
                var left = Math.Min(far, near); var right = Math.Max(far, near);
                if ((part.RightSide ? right : left) > qx)
                    crossings++;
            }
            return false;
        }

        private ArcPart[] CreateParts()
        {
            var startAngle = Math.Atan2(-_cy, -_cx);
            var sign = Math.Sign(_bulge);
            var sweep = Math.Abs(4 * Math.Atan(_bulge));
            var knots = new List<Knot> { new(0, 0), new(sweep, _uy) };
            foreach (var angle in new[] { Math.PI * .5, Math.PI * 1.5 })
            {
                var progress = sign * (angle - startAngle) % TwoPi;
                if (progress < 0) progress += TwoPi;
                if (progress > 0 && progress < sweep)
                    knots.Add(new Knot(progress, CircleYBound(angle < Math.PI)));
            }
            knots.Sort((a, b) => a.Progress.CompareTo(b.Progress));
            var parts = new ArcPart[knots.Count - 1];
            for (var i = 0; i < parts.Length; i++)
            {
                var middleAngle = startAngle + sign * (knots[i].Progress + knots[i + 1].Progress) * .5;
                parts[i] = new ArcPart(knots[i].Y, knots[i + 1].Y, Math.Cos(middleAngle) >= 0);
            }
            return parts;
        }

        private double CircleYBound(bool upper)
        {
            // The circle passes through the chord origin, so (cy-r)(cy+r)=-cx².
            // Rationalize the cancelling bound; direct cy-r erases a shallow arc's sagitta.
            if (upper && _cy < 0) return _cx * _cx / (_radius - _cy);
            if (!upper && _cy > 0) return -_cx * _cx / (_radius + _cy);
            return upper ? _cy + _radius : _cy - _radius;
        }
    }

    private readonly struct Knot
    {
        internal readonly double Progress, Y;
        internal Knot(double progress, double y) { Progress = progress; Y = y; }
    }

    private readonly struct ArcPart
    {
        internal readonly double StartY, EndY;
        internal readonly bool RightSide;
        internal ArcPart(double startY, double endY, bool rightSide)
        { StartY = startY; EndY = endY; RightSide = rightSide; }
    }

    internal static void ValidateTolerance(double tolerance)
    {
        if (!Finite(tolerance) || tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be finite and nonnegative.");
    }

    internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    private static double Hypot(double x, double y)
    {
        x = Math.Abs(x); y = Math.Abs(y);
        var maximum = Math.Max(x, y);
        if (maximum == 0 || double.IsInfinity(maximum)) return maximum;
        x /= maximum; y /= maximum;
        return maximum * Math.Sqrt(x * x + y * y);
    }
}
