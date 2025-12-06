using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Service
{
    public class ThLaneLineLayerManager
    {
        /// <summary>
        /// 获取Xref下的Beam图层
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<string> GeometryXrefLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o =>
                    {
                        var layerName = ThStructureUtils.OriginalFromXref(o.Name).ToUpper();
                        if (!layerName.Contains("AD-SIGN"))
                        {
                            return false;
                        }
                        // 若图层名包含AD-SIGN，
                        // 则继续判断是否包含HATCH
                        if (layerName.Contains("HATCH"))
                        {
                            return false;
                        }
                        // 返回指定的图层
                        return true;
                    }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }
        private static bool IsVisibleLayer(LayerTableRecord layerTableRecord)
        {
            return !(layerTableRecord.IsOff || layerTableRecord.IsFrozen);
        }
    }
}
