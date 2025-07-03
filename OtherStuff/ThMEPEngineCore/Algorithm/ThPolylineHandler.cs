using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeometryExtensions;
using System.Collections.Generic;

namespace ThMEPEngineCore.CAD
{
    public class ThPolylineHandler
    {
        private static double Height = 1.0; 
        public static Polyline Handle(Polyline polyline)
        {
            var result = HandleWhole(polyline);
            return HandleLast(result);
        }
        private static Polyline HandleWhole(Polyline polyline)
        {
            var segments = new PolylineSegmentCollection();
            for (int i=0;i<polyline.NumberOfVertices;i++)
            {
                var segmentType = polyline.GetSegmentType(i);
                if(segmentType == SegmentType.Arc)
                {
                    var arcSegment = polyline.GetArcSegment2dAt(i);                   
                    segments.Add(new PolylineSegment(arcSegment));
                    continue;
                }
                else if(segmentType == SegmentType.Point)
                {
                    continue;
                }
                else if(segmentType == SegmentType.Line)
                {
                    var firstSegment = polyline.GetLineSegmentAt(i);
                    var collinearGroups = new List<Line>();
                    collinearGroups.Add(new Line(firstSegment.StartPoint, firstSegment.EndPoint));
                    int j = i + 1;
                    for (; j < polyline.NumberOfVertices; j++)
                    {
                        var nextSegmentType = polyline.GetSegmentType(j);
                        if(nextSegmentType == SegmentType.Arc)
                        {
                            break;
                        }
                        else if (nextSegmentType == SegmentType.Line)
                        {
                            var nextSegment = polyline.GetLineSegmentAt(j);
                            if(nextSegment.Length==0.0)
                            {
                                continue;
                            }
                            var last = collinearGroups[collinearGroups.Count-1];                            
                            if (IsMerge(last.StartPoint,last.EndPoint, nextSegment.StartPoint, nextSegment.EndPoint, Height))
                            {
                                collinearGroups.Add(new Line(nextSegment.StartPoint, nextSegment.EndPoint));
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if(nextSegmentType == SegmentType.Coincident)
                        {
                            var nextSegment = polyline.GetLineSegmentAt(j);
                            if (nextSegment.Length == 0.0)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if(nextSegmentType == SegmentType.Point)
                        {
                            continue;
                        }
                        else if (nextSegmentType == SegmentType.Empty)
                        {
                            throw new NotSupportedException();
                        }
                    }
                    i = j - 1;
                    if (collinearGroups.Count > 1)
                    {
                        var mergeLine = Merge(firstSegment.StartPoint, collinearGroups);
                        segments.Add(new PolylineSegment(mergeLine.StartPoint.ToPoint2D(), mergeLine.EndPoint.ToPoint2D()));
                    }
                    else
                    {
                        segments.Add(new PolylineSegment(firstSegment.StartPoint.ToPoint2D(), firstSegment.EndPoint.ToPoint2D()));
                    }
                }
            }
            return segments.ToPolyline();
        }

        private static Polyline HandleLast(Polyline polyline)
        {
            var firstIndex = GetFirstSegmentIndex(polyline);
            var lastIndex = GetLastSegmentIndex(polyline);
            if(firstIndex==-1 || lastIndex==-1)
            {
                return polyline;
            }
            if(firstIndex== lastIndex)
            {
                return polyline;
            }
            var firstSegment = polyline.GetLineSegmentAt(firstIndex);
            var lastSegment = polyline.GetLineSegmentAt(lastIndex);
            if (!IsMerge(firstSegment.StartPoint, firstSegment.EndPoint, lastSegment.StartPoint, lastSegment.EndPoint,Height))
            {
                return polyline;
            }
            var maxPts = FindMaxPts(lastSegment.StartPoint, lastSegment.EndPoint, firstSegment.StartPoint, firstSegment.EndPoint);
            var segments = new PolylineSegmentCollection();
            for (int i = firstIndex + 1; i < lastIndex; i++)
            {
                var segmentType = polyline.GetSegmentType(i);
                if (segmentType == SegmentType.Arc)
                {
                    var arcSegment = polyline.GetArcSegment2dAt(i);
                    segments.Add(new PolylineSegment(arcSegment));
                }
                else if (segmentType == SegmentType.Point)
                {
                    continue;
                }
                else if (segmentType == SegmentType.Line)
                {
                    var lineSegment = polyline.GetLineSegmentAt(i);
                    segments.Add(new PolylineSegment(lineSegment.StartPoint.ToPoint2D(), lineSegment.EndPoint.ToPoint2D()));
                }
            }
            segments.Add(new PolylineSegment(maxPts.Item1.ToPoint2D(), maxPts.Item2.ToPoint2D()));
            return segments.ToPolyline();
        }

        private static Tuple<Point3d, Point3d> FindMaxPts(Point3d firstSp, Point3d firstEp, Point3d secondSp, Point3d secondEp)
        {
            var start = firstSp.DistanceTo(secondSp) > firstEp.DistanceTo(secondSp) ? firstSp : firstEp;
            var end = start.DistanceTo(secondEp) > start.DistanceTo(secondSp) ? secondEp : secondSp;
            return Tuple.Create(start, end);
        }

        private static int GetLastSegmentIndex(Polyline polyline)
        {
            var lastIndex = -1;
            for (int i = polyline.NumberOfVertices - 1; i >= 0; i--)
            {
                var segmentType = polyline.GetSegmentType(i);
                if (segmentType == SegmentType.Point)
                {
                    continue;
                }
                else if (segmentType == SegmentType.Arc)
                {
                    return lastIndex;
                }
                else if (segmentType == SegmentType.Line)
                {
                    lastIndex = i;
                    break;
                }
            }
            return lastIndex;
        }
        private static int GetFirstSegmentIndex(Polyline polyline)
        {
            var firstIndex = -1;
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var segmentType = polyline.GetSegmentType(i);
                if (segmentType == SegmentType.Point)
                {
                    continue;
                }
                else if (segmentType == SegmentType.Arc)
                {
                    return firstIndex;
                }
                else if (segmentType == SegmentType.Line)
                {
                    firstIndex = i;
                    break;
                }
            }
            return firstIndex;
        }
        private static bool IsMerge(Point3d firstSp,Point3d firstEp,Point3d secondSp,Point3d secondEp,double height)
        {
            //不处理自交多段线
            var spProjectPt = secondSp.GetProjectPtOnLine(firstSp, firstEp);
            var epProjectPt = secondEp.GetProjectPtOnLine(firstSp, firstEp);
            return 
                spProjectPt.DistanceTo(secondSp) <= height &&
                epProjectPt.DistanceTo(secondEp) <= height &&
                (IsIn(firstSp, firstEp, spProjectPt,1.0) ^ IsIn(firstSp, firstEp, epProjectPt, 1.0))
                ;
        }
        private static bool IsIn(Point3d lineSp,Point3d lineEp,Point3d projectPt,double tolerance=1.0)
        {
            var length = lineSp.DistanceTo(lineEp);
            var half1 = lineSp.DistanceTo(projectPt);
            var half2 = lineEp.DistanceTo(projectPt);

            return Math.Abs(half1 + half2 - length) <= tolerance;
        }
        private static Line Merge(Point3d startPt, List<Line> segments)
        {
            var targetPt = startPt;
            for (int i =0;i< segments.Count;i++)
            {
                var segment = segments[i];
                if(segment.EndPoint.DistanceTo(targetPt) > segment.StartPoint.DistanceTo(targetPt))
                {
                    targetPt = segment.EndPoint;
                }
                else
                {
                    targetPt = segment.StartPoint;
                }
            }
            return new Line(startPt, targetPt);
        }
    }
}
