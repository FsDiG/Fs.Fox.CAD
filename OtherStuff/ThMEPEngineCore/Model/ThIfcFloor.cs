using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcFloor : ThIfcBuildingElement
    {
        //
        public static ThIfcFloor Create(Curve curve)
        {
            return new ThIfcFloor()
            {
                Outline = curve,
            };
        }
    }
}
