using System;
using Autodesk.AutoCAD.Geometry;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSLineExtension
    {
        public static bool IsCollinear(this Line line, Line other)
        {
            var p1 = line.StartPoint.ToNTSCoordinate();
            var p2 = line.EndPoint.ToNTSCoordinate();
            var q1 = other.StartPoint.ToNTSCoordinate();
            var q2 = other.EndPoint.ToNTSCoordinate();
            return Orientation.Index(p1, q1, q2) == OrientationIndex.Collinear
                && Orientation.Index(p2, q1, q2) == OrientationIndex.Collinear;
        }

        public static bool Overlaps(this Line line, Line other)
        {
            return line.ToNTSLineString().Overlaps(other.ToNTSLineString());
        }

        public static Point3d Intersection(this Line line, Polyline other)
        {
            var geometry = line.ToNTSLineString().Intersection(other.ToNTSLineString());
            if (geometry is Point point)
            {
                return point.ToAcGePoint3d();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static bool IsOnLine(this Line line, Point3d pt)
        {
            return PointLocation.IsOnLine(pt.ToNTSCoordinate(), line.ToNTSGeometry().Coordinates);
        }

        public static Line Normalize(this Line line)
        {
            var geometry = line.ToNTSLineString();
            geometry.Normalize();
            return geometry.ToDbline();
        }

        public static Coordinate Intersection(this Line line1, Line line2, Intersect intersectType)
        {
            if(line1.IsCollinear(line2))
            {
                throw new NotSupportedException();
            }
            var linesegment1 = new LineSegment(line1.StartPoint.ToNTSCoordinate(), line1.EndPoint.ToNTSCoordinate());
            var linesegment2 = new LineSegment(line2.StartPoint.ToNTSCoordinate(), line2.EndPoint.ToNTSCoordinate());
            switch (intersectType)
            {
                case Intersect.ExtendBoth:
                    var intersectPt = linesegment1.LineIntersection(linesegment2);
                    if(intersectPt == null)
                    {
                        throw new NotSupportedException();
                    }
                    return intersectPt;
                case Intersect.OnBothOperands:
                    var geometry = line1.ToNTSLineString().Intersection(line2.ToNTSLineString());
                    if (geometry is Point point)
                    {
                        return point.Coordinate;
                    }
                    break;
            }
            return null;
        }
    }
}
