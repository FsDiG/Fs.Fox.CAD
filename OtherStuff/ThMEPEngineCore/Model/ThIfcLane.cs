using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model
{
    public class ThIfcLane : ThIfcSpace
    {
        public static ThIfcLane Create(Curve curve)
        {
            return new ThIfcLane()
            {
                Boundary = curve.Clone() as Curve,
            };
        }
    }
}
