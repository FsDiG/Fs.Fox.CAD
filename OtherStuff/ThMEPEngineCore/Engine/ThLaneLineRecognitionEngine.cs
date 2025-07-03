using DotNetARX;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThLaneLineRecognitionEngine : ThSpatialElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            using (var acadDatabase = AcadDatabase.Use(database))
            using (var lanelineDbExtension = new ThLaneLineDbExtension(database))
            {
                lanelineDbExtension.BuildElementCurves();
                lanelineDbExtension.LaneCurves.ForEach(o =>
                {
                    // 暂时用车道中心线来“表示”车道
                    Spaces.Add(ThIfcLane.Create(o));
                });
                if (polygon.Count > 0)
                {
                    DBObjectCollection dbObjs = new DBObjectCollection();
                    Spaces.ForEach(o => dbObjs.Add(o.Boundary));
                    ThCADCoreNTSSpatialIndex laneSpatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                    var pline = new Polyline()
                    {
                        Closed = true,
                    };
                    pline.CreatePolyline(polygon);
                    var filterObjs = laneSpatialIndex.SelectCrossingPolygon(pline);
                    Spaces = Spaces.Where(o => filterObjs.Contains(o.Boundary)).ToList();
                }
            }
        }

        public override void Recognize(List<ThRawIfcSpatialElementData> datas, Point3dCollection polygon)
        {
            throw new System.NotImplementedException();
        }

        public override void RecognizeMS(Database database, Point3dCollection polygon)
        {
            throw new System.NotImplementedException();
        }
    }
}
