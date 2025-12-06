using System;
using System.Linq;
using System.Text;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThSpaceBoundarLayerManager
    {
        public static List<string> CurveXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.Layers
                    .Where(o => IsSpaceBoundaryLayer(o.Name))
                    .Select(o => o.Name)
                    .ToList();
            }
        }
        public static bool IsSpaceBoundaryLayer(string name)
        {
            var layerName = ThStructureUtils.OriginalFromXref(name).ToUpper();
            if (!layerName.Contains("AD-AREA-OUTL") && !layerName.Contains("AD-FLOOR-AREA"))
            {
                return false;
            }
            string[] patterns = layerName.Split('-').Reverse().ToArray();
            if (patterns.Count() < 3)
            {
                return false;
            }
            return (patterns[0] == "OUTL"  &&  patterns[1] == "AREA"  &&  patterns[2] == "AD") ||
                (patterns[0] == "AREA" &&  patterns[1] == "FLOOR" && patterns[2] == "AD") ;
        }
    }
}
