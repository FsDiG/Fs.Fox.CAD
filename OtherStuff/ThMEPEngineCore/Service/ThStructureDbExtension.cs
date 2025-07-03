using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Service
{
    public abstract class ThStructureDbExtension:ThDbExtension
    {
        protected ThStructureDbExtension(Database db):base(db)
        {
        }
    }
}
