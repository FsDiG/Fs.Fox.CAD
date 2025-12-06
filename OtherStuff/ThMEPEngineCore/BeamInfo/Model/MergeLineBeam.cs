using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class MergeLineBeam : Beam
    {
        public MergeLineBeam(List<Beam> beams)
        {
            mergeBeams = new List<Beam>(beams);
            UpBeamLines = new List<Curve>();
            DownBeamLines = new List<Curve>();
        }

        public List<Beam> mergeBeams { get; set; }

        public List<Curve> UpBeamLines { get; set; }

        public List<Curve> DownBeamLines { get; set; }

        public override Polyline BeamBoundary
        {
            get
            {
                Vector3d zDir = Vector3d.ZAxis;
                Vector3d yDir = Vector3d.ZAxis.CrossProduct(BeamNormal);
                Matrix3d trans = new Matrix3d(new double[]{
                    BeamNormal.X, yDir.X, zDir.X, 0,
                    BeamNormal.Y, yDir.Y, zDir.Y, 0,
                    BeamNormal.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

                List<Point3d> allP = mergeBeams.SelectMany(x => new List<Point3d>()
                {
                    x.UpStartPoint,
                    x.UpEndPoint,
                    x.DownStartPoint,
                    x.DownEndPoint
                }).ToList();
                allP = allP.Select(x => x.TransformBy(trans.Inverse())).ToList();

                double minX = allP.OrderBy(x => x.X).First().X;
                double maxX = allP.OrderByDescending(x => x.X).First().X;
                double minY = allP.OrderBy(x => x.Y).First().Y;
                double maxY = allP.OrderByDescending(x => x.Y).First().Y;
                Point3d p1 = (new Point3d(minX, minY, 0));
                Point3d p2 = (new Point3d(maxX, minY, 0));
                Point3d p3 = (new Point3d(maxX, maxY, 0));
                Point3d p4 = (new Point3d(minX, maxY, 0));
                List < Point3d > points = new List<Point3d>() { p1, p2, p3, p4 };
                Polyline resPolyline = new Polyline(points.Count)
                {
                    Closed = true,
                };
                Point3d thisP = points.First();
                int index = 0;
                resPolyline.AddVertexAt(index, new Point2d(thisP.X, thisP.Y), 0, 0, 0);
                points.Remove(thisP);
                while (points.Count > 0)
                {
                    thisP = points.OrderBy(x => x.DistanceTo(thisP)).First();
                    index++;
                    resPolyline.AddVertexAt(index, new Point2d(thisP.X, thisP.Y), 0, 0, 0);
                    points.Remove(thisP);
                }
                
                resPolyline.TransformBy(trans);
                return resPolyline;
            }
        }
    }
}
