using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Model
{
    public class ThIfcArcBeam : ThIfcBeam
    {
        public Vector3d StartTangent { get; set; }
        public Vector3d EndTangent { get; set; }
        public double Radius { get; set; }
    }
}
