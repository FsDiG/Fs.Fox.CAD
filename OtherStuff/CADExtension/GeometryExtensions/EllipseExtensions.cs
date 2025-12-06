using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the Ellipse type.
    /// </summary>
    public static class EllipseExtensions
    {
        /// <summary>
        /// Generates a polyline to approximate an ellipse.
        /// </summary>
        /// <param name="ellipse">The ellipse to be approximated</param>
        /// <returns>A new Polyline instance</returns>
        public static Polyline ToPolyline(this Ellipse ellipse)
        {
            Polyline pline = new PolylineSegmentCollection(ellipse).ToPolyline();
            pline.Closed = ellipse.Closed;
            pline.Normal = ellipse.Normal;
            pline.Elevation = ellipse.Center.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, ellipse.Normal))).Z;
            return pline;
        }

        /// <summary>
        /// Get the ellipse parameter at specified angle.
        /// </summary>
        /// <param name="ellipse">The ellipse the method applies to.</param>
        /// <param name="angle">The angle to get the corresponding parameter.</param>
        /// <returns>The ellipse parameter at angle.</returns>
        public static double GetParamAtAngle(this Ellipse ellipse, double angle)
        {
            return Convert(angle, ellipse.MinorRadius / ellipse.MajorRadius);
        }

        /// <summary>
        /// Get the ellipse angle at specified parameter.
        /// </summary>
        /// <param name="ellipse">The ellipse the method applies to.</param>
        /// <param name="param">The parameter to get the corresponding angle.</param>
        /// <returns>The ellipse angle at parameter.</returns>
        public static double GetAngleAtParam(this Ellipse ellipse, double param)
        {
            return Convert(param, ellipse.MajorRadius / ellipse.MinorRadius);
        }

        private static double Convert(double angle, double ratio)
        {
            double pi2 = Math.PI * 2;
            double result = Math.Atan(ratio * Math.Tan(angle)) +
                Math.Cos(angle) < 0.0 ? Math.PI : 0.0;
            if (Math.Abs(result) < 1e-15)
                return 0.0;
            while (result < 0.0) result += pi2;
            while (result > pi2) result -= pi2;
            return result;
        }
    }
}
