using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcCurtainWall : ThIfcBuildingElement
    {
        public static ThIfcCurtainWall Create(Entity curve)
        {
            return new ThIfcCurtainWall()
            {
                Outline = curve,
                Uuid = Guid.NewGuid().ToString()
            };
        }
    }
}
