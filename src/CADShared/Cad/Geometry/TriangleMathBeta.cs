using System;

namespace Fs.Fox.Cad;

// Reworked from ZFGK.AutoCAD.Geometry.GeTriangle (Zhang Fan); SDK-free numeric core.
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

internal static class TriangleMathBeta
{
    internal static void Validate(TrianglePointDataBeta point, string parameterName)
    {
        if (!Finite(point.X) || !Finite(point.Y) || !Finite(point.Z))
            throw new ArgumentException("三角形顶点坐标必须为有限数值。", parameterName);
    }

    internal static TrianglePointDataBeta Centroid(TrianglePointDataBeta a,
        TrianglePointDataBeta b, TrianglePointDataBeta c) => new(
        Mean(a.X, b.X, c.X), Mean(a.Y, b.Y, c.Y), Mean(a.Z, b.Z, c.Z));

    internal static bool TrySlopeAndAspect(TrianglePointDataBeta a, TrianglePointDataBeta b,
        TrianglePointDataBeta c, double relativeTolerance, out double slopeDegrees, out double aspectDegrees)
    {
        if (!Finite(relativeTolerance) || relativeTolerance <= 0 || relativeTolerance >= 1)
            throw new ArgumentOutOfRangeException(nameof(relativeTolerance), "容差必须在 0 与 1 之间。");

        slopeDegrees = double.NaN;
        aspectDegrees = double.NaN;
        // Subtract before scaling so a large shared offset does not erase small coordinates on other axes.
        var u = Difference(a, b, false);
        var v = Difference(a, c, false);
        var w = Difference(b, c, false);
        if (!FinitePoint(u) || !FinitePoint(v) || !FinitePoint(w))
        {
            // Use the same factor for every edge, retaining their relative lengths.
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
        var normalLength = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        // Twice-area / longest-edge-squared is invariant under every permutation of the vertices.
        if (normalLength <= longestSquared * relativeTolerance || Math.Abs(nz) <= normalLength * relativeTolerance)
            return false; // Collinear or vertical: not a single-valued terrain plane z(x,y).
        if (nz < 0)
        {
            nx = -nx;
            ny = -ny;
            nz = -nz;
        }
        var horizontal = Math.Sqrt(nx * nx + ny * ny);
        if (horizontal <= normalLength * relativeTolerance)
        {
            slopeDegrees = 0;
            return true; // A horizontal plane has no downhill azimuth.
        }
        var slope = Math.Atan2(horizontal, nz) * 180 / Math.PI;
        // A nonvertical normal can still round to 90 degrees; do not return a value outside the public range.
        if (slope >= 90)
            return false;
        slopeDegrees = slope;
        // With an upward normal, (nx, ny) is the downhill projection of z(x,y), independent of winding.
        aspectDegrees = Math.Atan2(nx, ny) * 180 / Math.PI;
        if (aspectDegrees < 0)
            aspectDegrees += 360;
        if (aspectDegrees >= 360 || aspectDegrees == 0)
            aspectDegrees = 0; // Adding 360 to a tiny negative angle may round to 360; also normalize negative zero.
        return true;
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
        if (Finite(sum))
            return sum / 3;
        // Normalize only if summing the finite inputs overflows; normal cancellation keeps its precision.
        var scale = Math.Max(Math.Abs(a), Math.Max(Math.Abs(b), Math.Abs(c)));
        return (CompensatedSum(a / scale, b / scale, c / scale) / 3) * scale;
    }

    private static double CompensatedSum(double a, double b, double c)
    {
        // Neumaier compensation retains the low-order term when large coordinates cancel.
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

    private static bool FinitePoint(TrianglePointDataBeta point) => Finite(point.X) && Finite(point.Y) && Finite(point.Z);

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
