using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ThMEPEngineCore.Features
{
    public class ThShearWallFeatureCollection
    {
        public static FeatureCollection Construct(List<ThIfcWall> walls)
        {
            var features = new FeatureCollection();
            walls.ForEach(o => features.Add(ThThShearWallFeature.Construct(o)));
            return features;
        }
    }
}
