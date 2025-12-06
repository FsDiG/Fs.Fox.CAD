using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Model.Segment
{
    public class ThLinearSegment: ThSegment
    {
        public Vector3d Direction
        {
            get
            {
                return StartPoint.GetVectorTo(EndPoint);
            }
        }
        public override Polyline Extend(double length)
        {
            Vector3d direction = this.StartPoint.GetVectorTo(this.EndPoint);
            Vector3d perpendDir = direction.GetPerpendicularVector();
            double width = this.Outline.GetPoint3dAt(0).DistanceTo(this.Outline.GetPoint3dAt(1));
            Point3d newSp = this.StartPoint - direction.GetNormal().MultiplyBy(length);
            Point3d newEp = this.EndPoint + direction.GetNormal().MultiplyBy(length);
            Point3d pt1 = newSp - perpendDir.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt2 = newSp + perpendDir.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt3 = pt2 + direction.GetNormal().MultiplyBy(newSp.DistanceTo(newEp));
            Point3d pt4 = pt1 + direction.GetNormal().MultiplyBy(newSp.DistanceTo(newEp));
            Point3dCollection pts = new Point3dCollection() { pt1, pt2, pt3, pt4 };
            return pts.CreatePolyline();
        }
    }
}
