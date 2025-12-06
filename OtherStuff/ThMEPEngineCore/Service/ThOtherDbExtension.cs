using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public abstract class ThOtherDbExtension:ThDbExtension
    {
        protected ThOtherDbExtension(Database db):base(db)
        {
        }
    }
}
