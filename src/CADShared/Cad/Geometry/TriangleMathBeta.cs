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
        TrianglePointDataBeta c, double sineTolerance, out double slopeDegrees, out double aspectDegrees)
    {
        if (!Finite(sineTolerance) || sineTolerance <= 0 || sineTolerance >= 1)
            throw new ArgumentOutOfRangeException(nameof(sineTolerance), "容差必须在 0 与 1 之间。");

        slopeDegrees = double.NaN;
        aspectDegrees = double.NaN;
        // Subtract before scaling so a large shared offset does not erase small coordinates on other axes.
        var u = Difference(a, b);
        var v = Difference(a, c);
        var ux = u.X;
        var uy = u.Y;
        var uz = u.Z;
        var vx = v.X;
        var vy = v.Y;
        var vz = v.Z;
        // Normalize each edge separately so a short edge does not make the cross product underflow.
        if (!Normalize(ref ux, ref uy, ref uz) || !Normalize(ref vx, ref vy, ref vz))
            return false;
        var nx = uy * vz - uz * vy;
        var ny = uz * vx - ux * vz;
        var nz = ux * vy - uy * vx;
        var normalLength = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (normalLength <= sineTolerance || Math.Abs(nz) <= normalLength * sineTolerance)
            return false; // Collinear or vertical: not a single-valued terrain plane z(x,y).
        if (nz < 0)
        {
            nx = -nx;
            ny = -ny;
            nz = -nz;
        }
        var horizontal = Math.Sqrt(nx * nx + ny * ny);
        if (horizontal <= normalLength * sineTolerance)
        {
            slopeDegrees = 0;
            return true; // A horizontal plane has no downhill azimuth.
        }
        slopeDegrees = Math.Atan2(horizontal, nz) * 180 / Math.PI;
        aspectDegrees = Math.Atan2(nx, ny) * 180 / Math.PI;
        if (aspectDegrees < 0)
            aspectDegrees += 360;
        if (aspectDegrees == 0)
            aspectDegrees = 0; // Normalize negative zero.
        return true;
    }

    private static bool Normalize(ref double x, ref double y, ref double z)
    {
        var scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
        if (scale == 0)
            return false;
        x /= scale;
        y /= scale;
        z /= scale;
        var length = Math.Sqrt(x * x + y * y + z * z);
        x /= length;
        y /= length;
        z /= length;
        return true;
    }

    private static TrianglePointDataBeta Difference(TrianglePointDataBeta a, TrianglePointDataBeta b)
    {
        var x = b.X - a.X;
        var y = b.Y - a.Y;
        var z = b.Z - a.Z;
        if (Finite(x) && Finite(y) && Finite(z))
            return new TrianglePointDataBeta(x, y, z);
        // Halving both endpoints yields a parallel finite edge when direct subtraction overflows.
        return new TrianglePointDataBeta(b.X / 2 - a.X / 2, b.Y / 2 - a.Y / 2, b.Z / 2 - a.Z / 2);
    }

    private static double Mean(double a, double b, double c)
    {
        var mean = a / 3 + b / 3 + c / 3;
        if (Finite(mean) && (mean != 0 || (a == 0 && b == 0 && c == 0)))
            return mean;
        // At the extreme ends, rounding of three thirds can overflow or underflow.
        var scale = Math.Max(Math.Abs(a), Math.Max(Math.Abs(b), Math.Abs(c)));
        return ((a / scale + b / scale + c / scale) / 3) * scale;
    }

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
