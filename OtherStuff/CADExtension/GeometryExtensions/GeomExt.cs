using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcRx = Autodesk.AutoCAD.Runtime;

namespace GeometryExtensions
{
    /// <summary>
    /// AutoCAD coordinate systems enumeration.
    /// </summary>
    public enum CoordSystem
    {
        /// <summary>
        /// World Coordinate System.
        /// </summary>
        WCS = 0,

        /// <summary>
        /// Current User Coordinate System. 
        /// </summary>
        UCS,

        /// <summary>
        /// Display Coordinate System of the current viewport.
        /// </summary>
        DCS,

        /// <summary>
        /// Paper Space Display Coordinate System.
        /// </summary>
        PSDCS
    }

    /// <summary>
    /// Tangent type enum
    /// </summary>
    [Flags]
    public enum TangentType
    {
        /// <summary>
        /// Inner tangents of two circles.
        /// </summary>
        Inner = 1,

        /// <summary>
        /// Outer tangents of two circles.
        /// </summary>
        Outer = 2
    }

    /// <summary>
    /// Provides extension methods.
    /// </summary>
    internal static class GeomExt
    {
        /// <summary>
        /// Creates a new Polyline that is the result of projecting the polyline parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="pline">The polyline (any type) to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        internal static Polyline ProjectPolyline(Curve pline, Plane plane, Vector3d direction)
        {
            if (!(pline is Polyline) && !(pline is Polyline2d) && !(pline is Polyline3d))
                return null;
            plane = new Plane(Point3d.Origin.OrthoProject(plane), direction);
            using (DBObjectCollection oldCol = new DBObjectCollection())
            using (DBObjectCollection newCol = new DBObjectCollection())
            {
                pline.Explode(oldCol);
                foreach (DBObject obj in oldCol)
                {
                    Curve crv = obj as Curve;
                    if (crv != null)
                    {
                        Curve flat = crv.GetProjectedCurve(plane, direction);
                        newCol.Add(flat);
                    }
                    obj.Dispose();
                }
                PolylineSegmentCollection psc = new PolylineSegmentCollection();
                for (int i = 0; i < newCol.Count; i++)
                {
                    if (newCol[i] is Ellipse)
                    {
                        psc.AddRange(new PolylineSegmentCollection((Ellipse)newCol[i]));
                        continue;
                    }
                    Curve crv = (Curve)newCol[i];
                    Point3d start = crv.StartPoint;
                    Point3d end = crv.EndPoint;
                    double bulge = 0.0;
                    if (crv is Arc)
                    {
                        Arc arc = (Arc)crv;
                        double angle = arc.Center.GetVectorTo(start).GetAngleTo(arc.Center.GetVectorTo(end), arc.Normal);
                        bulge = Math.Tan(angle / 4.0);
                    }
                    psc.Add(new PolylineSegment(start.Convert2d(plane), end.Convert2d(plane), bulge));
                }
                foreach (DBObject o in newCol) o.Dispose();
                Polyline projectedPline = psc.Join(new Tolerance(1e-9, 1e-9))[0].ToPolyline();
                projectedPline.Normal = direction;
                projectedPline.Elevation =
                    plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, direction))).Z;
                if (!pline.StartPoint.Project(plane, direction).IsEqualTo(projectedPline.StartPoint, new Tolerance(1e-9, 1e-9)))
                {
                    projectedPline.Normal = direction = direction.Negate();
                    projectedPline.Elevation =
                        plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, direction))).Z;
                }
                return projectedPline;
            }
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the transformed MinPoint and MaxPoint of 'extents' 
        /// parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="extents">The Extents3d of a transformed from World to dirPlane Polyline.</param>
        /// <param name="plane">The plane onto which the points are to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection</param>
        /// <param name="dirPlane">The plane which origin is 0, 0, 0 and 'direction' is the normal.</param>
        /// <returns>The newly created Polyline.</returns>
        internal static Polyline ProjectExtents(Extents3d extents, Plane plane, Vector3d direction, Plane dirPlane)
        {
            Point3d pt1 = extents.MinPoint.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
            Point3d pt2 = extents.MaxPoint.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
            Polyline projectedPline = new Polyline(2);
            projectedPline.AddVertexAt(0, pt1.Project(plane, direction).Convert2d(), 0.0, 0.0, 0.0);
            projectedPline.AddVertexAt(1, pt2.Project(plane, direction).Convert2d(), 0.0, 0.0, 0.0);
            return projectedPline;
        }
    }
}
