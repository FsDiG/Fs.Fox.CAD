using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ThMEPEngineCore.Features
{
    public class ThGeometryFeatureCollection
    {
        public static FeatureCollection Construct(List<ThGeometry> geos)
        {
            var features = new FeatureCollection();
            geos.ForEach(o => features.Add(ThGeometryFeature.Construct(o)));
            return features;
        }
    }
}
