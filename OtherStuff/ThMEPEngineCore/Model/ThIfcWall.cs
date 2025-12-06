using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcWall : ThIfcBuildingElement
    {
        public static ThIfcWall Create(Entity curve)
        {
            return new ThIfcWall()
            {
                Outline = curve,
                Uuid = Guid.NewGuid().ToString()
            };
        }
    }
}
