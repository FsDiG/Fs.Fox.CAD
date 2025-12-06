using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the Polyline type.
    /// </summary>
    public static class PolylineExtensions
    {
        /// <summary>
        /// Breaks the polyline at specified point.
        /// </summary>
        /// <param name="pline">The polyline this method applies to.</param>
        /// <param name="brkPt">The point where to break the polyline.</param>
        /// <returns>An array of the two resullting polylines.</returns>
        public static Polyline[] BreakAtPoint(this Polyline pline, Point3d brkPt)
        {
            brkPt = pline.GetClosestPointTo(brkPt, false);

            // le point spécifié est sur le point de départ de la polyligne
            if (brkPt.IsEqualTo(pline.StartPoint))
                return new Polyline[2] { null, (Polyline)pline.Clone() };

            // le point spécifié est sur le point de fin de la polyligne
            if (brkPt.IsEqualTo(pline.EndPoint))
                return new Polyline[2] { (Polyline)pline.Clone(), null };

            double param = pline.GetParameterAtPoint(brkPt);
            int index = (int)param;
            int num = pline.NumberOfVertices;
            Polyline pl1 = (Polyline)pline.Clone();
            if (pline.Closed)
            {
                pl1.AddVertexAt(
                    pline.NumberOfVertices,
                    pline.GetPoint2dAt(0),
                    pline.GetStartWidthAt(num - 1),
                    pline.GetEndWidthAt(num - 1),
                    pline.GetBulgeAt(num - 1));
                pl1.Closed = false;
            }
            Polyline pl2 = (Polyline)pl1.Clone();

            // le point spécifié est sur un sommet de la polyligne
            if (Math.Round(param, 6) == index)
            {
                for (int i = pl1.NumberOfVertices - 1; i > index; i--)
                {
                    pl1.RemoveVertexAt(i);
                }
                for (int i = 0; i < index; i++)
                {
                    pl2.RemoveVertexAt(0);
                }
                return new Polyline[2] { pl1, pl2 };
            }

            // le point spécifié est sur un segment
            Point2d pt = brkPt.Convert2d(new Plane(Point3d.Origin, pline.Normal));
            for (int i = pl1.NumberOfVertices - 1; i > index + 1; i--)
            {
                pl1.RemoveVertexAt(i);
            }
            pl1.SetPointAt(index + 1, pt);
            for (int i = 0; i < index; i++)
            {
                pl2.RemoveVertexAt(0);
            }
            pl2.SetPointAt(0, pt);
            if (pline.GetBulgeAt(index) != 0.0)
            {
                double bulge = pline.GetBulgeAt(index);
                pl1.SetBulgeAt(index, MultiplyBulge(bulge, param - index));
                pl2.SetBulgeAt(0, MultiplyBulge(bulge, index + 1 - param));
            }
            return new Polyline[2] { pl1, pl2 };
        }

        /// <summary>
        /// Gets the centroid of the polyline.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The centroid of the polyline (OCS coordinates).</returns>
        public static Point2d Centroid2d(this Polyline pl)
        {
            Point2d cen = new Point2d();
            Triangle2d tri = new Triangle2d();
            CircularArc2d arc = new CircularArc2d();
            double tmpArea;
            double area = 0.0;
            int last = pl.NumberOfVertices - 1;
            Point2d p0 = pl.GetPoint2dAt(0);
            double bulge = pl.GetBulgeAt(0);

            if (bulge != 0.0)
            {
                arc = pl.GetArcSegment2dAt(0);
                area = arc.SignedArea();
                cen = arc.Centroid() * area;
            }
            for (int i = 1; i < last; i++)
            {
                tri.Set(p0, pl.GetPoint2dAt(i), pl.GetPoint2dAt(i + 1));
                tmpArea = tri.SignedArea;
                cen += (tri.Centroid * tmpArea).GetAsVector();
                area += tmpArea;
                bulge = pl.GetBulgeAt(i);
                if (bulge != 0.0)
                {
                    arc = pl.GetArcSegment2dAt(i);
                    tmpArea = arc.SignedArea();
                    area += tmpArea;
                    cen += (arc.Centroid() * tmpArea).GetAsVector();
                }
            }
            bulge = pl.GetBulgeAt(last);
            if ((bulge != 0.0) && (pl.Closed == true))
            {
                arc = pl.GetArcSegment2dAt(last);
                tmpArea = arc.SignedArea();
                area += tmpArea;
                cen += (arc.Centroid() * tmpArea).GetAsVector();
            }
            return cen.DivideBy(area);
        }

        /// <summary>
        /// Gets the centroid of the polyline.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The centroid of the polyline (WCS coordinates).</returns>
        public static Point3d Centroid(this Polyline pl)
        {
            return pl.Centroid2d().Convert3d(pl.Normal, pl.Elevation);
        }

        /// <summary>
        /// Adds an arc (fillet), if able, at each polyline vertex.
        /// </summary>
        /// <param name="pline">The instance to which the method applies.</param>
        /// <param name="radius">The arc radius.</param>
        public static void FilletAll(this Polyline pline, double radius)
        {
            int n = pline.Closed ? 0 : 1;
            for (int i = n; i < pline.NumberOfVertices - n; i += 1 + pline.FilletAt(i, radius))
            { }
        }

        /// <summary>
        /// Adds an arc (fillet) at the specified vertex.
        /// </summary>
        /// <param name="pline">The instance to which the method applies.</param>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="radius">The arc radius.</param>
        /// <returns>1 if the operation succeeded, 0 if it failed.</returns>
        public static int FilletAt(this Polyline pline, int index, double radius)
        {
            int prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
            {
                return 0;
            }
            LineSegment2d seg1 = pline.GetLineSegment2dAt(prev);
            LineSegment2d seg2 = pline.GetLineSegment2dAt(index);
            Vector2d vec1 = seg1.StartPoint - seg1.EndPoint;
            Vector2d vec2 = seg2.EndPoint - seg2.StartPoint;
            double angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            double dist = radius * Math.Tan(angle);
            if (dist == 0.0 || dist > seg1.Length || dist > seg2.Length)
            {
                return 0;
            }
            Point2d pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            Point2d pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            double bulge = Math.Tan(angle / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
            {
                bulge = -bulge;
            }
            pline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pline.SetPointAt(index + 1, pt2);
            return 1;
        }

        /// <summary>
        /// Evaluates if the points are clockwise.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        /// <returns>True if points are clockwise, False otherwise.</returns>
        private static bool Clockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)) < 1e-8;
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline pline, Plane plane, Vector3d direction)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (pline.Normal.IsPerpendicularTo(direction, tol))
            {
                Plane dirPlane = new Plane(Point3d.Origin, direction);
                if (!pline.IsWriteEnabled) pline.UpgradeOpen();
                pline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
                Extents3d extents = pline.GeometricExtents;
                pline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
                return GeomExt.ProjectExtents(extents, plane, direction, dirPlane);
            }

            return GeomExt.ProjectPolyline(pline, plane, direction);
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the curve along the given plane.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <returns>The projected polyline</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline pline, Plane plane)
        {
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }

        /// <summary>
        /// Applies a factor to a polyline bulge.
        /// </summary>
        /// <param name="bulge">The bulge this method applies to.</param>
        /// <param name="factor">the factor to apply to the bulge.</param>
        /// <returns>The new bulge.</returns>
        public static double MultiplyBulge(double bulge, double factor)
        {
            return Math.Tan(Math.Atan(bulge) * factor);
        }
    }
}
