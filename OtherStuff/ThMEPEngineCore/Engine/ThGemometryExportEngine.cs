using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Engine
{
    public class ThGemometryExportEngine:IDisposable
    {
        public List<ThIfcSpace> Spaces { get; set; }
        public ThGemometryExportEngine()
        {
            Spaces = new List<ThIfcSpace>();
        }
        public void Export(Database database, Point3dCollection polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            using (var roomDbExtension = new ThRoomDbExtension(database))
            {
                roomDbExtension.BuildElementCurves();
                if (polygon.Count > 0)
                {
                    DBObjectCollection dbObjs = new DBObjectCollection();
                    roomDbExtension.Rooms.ForEach(o => dbObjs.Add(o.Boundary));
                    var roomSpatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                    var polylines = new List<Polyline>();
                    foreach (var filterObj in roomSpatialIndex.SelectCrossingPolygon(polygon))
                    {
                        polylines.Add(filterObj as Polyline);
                    }
                    Spaces = roomDbExtension.Rooms.Where(o=> polylines.Contains(o.Boundary)).ToList();
                }
                else
                {
                    Spaces = roomDbExtension.Rooms;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
