using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using Autodesk.AutoCAD.Geometry;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSGeExtension
    {
        public static Point3d ToAcGePoint3d(this Coordinate coordinate)
        {
            if (!double.IsNaN(coordinate.Z))
            {
                return new Point3d(
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y),
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Z)
                    );
            }
            else
            {
                return new Point3d(
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y),
                    0);
            }
        }

        public static Point3d ToAcGePoint3d(this Point point)
        {
            return point.Coordinate.ToAcGePoint3d();
        }

        public static Point2d ToAcGePoint2d(this Coordinate coordinate)
        {
            return new Point2d(
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y)
                );
        }

        public static Point2d ToAcGePoint2d(this Point point)
        {
            return point.Coordinate.ToAcGePoint2d();
        }

        public static Coordinate ToNTSCoordinate(this Point3d point)
        {
            return new Coordinate(
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                    ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.Y)
                    );

        }

        public static Coordinate ToNTSCoordinate(this Point2d point)
        {
            return new Coordinate(
                ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.Y)
                );
        }

        public static Coordinate[] ToNTSCoordinates(this Point3dCollection points)
        {
            var coordinates = new List<Coordinate>();
            foreach(Point3d pt in points)
            {
                coordinates.Add(pt.ToNTSCoordinate());
            }
            return coordinates.ToArray();
        }

        public static Point3dCollection ToAcGePoint3ds(this Coordinate[] coordinates)
        {
            var points = new Point3dCollection();
            foreach(var coordinate in coordinates)
            {
                points.Add(coordinate.ToAcGePoint3d());
            }
            return points;
        }

        public static bool IsReflex(Point3d p1, Point3d p2, Point3d p3)
        {
            return (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y) < 0;
        }

        public static bool IsConvex(Point3d p1, Point3d p2, Point3d p3)
        {
            return (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y) > 0;
        }

        public static bool IsCollinear(Point3d p1, Point3d p2, Point3d p3)
        {
            var coordinate1 = p1.ToNTSCoordinate();
            var coordinate2 = p2.ToNTSCoordinate();
            var coordinate3 = p3.ToNTSCoordinate();
            return Orientation.Index(coordinate1, coordinate2, coordinate3) == OrientationIndex.Collinear;
        }
    }
}
