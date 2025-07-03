using System;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Engine;

namespace ThMEPEngineCore.Temp
{
    public class ThShearWallExtractor : ThExtractorBase, IExtract,IPrint, IBuildGeometry
    {
        public List<Entity> Walls { get; private set; }
        public ThShearWallExtractor()
        {
            Walls = new List<Entity>();
            Category = "ShearWall";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            using (var engine=new ThShearWallRecognitionEngine())
            {
                engine.Recognize(database, pts);
                engine.Elements.ForEach(o=>Walls.Add(o.Outline));
            }
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            Walls.ForEach(o =>
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
            using (var db=AcadDatabase.Use(database))
            {
                var wallIds = new ObjectIdList();
                Walls.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    wallIds.Add(db.ModelSpace.Add(o));
                });
                if (wallIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), wallIds);
                }
            }
        }
    }
}
