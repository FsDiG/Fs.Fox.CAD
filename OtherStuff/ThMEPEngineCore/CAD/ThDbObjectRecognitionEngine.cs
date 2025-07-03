using ThCADCore.NTS;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.CAD
{
    public class ThDbObjectRecognitionEngine
    {
        public List<Entity> DbObjects { get; set; }

        public ThDbObjectRecognitionEngine()
        {
            DbObjects = new List<Entity>();
        }

        public void Recognize(ThDbExtension dbExtension, Point3dCollection polygon)
        {
            dbExtension.BuildElementCurves();
            if (polygon.Count > 0)
            {
                DBObjectCollection dbObjs = new DBObjectCollection();
                dbExtension.DbObjects.ForEach(o => dbObjs.Add(o));
                var spatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                foreach (var filterObj in spatialIndex.SelectCrossingPolygon(polygon))
                {
                    DbObjects.Add(filterObj as Entity);
                }
            }
            else
            {
                DbObjects.AddRange(dbExtension.DbObjects);
            }
        }
    }
}
