using System;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThColumnExtractor : ThExtractorBase, IExtract,IPrint, IBuildGeometry
    {
        public List<Polyline> Columns { get; private set; }
        public ThColumnExtractor()
        {
            Columns = new List<Polyline>();
            Category = "Column";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractColumnService();
            instance.Extract(database, pts);
            Columns = instance.Columns;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            Columns.ForEach(o =>
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
                var columnIds = new ObjectIdList();
                Columns.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    columnIds.Add(db.ModelSpace.Add(o));
                });
                if (columnIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), columnIds);
                }
            }
        }
    }
}
