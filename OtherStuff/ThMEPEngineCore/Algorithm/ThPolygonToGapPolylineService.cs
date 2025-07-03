using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using Linq2Acad;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Algorithm
{
    public class ThPolygonToGapPolylineService
    {
        ///目前主要用于支撑对剪力墙带一个洞的裁剪
        ///暂时将线延伸设为5mm,之前设1,2mm有没成功的情况
        ///打断点偏移的距离要大于线延伸的距离
        private double PointOffsetDistance = 10.0; 
        private const double LineExtendDistance = 5.0;        
        private Polygon CurrentPolygon { get; set; }
        private ThPolygonToGapPolylineService(Polygon polygon)
        {
            CurrentPolygon = polygon;
            if(PointOffsetDistance<= LineExtendDistance)
            {
                PointOffsetDistance=LineExtendDistance + 5.0;
            }
        }
        public static List<Polyline> ToGapPolyline(Polygon polygon)
        {
            List<Polyline> gapPolylines = new List<Polyline>();
            if(polygon==null || polygon.Shell==null)
            {
                return gapPolylines;
            }
            var intstance = new ThPolygonToGapPolylineService(polygon);
            var gapPolyline = intstance.ToGapPolyline();
            if(gapPolyline != null && gapPolyline.Area>0.0)
            {
                gapPolylines.Add(gapPolyline);
            }
            return gapPolylines;
        }
        private Polyline ToGapPolyline()
        {
            var shell= CurrentPolygon.Shell.ToDbPolyline();
            var holes = new List<Polyline>();
            CurrentPolygon.Holes.ForEach(o => holes.Add(o.ToDbPolyline()));
            if (holes.Count == 0)
            {
                return shell;
            }
            else if(shell.Area==0.0 && holes.Count==1)
            {
                return holes[0];
            }
            else if(shell.Area > 0.0 && holes.Count == 1)
            {
                Polyline shellOutline = shell.ToNTSLineString().ToDbPolyline();
                return BuildOutermostPolyline(shellOutline, holes[0].ToNTSLineString().ToDbPolyline());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Polyline BuildOutermostPolyline(Polyline outerPolyline, Polyline innerPolyline)
        {            
            List<Line> lines = new List<Line>();
            List<Tuple<int, Point3d, Point3d>> splitSegments = new List<Tuple<int, Point3d, Point3d>>();
            for (int i = 0; i < innerPolyline.NumberOfVertices; i++)
            {
                var firstLineSegment = innerPolyline.GetLineSegmentAt(i);
                if(firstLineSegment.Length<= PointOffsetDistance*2)
                {
                    continue;
                }
                var pts = BuildOffsetPoints(firstLineSegment, PointOffsetDistance);
                if (pts.Count != 2)
                {
                    continue;
                }
                Vector3d? extendVec = LineExtendVector(innerPolyline, pts[0], pts[0].GetVectorTo(pts[1]));
                if (extendVec == null)
                {
                    continue;
                }
                Point3d firstProjectPt = outerPolyline.GetClosestPointTo(pts[0], false);
                Point3d secondProjectPt = outerPolyline.GetClosestPointTo(pts[1], false);
                for (int j = 0; j < outerPolyline.NumberOfVertices; j++)
                {
                    var secondLineSegment = outerPolyline.GetLineSegmentAt(j);
                    if (secondLineSegment.Length <= PointOffsetDistance * 2)
                    {
                        continue;
                    }
                    if (IsIn(secondLineSegment, firstProjectPt) &&
                        IsIn(secondLineSegment, secondProjectPt))
                    {
                        splitSegments.Add(Tuple.Create(i, pts[0], pts[1]));
                        splitSegments.Add(Tuple.Create(j, firstProjectPt, secondProjectPt));
                        lines.Add(new Line(pts[0], firstProjectPt));
                        lines.Add(new Line(pts[1], secondProjectPt));
                        break;
                    }
                }
                if (splitSegments.Count > 0)
                {
                    break;
                }
            }
            if (splitSegments.Count == 2)
            {
                lines.AddRange(GetLines(innerPolyline, splitSegments[0]));
                lines.AddRange(GetLines(outerPolyline, splitSegments[1]));
            }
            List<Line> extendLines = new List<Line>();
            lines.ForEach(o =>
            {
                Point3d sp = o.StartPoint - o.LineDirection().MultiplyBy(LineExtendDistance);
                Point3d ep = o.EndPoint + o.LineDirection().MultiplyBy(LineExtendDistance);
                extendLines.Add(new Line(sp, ep));
            });
            lines.ForEach(o =>o.Dispose());
            var mergelines = ThLineMerger.Merge(extendLines);
            List<Polyline> polygonPolyines = new List<Polyline>();
            if (mergelines.Count>0)
            {
                DBObjectCollection dbObjs = new DBObjectCollection();
                mergelines.ForEach(o => dbObjs.Add(o));
                var unionObjs = dbObjs.Polygonize();                
                unionObjs.ForEach(o =>
                {
                    if (o is Polygon polygon)
                    {
                        polygonPolyines.Add(polygon.Shell.ToDbPolyline());
                    }
                });
            }
            return polygonPolyines.Count > 0 ? polygonPolyines.OrderByDescending(o => o.Area).First() : null; 
        }
        private Vector3d? LineExtendVector(Polyline self,Point3d pt,Vector3d ptOnLineVec)
        {
            var perpendVec = ptOnLineVec.GetPerpendicularVector().GetNormal();
            Point3d extendPt1 = pt - perpendVec.MultiplyBy(1e6);
            Point3d extendPt2 = pt + perpendVec.MultiplyBy(1e6);
            if(IntsertPoints(pt, extendPt1, self).Count==1)
            {
                return pt.GetVectorTo(extendPt1).GetNormal();
            }
            else if(IntsertPoints(pt, extendPt2, self).Count == 1)
            {
                return pt.GetVectorTo(extendPt2).GetNormal();
            }
            else
            {
                return null;
            }
        }
        private Point3dCollection IntsertPoints(Point3d basePt,Point3d extendPt, Polyline polyline)
        {
            Point3dCollection intersectPts = new Point3dCollection();
            Line extendLine = new Line(basePt, extendPt);
            extendLine.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            return intersectPts;
        }
        private List<Line> GetLines(Polyline polyline , Tuple<int, Point3d, Point3d> segmentItem)
        {
            List<Line> lines = new List<Line>();
            for(int i=0;i<polyline.NumberOfVertices;i++)
            {
                var lineSegment = polyline.GetLineSegmentAt(i);
                if (i != segmentItem.Item1)
                {
                    lines.Add(new Line(lineSegment.StartPoint, lineSegment.EndPoint));
                }
                else
                {
                    if(lineSegment.StartPoint.DistanceTo(segmentItem.Item2)<
                        lineSegment.StartPoint.DistanceTo(segmentItem.Item3))
                    {
                        lines.Add(new Line(lineSegment.StartPoint, segmentItem.Item2));
                        lines.Add(new Line(segmentItem.Item3, lineSegment.EndPoint));
                    }
                    else
                    {
                        lines.Add(new Line(lineSegment.StartPoint, segmentItem.Item3));
                        lines.Add(new Line(segmentItem.Item2, lineSegment.EndPoint));
                    }
                }
            }
            return lines.Where(o=>o.Length>0).ToList();
        }
        private List<Point3d> BuildOffsetPoints(LineSegment3d lineSegment, double offsetDis)
        {
            List<Point3d> offsetPts = new List<Point3d>();
            Point3d midPt = ThGeometryTool.GetMidPt(lineSegment.StartPoint, lineSegment.EndPoint);
            Point3d firstPt = midPt + midPt.GetVectorTo(lineSegment.StartPoint).GetNormal().MultiplyBy(offsetDis);
            Point3d secondPt = midPt + midPt.GetVectorTo(lineSegment.EndPoint).GetNormal().MultiplyBy(offsetDis);
            if(IsIn(lineSegment, firstPt))
            {
                offsetPts.Add(firstPt);
            }
            if (IsIn(lineSegment, secondPt))
            {
                offsetPts.Add(secondPt);
            }            
            return offsetPts;
        }
        private bool IsIn(LineSegment3d lineSegment,Point3d pt,double tolerance=1.0)
        {
            return ThGeometryTool.IsPointInLine(lineSegment.StartPoint, lineSegment.EndPoint, pt, tolerance);
        }
    }
}
