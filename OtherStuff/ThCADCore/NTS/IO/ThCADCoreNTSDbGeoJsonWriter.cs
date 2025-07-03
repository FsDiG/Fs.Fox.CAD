using NetTopologySuite.IO;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS.IO
{
    public class ThCADCoreNTSDbGeoJsonWriter : GeoJsonWriter
    {
        public string Write(Curve curve)
        {
            return base.Write(curve.ToNTSGeometry());
        }
    }
}
