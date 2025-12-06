using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcWindow : ThIfcBuildingElement
    {
        public static ThIfcWindow Create(Entity entity)
        {
            return new ThIfcWindow()
            {
                Outline = entity,
                Uuid = Guid.NewGuid().ToString()
            };
        }
    }
}
