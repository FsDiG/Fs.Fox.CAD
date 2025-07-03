using System;
using Autodesk.AutoCAD.Geometry;

namespace ThMEPEngineCore.Model.Hvac
{
    public class ThIfcDuctTeeParameters
    {
        /// <summary>
        /// 中心点
        /// </summary>
        public Point3d CenterPoint { get; set; }

        /// <summary>
        /// 支路管道直径
        /// </summary>
        public double BranchDiameter { get; set; }

        /// <summary>
        /// 主路大端管道直径
        /// </summary>
        public double MainBigDiameter { get; set; }

        /// <summary>
        /// 主路小端管道直径
        /// </summary>
        public double MainSmallDiameter { get; set; }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public double RotateAngle { get; set; }
    }
    public class ThIfcDuctTee : ThIfcDuctFitting
    {
        public ThIfcDuctTeeParameters Parameters { get; set; }

        public ThIfcDuctTee(ThIfcDuctTeeParameters parameters)
        {
            Parameters = parameters;
            Parameters.CenterPoint = Point3d.Origin;
        }

        public static ThIfcDuctTee Create(ThIfcDuctTeeParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
