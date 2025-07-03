using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.IO.GeoJSON;

namespace ThMEPEngineCore.IO
{
    public class ThGeoOutput
    {
        public static void Output(List<ThGeometry> geos,string path,string fileName)
        {
            using (StreamWriter geoJson = File.CreateText(Path.Combine(path, string.Format("{0}.Info.geojson", fileName))))
            using (JsonTextWriter writer = new JsonTextWriter(geoJson)
            {
                Indentation = 4,
                IndentChar = ' ',
                Formatting = Formatting.Indented,
            })
            {
                var geoJsonWriter = new ThGeometryJsonWriter();
                geoJsonWriter.Write(geos, writer);
            }
        }
    }
}
