using System;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThDoorExtractor :ThExtractorBase , IExtract , IPrint, IBuildGeometry
    {
        public List<Polyline> Doors { get; private set; }

        public ThDoorExtractor()
        {
            Doors = new List<Polyline>();
            Category = "Door";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractDoorService();
            instance.Extract(database, pts);
            Doors = instance.Doors;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            Doors.ForEach(o =>
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
                var doorIds = new ObjectIdList();
                Doors.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    doorIds.Add(db.ModelSpace.Add(o));                    
                });
                if (doorIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), doorIds);
                }
            }
        }
    }
}
