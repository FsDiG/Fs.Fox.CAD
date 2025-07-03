using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThCADExtension;
using GeometryExtensions;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.CAD
{
    public static class ThGeometryTool
    {
        /// <summary>
        /// 计算最大最小点
        /// </summary>
        /// <returns></returns>
        public static List<Point3d> CalBoundingBox(List<Point3d> points)
        {
            double maxX = points.Max(x => x.X);
            double minX = points.Min(x => x.X);
            double maxY = points.Max(x => x.Y);
            double minY = points.Min(x => x.Y);

            return new List<Point3d>(){                
                new Point3d(minX, minY, 0),
                new Point3d(maxX, maxY, 0)
            };
        }
        public static bool IsParallelToEx(this Vector3d vector, Vector3d other)
        {
            double angle = vector.GetAngleTo(other) / Math.PI * 180.0;
            return (angle < ThMEPEngineCoreCommon.LOOSE_PARALLEL_ANGLE) || ((180.0 - angle) < ThMEPEngineCoreCommon.LOOSE_PARALLEL_ANGLE);
        }
        public static bool IsParallelToEx(this Line first, Line second)
        {
            var firstVec = first.LineDirection();
            var secondVec = second.LineDirection();
            return firstVec.IsParallelToEx(secondVec);
        }
        public static bool IsCollinearEx(Point3d firstSp, Point3d firstEp,
            Point3d secondSp, Point3d secondEp, double tolerance = 1.0)
        {
            return
                IsCollinearEx(firstSp, firstEp, secondSp, tolerance) &&
                IsCollinearEx(firstSp, firstEp, secondEp, tolerance);
        }
        public static bool IsCollinearEx(Point3d firstPt, Point3d secondPt,
            Point3d thirdPt, double tolerance = 1.0)
        {
            Vector3d firstVec = firstPt.GetVectorTo(secondPt);
            Vector3d secondVec = firstPt.GetVectorTo(thirdPt);
            return firstVec.CrossProduct(secondVec).Length <= firstVec.Length * tolerance;
        }
        public static bool IsOverlapEx(Point3d firstSp, Point3d firstEp,
            Point3d secondSp, Point3d secondEp)
        {
            if(IsCollinearEx(firstSp, firstEp, secondSp, secondEp))
            {
                List<Point3d> pts = new List<Point3d> { firstSp, firstEp, secondSp, secondEp };
                var maxPts= GetCollinearMaxPts(pts);
                return (firstSp.DistanceTo(firstEp) + secondSp.DistanceTo(secondEp)) >
                    maxPts.Item1.DistanceTo(maxPts.Item2);
            }
            return false;
        }
        public static Point3d GetMidPt(this Point3d pt1, Point3d pt2)
        {
            return pt1 + pt1.GetVectorTo(pt2) * 0.5;
        }
        public static Point3d GetProjectPtOnLine(this Point3d outerPt, Point3d startPt,Point3d endPt)
        {
            Vector3d firstVec = startPt.GetVectorTo(endPt);
            Vector3d secondVec = startPt.GetVectorTo(outerPt);
            double angle = firstVec.GetAngleTo(secondVec);
            double distance = Math.Cos(angle) * secondVec.Length;
            return startPt + firstVec.GetNormal().MultiplyBy(distance);
        }
        public static bool IsIntersects(this Entity firstEnt, Entity secondEnt)
        {            
            return firstEnt.IntersectWithEx(secondEnt).Count > 0 ? true : false;
        }
        public static Point3dCollection IntersectWithEx(this Entity firstEntity, Entity secondEntity)
        {
            Point3dCollection pts = new Point3dCollection();
            Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            firstEntity.IntersectWith(secondEntity, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);
            plane.Dispose();
            return pts;
        }
        public static bool IsPointOnLine(Point3d lineSp,Point3d lineEp,Point3d outerPt,double tolerance=0.0001)
        {
            Vector3d vec = lineSp.GetVectorTo(lineEp);
            Plane plane = new Plane(lineSp, vec);
            Matrix3d wcsToUcs = Matrix3d.WorldToPlane(plane);
            Point3d newPt=outerPt.TransformBy(wcsToUcs);
            if(Math.Abs(newPt.X)<= tolerance && Math.Abs(newPt.Y) <= tolerance)
            {
                if(newPt.Z>=0 && newPt.Z<= lineSp.DistanceTo(lineEp))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsPointInLine(Point3d lineSp, Point3d lineEp, Point3d outerPt,double tolerance=0.0)
        {
            Vector3d vec = lineSp.GetVectorTo(lineEp).GetNormal();
            Point3d sp = lineSp + vec.MultiplyBy(tolerance);
            Point3d ep = lineEp - vec.MultiplyBy(tolerance);
            return IsPointOnLine(sp, ep, outerPt);
        }
        public static bool IsProjectionPtInLine(Point3d lineSp, Point3d lineEp, Point3d outerPt)
        {
            Vector3d vec = lineSp.GetVectorTo(lineEp);
            Plane plane = new Plane(lineSp, vec);
            Matrix3d wcsToUcs = Matrix3d.WorldToPlane(plane);
            Point3d newPt = outerPt.TransformBy(wcsToUcs);
            return newPt.Z >= 0 && newPt.Z <= lineSp.DistanceTo(lineEp);
        }
        public static Polyline TextOBB(this DBText dBText)
        {
            Matrix3d clockwiseMat = Matrix3d.Rotation(-1.0 * dBText.Rotation, Vector3d.ZAxis, dBText.Position);
            DBText newText = dBText.GetTransformedCopy(clockwiseMat) as DBText;
            Polyline obb = newText.GeometricExtents.ToRectangle();
            Matrix3d counterClockwiseMat = Matrix3d.Rotation(dBText.Rotation, Vector3d.ZAxis, dBText.Position);
            obb.TransformBy(counterClockwiseMat);
            return obb;
        }
        public static Polyline TextOBB(this MText mText)
        {
            Matrix3d clockwiseMat = Matrix3d.Rotation(-1.0 * mText.Rotation, Vector3d.ZAxis, mText.Location);
            var newText = mText.GetTransformedCopy(clockwiseMat) as MText;
            Polyline obb = newText.GeometricExtents.ToRectangle();
            Matrix3d counterClockwiseMat = Matrix3d.Rotation(mText.Rotation, Vector3d.ZAxis, mText.Location);
            obb.TransformBy(counterClockwiseMat);
            return obb;
        }
        public static Point3dCollection EntityVertices(this Entity entity)
        {
            // 暂不支持弧
            Point3dCollection pts = new Point3dCollection();
            if (entity is Polyline polyline)
            {
                return polyline.Vertices();
            }
            else if(entity is Line line)
            {
                pts.Add(line.StartPoint);
                pts.Add(line.EndPoint);
            }
            else if (entity is Arc arc)
            {
                return arc.ToPolyline().Vertices();
            }
            else if(entity is MPolygon mPolygon)
            {
                pts = mPolygon.Vertices();
            }
            else
            {
                throw new NotSupportedException();
            }
            return pts;
        }        
        public static bool IsPerpendicular(Vector3d firstVec,Vector3d secondVec,double tolerance=1.0)
        {
            double rad = firstVec.GetAngleTo(secondVec);
            double ang = rad / Math.PI * 180.0;
            return Math.Abs(ang-90.0)<= tolerance;
        }
        public static double ProjectionDis(this Vector3d a,Vector3d b)
        {
            double rad = a.GetAngleTo(b);
            return b.Length * Math.Cos(rad);
        }
        public static Point3dCollection IntersectPts(
            Line first,Line second,Intersect intersectType,double distance=1.0)
        {
            var pts = new Point3dCollection();
            var firstVec = first.StartPoint.GetVectorTo(first.EndPoint).GetNormal();
            var secondVec = second.StartPoint.GetVectorTo(second.EndPoint).GetNormal();

            var firstSp = first.StartPoint - firstVec.MultiplyBy(distance);
            var firstEp = first.EndPoint + firstVec.MultiplyBy(distance);

            var secondSp = second.StartPoint - secondVec.MultiplyBy(distance);
            var secondEp = second.EndPoint + secondVec.MultiplyBy(distance);

            var firstNew = new Line(firstSp, firstEp);
            var secondNew = new Line(secondSp, secondEp);

            firstNew.IntersectWith(secondNew, intersectType, pts, IntPtr.Zero, IntPtr.Zero);
            return pts;
        }
        public static Tuple<Point3d, Point3d> GetCollinearMaxPts(this List<Line> lines)
        {
            //传入的线要共线
            var pts = new List<Point3d>();
            lines.ForEach(o =>
            {
                pts.Add(o.StartPoint);
                pts.Add(o.EndPoint);
            });
            return GetCollinearMaxPts(pts);
        }
        public static Tuple<Point3d,Point3d> GetCollinearMaxPts(this List<Point3d> pts)
        {
            if (pts.Count == 0)
            {
                return Tuple.Create(Point3d.Origin, Point3d.Origin);
            }
            else if (pts.Count == 1)
            {
                return Tuple.Create(pts[0], pts[0]);
            }
            else
            {
                Point3d first = pts[0];
                Point3d second = pts[pts.Count - 1];
                for (int i = 0; i < pts.Count - 1; i++)
                {
                    for (int j = i + 1; j < pts.Count; j++)
                    {
                        if (pts[i].DistanceTo(pts[j]) > first.DistanceTo(second))
                        {
                            first = pts[i];
                            second = pts[j];
                        }
                    }
                }
                return Tuple.Create(first, second);
            }
        }
        public static bool IsOverlap(Point3d firstSp, Point3d firstEp, 
            Point3d secondSp, Point3d secondEp,bool includedJoin=true)
        {
            //第二根线在第一根线上的投影是否重叠
            Vector3d vec = firstSp.GetVectorTo(firstEp).GetNormal();
            Plane plane = new Plane(firstSp, vec);
            Matrix3d wcsToUcs = Matrix3d.WorldToPlane(plane);
            Point3d newSecondSp = secondSp.TransformBy(wcsToUcs);
            Point3d newSecondEp = secondEp.TransformBy(wcsToUcs);
            double minZ = Math.Min(newSecondSp.Z, newSecondEp.Z);
            double maxZ = Math.Max(newSecondSp.Z, newSecondEp.Z);
            if(includedJoin)
            {
                if (maxZ < 0)
                {
                    return false;
                }
                if (minZ > firstSp.DistanceTo(firstEp))
                {
                    return false;
                }
            }
            else
            {
                if (maxZ <= 0)
                {
                    return false;
                }
                if (minZ >= firstSp.DistanceTo(firstEp))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsOverlap(this Line first ,Line second, bool includedJoin = true)
        {
            return IsOverlap(first.StartPoint, first.EndPoint, 
                second.StartPoint, second.EndPoint, includedJoin);
        }
        public static bool IsPointOnPolyline(this Point3d pt,Polyline polyline,double tolerance=0.0001)
        {
            for(int i=0;i<polyline.NumberOfVertices;i++)
            {
               var segmentType = polyline.GetSegmentType(i);
                if(segmentType==SegmentType.Line)
                {
                   var lineSegment = polyline.GetLineSegmentAt(i);
                    if (IsPointOnLine(lineSegment.StartPoint, lineSegment.EndPoint, pt))
                    {
                        return true;
                    }                    
                }
                else if(segmentType == SegmentType.Arc)
                {
                    var arcSegment = polyline.GetArcSegmentAt(i);
                    Arc arc = new Arc(arcSegment.Center, arcSegment.Normal, 
                        arcSegment.Radius, arcSegment.StartAngle, arcSegment.EndAngle);
                    if (pt.IsPointOnArc(arc))
                    {
                        return true;
                    }
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        public static bool IsPointOnLine(this Point3d pt, Line line, double tolerance = 0.0001)
        {
            return IsPointOnLine(line.StartPoint, line.EndPoint, pt, tolerance);
        }
        public static bool IsPointOnArc(this Point3d pt, Arc arc, double tolerance = 0.0001)
        {
            if (Math.Abs(pt.DistanceTo(arc.Center) - arc.Radius) <= tolerance)
            {
                var vec = arc.Center.GetVectorTo(pt);
                var ang = vec.GetAngleTo(Vector3d.XAxis, arc.Normal);
                return ang >= arc.StartAngle && ang <= arc.EndAngle;
            }
            return false;
        }
    }
}
