using System;
using System.Linq;
using ThCADCore.NTS;
using GeometryExtensions;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.Geometry;
using ThMEPEngineCore.BeamInfo.Model;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Model
{
    public sealed class ThIfcLineBeam : ThIfcBeam
    {
        public Vector3d Direction
        {
            get
            {
                return this.StartPoint.GetVectorTo(this.EndPoint);
            }
        }
        public double Length
        {
            get
            {
                return StartPoint.DistanceTo(EndPoint);
            }
        }

        private ThIfcLineBeam()
        {
            //
        }

        public static ThIfcLineBeam Create(Polyline polyline, double height = 0.0)
        {
            var segments = new PolylineSegmentCollection(polyline);
            var enumerable = segments.Where(o => o.IsLinear).OrderByDescending(o => o.ToLineSegment().Length);
            var beam = new LineBeam(
                new Line(enumerable.ElementAt(0).StartPoint.ToPoint3d(), enumerable.ElementAt(0).EndPoint.ToPoint3d()),
                new Line(enumerable.ElementAt(1).StartPoint.ToPoint3d(), enumerable.ElementAt(1).EndPoint.ToPoint3d()));
            return new ThIfcLineBeam()
            {
                Height = height,
                Width = beam.Width,
                EndPoint = beam.EndPoint,
                Outline = beam.BeamBoundary,
                StartPoint = beam.StartPoint,
                Uuid = Guid.NewGuid().ToString(),
            };
        }
        public static ThIfcLineBeam Create(ThIfcBeamAnnotation annotation)
        {
            var outline = ThLineBeamOutliner.CreatOutline(annotation.StartPoint, annotation.EndPoint, annotation.Size.X);
            outline.TransformBy(annotation.Matrix);
            return Create(outline, annotation.Size.Y);
        }
        public static ThIfcLineBeam Create(ThIfcLineBeam olderLineBeam,double startExtend,double endExtend)
        {
            return Create(ThLineBeamOutliner.ExtendBoth(olderLineBeam, startExtend, endExtend), olderLineBeam.Height);
        }
        public static ThIfcLineBeam Create(ThIfcLineBeam olderLineBeam, Point3d startPt, Point3d endPt)
        {
            return Create(ThLineBeamOutliner.CreatOutline(startPt, endPt, olderLineBeam.Width), olderLineBeam.Height);
        }
    }
}
