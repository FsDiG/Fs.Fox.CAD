using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Features
{
    public class ThBeamFeature
    {
        public static Feature Construct(ThIfcBeam beam)
        {
            var poly = beam.Outline as Polyline;
            if (poly != null)
            {
                using (var ov = new ThCADCoreNTSFixedPrecision())
                {
                    var geometry = poly.ToNTSPolygon();
                    geometry.Normalize();
                    return new Feature()
                    {
                        Geometry = geometry,
                        BoundingBox = geometry.EnvelopeInternal,
                    };
                }
            }
            return null;
        }
    }
}
