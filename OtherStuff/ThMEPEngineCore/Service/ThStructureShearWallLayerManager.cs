using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThStructureShearWallLayerManager
    {
        public static List<string> CurveXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o => IsShearWallCurveLayer(o.Name))
                    .Select(o => o.Name)
                    .ToList();
            }
        }

        public static List<string> HatchXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o => IsShearWallHatchLayer(o.Name))
                    .Select(o => o.Name)
                    .ToList();
            }
        }

        private static bool IsVisibleLayer(LayerTableRecord layerTableRecord)
        {
            return !(layerTableRecord.IsOff || layerTableRecord.IsFrozen);
        }

        private static bool IsShearWallCurveLayer(string name)
        {
            var layerName = ThStructureUtils.OriginalFromXref(name).ToUpper();
            if (!layerName.Contains("S_WALL"))
            {
                return false;
            }
            // 若图层名包含S_WALL，
            // 则继续判断是否包含TEXT
            if (layerName.Contains("HATCH"))
            {
                return false;
            }
            if (layerName.Contains("OTHE"))
            {
                return false;
            }
            if (layerName.Contains("CAP"))
            {
                return false;
            }
            if (layerName.Contains("TEXT"))
            {
                return false;
            }
            if (layerName.Contains("DETL"))
            {
                return false;
            }
            if (layerName.Contains("TPTN"))
            {
                return false;
            }
            // 返回指定的图层
            return true;
        }

        private static bool IsShearWallHatchLayer(string name)
        {
            string[] patterns = ThStructureUtils.OriginalFromXref(name).ToUpper().Split('_').Reverse().ToArray();
            if (patterns.Count() < 3)
            {
                return false;
            }
            return (patterns[0] == "HACH") && (patterns[1] == "WALL") && (patterns[2] == "S");
        }
    }
}
