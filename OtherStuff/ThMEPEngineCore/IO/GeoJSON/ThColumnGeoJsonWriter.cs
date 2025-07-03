using System;
using Newtonsoft.Json;
using NetTopologySuite.IO;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Features;
using System.Collections.Generic;

namespace ThMEPEngineCore.IO.GeoJSON
{
    public class ThColumnGeoJsonWriter : GeoJsonWriter
    {
        public void Write(List<ThIfcColumn> columns, JsonWriter writer)
        {
            Write(ThColumnFeatureCollection.Construct(columns), writer);
        }
    }
}
