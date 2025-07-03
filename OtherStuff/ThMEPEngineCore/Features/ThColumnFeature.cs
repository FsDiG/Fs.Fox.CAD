using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Features
{
    public class ThColumnFeature
    {
        public static Feature Construct(ThIfcColumn beam)
        {
            var poly = beam.Outline as Polyline;
            if (poly != null)
            {
                var geometry = poly.ToNTSPolygon();
                return new Feature()
                {
                    Geometry = geometry,
                    BoundingBox = geometry.EnvelopeInternal,
                };
            }
            return null;
        }
    }
}
