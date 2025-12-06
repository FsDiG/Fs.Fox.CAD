using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRawIfcDistributionElementData
    {
        public object Data { get; set; }
        public Entity Geometry { get; set; }
    }

    public abstract class ThDistributionElementExtractionEngine
    {
        public List<ThRawIfcDistributionElementData> Results { get; protected set; }

        public abstract void Extract(Database database);
    }
}
