using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ThMEPEngineCore.Features
{
    public class ThBeamFeatureCollection
    {
        public static FeatureCollection Construct(List<ThIfcBeam> beams)
        {
            var features = new FeatureCollection();
            beams.ForEach(o => features.Add(ThBeamFeature.Construct(o)));
            return features;
        }
    }
}
