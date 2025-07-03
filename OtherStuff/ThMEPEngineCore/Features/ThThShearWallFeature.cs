using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Features
{
    public class ThThShearWallFeature
    {
        public static Feature Construct(ThIfcWall wall)
        {
            if (wall.Outline is Polyline poly)
            {
                var geometry = poly.ToNTSPolygon();
                return new Feature()
                {
                    Geometry = geometry,
                    BoundingBox = geometry.EnvelopeInternal,
                };
            }
            else if(wall.Outline is MPolygon mPolygon)
            {
                var geometry = mPolygon.ToNTSGeometry();
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
