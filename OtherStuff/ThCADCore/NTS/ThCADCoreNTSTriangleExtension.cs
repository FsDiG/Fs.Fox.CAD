using NetTopologySuite.Geometries;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSTriangleExtension
    {
        public static Coordinate[] Vertices(this Triangle triangle)
        {
            return new Coordinate[]
            {
                triangle.P0,
                triangle.P1,
                triangle.P2,
            };
        }
    }
}
