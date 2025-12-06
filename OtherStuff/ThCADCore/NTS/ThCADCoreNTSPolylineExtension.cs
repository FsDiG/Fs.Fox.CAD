using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSPolylineExtension
    {
        public static Circle MinimumBoundingCircle(this Polyline polyline)
        {
            var mbc = new MinimumBoundingCircle(polyline.ToNTSLineString());
            return new Circle(mbc.GetCentre().ToAcGePoint3d(), Vector3d.ZAxis, mbc.GetRadius());
        }

        public static Polyline MinimumBoundingBox(this Polyline polyline)
        {
            var geometry = polyline.ToNTSLineString().Envelope;
            if (geometry is LineString lineString)
            {
                return lineString.ToDbPolyline();
            }
            else if (geometry is LinearRing linearRing)
            {
                return linearRing.ToDbPolyline();
            }
            else if (geometry is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline GetMinimumRectangle(this Polyline polyline)
        {
            var geom = polyline.ToNTSLineString();
            var rectangle = MinimumDiameter.GetMinimumRectangle(geom);
            if (rectangle is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline ConvexHull(this Polyline polyline)
        {
            var convexHull = new ConvexHull(polyline.ToNTSLineString());
            var geometry = convexHull.GetConvexHull();
            if (geometry is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline ConvexHull(this List<Point3d> srcPts)
        {
            var coordinates = new List<Coordinate>();
            srcPts.ForEach(e => coordinates.Add(e.ToNTSCoordinate()));

            var convexHull = new ConvexHull(coordinates.ToArray(), ThCADCoreNTSService.Instance.GeometryFactory);
            var geometry = convexHull.GetConvexHull();
            if (geometry is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline GetOctagonalEnvelope(this Polyline polyline)
        {
            var geometry = OctagonalEnvelope.GetOctagonalEnvelope(polyline.ToNTSLineString());
            if (geometry is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static bool IsClosed(this Polyline polyline)
        {
            var geometry = polyline.ToNTSLineString() as LineString;
            return geometry.IsClosed;
        }
    }
}
