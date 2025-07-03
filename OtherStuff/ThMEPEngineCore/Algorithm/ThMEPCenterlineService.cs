using System;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPCenterlineService
    {
        public static List<Line> CenterLineExtraction(Polyline polyline, double distanceTolerance)
        {
            // TODO
            // 指定UCS->以便走道正交化

            // 简化polygon
            var pline = ThCADCoreNTSSimplify.TPSimplify(polyline, distanceTolerance);

            // 初步绘制中心线
            var objs = ThCADCoreNTSCenterlineBuilder.Centerline(pline, distanceTolerance);
            var lines = new List<Line>();
            foreach (Curve curve in objs)
            {
                const double lengthThreshold = 1.0;
                if (curve is Line line)
                {
                    if (line.Length > lengthThreshold && !lines.Contains(line))
                    {
                        lines.Add(line.WashClone() as Line);
                    }
                }
                else if (curve is Polyline poly)
                {
                    var entitySet = new DBObjectCollection();
                    poly.Explode(entitySet);
                    foreach (var item in entitySet)
                    {
                        if (item is Line l)
                        {
                            if (l.Length > lengthThreshold && !lines.Contains(l))
                            {
                                lines.Add(l.WashClone() as Line);
                            }
                        }
                    }
                }
            }

            // 删除重复线段
            var mergedLines = ThMEPLineExtension.LineMerge(lines, 1.0, 1.0, Math.PI / 180.0);

            // 找到叉点及端点（需延伸至边缘的点），并连接叉点
            var connection = ConnectionSimplify(mergedLines);

            // 找到需要延伸的端点（需要延伸至与边界相交的点）
            var edgePoints = new List<Point3d>();
            var AllPts = new List<Point3d>();
            connection.ForEach(x =>
            {
                AllPts.Add(x.StartPoint);
                AllPts.Add(x.EndPoint);
            });
            var forkpts = new Dictionary<Point3d, int>();
            AllPts.GroupBy(o => o).ToList().ForEach(o => forkpts.Add(o.Key, o.Count()));
            forkpts.Where(x => x.Value == 1).ToList().ForEach(y => edgePoints.Add(y.Key));

            // 中心线正交化
            // TODO
            return connection;
        }

        // 找到所有交叉点及其连接关系
        private static List<Line> ConnectionSimplify(List<Line> lines)
        {
            var ForkPointRelationship = new List<Tuple<Point3d, Point3d>>();
            var pts = new Dictionary<Point3d, int>();
            var AllPts = new List<Point3d>();
            lines.ForEach(o =>
            {
                AllPts.Add(o.StartPoint);
                AllPts.Add(o.EndPoint);
            });
            var groups = AllPts.GroupBy(o => o).ToList();
            groups.ForEach(o => pts.Add(o.Key, o.Count()));

            // 找出所有交叉点
            var forkPoints = new List<Point3d>();
            pts.Where(o => o.Value == 3).ToList().ForEach(x => forkPoints.Add(x.Key));
            var line_clone = new List<Line>();
            lines.ForEach(o => line_clone.Add(o));

            // 剪枝
            pts.Where(o => o.Value == 1).ToList().ForEach(pt =>
            {
                var branch = SplitBranch(pt.Key, lines, forkPoints);
                branch.Item1.ForEach(x =>
                {
                    line_clone.Remove(x);
                    AllPts.Remove(x.StartPoint);
                    AllPts.Remove(x.EndPoint);
                });
            });

            // 确定连接关系
            while (AllPts.Count != 0)
            {
                var points = new Dictionary<Point3d, int>();
                var gps = AllPts.GroupBy(o => o).ToList();
                gps.ForEach(o => points.Add(o.Key, o.Count()));
                foreach (var pt in points)
                {
                    if (pt.Value == 1 && AllPts.Count != 0)
                    {
                        var connection = SplitBranch(pt.Key, line_clone, forkPoints);
                        ForkPointRelationship.Add(Tuple.Create(pt.Key, connection.Item2));
                        connection.Item1.ForEach(x =>
                        {
                            line_clone.Remove(x);
                            AllPts.Remove(x.StartPoint);
                            AllPts.Remove(x.EndPoint);
                        });
                    }
                }
            }

            // 去重
            var results = new List<Line>();
            ForkPointRelationship.ForEach(o =>
            {
                var line = new Line(o.Item1, o.Item2);
                var line_reverse = new Line(o.Item2, o.Item1);
                if (!(results.Contains(line) || results.Contains(line_reverse)))
                {
                    results.Add(line);
                }
            });
            return results;
        }

        private static Tuple<List<Line>, Point3d> SplitBranch(Point3d point, List<Line> lines, List<Point3d> pts)
        {
            var lineBranch = new List<Line>();
            var startline = lines.FindAll(o => o.StartPoint == point || o.EndPoint == point);
            if (startline.Count != 1)
            {
                throw new NotSupportedException();
            }
            var anotherPt = (startline.First().StartPoint == point) ? startline.First().EndPoint : startline.First().StartPoint;
            if (pts.Contains(anotherPt))
            {
                return Tuple.Create(startline, anotherPt);
            }
            else
            {
                while (!pts.Contains(anotherPt))
                {
                    var sLine = startline.First();
                    anotherPt = (sLine.StartPoint == point) ? sLine.EndPoint : sLine.StartPoint;
                    startline = lines.FindAll(o => o.StartPoint == anotherPt || o.EndPoint == anotherPt);
                    startline.Remove(sLine);
                    lineBranch.Add(sLine);
                    point = anotherPt;
                }
                return Tuple.Create(lineBranch, point);
            }

        }
    }
}
