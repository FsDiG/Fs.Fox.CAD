using System;
using System.Linq;
using System.Collections.Generic;
using NFox.Cad;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Engine;
using ThMEPEngineCore.LaneLine;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThLaneLineExtractor : ThExtractorBase,IExtract,IPrint, IBuildGeometry
    {
        public List<Line> LaneLines { get; private set; }
        public ThLaneLineExtractor()
        {
            LaneLines = new List<Line>();
            Category = "LaneLine";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            using (var engine = new ThLaneLineRecognitionEngine())
            {
                engine.Recognize(database, pts);

                // 车道中心线处理
                var curves = engine.Spaces.Select(o => o.Boundary).ToList();
                var lines = ThLaneLineSimplifier.Simplify(curves.ToCollection(), 1500);
                LaneLines = lines.Where(o => o.Length >= 3000).ToList();
            }
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            LaneLines.ForEach(o =>
            {                
                var geometry = new ThGeometry();
                geometry.Properties.Add("Category", Category);
                geometry.Boundary = o;
                geos.Add(geometry);
            });
            return geos;
        }

        public void Print(Database database)
        {
            using (var db =AcadDatabase.Use(database))
            {
                var laneLineIds = new ObjectIdList();
                LaneLines.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    laneLineIds.Add(db.ModelSpace.Add(o));
                });
                if (laneLineIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), laneLineIds);
                }
            }
        }
    }
}
