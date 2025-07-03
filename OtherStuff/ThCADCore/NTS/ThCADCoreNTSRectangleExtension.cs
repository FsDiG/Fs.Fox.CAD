using NetTopologySuite.Utilities;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSRectangleExtension
    {
        public static Envelope ToEnvelope(this Extents3d extents)
        {
            return new Envelope(extents.MinPoint.ToNTSCoordinate(),
                extents.MaxPoint.ToNTSCoordinate());
        }

        public static Polygon ToNTSPolygon(this Extents3d extents)
        {
            var shapeFactory = new GeometricShapeFactory(ThCADCoreNTSService.Instance.GeometryFactory)
            {
                Envelope = extents.ToEnvelope(),
            };
            return shapeFactory.CreateRectangle();
        }
    }
}
