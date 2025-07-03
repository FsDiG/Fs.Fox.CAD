using System.Linq;
using ThCADCore.NTS;
using Newtonsoft.Json;
using Dreambuild.AutoCAD;
using NetTopologySuite.IO;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Features;
using System.Collections.Generic;

namespace ThMEPEngineCore.IO.GeoJSON
{
    public class ThBeamGeoJsonWriter : GeoJsonWriter
    {
        public void Write(List<ThIfcBeam> beams, JsonWriter writer)
        {
            ThBeamFeatureCollection.Construct(beams)
                .OrderBy(o => o.Geometry, new ThCADCoreNTSGeometryComparer()).ForEach(o => Write(o, writer));
        }
    }
}
