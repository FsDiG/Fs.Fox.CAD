using NFox.Cad;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm.Locate;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSPolygonExtension
    {
        public static DBObjectCollection Difference(this AcPolygon polygon, DBObjectCollection curves)
        {
            return polygon.ToNTSPolygon().Difference(curves.UnionGeometries()).ToDbCollection();
        }

        public static DBObjectCollection DifferenceMP(this AcPolygon polygon, DBObjectCollection curves)
        {
            return polygon.ToNTSPolygon().Difference(curves.UnionGeometries()).ToDbCollection(true);
        }

        public static DBObjectCollection Intersection(this AcPolygon polygon, DBObjectCollection curves)
        {
            return polygon.ToNTSPolygon().Intersection(curves.UnionGeometries()).ToDbCollection();
        }

        public static bool Contains(this AcPolygon polygon, Point3d pt)
        {
            var locator = new SimplePointInAreaLocator(polygon.ToNTSPolygon());
            return locator.Locate(pt.ToNTSCoordinate()) == Location.Interior;
        }

        public static bool ContainsOrOnBoundary(this AcPolygon polygon, Point3d pt)
        {
            var locator = new SimplePointInAreaLocator(polygon.ToNTSPolygon());
            var locateRes = locator.Locate(pt.ToNTSCoordinate());
            return locateRes == Location.Interior || locateRes == Location.Boundary;
        }

        public static bool IndexedContains(this AcPolygon polygon, Point3d pt)
        {
            var locator = new IndexedPointInAreaLocator(polygon.ToNTSPolygon());
            return locator.Locate(pt.ToNTSCoordinate()) == Location.Interior;
        }

        public static bool Contains(this AcPolygon polygon, Curve curve)
        {
            return polygon.ToNTSPolygon().Contains(curve.ToNTSGeometry());
        }
        public static bool Intersects(this AcPolygon polygon, Entity entity)
        {
            return polygon.ToNTSPolygon().Intersects(entity.ToNTSGeometry());
        }
        public static bool IsRectangle(this AcPolygon polygon)
        {
            return polygon.ToNTSPolygon().IsRectangle;
        }

        public static Point3d GetCentroidPoint(this AcPolygon polygon)
        {
            return Centroid.GetCentroid(polygon.ToNTSPolygon()).ToAcGePoint3d();
        }

        public static DBObjectCollection MakeValid(this AcPolygon polygon)
        {
            // zero-width buffer trick:
            //  http://lin-ear-th-inking.blogspot.com/2020/12/fixing-buffer-for-fixing-polygons.html
            // self-union trick:
            //  http://lin-ear-th-inking.blogspot.com/2020/06/jts-overlayng-tolerant-topology.html
            return polygon.ToNTSPolygon().Buffer(0).ToDbCollection();
        }

        public static DBObjectCollection GeometryIntersection(this AcPolygon polyFirst, AcPolygon polySec)
        {
            return polyFirst.ToNTSPolygon().Intersection(polySec.ToNTSPolygon())
                .ToDbCollection()
                .Cast<Entity>()
                .Where(o => o is Polyline)
                .ToCollection();
        }
    }
}
