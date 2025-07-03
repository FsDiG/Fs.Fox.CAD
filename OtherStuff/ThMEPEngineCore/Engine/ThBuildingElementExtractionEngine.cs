using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRawIfcBuildingElementData
    {
        public object Data { get; set; }
        public Entity Geometry { get; set; }
    }

    public abstract class ThBuildingElementExtractionEngine
    {
        public List<ThRawIfcBuildingElementData> Results { get; protected set; }

        public abstract void Extract(Database database);
    }
}
