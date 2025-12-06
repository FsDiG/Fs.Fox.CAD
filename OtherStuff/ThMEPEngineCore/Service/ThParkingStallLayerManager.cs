using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThParkingStallLayerManager
    {
        public static List<string> XrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o => IsParkingStallLayer(o.Name))
                    .ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }

        private static bool IsVisibleLayer(LayerTableRecord layerTableRecord)
        {
            return !(layerTableRecord.IsOff || layerTableRecord.IsFrozen);
        }

        private static bool IsParkingStallLayer(string name)
        {
            string rawName = ThStructureUtils.OriginalFromXref(name);
            string[] patterns = rawName.ToUpper().Split('-').Reverse().ToArray();
            if (patterns.Count() == 3)
            {
                return (patterns[0] == "CARS") && (patterns[1] == "EQPM") && (patterns[2] == "AE");
            }
            return false;
        }
    }
}
