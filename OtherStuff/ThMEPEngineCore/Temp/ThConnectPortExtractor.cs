using System;
using System.Collections.Generic;
using DotNetARX;
using Linq2Acad;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThConnectPortExtractor :ThExtractorBase , IExtract , IPrint, IBuildGeometry
    {
        public Dictionary<Polyline, string> ConnectPorts { get; private set; }
        public ThConnectPortExtractor()
        {
            ConnectPorts = new Dictionary<Polyline, string>();
            Category = "ConnectPort";
        }

        public void Extract(Database database, Point3dCollection pts)
        {
            var instance = new ThExtractConnectPortsService();
            instance.Extract(database, pts);
            ConnectPorts = instance.ConnectPorts;
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            ConnectPorts.ForEach(o =>
            {                
                var geometry = new ThGeometry();
                geometry.Properties.Add("Category", Category);
                geometry.Properties.Add("Code", o.Value);
                geometry.Boundary = o.Key;
                geos.Add(geometry);
            });
            return geos;
        }

        public void Print(Database database)
        {
            using (var db =AcadDatabase.Use(database))
            {
                var connectPortIds = new ObjectIdList();
                ConnectPorts.ForEach(o =>
                {                    
                    o.Key.ColorIndex = ColorIndex;
                    o.Key.SetDatabaseDefaults();
                    connectPortIds.Add(db.ModelSpace.Add(o.Key));
                });
                if (connectPortIds.Count > 0)
                {
                    GroupTools.CreateGroup(db.Database, Guid.NewGuid().ToString(), connectPortIds);
                }
            }
        }
    }
}
