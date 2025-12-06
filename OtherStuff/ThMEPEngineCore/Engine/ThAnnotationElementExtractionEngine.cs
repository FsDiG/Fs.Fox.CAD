using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRawIfcAnnotationElementData
    {
        public object Data { get; set; }
        public Entity Geometry { get; set; }
    }

    public abstract class ThAnnotationElementExtractionEngine
    {
        public List<ThRawIfcAnnotationElementData> Results { get; protected set; }

        public abstract void Extract(Database database);

        public abstract void ExtractFromMS(Database database);
    }
}
