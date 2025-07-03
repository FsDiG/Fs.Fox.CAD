using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model.Segment
{
    public abstract class ThSegment
    {
        public virtual Point3d StartPoint { get; set; }
        public virtual Point3d EndPoint { get; set; }
        public virtual double Width { get; set; }
        public virtual Vector3d Normal { get; set; }
        public virtual Polyline Outline { get; set; }
        public abstract Polyline Extend(double length);
    }
}
