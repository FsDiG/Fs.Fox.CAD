using System;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the CircularArc2dType
    /// </summary>
    public static class CircularArc3dExtensions
    {
        /// <summary>
        /// Returns the tangents between the active CircularArc3d instance complete circle and a point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument.
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center
        /// to the point before the one on the right side. 
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="pt">The Point3d to which tangents are searched</param>
        /// <returns>An array of LineSegement3d representing the tangents (2) or null if there is none.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        public static LineSegment3d[]? GetTangentsTo(this CircularArc3d arc, Point3d pt)
        {
            // check if arc and point lies on the plane
            Vector3d normal = arc.Normal;
            Matrix3d WCS2OCS = Matrix3d.WorldToPlane(normal);
            double elevation = arc.Center.TransformBy(WCS2OCS).Z;
            if (Math.Abs(elevation - pt.TransformBy(WCS2OCS).Z) < Tolerance.Global.EqualPoint)
            {
                throw new Autodesk.AutoCAD.Runtime.Exception(
                    Autodesk.AutoCAD.Runtime.ErrorStatus.NonCoplanarGeometry);
            }

            Plane plane = new Plane(Point3d.Origin, normal);
            Matrix3d OCS2WCS = Matrix3d.PlaneToWorld(plane);
            CircularArc2d ca2d = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            LineSegment2d[] lines2d = ca2d.GetTangentsTo(pt.Convert2d(plane));

            if (lines2d == null)
            {
                return null;
            }

            LineSegment3d[]? result = new LineSegment3d[lines2d.Length];
            for (int i = 0; i < lines2d.Length; i++)
            {
                LineSegment2d ls2d = lines2d[i];
                result[i] = new LineSegment3d(ls2d.StartPoint.Convert3d(normal, elevation), ls2d.EndPoint.Convert3d(normal, elevation));
            }
            return result;
        }

        /// <summary>
        /// Returns the tangents between the active CircularArc3d instance complete circle and another one.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument.
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both,
        /// the tangent on the left side of the line from this circular arc center to the other one before the one on the right side. 
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="other">The CircularArc3d to which searched for tangents.</param>
        /// <param name="flags">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment3d representing the tangents (maybe 2 or 4) or null if there is none.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        public static LineSegment3d[]? GetTangentsTo(this CircularArc3d arc, CircularArc3d other, TangentType flags)
        {
            // check if circles lies on the same plane
            Vector3d normal = arc.Normal;
            Matrix3d WCS2OCS = Matrix3d.WorldToPlane(normal);
            double elevation = arc.Center.TransformBy(WCS2OCS).Z;
            if (!(normal.IsParallelTo(other.Normal) &&
                Math.Abs(elevation - other.Center.TransformBy(WCS2OCS).Z) < Tolerance.Global.EqualPoint))
            {
                throw new Autodesk.AutoCAD.Runtime.Exception(
                    Autodesk.AutoCAD.Runtime.ErrorStatus.NonCoplanarGeometry);
            }

            Plane plane = new Plane(Point3d.Origin, normal);
            Matrix3d OCS2WCS = Matrix3d.PlaneToWorld(plane);
            CircularArc2d ca2d1 = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            CircularArc2d ca2d2 = new CircularArc2d(other.Center.Convert2d(plane), other.Radius);
            LineSegment2d[] lines2d = ca2d1.GetTangentsTo(ca2d2, flags);

            if (lines2d == null)
            {
                return null;
            }

            LineSegment3d[]? result = new LineSegment3d[lines2d.Length];
            for (int i = 0; i < lines2d.Length; i++)
            {
                LineSegment2d ls2d = lines2d[i];
                result[i] = new LineSegment3d(ls2d.StartPoint.Convert3d(normal, elevation), ls2d.EndPoint.Convert3d(normal, elevation));
            }
            return result;
        }
    }
}
