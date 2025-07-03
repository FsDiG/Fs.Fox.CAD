using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public abstract class ThIfcBeam : ThIfcBuildingElement
    {
        public Point3d StartPoint { get; set; }
        public Point3d EndPoint { get; set; }
        public double Width { get; set; } 
        public double Height { get; set; }
        public BeamComponentType ComponentType { get; set; }
    }
    public enum BeamComponentType
    {
        Undefined = 0,
        /// <summary>
        /// 主梁
        /// 两端均为竖向构件
        /// </summary>
        PrimaryBeam = 1,
        /// <summary>
        /// 半主梁
        /// 一端为竖向构件，另一端为主梁
        /// </summary>
        HalfPrimaryBeam = 2,
        /// <summary>
        /// 悬挑主梁
        /// 一端为竖向构件,另一端无主梁或竖向构件，且无延续构件
        /// </summary>
        OverhangingPrimaryBeam = 3,
        /// <summary>
        /// 次梁
        /// 两端均为主梁或半主梁
        /// </summary>
        SecondaryBeam = 4
    }
}
