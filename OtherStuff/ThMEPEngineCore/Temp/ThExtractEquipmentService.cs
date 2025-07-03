using System.Linq;
using System.Collections.Generic;
using NFox.Cad;
using Linq2Acad;
using ThCADCore.NTS;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThExtractEquipmentService : ThExtractService
    {
        public Dictionary<string, List<Polyline>> Equipments { get; set; }
        public ThExtractEquipmentService()
        {
            Equipments = new Dictionary<string, List<Polyline>>();
        }
        public override void Extract(Database db,Point3dCollection pts)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                var blocks = new List<BlockReference>();
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (ent is BlockReference br && br.Bounds != null)
                    {
                        var name = br.GetEffectiveName();
                        if (IsFireHydrantBlkName(name))
                        {
                            blocks.Add(br);
                        }
                    }
                }

                if(pts.Count>=3)
                {
                    var spatialIndex = new ThCADCoreNTSSpatialIndex(blocks.ToCollection());
                    var objs = spatialIndex.SelectCrossingPolygon(pts);
                    blocks = objs.Cast<BlockReference>().ToList();
                }

                foreach (var br in blocks)
                {
                    var name = br.GetEffectiveName();
                    var obb = GetBlockOBB(db, br, br.BlockTransform);
                    if (Equipments.ContainsKey(name))
                    {
                        Equipments[name].Add(obb);
                    }
                    else
                    {
                        Equipments.Add(name, new List<Polyline> { obb });
                    }
                }
            }
        }

        private bool IsFireHydrantBlkName(string blkName)
        {
            string queryChars = "-新";
            int index = blkName.LastIndexOf(queryChars);
            return index >= 0 ? index + queryChars.Length == blkName.Length : false;
        }

        private Polyline GetBlockOBB(Database database, BlockReference blockObj, Matrix3d matrix)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var btr = acadDatabase.Blocks.Element(blockObj.BlockTableRecord);
                var polyline = btr.GeometricExtents().ToRectangle().GetTransformedCopy(matrix) as Polyline;
                return polyline;
            }
        }
    }
}
