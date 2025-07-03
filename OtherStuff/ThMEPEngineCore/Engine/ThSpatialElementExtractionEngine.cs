using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRawIfcSpatialElementData
    {
        public object Data { get; set; }
        public Entity Geometry { get; set; }
    }

    public abstract class ThSpatialElementExtractionEngine
    {
        public List<ThRawIfcSpatialElementData> Results { get; protected set; }

        public ThSpatialElementExtractionEngine()
        {
            Results = new List<ThRawIfcSpatialElementData>();
        }

        public abstract void Extract(Database database);

        public abstract void ExtractFromMS(Database database);
    }
}
