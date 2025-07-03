using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSGeometryComparer : IComparer<Geometry>
    {
        public int Compare(Geometry x, Geometry y)
        {
            return x.CompareTo(y);
        }
    }
}
