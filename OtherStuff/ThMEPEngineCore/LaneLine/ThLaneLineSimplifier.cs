using System;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.LaneLine
{
    public class ThLaneLineSimplifier
    {
        public static List<Line> RemoveDangles(DBObjectCollection curves, double threshold)
        {
            // 将多段线炸成线段
            // 不考虑圆弧了，但是还存在有多段线的情况
            var objs = ExplodeCurves(curves);
            var lines = objs.Where(o => o is Line).Cast<Line>().ToList();
            lines = BreakLine(lines);
            lines = RMLine(lines, threshold);
            for (int i = 0; i < lines.Count; i++)
            {
                var index_s = new List<int>(FindCloseLinesToPoint(lines[i].StartPoint, lines, threshold));
                if (index_s.Count > 1)
                {
                    FindCentroidAndConn(lines, index_s);
                }

                var index_e = new List<int>(FindCloseLinesToPoint(lines[i].EndPoint, lines, threshold));
                if (index_e.Count > 1)
                {
                    FindCentroidAndConn(lines, index_e);
                }
            }
            ConnPointToLine(lines, threshold);

            return lines;
        }

        private static List<Line> BreakLine(List<Line> lines)
        {
            var linesTidal = new List<Line>();
            for (int i = 0; i < lines.Count; i++)
            {
                var linesTemp = new List<Line>(lines);
                linesTemp.RemoveAt(i);
                linesTidal.AddRange(ConnIntersectPoints(lines[i], linesTemp));
            }
            return linesTidal;
        }

        //找出所有相交点和端点，排序、去重、连接
        private static List<Line> ConnIntersectPoints(Line line, List<Line> lines)
        {
            var points = new List<Point3d>
            {
                line.StartPoint
            };
            for (int i = 0; i < lines.Count; i++)
            {
                var pointsTemp = new Point3dCollection();
                line.IntersectWith(lines[i], 0, pointsTemp, IntPtr.Zero, IntPtr.Zero);
                foreach (Point3d point in pointsTemp)
                {
                    points.Add(point);
                }
            }
            points.Add(line.EndPoint);

            points.Distinct();
            SortPoints(points);

            var linesTidal = new List<Line>();
            for (int j = 0; j < points.Count - 1; j++)
            {
                linesTidal.Add(new Line(points[j], points[j + 1]));
            }

            return linesTidal;
        }

        private static void SortPoints(List<Point3d> points)
        {
            int min;
            for (int i = 0; i < points.Count - 1; ++i)
            {
                min = i;
                for (int j = i + 1; j < points.Count; ++j)
                {
                    if ((points[j].X + points[j].Y) < (points[min].X + points[min].Y))
                        min = j;
                }
                var t = new Point3d(points[min].X, points[min].Y, points[min].Z);
                points[min] = points[i];
                points[i] = t;
            }
        }

        //删除长度小于threshold的线
        private static List<Line> RMLine(List<Line> lines, double threshold)
        {
            return lines.Where(o => o.Length > threshold).ToList();
        }

        //对点 查找阈值范围内的线，并用数组记录下来
        private static List<int> FindCloseLinesToPoint(Point3d point, List<Line> lines, double threshold)
        {
            var index = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (GetEndPointDistance(point, lines[i]) < threshold)
                {
                    index.Add(i);
                }
            }

            return index;
        }

        private static void FindCentroidAndConn(List<Line> lines, List<int> index)
        {
            Point3d point0;
            var points = new List<Point3d>();
            var flags = new List<bool>();
            var linesTidal = new List<Line>();
            double distance_s = GetEndPointDistance(lines[index[0]].StartPoint, lines[index[1]]);
            double distance_e = GetEndPointDistance(lines[index[0]].EndPoint, lines[index[1]]);

            if (distance_s < distance_e)
            {
                point0 = lines[index[0]].StartPoint;
                points.Add(lines[index[0]].EndPoint);
                flags.Add(false);
            }
            else
            {
                point0 = lines[index[0]].EndPoint;
                points.Add(lines[index[0]].StartPoint);
                flags.Add(true);
            }
            Point3d centroid = point0;

            for (int i = 1; i < index.Count; i++)
            {
                Point3d point = FindClosePoint(centroid, lines[index[i]], out Point3d other, out bool flag);
                centroid = new Point3d((point.X + centroid.X) / 2,
                                       (point.Y + centroid.Y) / 2,
                                       (point.Z + centroid.Z) / 2);
                points.Add(other);
                flags.Add(flag);
            }

            for (int j = 0; j < index.Count; j++)
            {
                if (flags[j])
                {
                    lines[index[j]] = new Line(points[j], centroid);

                }
                else
                {
                    lines[index[j]] = new Line(centroid, points[j]);
                }
            }
        }

        private static Point3d FindClosePoint(Point3d point, Line line, out Point3d other, out bool flag)
        {
            //表示点other是否是起点
            flag = false;
            Point3d result;
            double distance_s = point.DistanceTo(line.StartPoint);
            double distance_e = point.DistanceTo(line.EndPoint);

            if (distance_s < distance_e)
            {
                result = line.StartPoint;
                other = line.EndPoint;
            }
            else
            {
                result = line.EndPoint;
                other = line.StartPoint;
                flag = true;
            }

            return result;
        }

        //计算点到线和线的两个端点的最近距离
        private static double GetEndPointDistance(Point3d point, Line line)
        {
            double distance;
            double distance_s = point.DistanceTo(line.StartPoint);
            double distance_e = point.DistanceTo(line.EndPoint);

            if (distance_s < distance_e)
            {
                distance = distance_s;
            }
            else
            {
                distance = distance_e;
            }

            return distance;
        }

        private static void ConnPointToLine(List<Line> lines, double threshold)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines.Count; j++)
                {
                    var closePoint = new Point3d();
                    if (GetCloseDistance(lines[i].StartPoint, lines[j], threshold, out closePoint))
                    {
                        lines[i] = new Line(lines[i].EndPoint, closePoint);
                    }

                    if (GetCloseDistance(lines[i].EndPoint, lines[j], threshold, out closePoint))
                    {
                        lines[i] = new Line(lines[i].StartPoint, closePoint);
                    }
                }
            }
        }

        private static bool GetCloseDistance(Point3d point, Line line, double threshold, out Point3d closePoint)
        {
            closePoint = line.GetClosestPointTo(point, false);
            double distance = point.DistanceTo(closePoint);

            if (distance < threshold && distance > 0)
            {
                return true;
            }

            return false;
        }

        public static List<Line> Simplify(DBObjectCollection curves, double threshold)
        {
            // 炸开输入的车道线
            var objs = ExplodeCurves(curves);

            // 判断连接关系，并根据连接关系，对线和圆弧进行分组 group 及连接关系分类 catagory
            // 1-弧+线 2-弧+单弧 3-弧+多弧 (弧+null的情况已转换为弧+线)
            var correlationship = EstimateCorrelationship(objs, threshold);

            // 根据分类，进行简化处理 processing (无法处理的直接连接arc首尾，此时输出值无arc)
            // category = 1 : 弧的两端均连接直线 或 弧的一端连接直线，另一端连接null
            // category = 2 : 弧的某一端连接一条弧线
            // category = 3 : 弧的某一端连接多条弧线
            var processedlines = SimplifyByCategory(objs, correlationship);

            // 对于简化处理后的结果，做后处理 post-processing
            // 小区域封闭图形以及部分外伸线需要设计师手动剔除
            var lines_filtered = LineMerge(processedlines, Math.PI / 180.0);
            var postprocessedLines = PostProcessing(lines_filtered, Math.PI / 90.0);

            // 最后返回结果
            return postprocessedLines;
        }

        private static List<Curve> ExplodeCurves(DBObjectCollection curves)
        {
            var objs = new List<Curve>();
            foreach (Curve curve in curves)
            {
                if (curve is Line line && line.Length > 10.0)
                {
                    objs.Add(line.WashClone() as Line);
                }
                else if (curve is Arc arc && arc.Length > 10.0)
                {
                    objs.Add(arc.WashClone() as Arc);
                }
                else if (curve is Polyline polyline)
                {
                    var entitySet = new DBObjectCollection();
                    polyline.Explode(entitySet);
                    objs.AddRange(ExplodeCurves(entitySet));
                }
            }
            return objs;
        }

        private static List<Tuple<List<Curve>, int>> EstimateCorrelationship(List<Curve> curves, double threshold)
        {
            var correlationship = new List<Tuple<List<Curve>, int>>();
            var arcs = curves.FindAll(o => o is Arc);
            arcs.ForEach(x => {
                var result = FindConnection(x as Arc, curves, threshold);
                correlationship.Add(Tuple.Create(result.Item1, result.Item2));
                curves = result.Item3;
            });
            return correlationship;
        }

        private static List<Line> SimplifyByCategory(List<Curve> curves, List<Tuple<List<Curve>, int>> lists)
        {
            var simplifiedLines = new List<Line>();

            var lists_index = new List<Tuple<List<int>, int>>();
            //如果是新构造的直线，则在curves中新增直线，并以索引寻找line/arc
            lists.ForEach(t =>
            {
                var list_int = new List<int>();
                t.Item1.ForEach(l =>
                {
                    if (!curves.Contains(l))
                    {
                        curves.Add(l);
                    }
                    list_int.Add(curves.IndexOf(l));
                });
                lists_index.Add(Tuple.Create(list_int, t.Item2));
            });

            lists_index.ForEach(tuple =>
            {
                switch (tuple.Item2)
                {
                    // 一条弧线两段连接的均为直线
                    case 1:
                    case 2:
                        if (tuple.Item1.Count != 2)
                        {
                            throw new NotSupportedException();
                        }
                        else
                        {
                            var intersection = IntersectLines(curves[tuple.Item1[0]] as Line, curves[tuple.Item1[1]] as Line);
                            curves[tuple.Item1[0]] = intersection.Item1;
                            curves[tuple.Item1[1]] = intersection.Item2;
                        }
                        break;
                    case 3:
                        for (int i = 0; i < tuple.Item1.Count - 1; i++)
                        {
                            for (int j = i + 1; j < tuple.Item1.Count; j++)
                            {
                                if (!IsLooseCollinear(curves[tuple.Item1[i]] as Line, curves[tuple.Item1[j]] as Line, Math.PI / 90.0))
                                {
                                    var intersection = IntersectLines(curves[tuple.Item1[i]] as Line, curves[tuple.Item1[j]] as Line);
                                    curves[tuple.Item1[0]] = intersection.Item1;
                                    curves[tuple.Item1[1]] = intersection.Item2;
                                }
                            }
                        }
                        break;
                    default:
                        tuple.Item1.ForEach(index =>
                        {
                            curves[index] = (curves[index] is Arc) ? new Line(curves[index].StartPoint, curves[index].EndPoint) : curves[index];
                        });
                        break;
                }
            });
            curves.ForEach(o => { if (o is Line line && line.Length > 10.0) simplifiedLines.Add(line); });
            return simplifiedLines;
        }

        private static List<Line> PostProcessing(List<Line> lines, double tolerance)
        {
            // 如果某条车道线两端相连的直线也相连，则删除该车道线
            var line_axial = new List<Line>();
            var line_inclined = new List<Line>();
            foreach (var line in lines)
            {
                var angle_x = line.LineDirection().ToVector2d().GetAngleTo(new Vector2d(1.0, 0.0));
                angle_x = Math.Min(angle_x, Math.PI - angle_x);
                var angle_y = line.LineDirection().ToVector2d().GetAngleTo(new Vector2d(0.0, 1.0));
                angle_y = Math.Min(angle_y, Math.PI - angle_y);
                if (angle_x < Math.PI / 18.0 || angle_y < Math.PI / 18.0)
                {
                    line_axial.Add(line);
                }
                else
                {
                    line_inclined.Add(line);
                }
            }
            foreach (var item in line_inclined)
            {
                bool flag = false;
                var line_start = lines.FindAll(o => (!IsLooseCollinear(o, item, tolerance)) && o.Intersection(item, Intersect.OnBothOperands) != null &&
                o.Intersection(item, Intersect.OnBothOperands).ToAcGePoint3d().DistanceTo(item.StartPoint) < 1.0);
                var line_end = lines.FindAll(o => (!IsLooseCollinear(o, item, tolerance)) && o.Intersection(item, Intersect.OnBothOperands) != null &&
                o.Intersection(item, Intersect.OnBothOperands).ToAcGePoint3d().DistanceTo(item.EndPoint) < 1.0);
                line_start.Remove(item);
                line_end.Remove(item);
                if (line_start != null && line_end != null)
                {
                    foreach (var ls in line_start)
                    {
                        foreach (var le in line_end)
                        {
                            if (!IsLooseCollinear(ls, le, tolerance))
                            {
                                var intersect_pt = ls.Intersection(le, Intersect.OnBothOperands);
                                if (intersect_pt != null)
                                {
                                    var PT = intersect_pt.ToAcGePoint3d();
                                    flag = (PT.DistanceTo(ls.GetClosestPointTo(PT, false)) < 1.0 && PT.DistanceTo(le.GetClosestPointTo(PT, false)) < 1.0) ? true : false;
                                }
                            }
                        }
                    }
                }
                if (flag) lines.Remove(item);
            }
            return lines;
        }

        private static Tuple<List<Curve>, int, List<Curve>> FindConnection(Arc arc, List<Curve> curves, double threshold)
        {
            var connection = new List<Curve>();
            // 找startpoint的连接关系
            var candidates_s = curves.FindAll(o => arc.StartPoint.DistanceTo(o.GetClosestPointTo(arc.StartPoint, false)) < threshold);
            candidates_s.Remove(arc);
            var connection_s = FilterCandidates(arc, candidates_s, true);
            connection_s.Item1.ForEach(s_i =>
            {
                if (!curves.Contains(s_i))
                {
                    curves.Add(s_i);
                }
            });

            // 找endpoint的连接关系
            var candidates_e = curves.FindAll(o => arc.EndPoint.DistanceTo(o.GetClosestPointTo(arc.EndPoint, false)) < threshold);
            candidates_e.Remove(arc);
            var connection_e = FilterCandidates(arc, candidates_e, false);
            connection_e.Item1.ForEach(e_i =>
            {
                if (!curves.Contains(e_i))
                {
                    curves.Add(e_i);
                }
            });

            // 判定所属类别
            int category = Math.Max(connection_s.Item2, connection_e.Item2);
            connection.AddRange(connection_s.Item1);
            connection.AddRange(connection_e.Item1);
            return Tuple.Create(connection, category, curves);
        }

        private static Tuple<List<Curve>, int> FilterCandidates(Arc arc, List<Curve> candidates_origin, bool isStartPoint)
        {
            var candidates = new List<Curve>();
            // 筛选原则： 如果与arc某端相连的是直线，则保留；如果是弧线，则保留切线方向差角小于45°的弧线
            var vector = isStartPoint ? new Vector2d(Math.Sin(arc.StartAngle), -Math.Cos(arc.StartAngle)) : new Vector2d(-Math.Sin(arc.EndAngle), Math.Cos(arc.EndAngle));
            var pt = isStartPoint ? arc.StartPoint : arc.EndPoint;
            candidates_origin.ForEach(o =>
            {
                if (o is Line line)
                {
                    // 排除弧线两端直线平行或近乎平行的情况 “U型”
                    var vector_another = isStartPoint ? new Vector2d(-Math.Sin(arc.EndAngle), Math.Cos(arc.EndAngle)) : new Vector2d(Math.Sin(arc.StartAngle), -Math.Cos(arc.StartAngle));
                    var angle = line.LineDirection().ToVector2d().GetAngleTo(vector_another);
                    if (Math.Min(angle, Math.PI - angle) > Math.PI / 90.0)  // 容差为2°
                    {
                        candidates.Add(o);
                    }
                }
                else
                {
                    var ptNear = o.GetClosestPointTo(pt, false);
                    var vector_near = (o as Arc).Center.GetVectorTo(ptNear);
                    vector_near = vector_near.RotateBy(Math.PI / 2.0, new Vector3d(0.0, 0.0, 1.0));
                    var angle = vector_near.ToVector2d().GetAngleTo(vector);
                    if (angle < Math.PI / 4.0 || angle > Math.PI * 3.0 / 4.0)
                    {
                        candidates.Add(o);
                    }
                }
            });

            // 判断连接端的种类（弧/线）
            int category = 0;
            if (candidates.Count == 0)
            {
                var vector_another = isStartPoint ? new Vector2d(-Math.Sin(arc.EndAngle), Math.Cos(arc.EndAngle)) : new Vector2d(Math.Sin(arc.StartAngle), -Math.Cos(arc.StartAngle));
                // 弧的另一端点延伸方向与x轴的夹角
                var angle2X_a = vector_another.GetAngleTo(new Vector2d(1.0, 0.0)); 
                angle2X_a = Math.Min(angle2X_a, Math.PI - angle2X_a);
                if (angle2X_a < Math.PI / 90.0 || Math.PI / 2.0 - angle2X_a < Math.PI / 90.0) //容差为2°
                {
                    var extendVector = (angle2X_a < Math.PI / 2.0 - angle2X_a) ? new Vector2d(0.0, 1.0) : new Vector2d(1.0, 0.0);
                    candidates.Add(new Line(pt, pt + extendVector.ToVector3d()));
                }
                else
                {
                    var angle2X = vector.GetAngleTo(new Vector2d(1.0, 0.0));
                    angle2X = Math.Min(angle2X, Math.PI - angle2X);
                    var extendVector = (angle2X < Math.PI / 2.0 - angle2X) ? new Vector2d(1.0, 0.0) : new Vector2d(0.0, 1.0);
                    candidates.Add(new Line(pt, pt + extendVector.ToVector3d()));
                }
                category = 1;
            }
            else if (candidates.Count >= 1)
            {
                // 与弧线某端相连的有直线也有弧线
                if (candidates.Exists(x => x is Line))
                {
                    candidates.RemoveAll(a => a is Arc);
                    if (candidates.Count > 1)
                    {
                        var dist = new List<double>();
                        candidates.ForEach(b => dist.Add(pt.DistanceTo(b.GetClosestPointTo(pt, false))));
                        var temp = candidates[dist.IndexOf(dist.Min())];
                        candidates = new List<Curve>();
                        candidates.Add(temp);
                    }
                    category = 1;
                }
                // 与弧线起点/终点端相连的全都是弧线
                else
                {
                    if (candidates.Count == 1)
                    {
                        var arc_next = candidates[0];
                        var ptNear = arc_next.GetClosestPointTo(pt, false);
                        var vector_another = isStartPoint ? new Vector2d(-Math.Sin(arc.EndAngle), Math.Cos(arc.EndAngle)) : new Vector2d(Math.Sin(arc.StartAngle), -Math.Cos(arc.StartAngle));
                        var angle2X_a = vector_another.GetAngleTo(new Vector2d(1.0, 0.0));
                        angle2X_a = Math.Min(angle2X_a, Math.PI - angle2X_a);
                        if (angle2X_a < Math.PI / 45.0 || Math.PI / 2.0 - angle2X_a < Math.PI / 45.0) //容差为4°
                        {
                            var extendVector = (angle2X_a < Math.PI / 2.0 - angle2X_a) ? new Vector2d(0.0, 1.0) : new Vector2d(1.0, 0.0);
                            candidates.Add(new Line(pt, pt + extendVector.ToVector3d()));
                        }
                        else
                        {
                            var angle2X = vector.GetAngleTo(new Vector2d(1.0, 0.0));
                            angle2X = Math.Min(angle2X, Math.PI - angle2X);
                            var extendVector = (angle2X < Math.PI / 2.0 - angle2X) ? new Vector2d(1.0, 0.0) : new Vector2d(0.0, 1.0);
                            candidates.Add(new Line(pt, pt + extendVector.ToVector3d()));
                        }
                        candidates.Remove(arc_next);
                        category = 2;
                    }
                    // 弧端连接多端弧的情况
                    else
                    {
                        var candidates_results = new List<Curve>();
                        foreach (var arc_next in candidates)
                        {
                            var ptNear = arc_next.GetClosestPointTo(pt, false);
                            var vector_another = isStartPoint ? new Vector2d(-Math.Sin(arc.EndAngle), Math.Cos(arc.EndAngle)) : new Vector2d(Math.Sin(arc.StartAngle), -Math.Cos(arc.StartAngle));
                            var angle2X_a = vector_another.GetAngleTo(new Vector2d(1.0, 0.0));
                            angle2X_a = Math.Min(angle2X_a, Math.PI - angle2X_a);
                            if (angle2X_a < Math.PI / 45.0 || Math.PI / 2.0 - angle2X_a < Math.PI / 45.0) //容差为4°
                            {
                                var extendVector = (angle2X_a < Math.PI / 2.0 - angle2X_a) ? new Vector2d(0.0, 1.0) : new Vector2d(1.0, 0.0);
                                candidates_results.Add(new Line(pt, pt + extendVector.ToVector3d()));
                            }
                            else
                            {
                                var angle2X = vector.GetAngleTo(new Vector2d(1.0, 0.0));
                                angle2X = Math.Min(angle2X, Math.PI - angle2X);
                                var extendVector = (angle2X < Math.PI / 2.0 - angle2X) ? new Vector2d(1.0, 0.0) : new Vector2d(0.0, 1.0);
                                candidates_results.Add(new Line(pt, pt + extendVector.ToVector3d()));
                            }
                        }
                        category = 3;
                        return Tuple.Create(candidates_results, category);
                    }
                }
            }
            return Tuple.Create(candidates, category);
        }

        private static Tuple<Line, Line> IntersectLines(Line line1, Line line2)
        {
            if (IsLooseCollinear(line1, line2, Math.PI / 90.0))
            {
                return Tuple.Create(line1, line2);
            }
            else
            {
                var intersectPt = line1.Intersection(line2, Intersect.ExtendBoth).ToAcGePoint3d();
                if (intersectPt.DistanceTo(line1.GetClosestPointTo(intersectPt, false)) > 1.0)
                {
                    line1 = (intersectPt.DistanceTo(line1.StartPoint) < intersectPt.DistanceTo(line1.EndPoint)) ?
                    new Line(intersectPt, line1.EndPoint) : new Line(line1.StartPoint, intersectPt);
                }
                else
                {
                    line1 = line1.Clone() as Line;
                    line1.Linetype = "ByLayer";
                }
                if (intersectPt.DistanceTo(line2.GetClosestPointTo(intersectPt, false)) > 1.0)
                {
                    line2 = (intersectPt.DistanceTo(line2.StartPoint) < intersectPt.DistanceTo(line2.EndPoint)) ?
                    new Line(intersectPt, line2.EndPoint) : new Line(line2.StartPoint, intersectPt);
                }
                else
                {
                    line2 = line2.Clone() as Line;
                    line2.Linetype = "ByLayer";
                }
            }
            return Tuple.Create(line1, line2);
        }

        private static bool IsLooseCollinear(Line line1, Line line2, double tolerance)
        {
            var angle = line1.LineDirection().GetAngleTo(line2.LineDirection());
            angle = Math.Min(angle, Math.PI - angle);
            if (angle < tolerance)
            {
                return true;
            }
            return false;
        }

        public static List<Line> LineMerge(List<Line> lines, double tolerance)
        {
            var results = new List<Line>();
            lines.ForEach(o =>
            {
                // 找到与线段o有重合点、重合段的线段
                var collinear = lines.FindAll(x => IsLooseCollinear(o, x, tolerance) &&
                (Math.Abs(x.StartPoint.DistanceTo(o.StartPoint) + x.StartPoint.DistanceTo(o.EndPoint) - o.Length) < 1.0 ||
                Math.Abs(x.EndPoint.DistanceTo(o.StartPoint) + x.EndPoint.DistanceTo(o.EndPoint) - o.Length) < 1.0));
                collinear.Remove(o);
                // 重合段的合并处理
                if (collinear.Count == 0)
                {
                    if (!results.Contains(o)) results.Add(o);
                }
                else
                {
                    var points = new List<Point3d>();
                    points.Add(o.StartPoint);
                    points.Add(o.EndPoint);
                    collinear.ForEach(y =>
                    {
                        if (!points.Contains(y.StartPoint)) points.Add(y.StartPoint);
                        if (!points.Contains(y.EndPoint)) points.Add(y.EndPoint);
                        if (lines.IndexOf(o) < lines.IndexOf(y)) lines.Remove(y);
                        if (results.Contains(y)) results.Remove(y);
                    });
                    var distMax = Tuple.Create(0.0, 0, 0);
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        for (int j = i + 1; j < points.Count; j++)
                        {
                            var dist = points[i].DistanceTo(points[j]);
                            if (dist > distMax.Item1)
                            {
                                distMax = Tuple.Create(dist, i, j);
                            }
                        }
                    }
                    var l_new = new Line(points[distMax.Item2], points[distMax.Item3]);
                    lines[lines.IndexOf(o)] = l_new;
                    if (!results.Contains(l_new)) results.Add(l_new);
                }
            });
            return results;
        }
    }
}
