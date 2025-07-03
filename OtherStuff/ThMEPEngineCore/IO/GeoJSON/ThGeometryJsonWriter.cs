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
    public class ThGeometryJsonWriter : GeoJsonWriter
    {
        public void Write(List<ThGeometry> geos, JsonWriter writer)
        {
            ThGeometryFeatureCollection.Construct(geos)
                .OrderBy(o => o.Geometry, new ThCADCoreNTSGeometryComparer()).ForEach(o => Write(o, writer));
        }
    }
}
