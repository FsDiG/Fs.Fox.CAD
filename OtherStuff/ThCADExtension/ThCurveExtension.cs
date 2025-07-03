using System;
using NFox.Cad;
using DotNetARX;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThCurveExtension
    {
        /// <summary>
        /// 获取曲线的中点
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static Point3d GetMidpoint(this Curve curve)
        {
            double d1 = curve.GetDistanceAtParameter(curve.StartParam);
            double d2 = curve.GetDistanceAtParameter(curve.EndParam);
            return curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));
        }


        // Make sure the pt1 and pt2 are on the Curve before calling this method.
        //  https://spiderinnet1.typepad.com/blog/2012/10/autocad-net-isonpoint3d-curvegetclosestpointto-curvegetparameteratpoint.html
        public static double GetLength(this Curve ent, Point3d pt1, Point3d pt2)
        {
            double dist1 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt1, false)));
            double dist2 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt2, false)));

            return Math.Abs(dist1 - dist2);
        }

        // https://www.keanw.com/2012/01/testing-whether-a-point-is-on-any-autocad-curve-using-net.html
        public static bool IsPointOnCurve(this Curve curve, Point3d pt, Tolerance tolerance)
        {
            return pt.IsEqualTo(curve.GetClosestPointTo(pt, false), tolerance);
        }

        public static double BulgeFromCurve(this Curve cv, bool clockwise)
        {
            double bulge = 0.0;
            Arc a = cv as Arc;
            if (a != null)
            {
                double newStart;
                // The start angle is usually greater than the end,
                // as arcs are all counter-clockwise.
                // (If it isn't it's because the arc crosses the
                // 0-degree line, and we can subtract 2PI from the
                // start angle.)
                if (a.StartAngle > a.EndAngle)
                    newStart = a.StartAngle - 8 * Math.Atan(1);
                else
                    newStart = a.StartAngle;

                // Bulge is defined as the tan of
                // one fourth of the included angle
                bulge = Math.Tan((a.EndAngle - newStart) / 4);
                // If the curve is clockwise, we negate the bulge
                if (clockwise)
                    bulge = -bulge;

            }
            return bulge;
        }

        public static LinearEntity3d ToGeLine(this Line line)
        {
            return new Line3d(line.StartPoint, line.EndPoint);
        }

        public static Curve WashClone(this Curve curve)
        {
            return curve.ToCurve3d().ToCurve();
        }
    }
}
