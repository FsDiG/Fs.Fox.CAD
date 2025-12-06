using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ThMEPEngineCore.Service
{
    public class ThLineBeamOutliner
    {
        public Polyline Outline { get; set; }

        public Point3d StartPoint
        {
            get
            {
                return ThGeometryTool.GetMidPt(Outline.GetPoint3dAt(0), Outline.GetPoint3dAt(1));
            }
        }

        public Point3d EndPoint
        {
            get
            {
                return ThGeometryTool.GetMidPt(Outline.GetPoint3dAt(2), Outline.GetPoint3dAt(3));
            }
        }

        public double Width
        {
            get
            {
                return Outline.GetPoint3dAt(0).DistanceTo(Outline.GetPoint3dAt(1));
            }
        }

        public Vector3d Direction
        {
            get
            {
                return StartPoint.GetVectorTo(EndPoint);
            }
        }
        public ThLineBeamOutliner(ThIfcLineBeam lineBeam)
        {
            Outline = lineBeam.Outline.Clone() as Polyline;
        }

        public static Polyline Extend(ThIfcBeam beam, double lengthIncrement, double widthIncrement)
        {
            if (beam is ThIfcLineBeam lineBeam)
            {
                return Extend(lineBeam, lengthIncrement, widthIncrement);
            }
            else if (beam is ThIfcArcBeam arcBeam)
            {
                return Extend(arcBeam, lengthIncrement, widthIncrement);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline Extend(ThIfcLineBeam lineBeam, double lengthIncrement, double widthIncrement)
        {
            var outliner = new ThLineBeamOutliner(lineBeam);
            outliner.Extend(lengthIncrement, widthIncrement);
            return outliner.Outline;
        }

        public static Polyline Extend(ThIfcArcBeam arcBeam, double lengthIncrement, double widthIncrement)
        {
            throw new NotSupportedException();
        }

        public static Polyline ExtendBoth(ThIfcBeam beam, double startExtendLength, double endExtendLength)
        {
            if (beam is ThIfcLineBeam lineBeam)
            {
                return ExtendBoth(lineBeam, startExtendLength, endExtendLength);
            }
            else if (beam is ThIfcArcBeam arcBeam)
            {
                return ExtendBoth(arcBeam, startExtendLength, startExtendLength);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polyline ExtendBoth(ThIfcLineBeam lineBeam, double startExtendLength, double endExtendLength)
        {
            var outliner = new ThLineBeamOutliner(lineBeam);
            outliner.ExtendBoth(startExtendLength, endExtendLength);
            return outliner.Outline;
        }

        public static Polyline ExtendBoth(ThIfcArcBeam arcBeam, double startExtendLength, double endExtendLength)
        {
            throw new NotSupportedException();
        }

        public static Polyline CreatOutline(Point3d startPt, Point3d endPt, double width)
        {
            Vector3d direction = startPt.GetVectorTo(endPt);
            Vector3d perpendDir = direction.GetPerpendicularVector();
            Point3d pt1 = startPt - perpendDir.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt2 = startPt + perpendDir.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt3 = pt2 + direction.GetNormal().MultiplyBy(startPt.DistanceTo(endPt));
            Point3d pt4 = pt1 + direction.GetNormal().MultiplyBy(startPt.DistanceTo(endPt));
            Point3dCollection pts = new Point3dCollection() { pt1, pt2, pt3, pt4 };
            return pts.CreatePolyline();
        }

        public void Extend(double lengthIncrement, double widthIncrement)
        {
            Outline = CreatOutline(
                StartPoint - Direction.GetNormal() * lengthIncrement,
                EndPoint + Direction.GetNormal() * lengthIncrement,
                Width + 2 * widthIncrement);
        }
        public void ExtendBoth(double startExtendLength, double endExtendLength)
        {
            Outline = CreatOutline(
                StartPoint - Direction.GetNormal() * startExtendLength,
                EndPoint + Direction.GetNormal() * endExtendLength,
                Width);
        }
    }
}
