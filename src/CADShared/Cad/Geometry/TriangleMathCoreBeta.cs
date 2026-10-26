using System;

namespace Fs.Fox.Cad;

// 与宿主 SDK 解耦的三维坐标快照；数值回归直接调用同一份生产核心。
internal readonly struct TrianglePointDataBeta
{
    internal readonly double X;
    internal readonly double Y;
    internal readonly double Z;

    internal TrianglePointDataBeta(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

internal static class TriangleMathCoreBeta
{
    internal static void Validate(TrianglePointDataBeta point, string parameterName)
    {
        if (!IsFinite(point.X) || !IsFinite(point.Y) || !IsFinite(point.Z))
            throw new ArgumentException("三角形顶点坐标必须为有限数值。", parameterName);
    }

    internal static TrianglePointDataBeta GetCentroid(TrianglePointDataBeta a,
        TrianglePointDataBeta b, TrianglePointDataBeta c) => new(
        Mean(a.X, b.X, c.X), Mean(a.Y, b.Y, c.Y), Mean(a.Z, b.Z, c.Z));

    internal static bool TryGetSlopeAndAspectDegrees(TrianglePointDataBeta a, TrianglePointDataBeta b,
        TrianglePointDataBeta c, double relativeTolerance, out double slopeDegrees, out double aspectDegrees)
    {
        if (!IsFinite(relativeTolerance) || relativeTolerance <= 0 || relativeTolerance >= 1)
            throw new ArgumentOutOfRangeException(nameof(relativeTolerance), "容差必须在 0 与 1 之间。");

        slopeDegrees = double.NaN;
        aspectDegrees = double.NaN;
        // 先求坐标差再归一化，避免某轴的大公共偏移把其他轴的有效小差值压成零。
        var u = Difference(a, b, false);
        var v = Difference(a, c, false);
        var w = Difference(b, c, false);
        if (!IsFinitePoint(u) || !IsFinitePoint(v) || !IsFinitePoint(w))
        {
            // 仅在有限端点相减溢出时先将所有坐标减半；三条边必须使用同一比例，保持面积/最长边平方的比值。
            u = Difference(a, b, true);
            v = Difference(a, c, true);
            w = Difference(b, c, true);
        }
        var scale = Math.Max(MaxAbs(u), Math.Max(MaxAbs(v), MaxAbs(w)));
        if (scale == 0)
            return false;
        var ux = u.X / scale;
        var uy = u.Y / scale;
        var uz = u.Z / scale;
        var vx = v.X / scale;
        var vy = v.Y / scale;
        var vz = v.Z / scale;
        var wx = w.X / scale;
        var wy = w.Y / scale;
        var wz = w.Z / scale;
        var longestSquared = Math.Max(ux * ux + uy * uy + uz * uz,
            Math.Max(vx * vx + vy * vy + vz * vz, wx * wx + wy * wy + wz * wz));
        var nx = uy * vz - uz * vy;
        var ny = uz * vx - ux * vz;
        var nz = ux * vy - uy * vx;
        var normalLength = Length(nx, ny, nz);
        // |u×v| 是两倍面积；除以三边中最长边的平方后无量纲，避免换首顶点就改变退化阈值。
        // 不以“平方后等于零”判断退化：极瘦但仍大于调用方容差的法向量，其分量平方可能下溢。
        if (normalLength <= longestSquared * relativeTolerance || Math.Abs(nz) <= normalLength * relativeTolerance)
            return false; // 共线或近垂直面不能作为单值地形面 z(x,y)，两个输出保留 NaN。
        if (nz < 0)
        {
            nx = -nx;
            ny = -ny;
            nz = -nz;
        }
        var horizontal = Length(nx, ny, 0);
        if (horizontal <= normalLength * relativeTolerance)
        {
            slopeDegrees = 0;
            return true; // 水平面没有唯一的下坡方向，坡向继续保留 NaN。
        }
        var slope = Math.Atan2(horizontal, nz) * 180 / Math.PI;
        // 非垂直法线的角度也可能舍入到 90 度；成功结果必须仍满足公开的半开区间。
        if (slope >= 90)
            return false;
        slopeDegrees = slope;
        // 法向量统一朝上后，地形梯度为 (-nx/nz,-ny/nz)，故 (nx,ny) 指向下坡，且不依赖绕序。
        aspectDegrees = Math.Atan2(nx, ny) * 180 / Math.PI;
        if (aspectDegrees < 0)
            aspectDegrees += 360;
        if (aspectDegrees >= 360 || aspectDegrees == 0)
            aspectDegrees = 0; // 极小负角加一周可能舍入为 360，同时把负零归一为正零。
        return true;
    }

    private static double Length(double x, double y, double z)
    {
        // 此处的法向量来自已归一化的边，分量有界；先按最大分量缩放，避免有效小量在平方时下溢。
        var scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
        if (scale == 0)
            return 0;
        x /= scale;
        y /= scale;
        z /= scale;
        return scale * Math.Sqrt(x * x + y * y + z * z);
    }

    private static TrianglePointDataBeta Difference(TrianglePointDataBeta a, TrianglePointDataBeta b, bool half)
    {
        return half
            ? new TrianglePointDataBeta(b.X / 2 - a.X / 2, b.Y / 2 - a.Y / 2, b.Z / 2 - a.Z / 2)
            : new TrianglePointDataBeta(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
    }

    private static double Mean(double a, double b, double c)
    {
        var sum = CompensatedSum(a, b, c);
        if (IsFinite(sum))
            return sum / 3;
        // 普通正负抵消沿用未缩放的补偿结果；只有求和溢出才归一化，避免先缩放丢失较小残差。
        var scale = Math.Max(Math.Abs(a), Math.Max(Math.Abs(b), Math.Abs(c)));
        return (CompensatedSum(a / scale, b / scale, c / scale) / 3) * scale;
    }

    private static double CompensatedSum(double a, double b, double c)
    {
        // Neumaier 补偿累计两次加法的低位残差，例如 1e16 + 1 - 1e16 仍能保留中间的 1。
        var sum = a;
        var next = sum + b;
        var error = Math.Abs(sum) >= Math.Abs(b) ? (sum - next) + b : (b - next) + sum;
        sum = next;
        next = sum + c;
        error += Math.Abs(sum) >= Math.Abs(c) ? (sum - next) + c : (c - next) + sum;
        return next + error;
    }

    private static double MaxAbs(TrianglePointDataBeta point) =>
        Math.Max(Math.Abs(point.X), Math.Max(Math.Abs(point.Y), Math.Abs(point.Z)));

    private static bool IsFinitePoint(TrianglePointDataBeta point) => IsFinite(point.X) && IsFinite(point.Y) && IsFinite(point.Z);

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
