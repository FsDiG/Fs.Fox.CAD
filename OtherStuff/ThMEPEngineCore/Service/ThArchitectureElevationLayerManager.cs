using System.Linq;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public  class ThArchitectureElevationLayerManager
    {
        public static List<string> TextXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o => IsSpaceNameLayer(o.Name))
                    .Select(o => o.Name)
                    .ToList();
            }
        }
        private static bool IsSpaceNameLayer(string name)
        {
            var layerName = ThStructureUtils.OriginalFromXref(name).ToUpper();
            // 图层名未包含S_BEAM         
            string[] patterns = layerName.Split('-').ToArray();
            if (patterns.Count() < 3)
            {
                return false;
            }          
            return (patterns[0] == "AD" && patterns[1] == "LEVL" && patterns[2] == "HIGH")||(patterns[0] == "AD" && patterns[1] == "FLOOR" && patterns[2] == "AREA") ||
               (patterns[0] == "AD" && patterns[1] == "NAME" && patterns[2] == "ROOM"); ;
        }
        private static bool IsVisibleLayer(LayerTableRecord layerTableRecord)
        {
            return !(layerTableRecord.IsOff || layerTableRecord.IsFrozen);
        }
    }
}
