using System;
using Autodesk.AutoCAD.Geometry;

namespace ThMEPEngineCore.Model.Hvac
{
    public class ThIfcDuctElbowParameters
    {
        /// <summary>
        /// 弯头角度(弧度)
        /// </summary>
        public double ElbowDegree { get; set; }

        /// <summary>
        /// 管道直径
        /// </summary>
        public double PipeOpenWidth { get; set; }

        /// <summary>
        /// 末端直管段长度
        /// </summary>
        public double ReserveLength { get; set; }

        /// <summary>
        /// 单侧折弯长度
        /// </summary>
        public double SingleLength { get; set; }

        /// <summary>
        /// 角点
        /// </summary>
        public Point3d CornerPoint { get; set; }

        /// <summary>
        /// 中心点
        /// </summary>
        public Point3d CenterPoint { get; set; }

        /// <summary>
        /// 等分角度
        /// </summary>
        public double BisectorAngle { get; set; }
    }

    public class ThIfcDuctElbow : ThIfcDuctFitting
    {
        public ThIfcDuctElbowParameters Parameters { get; private set; }

        public ThIfcDuctElbow(ThIfcDuctElbowParameters parameters)
        {
            Parameters = parameters;
            Parameters.CornerPoint = Point3d.Origin;
            Parameters.CenterPoint = Parameters.CornerPoint + new Vector3d(-0.7 * Parameters.PipeOpenWidth, -Math.Abs(0.7 * Parameters.PipeOpenWidth * Math.Tan(0.5 * (Parameters.ElbowDegree * Math.PI / 180))), 0);
            var Bisectorvector = new Vector2d(Parameters.CenterPoint.X - Parameters.CornerPoint.X , Parameters.CenterPoint.Y - Parameters.CornerPoint.Y);
            Parameters.BisectorAngle = Bisectorvector.Angle;
        }

        public static ThIfcDuctElbow Create(ThIfcDuctElbowParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
