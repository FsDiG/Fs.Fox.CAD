using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public interface IExtract
    {
        void Extract(Database database, Point3dCollection pts);
    }
}
