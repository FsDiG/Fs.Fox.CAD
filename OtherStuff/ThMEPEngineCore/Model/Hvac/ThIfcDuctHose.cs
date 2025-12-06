using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThMEPEngineCore.Model.Hvac
{
    public class ThIfcDuctHoseParameters
    {
        public double Length { get; set; }
        public double Width { get; set; }
        public double RotateAngle { get; set; }
        public Point3d InsertPoint { get; set; }
    }
    public class ThIfcDuctHose : ThIfcDuctFitting
    {
        public ThIfcDuctHoseParameters Parameters { get; set; }
        public ThIfcDuctHose(ThIfcDuctHoseParameters parameters)
        {
            Parameters = parameters;
        }

        public void SetHoseInsertPoint()
        {
            Point3d originpoint = Point3d.Origin;
            this.Parameters.InsertPoint = originpoint.TransformBy(Matrix3d.Displacement(new Vector3d(0.5 * Parameters.Width, 0, 0)));
        }

        public static ThIfcDuctReducing Create(ThIfcDuctReducingParameters parameters)
        {
            throw new NotImplementedException();
        }

    }
}
