using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThBeamLayerManager
    {
        /// <summary>
        /// 获取ModelSpace下的Beam图层
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<string> GeometryLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers
                    .Where(o => IsVisibleLayer(o))
                    .Where(o =>
                    {
                        // 图层名未包含S_BEAM
                        if (!o.Name.ToUpper().Contains("S_BEAM"))
                        {
                            return false;
                        }

                        // 若图层名包含S_BEAM，
                        // 则继续判断是否包含TEXT
                        if (o.Name.ToUpper().Contains("TEXT"))
                        {
                            return false;
                        }

                        // 继续判断是否包含REIN
                        if (o.Name.ToUpper().Contains("REIN"))
                        {
                            return false;
                        }

                        // 返回指定的图层
                        return true;
                    }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }
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
                        if (!layerName.Contains("S_BEAM"))
                        {
                            return false;
                        }
                        // 若图层名包含S_BEAM，
                        // 则继续判断是否包含TEXT
                        if (layerName.Contains("TEXT"))
                        {
                            return false;
                        }

                        // 继续判断是否包含REIN
                        if (layerName.Contains("REIN"))
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
        public static List<string> AnnotationLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers.Where(o =>
                {
                    if (o.Name.ToUpper().Contains("S_BEAM_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.ToUpper().Contains("S_BEAM_SECD_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.ToUpper().Contains("S_BEAM_XL_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.ToUpper().Contains("S_BEAM_WALL_TEXT"))
                    {
                        return true;
                    }
                    return false;
                }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }
        public static List<string> AnnotationXrefLayers(Database database)
        {
            List<string> layerSuffix = new List<string> { "S_BEAM_TEXT_HORZ", "S_BEAM_TEXT_VERT", "S_BEAM_WALL_TEXT",
                "S_BEAM_SECD_TEXT_HORZ", "S_BEAM_SECD_TEXT_VERT"," S_BEAM_XL_TEXT_HORZ","S_BEAM_XL_TEXT_VERT"};
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers.Where(m =>
                {
                    var layerName = ThStructureUtils.OriginalFromXref(m.Name).ToUpper();
                    return layerSuffix.Where(o =>
                    {
                        int index = layerName.LastIndexOf(o);
                        if (index != -1)
                        {
                            if (index + o.Length == layerName.Length)
                            {
                                return true;
                            }
                        }
                        return false;
                    }).Any();
                }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }
    }
}
