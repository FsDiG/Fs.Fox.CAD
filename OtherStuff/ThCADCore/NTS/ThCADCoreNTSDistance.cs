using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Operation.Distance;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSDistance
    {
        public static double Distance(this Polyline polyline, Point3d point)
        {
            var distance = new PointPairDistance();
            DistanceToPoint.ComputeDistance(polyline.ToNTSLineString(),
                point.ToNTSCoordinate(), distance);
            return distance.Distance;
        }

        public static double Distance(this Curve line, Curve curve)
        {
            return line.ToNTSGeometry().Distance(curve.ToNTSGeometry());
        }

        public static double IndexedDistance(this Curve line, Curve curve)
        {
            return IndexedFacetDistance.Distance(line.ToNTSGeometry(), curve.ToNTSGeometry());
        }
    }
}
