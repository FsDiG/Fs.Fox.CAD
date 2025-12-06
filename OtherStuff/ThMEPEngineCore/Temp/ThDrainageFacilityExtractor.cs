using System;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThDrainageFacilityExtractor : ThExtractorBase,IExtract,IPrint, IBuildGeometry
    {
        public List<Curve> DrainageFacilities { get; private set; }      
        public ThDrainageFacilityExtractor()
        {
            DrainageFacilities = new List<Curve>();
            Category = "DrainageFacility";
        }
        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractDrainageFacilityService();
            instance.Extract(database, pts);
            DrainageFacilities = instance.Facilities;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            DrainageFacilities.ForEach(o =>
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
            using (var db = AcadDatabase.Use(database))
            {
                var drainageFacilityIds = new ObjectIdList();
                DrainageFacilities.ForEach(o =>
                {
                    o.ColorIndex = ColorIndex;
                    o.SetDatabaseDefaults();
                    drainageFacilityIds.Add(db.ModelSpace.Add(o));
                });
                if (drainageFacilityIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), drainageFacilityIds);
                }
            }
        }
    }
}
