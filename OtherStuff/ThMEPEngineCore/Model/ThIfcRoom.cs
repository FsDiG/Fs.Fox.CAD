using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcRoom : ThIfcSpatialElement
    {
        public string Name { get; set; }
        public static ThIfcRoom Create(Curve curve)
        {
            return new ThIfcRoom()
            {
                Boundary = curve,
                Uuid = Guid.NewGuid().ToString()
            };
        }
    }
}
