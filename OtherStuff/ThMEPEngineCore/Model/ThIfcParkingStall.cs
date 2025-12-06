using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model
{
    public class ThIfcParkingStall : ThIfcSpace
    {
        public static ThIfcParkingStall Create(Curve boundary)
        {
            return new ThIfcParkingStall()
            {
                Boundary = boundary,
            };
        }
    }
}
