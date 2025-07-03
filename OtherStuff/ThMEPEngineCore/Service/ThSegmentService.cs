using System;
using Linq2Acad;
using System.Linq;
using ThCADExtension;
using System.Collections.Generic;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.Model.Segment;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.BeamInfo.Business;
using ThMEPEngineCore.Interface;

namespace ThMEPEngineCore.Service
{
    public class ThSegmentService
    {
        protected Polyline Outline { get; set; }
        public List<ThSegment> Segments { get; set; }       
        public ThSegmentService(Polyline polyline)
        {
            Outline = polyline;
            Segments = new List<ThSegment>();
        }
        public void Segment(Point3dCollection intersPts) 
        {
            if (intersPts.Count > 1)
            {
                List<Curve3d> Curve3Ds = FillCurve3Ds(intersPts);
                var linearBeams = HandleLinearSegment(Curve3Ds);
                var arcBeams = HandleArcSegment(Curve3Ds);
                linearBeams.ForEach(o => Segments.Add(CreateSegment(o)));
                arcBeams.ForEach(o => Segments.Add(CreateSegment(o)));               
            }
        }
        public void SegmentAll(ICalculateBeam calculateBeam)
        {
            DBObjectCollection dbObjs = new DBObjectCollection();
            Outline.Explode(dbObjs);
            DBObjectCollection validObjs = new DBObjectCollection();
            foreach (var obj in dbObjs)
            {
                if(obj is Line line && line.Length>0)
                {
                    validObjs.Add(line);
                }
                else if(obj is Arc arc && arc.Length > 0)
                {
                    validObjs.Add(arc);
                }
            }
            var beams = calculateBeam.GetBeamInfo(validObjs);
            beams.ForEach(o => Segments.Add(CreateSegment(o)));
            foreach(DBObject dbObj in dbObjs)
            {
                dbObj.Dispose();
            }
        }
        private List<Curve3d> FillCurve3Ds(Point3dCollection pts)
        {
            List<Curve3d> curveSegments = new List<Curve3d>();
            foreach (Point3d pt in pts)
            {
                var curves = GetSegmentByPoint(Outline, pt);
                curves.ForEach(o =>
                {
                    if (curveSegments.IndexOf(o) < 0)
                    {
                        curveSegments.Add(o);
                    }
                });
            }
            return curveSegments;
        }
        private List<Beam> HandleLinearSegment(List<Curve3d> Curve3Ds)
        {
            List<Curve3d> linearSegments = Curve3Ds.Where(o => o is LineSegment3d).ToList();
            DBObjectCollection dbObjs = new DBObjectCollection();
            linearSegments.ForEach(o=>
            {
                Line line = new Line(o.StartPoint,o.EndPoint);
                dbObjs.Add(line);
            });
            CalBeamStruService calBeamStruService = new CalBeamStruService();
            var beams = calBeamStruService.GetBeamInfo(dbObjs);
            beams.ForEach(o =>
            {
                dbObjs.Remove(o.UpBeamLine);
                dbObjs.Remove(o.DownBeamLine);
            });
            foreach (DBObject dbObj in dbObjs)
            {
                dbObj.Dispose();
            }
            return beams;
        }
        private List<Beam> HandleArcSegment(List<Curve3d> Curve3Ds)
        {
            List<Curve3d> arcSegments = Curve3Ds.Where(o => o is CircularArc3d).ToList();
            DBObjectCollection dbObjs = new DBObjectCollection();
            arcSegments.ForEach(o =>
            {
                CircularArc3d circularArc = o as CircularArc3d;
                Arc arc = new Arc(circularArc.Center, circularArc.Normal, circularArc.Radius, circularArc.StartAngle, circularArc.EndAngle);
                dbObjs.Add(arc);
            });
            CalBeamStruService calBeamStruService = new CalBeamStruService();
            var beams = calBeamStruService.GetBeamInfo(dbObjs);
            beams.ForEach(o =>
            {
                dbObjs.Remove(o.UpBeamLine);
                dbObjs.Remove(o.DownBeamLine);
            });        
            foreach (DBObject dbObj in dbObjs)
            {
                dbObj.Dispose();
            }
            return beams;
        }
        protected List<Curve3d> GetSegmentByPoint(Polyline pl, Point3d pt)
        {
            List<Curve3d> segments = new List<Curve3d>();
            Curve3d seg;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                seg = null;
                SegmentType segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);
                if (seg != null && seg.IsOn(pt, ThCADCommon.Global_Tolerance))
                {
                    segments.Add(seg);
                }
            }
            return segments;
        }

        private ThLinearSegment CreateSegment(LineBeam lineBeam)
        {
            ThLinearSegment thLinearSegment = new ThLinearSegment()
            {
                StartPoint = lineBeam.StartPoint,
                EndPoint = lineBeam.EndPoint,
                Normal = lineBeam.BeamNormal,
                Outline = lineBeam.BeamBoundary,
                Width = Outline.GetPoint3dAt(0).DistanceTo(Outline.GetPoint3dAt(1))
            };
            return thLinearSegment;
        }
        private ThArcSegment CreateSegment(ArcBeam arcBeam)
        {
            ThArcSegment thArcSegment = new ThArcSegment()
            {
                StartPoint = arcBeam.StartPoint,
                EndPoint = arcBeam.EndPoint,
                Normal = arcBeam.BeamNormal,
                Outline = arcBeam.BeamBoundary,
                Width = Outline.GetPoint3dAt(0).DistanceTo(Outline.GetPoint3dAt(1)),
                Center = arcBeam.CenterPoint,
                Radius= Math.Abs(arcBeam.CenterPoint.DistanceTo(arcBeam.UpStartPoint)- arcBeam.CenterPoint.DistanceTo(arcBeam.DownStartPoint))
            };
            return thArcSegment;
        }
        private ThSegment CreateSegment(Beam beam)
        {
            if(beam is LineBeam lineBeam)
            {
                return CreateSegment(lineBeam);
            }
            else if(beam is ArcBeam arcBeam)
            {
                return CreateSegment(arcBeam);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
