using System;
using System.Collections.Generic;
using Linq2Acad;
using DotNetARX;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThCenterLineExtractor :ThExtractorBase, IExtract,IPrint,IBuildGeometry
    {
        public List<Curve> CenterLines { get; private set; }
        public ThCenterLineExtractor()
        {
            CenterLines = new List<Curve>();
            Category = "中心线";
        }
        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractCenterLineService()
            {
                CenterLineLayer = "中心线示意",
            };
            instance.Extract(database, pts);
            CenterLines = instance.CenterLines;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            CenterLines.ForEach(o =>
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
                var centerLineIds = new ObjectIdList();
                CenterLines.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    centerLineIds.Add(db.ModelSpace.Add(o));
                });
                if (centerLineIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), centerLineIds);
                }
            }
        }
    }
}
