using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model
{
    public class ThIfcColumn : ThIfcBuildingElement
    {
        public static ThIfcColumn Create(Curve curve)
        {
            return new ThIfcColumn()
            {
                Outline = curve,
                Uuid = Guid.NewGuid().ToString()
            };
        }
    }
}
