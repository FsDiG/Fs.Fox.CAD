using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public abstract class ThExtractService
    {
        public abstract void Extract(Database db, Point3dCollection pts);
    }
}
