using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model.Segment
{
    public class ThArcSegment : ThSegment
    {
        /// <summary>
        /// 起点切线方向
        /// </summary>
        public Vector3d StartTangent { get; set; }
        /// <summary>
        /// 终点切线方向
        /// </summary>
        public Vector3d EndTangent { get; set; }
        public double TotalAngle
        {
            get
            {
                return EndAngle - StartAngle;
            }
        }
        public double Length
        {
            get
            {
                return (EndAngle - StartAngle) * Radius;
            }
        }
        public double EndAngle
        {
            get
            {
                return CalculateEndAngle();
            }
        }
        public double StartAngle
        {
            get
            {
                return CalculateStartAngle();
            }
        }
        public double Radius { get; set; }
        public Point3d Center { get; set; }
        private double CalculateStartAngle()
        {
            Plane plane = new Plane(Center, Normal);
            Matrix3d wcs2Ucs = Matrix3d.WorldToPlane(plane);
            Point3d newSpt = StartPoint.TransformBy(wcs2Ucs);
            plane.Dispose();
            return Vector3d.XAxis.GetAngleTo(Point3d.Origin.GetVectorTo(newSpt), Vector3d.ZAxis);
        }
        private double CalculateEndAngle()
        {
            Plane plane = new Plane(Center, Normal);
            Matrix3d wcs2Ucs = Matrix3d.WorldToPlane(plane);
            Point3d newEpt = EndPoint.TransformBy(wcs2Ucs);
            plane.Dispose();
            return Vector3d.XAxis.GetAngleTo(Point3d.Origin.GetVectorTo(newEpt), Vector3d.ZAxis);
        }

        public override Polyline Extend(double length)
        {
            throw new NotImplementedException();
        }
    }
}
