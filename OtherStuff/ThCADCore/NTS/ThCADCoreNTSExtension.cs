using NetTopologySuite.Geometries;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSExtension
    {
        public static Polygon ToPolygon(this LinearRing linearRing)
        {
            return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon(linearRing);
        }

        public static MultiLineString ToNTSMultiLineString(this Polygon polygon)
        {
            if (polygon.NumInteriorRings == 0)
            {
                var lineStrings = new LineString[] { polygon.ExteriorRing };
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiLineString(lineStrings);
            }
            else
            {
                return polygon.Boundary as MultiLineString;
            }
        }
    }
}
