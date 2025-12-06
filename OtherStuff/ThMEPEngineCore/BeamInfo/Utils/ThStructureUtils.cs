using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThMEPEngineCore.BeamInfo.Utils
{
    public static class ThStructureUtils
    {
        public static ObjectIdCollection AddToDatabase(List<Entity> ents)
        {
            using (var db = AcadDatabase.Active())
            {
                var objs = new ObjectIdCollection();
                ents.ForEach(o => objs.Add(db.ModelSpace.Add(o)));           
                return objs;
            }
        }
        public static List<Entity> Explode(Database database, bool keepUnvisible = false)
        {
            using (var db = AcadDatabase.Use(database))
            {
                // 炸对象只需要以“只读”的方式打开对象
                // 所以这里无需考虑图层对“炸”对象的影响
                var blockRefs = db.ModelSpace
                    .OfType<BlockReference>()
                    .Where(p => p.Visible)
                    .Where(p => p.IsBlockReferenceExplodable());
                var resEntityLst = new List<Entity>();
                blockRefs.ForEach(i => resEntityLst.AddRange(Explode(db, i, keepUnvisible)));
                return resEntityLst;
            }
        }
        /// <summary>
        /// 炸块
        /// </summary>
        /// <param name="br">块</param>
        /// <param name="keepUnvisibleEnts">保留隐藏的物体</param>
        /// <returns></returns>
        public static List<Entity> Explode(AcadDatabase db, BlockReference br, bool keepUnVisible = true)
        {
            // 炸对象只需要以“只读”的方式打开对象
            // 所以这里无需考虑图层对“炸”对象的影响
            List<Entity> entities = new List<Entity>();
            if (!br.Visible || !br.IsBlockReferenceExplodable())
            {
                return entities;
            }

            try
            {
                DBObjectCollection collection = new DBObjectCollection();
                br.Explode(collection);
                foreach (Entity ent in collection)
                {
                    if (!keepUnVisible && ent.Visible == false)
                    {
                        continue;
                    }
                    if (ent is BlockReference newBr)
                    {
                        entities.AddRange(Explode(db, newBr, keepUnVisible));
                    }
                    else if (ent is Mline mline)
                    {
                        var lines = new DBObjectCollection();
                        mline.Explode(lines);
                        entities.AddRange(lines.Cast<Entity>());
                    }
                    else if (ent is DBPoint)
                    {
                        continue;
                    }
                    else
                    {
                        entities.Add(ent);
                    }
                }
            }
            catch
            {
                // 
            }

            return entities;
        }

        /// <summary>
        /// 过滤指定图层上的曲线
        /// </summary>
        /// <param name="ents"></param>
        /// <param name="layerNames"></param>
        /// <param name="fullMatch"></param>
        /// <returns></returns>
        public static List<Entity> FilterCurveByLayers(List<Entity> ents, List<string> layerNames, bool fullMatch = false)
        {
            List<Entity> filterEnts = new List<Entity>();
            layerNames = layerNames.Select(i => i.ToUpper()).ToList();
            if (fullMatch)
            {
                filterEnts = ents.Where(i => layerNames.IndexOf(i.Layer.ToUpper()) >= 0 && i is Curve).Select(i => i).ToList();
            }
            else
            {
                filterEnts = ents.Where(i =>
                {
                    bool containsLayer = false;
                    containsLayer = ((Func<Entity, bool>)((ent) =>
                    {
                        bool contains = false;
                        foreach (string layerName in layerNames)
                        {
                            int index = ent.Layer.ToUpper().LastIndexOf(layerName);
                            if (index >= 0 && (index + layerName.Length) == i.Layer.Length)
                            {
                                contains = true;
                                break;
                            }
                        }
                        return contains;
                    }))(i);
                    if (containsLayer && i is Curve)
                    {
                        return true;
                    }
                    return false;
                }).Select(i => i).ToList();
            }
            return filterEnts;
        }

        /// <summary>
        /// 过滤指定图层上的文字
        /// </summary>
        /// <param name="ents"></param>
        /// <param name="layerNames"></param>
        /// <param name="fullMatch"></param>
        /// <returns></returns>
        public static List<Entity> FilterAnnotationByLayers(List<Entity> ents, List<string> layerNames, bool fullMatch = false)
        {
            List<Entity> filterEnts = new List<Entity>();
            layerNames = layerNames.Select(i => i.ToUpper()).ToList();
            if (fullMatch)
            {
                filterEnts = ents.Where(i => layerNames.IndexOf(i.Layer.ToUpper()) >= 0 &&
                (i is DBText || i is MText || i is Dimension)).Select(i => i).ToList();
            }
            else
            {
                filterEnts = ents.Where(i =>
                {
                    bool containsLayer = ((Func<Entity, bool>)((ent) =>
                    {
                        bool contains = false;
                        foreach (string layerName in layerNames)
                        {
                            int index = ent.Layer.ToUpper().LastIndexOf(layerName);
                            if (index >= 0 && (index + layerName.Length) == i.Layer.Length)
                            {
                                contains = true;
                                break;
                            }
                        }
                        return contains;
                    }))(i);
                    if (containsLayer && (i is DBText || i is MText || i is Dimension))
                    {
                        return true;
                    }
                    return false;
                }).Select(i => i).ToList();
            }
            return filterEnts;
        }
    }
}
