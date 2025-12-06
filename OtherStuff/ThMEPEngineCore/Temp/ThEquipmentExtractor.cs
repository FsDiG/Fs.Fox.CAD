using System;
using System.Collections.Generic;
using Linq2Acad;
using DotNetARX;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Temp
{
    public class ThEquipmentExtractor :ThExtractorBase,IExtract,IPrint, IBuildGeometry
    {
        public Dictionary<string, List<Polyline>> Equipments { get; private set; }
        public ThEquipmentExtractor()
        {
            Equipments = new Dictionary<string, List<Polyline>>();
            Category= "Equipment";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractEquipmentService();
            instance.Extract(database, pts);
            Equipments = instance.Equipments;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            Equipments.ForEach(e =>
            {
                e.Value.ForEach(v =>
                {
                    var geometry = new ThGeometry();
                    geometry.Properties.Add("Category", Category);
                    geometry.Properties.Add("Name", e.Key);
                    geometry.Boundary = v;
                    geos.Add(geometry);
                });
            });
            return geos;
        }

        public void Print(Database database)
        {
            using (var db =AcadDatabase.Use(database))
            {
                var equipIds = new ObjectIdList();
                Equipments.ForEach(e =>
                {
                    e.Value.ForEach(v =>
                    {
                        v.ColorIndex = ColorIndex;
                        v.SetDatabaseDefaults();
                        equipIds.Add(db.ModelSpace.Add(v));
                    });
                });
                if (equipIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), equipIds);
                }
            }
        }
    }
}
