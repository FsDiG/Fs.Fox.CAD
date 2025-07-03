using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Algorithm
{
    public class ThLineMerger
    {
        private List<Line> Lines { get; set; }
        public List<Line> Results { get; private set; }
        private List<Tuple<Point3d, Point3d, List<Line>>> LineGroups { get; set; }
        private ThLineMerger(List<Line> lines)
        {
            Lines = lines;
            Results = new List<Line>();
            LineGroups = new List<Tuple<Point3d, Point3d, List<Line>>>();
        }
        public static List<Line> Merge(List<Line> lines)
        {
            var instance = new ThLineMerger(lines);
            instance.Merge();
            return instance.Results;
        }
        private void Merge()
        {
            Lines.ForEach(o => ToGroup(o));
            LineGroups.ForEach(o =>
            {
                Results.Add(new Line(o.Item1,o.Item2));
            });
        }
        private void ToGroup(Line line)
        {
            var res=LineGroups.Where(o =>
            {
                if ((line.EndPoint - line.StartPoint).GetNormal().IsParallelTo((o.Item1 - o.Item2).GetNormal(), new Tolerance(1, 1)))
                {
                    if (line.EndPoint.IsEqualTo(o.Item1, new Tolerance(1, 1)) ||
                        line.StartPoint.IsEqualTo(o.Item1, new Tolerance(1, 1)) ||
                        line.EndPoint.IsEqualTo(o.Item2, new Tolerance(1, 1)) ||
                        line.StartPoint.IsEqualTo(o.Item2, new Tolerance(1, 1)))
                    {
                        return true;
                    }
                    return false;
                }
                //bool isOverlap = ThGeometryTool.IsOverlapEx(
                //   line.StartPoint, line.EndPoint, o.Item1, o.Item2);
                //if (!isOverlap)
                //{
                    
                //}
                return false;
            });
            if (!res.Any())
            {
                LineGroups.Add(Tuple.Create(line.StartPoint, line.EndPoint, new List<Line> { line }));
            }
            else
            {
                var item = res.First();
                LineGroups.Remove(item);
                item.Item3.Add(line);
                var newRangePts = BuildNewRangePoints(item.Item1, item.Item2, line);
                LineGroups.Add(Tuple.Create(newRangePts.Item1, newRangePts.Item2, item.Item3));
            }
        }
        private bool IsJointLink(Point3d firstSp,Point3d firstEp,Point3d secondSp,Point3d secondEp,double tolerance=0.1)
        {
            if (ThGeometryTool.IsCollinearEx(firstSp, firstEp, secondSp, secondEp))
            {
                List<Point3d> pts = new List<Point3d> { firstEp, secondSp, secondEp };
                double maxDistance = pts.Select(o => o.DistanceTo(firstSp)).OrderByDescending(o => o).First();
                var firstLength = firstSp.DistanceTo(firstEp);
                var secondLength = secondSp.DistanceTo(secondEp);
                return Math.Abs(maxDistance - firstLength - secondLength) <= tolerance;
            }
            return false;
        }
        private Tuple<Point3d, Point3d> BuildNewRangePoints(Point3d oldSp, Point3d oldEp, Line insertLine)
        {
            Point3d newSp = oldSp;
            Point3d newEp = oldEp;
            Vector3d vec = oldSp.GetVectorTo(oldEp).GetNormal();
            Plane plane = new Plane(oldSp, vec);
            Matrix3d worldToPlane = Matrix3d.WorldToPlane(plane);          
            Point3d lineSp = insertLine.StartPoint.TransformBy(worldToPlane);
            Point3d lineEp = insertLine.EndPoint.TransformBy(worldToPlane);
            double minZ = Math.Min(lineSp.Z, lineEp.Z);
            double maxZ = Math.Max(lineSp.Z, lineEp.Z);
            double oldLength = oldSp.DistanceTo(oldEp);
            if (minZ<0.0)
            {
                newSp = newSp + vec.MultiplyBy(minZ);
            }
            if(maxZ> oldLength)
            {
                newEp = newEp + vec.MultiplyBy(maxZ- oldLength);
            }
            plane.Dispose();
            return Tuple.Create(newSp, newEp);
        }
    }
}
