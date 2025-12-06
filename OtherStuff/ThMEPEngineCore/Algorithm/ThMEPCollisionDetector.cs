using System;
using ThCADCore.NTS;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPCollisionDetector
    {
        private Dictionary<string, ThCADCoreNTSSpatialIndex> SpatialIndexes { get; set; }

        public ThMEPCollisionDetector()
        {
            SpatialIndexes = new Dictionary<string, ThCADCoreNTSSpatialIndex>();
        }

        public void AddSpatialIndex(ThCADCoreNTSSpatialIndex spatialIndex, string context)
        {
            SpatialIndexes[context] = spatialIndex;
        }

        public void RemoveSpatialIndex(string context)
        {
            if (SpatialIndexes.ContainsKey(context))
            {
                SpatialIndexes.Remove(context);
            }
        }

        public bool CollidePoly(Polyline poly)
        {
            throw new NotImplementedException();
        }
    }
}
