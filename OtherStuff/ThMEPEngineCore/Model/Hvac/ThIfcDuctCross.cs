using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model.Hvac
{
    public class ThIfcDuctCrossParameters
    {
        /// <summary>
        /// 四通大端管道截面宽度
        /// </summary>
        public double BigEndWidth { get; set; }

        /// <summary>
        /// 四通与大端对接管道截面宽度
        /// </summary>
        public double mainSmallEndWidth { get; set; }

        /// <summary>
        /// 四通侧路大端管道截面宽度
        /// </summary>
        public double SideBigEndWidth { get; set; }

        /// <summary>
        /// 四通侧路小端管道截面宽度
        /// </summary>
        public double SideSmallEndWidth { get; set; }

        /// <summary>
        /// 四通中心点
        /// </summary>
        public Point3d Center { get; set; }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public double RotateAngle { get; set; }

    }

    public class ThIfcDuctCross : ThIfcDuctFitting
    {
        public ThIfcDuctCrossParameters Parameters { get; private set; }

        public ThIfcDuctCross(ThIfcDuctCrossParameters parameters)
        {
            Parameters = parameters;
            Parameters.Center = Point3d.Origin;
        }

        public static ThIfcDuctCross Create(ThIfcDuctCrossParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
