using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcSpatialElement : ThIfcProduct
    {
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public List<string> Tags { get; set; } = new List<string>();
        public Curve Boundary { get; set; }
    }
}
