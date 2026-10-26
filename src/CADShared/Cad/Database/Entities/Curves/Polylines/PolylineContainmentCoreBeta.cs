using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

/// <summary>闭合中心线区域的解析查询核心；只读取数值快照，不依赖宿主求交或随机射线重试。</summary>
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
        // 在任何关系判断（包括平面外判断）之前完整验证源数据；宽度不是中心线区域的输入。
        for (var i = 0; i < vertices.Count; i++)
            if (!IsFinite(vertices[i].X) || !IsFinite(vertices[i].Y) || !IsFinite(vertices[i].Bulge))
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
                continue; // 重合直段不增加面积或射线穿越；零弦圆弧已单独拒绝。
            }
            segments.Add(new Segment(a, b));
        }
        if (segments.Count == 0)
            throw new InvalidOperationException("The contour has no nonzero segment.");
        return new Loop(segments.ToArray());
    }

    internal static PolylinePointRelationBeta ClassifyPoint(IReadOnlyList<PolylineVertexDataBeta> vertices,
        double x, double y, double tolerance) => Prepare(vertices).Classify(x, y, tolerance);

    /// <summary>独立的已准备轮廓；各段持有值拷贝，调用方以后修改顶点数组不影响查询。</summary>
    internal sealed class Loop
    {
        private readonly Segment[] _segments;
        internal Loop(Segment[] segments) => _segments = segments;

        internal PolylinePointRelationBeta Classify(double x, double y, double tolerance)
        {
            ValidateTolerance(tolerance);
            if (!IsFinite(x) || !IsFinite(y))
                throw new ArgumentException("Query coordinates must be finite.");
            // 每条段可能穿越零次、一次或两次；只累计奇偶性。任何输入段的边界优先于填充结果。
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
        // 原始端点和包围框使用 OCS 坐标；查询中先以 A 为原点，再除以弦长，避免全局偏移影响方程。
        private readonly double _ax, _ay, _bx, _by, _length, _ux, _uy, _bulge;
        // 圆心/半径同样以弦长归一化，轴向仍与 OCS 一致；k 是圆心沿弦左法向的归一化偏移。
        private readonly double _cx, _cy, _radius, _k;
        private readonly double _minX, _maxX, _minY, _maxY;
        private readonly ArcPart[] _parts;

        internal Segment(PolylineVertexDataBeta a, PolylineVertexDataBeta b)
        {
            _ax = a.X; _ay = a.Y; _bx = b.X; _by = b.Y; _bulge = a.Bulge;
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            _length = Hypot(dx, dy);
            if (!IsFinite(_length) || _length == 0)
                throw new InvalidOperationException("The chord length cannot be represented.");
            _ux = dx / _length; _uy = dy / _length;
            if ((dx != 0 && _ux == 0) || (dy != 0 && _uy == 0))
                throw new InvalidOperationException("A chord direction component underflows.");
            _parts = Array.Empty<ArcPart>();
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
                // 全圆范围只是保守预筛，不代表所选圆弧。按局部圆尺度留出舍入量，避免浅弧因相消被排除。
                var padding = 64 * Epsilon * (_radius + Math.Abs(_cx) + Math.Abs(_cy) + 1);
                _minX = _ax + _length * (_cx - _radius - padding);
                _maxX = _ax + _length * (_cx + _radius + padding);
                _minY = _ay + _length * (_cy - _radius - padding);
                _maxY = _ay + _length * (_cy + _radius + padding);
            }
            else
            {
                // 直段必须直接使用输入端点。a + length*(delta/length) 的往返舍入可能把
                // 真实下端点排除在 minY 外，破坏顶点处的半开计数，令区域外点被误判为内部。
                _minX = Math.Min(_ax, _bx); _maxX = Math.Max(_ax, _bx);
                _minY = Math.Min(_ay, _by); _maxY = Math.Max(_ay, _by);
            }
            if (!IsFinite(_minX) || !IsFinite(_maxX) || !IsFinite(_minY) || !IsFinite(_maxY))
                throw new InvalidOperationException("The contour's coordinate range cannot be represented.");
        }

        internal bool Query(double x, double y, double tolerance, out int crossings)
        {
            crossings = 0;
            if (PolylineGeometryCoreBeta.WithinDistance(x - _ax, y - _ay, tolerance) ||
                PolylineGeometryCoreBeta.WithinDistance(x - _bx, y - _by, tolerance))
                return true;
            var canCheckBoundary = !((x < _minX && _minX - x > tolerance) || (x > _maxX && x - _maxX > tolerance) ||
                (y < _minY && _minY - y > tolerance) || (y > _maxY && y - _maxY > tolerance));
            // 足够靠左的点仍需累计穿越，但不必换算局部 X 或计算边界距离；
            // 用负无穷作为比较哨兵，避免一个普通区域外查询仅因全局轴差溢出而失败。
            var canCross = y >= _minY && y <= _maxY && x <= _maxX;
            if (!canCheckBoundary && !canCross) return false;
            var dx = x - _ax; var dy = y - _ay;
            var qx = canCheckBoundary ? dx / _length : double.NegativeInfinity;
            var qy = dy / _length;
            var localTolerance = tolerance / _length;
            if (!IsFinite(qy) || (dy != 0 && qy == 0) || (canCheckBoundary &&
                (!IsFinite(qx) || !IsFinite(localTolerance) || (dx != 0 && qx == 0))))
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
            // 将归一化查询点投影到弦切向 t 和左法向 n，圆方程为
            // t(t-1) + n(n+b/2) - n/(2b) = 0。b 与 1/b 分项保留，
            // 避免先求大圆心/半径再相减，丢失浅弧矢高或近整圆的端点信息。
            if (canCheckBoundary)
            {
                var along = qx * _ux + qy * _uy;
                var normal = qy * _ux - qx * _uy;
                var f = along * (along - 1) + normal * (normal + _bulge * .5) - normal * (.5 / _bulge);
                var errorF = 32 * Epsilon * (Math.Abs(along) * (Math.Abs(along) + 1) +
                    Math.Abs(normal) * (Math.Abs(normal) + Math.Abs(_bulge) * .5 + .5 / Math.Abs(_bulge)));
                // 浅弧将法向坐标误差放大约 1/(2b)；同时传播局部投影和方程运算的舍入量，
                // 避免同一条弧仅因旋转后投影相消而漏判边界。这不等于所有输入都有统一误差保证。
                var errorAlong = 32 * Epsilon * (Math.Abs(qx * _ux) + Math.Abs(qy * _uy));
                var errorNormal = 32 * Epsilon * (Math.Abs(qy * _ux) + Math.Abs(qx * _uy));
                errorF += errorAlong * (2 * Math.Abs(along) + 1) +
                    errorNormal * (2 * Math.Abs(normal) + Math.Abs(_bulge) * .5 + .5 / Math.Abs(_bulge));
                var radialDenominator = Hypot(qx - _cx, qy - _cy) + _radius;
                if (!IsFinite(f) || !IsFinite(errorF) || !IsFinite(radialDenominator))
                    throw new InvalidOperationException("The boundary distance cannot be resolved numerically.");
                // |距离²-半径²|/(距离+半径) 给出径向边界距离；两端有向叉积限定所选弧，
                // 小弧取两个半平面的交，主弧取并，不能把同一支撑圆的补弧也算作边界。
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
                // 原始顶点和圆极值统一采用半开 Y 区间。切线最低点计两次、最高点计零次，
                // 都不翻转填充奇偶性；两条相邻普通边只贡献应有的一次穿越。
                if ((part.StartY > qy) == (part.EndY > qy))
                    continue;
                var constant = qy * (qy - _uy + _bulge * .5 * _ux) - qy * (.5 / _bulge) * _ux;
                var discriminant = _cx * _cx - constant;
                var errorD = 64 * Epsilon * (_cx * _cx + Math.Abs(constant));
                if (!IsFinite(discriminant) || discriminant < -errorD)
                    throw new InvalidOperationException("An arc crossing cannot be resolved numerically.");
                var root = Math.Sqrt(Math.Max(0, discriminant));
                // 先求无相消的远根，再用根积反求近根；直接 cx-sqrt(D) 会丢失靠近弦原点的交点。
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
            // 只在所选弧内的上下极值处分段；圆弧至多经过两个极值，因此最多得到三段 Y 单调弧。
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
            // 支撑圆经过局部原点，故 (cy-r)(cy+r)=-cx²；对近值相减的一侧有理化，保留浅弧矢高。
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
        if (!IsFinite(tolerance) || tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be finite and nonnegative.");
    }

    internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    private static double Hypot(double x, double y)
    {
        x = Math.Abs(x); y = Math.Abs(y);
        var maximum = Math.Max(x, y);
        if (maximum == 0 || double.IsInfinity(maximum)) return maximum;
        x /= maximum; y /= maximum;
        return maximum * Math.Sqrt(x * x + y * y);
    }
}
