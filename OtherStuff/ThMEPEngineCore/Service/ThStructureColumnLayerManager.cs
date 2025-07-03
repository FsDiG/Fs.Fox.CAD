using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThStructureColumnLayerManager
    {
        public static List<string> CurveXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o => IsColumnCurveLayer(o.Name))
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
                    .Where(o => IsColumnHatchLayer(o.Name))
                    .Select(o => o.Name)
                    .ToList();
            }
        }

        private static bool IsVisibleLayer(LayerTableRecord layerTableRecord)
        {
            return !(layerTableRecord.IsOff || layerTableRecord.IsFrozen);
        }

        private static bool IsColumnCurveLayer(string name)
        {
            var layerName = ThStructureUtils.OriginalFromXref(name).ToUpper();

            // 图层名未包含S_BEAM
            if (!layerName.Contains("S_COLU"))
            {
                return false;
            }

            // 若图层名包含S_BEAM，
            // 则继续判断是否包含HACH
            if (layerName.Contains("HACH"))
            {
                return false;
            }

            // 继续判断是否包含CAP
            if (layerName.Contains("CAP"))
            {
                return false;
            }

            // 继续判断是否包含TEXT
            if (layerName.Contains("TEXT"))
            {
                return false;
            }

            // 继续判断是否包含DIMS
            if (layerName.Contains("DIMS"))
            {
                return false;
            }

            // 继续判断是否包含DIMS
            if (layerName.Contains("OTHE"))
            {
                return false;
            }

            // 返回指定的图层
            return true;
        }

        private static bool IsColumnHatchLayer(string name)
        {
            string[] patterns = ThStructureUtils.OriginalFromXref(name).ToUpper().Split('_').Reverse().ToArray();
            if (patterns.Count() < 3)
            {
                return false;
            }
            return (patterns[0] == "HACH") && (patterns[1] == "COLU") && (patterns[2] == "S");
        }
    }
}
