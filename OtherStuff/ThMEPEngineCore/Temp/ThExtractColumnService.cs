using NFox.Cad;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThExtractColumnService:ThExtractService
    {
        public List<Polyline> Columns { get; set; }
        public ThExtractColumnService()
        {
            Columns = new List<Polyline>();
        }

        public override void Extract(Database db,Point3dCollection pts)
        {
            using (var acadDatabase = AcadDatabase.Use(db))
            {
                Columns=acadDatabase.ModelSpace
                    .OfType<Polyline>()
                    .Where(o => IsColumnLayer(o.Layer))
                    .Select(o=>o.Clone() as Polyline)
                    .ToList();
                if(pts.Count>=3)
                {
                    var spatialIndex = new ThCADCoreNTSSpatialIndex(Columns.ToCollection());
                    var objs = spatialIndex.SelectCrossingPolygon(pts);
                    Columns = objs.Cast<Polyline>().ToList();
                }
            }
        }        

        private bool IsColumnLayer(string layerName)
        {
            return layerName == "柱";
        }
    }
}
