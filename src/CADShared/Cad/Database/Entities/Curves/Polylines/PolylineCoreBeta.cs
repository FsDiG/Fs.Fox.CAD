using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

/// <summary>OCS 平面中的顶点快照；仅含数值，不持有 CAD 对象或数据库资源。</summary>
internal readonly struct PolylineVertexDataBeta
{
    // 凸度及段宽属于从此顶点出发的段；开放线末点虽没有出段，仍保留其未使用字段供往返操作。
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

/// <summary>只变换数值快照的多段线核心；所有返回数组与输入独立，不负责 OCS 转换或数据库写入。</summary>
internal static class PolylineCoreBeta
{
    private const double MinimumNormal = 2.2250738585072014e-308;

    internal static void ValidateTolerance(double tolerance)
    {
        if (!IsFinite(tolerance) || tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be finite and nonnegative.");
    }

    internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    // 判断二维轴差是否落入距离阈值；次正规量先共同缩放，避免直接长度返回值的粗量化误判。
    // 调用方先验证容差有限且非负；轴差可以因两个有限坐标相减而溢出，此时直接拒绝匹配。
    internal static bool WithinDistance(double dx, double dy, double tolerance)
    {
        var x = Math.Abs(dx);
        var y = Math.Abs(dy);
        if (!(x <= tolerance && y <= tolerance))
            return false;
        if (tolerance == 0)
            return true;

        // 共乘 2^52 精确提升次正规量，避免 Hypot(Epsilon,Epsilon) 舍入为 Epsilon 而误接受。
        // 不改通用 Hypot 的长度返回值；正常尺度仍沿用原比较，保留 5-12-13 等合法边界。
        if (tolerance < MinimumNormal)
        {
            const double scale = 4503599627370496;
            x *= scale;
            y *= scale;
            tolerance *= scale;
        }
        return Hypot(x, y) <= tolerance;
    }

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
            // 末点字段仍须有限，但开放线的末点不形成实际段，不能拿它的 bulge 解释闭合段。
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
                // 将未使用的末点数据继续留在末点，保证连续反向两次还原全部数值字段。
                var terminal = vertices[vertices.Count - 1];
                result[i] = new PolylineVertexDataBeta(point.X, point.Y,
                    terminal.Bulge, terminal.StartWidth, terminal.EndWidth);
                continue;
            }

            // 反向后的出段来自原来的入段；闭合线固定首点，段凸度取反，起止宽度对换。
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
                // 验证阶段已拒绝零弦圆弧。重合直段只覆盖出段数据，原入段及其段宽保持不变。
                compact[compact.Count - 1] = vertex;
            }
            else
                compact.Add(vertex);
        }

        if (closed && compact.Count > 1 && SamePoint(compact[compact.Count - 1], compact[0]))
            compact.RemoveAt(compact.Count - 1);
        if (compact.Count < 2)
            throw new InvalidOperationException("Cleaning would leave no nonzero segment.");

        // 闭合首点是固定锚点；临时附加首点来处理末段，但不跨首点合并。
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
            // 次正规段的已舍入长度不能提供可靠单位方向，保留非零段而不尝试合并；
            // 上面的重合顶点压缩仍然有效，不额外拒绝这些输入或扩大距离容差。
            if (first.Bulge == 0 && first.EndWidth == width && IsFinite(length) && length >= MinimumNormal)
            {
                var ux = dx / length;
                var uy = dy / length;
                // 局部转向只需方向符号，按最大分量缩放，避免两次独立单位化给精确直角引入正点积。
                var directionScale = Math.Max(Math.Abs(dx), Math.Abs(dy));
                var previousDirectionX = dx / directionScale;
                var previousDirectionY = dy / directionScale;
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
                    if (!IsFinite(stepLength) || stepLength < MinimumNormal)
                        break;
                    var stepScale = Math.Max(Math.Abs(stepX), Math.Abs(stepY));
                    var directionX = stepX / stepScale;
                    var directionY = stepY / stepScale;
                    var forwardTurn = previousDirectionX * directionX + previousDirectionY * directionY;
                    var projection = px * ux + py * uy;
                    var deviation = Math.Abs(px * uy - py * ux);
                    // 参考线始终固定。原顶点和最终端点均在半容差带内，因此原顶点到最终弦的
                    // 距离至多为整个容差；参考投影严格递增，保证比较点位于有限弦段而非延长线。
                    // 另用相邻原段的点积保护局部折返，避免窄带内的 90 度或更大转向被消掉。
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
                if (WithinDistance(a.X - b.X, a.Y - b.Y, tolerance))
                {
                    match = firstEnd * 2 + secondEnd;
                    count++;
                }
            }
        }

        if (count != 1)
            return false;

        // 四种唯一配对依次为首/首、首/末、末/首、末/末。只调整第二条线的排列和方向，
        // 第一条线始终按原行进方向保留；容差过大导致多组配对时，前面已保守拒绝。
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
            // 不吸附或移动原圆弧端点；间隙另建直段，段宽从前段终宽过渡到后段起宽。
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
            // 全线恒宽使新增直段/圆弧的接头连续；变宽或跨段不连续时拒绝，不能丢弃原宽度。
            if (start.StartWidth != width || start.EndWidth != width)
                throw new NotSupportedException("Filleting requires one constant width on all actual segments.");
            var end = vertices[(i + 1) % vertices.Count];
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var length = Hypot(dx, dy);
            if (!IsFinite(length) || length <= tolerance)
                throw new InvalidOperationException("Every segment must have a finite length greater than tolerance.");
            lengths[i] = length;
            // 按最大分量缩放后判断转向；先单位化会给成比例的直段引入伪小转角。
            var scale = Math.Max(Math.Abs(dx), Math.Abs(dy));
            directionsX[i] = dx / scale;
            directionsY[i] = dy / scale;
            // 非零方向分量下溢会吞掉真实转角；弦长有限并不保证方向和切点仍可表示。
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
            // 距离容差不参与角度分类：精确同向共线角保留，精确 180 度折返无法构造圆角。
            if (cross == 0)
            {
                if (dot <= 0)
                    throw new InvalidOperationException("A reversing collinear corner cannot be filleted.");
                continue;
            }
            // 回退量 d = r*tan(|转角|/2)。按点积符号选择半角恒等式，避免近折返时分母相消；
            // 每个角均使用调用者指定半径，不能为适应短边而静默缩小。
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

        // 先规划全部角再输出。相邻角各自可行并不意味着共享边能容纳两端回退；
        // 切点恰好相接也拒绝，每条剩余直段都必须严格长于距离容差。
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
            // 大坐标偏移可能吞掉可表示的回退量；以实际输出弦长复核半径，避免返回舍入后变形的圆角。
            if (!IsFinite(chord) || chord <= tolerance || !IsFinite(actualRadius) ||
                Math.Abs(actualRadius - radius) > FilletPrecision(radius, tolerance))
                throw new InvalidOperationException("The requested fillet radius cannot be represented at these coordinates.");
            entries[i] = entry;
            exits[i] = exit;
        }
        // 再检查已表示的切点，防止理论预算通过，但大偏移舍入使剩余边塌缩或方向反转。
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

    // 零距离容差仍须允许局部 double 运算的舍入；此下限只用于精度复核，不放宽共享边预算。
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
