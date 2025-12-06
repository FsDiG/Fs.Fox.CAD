using System;
using System.Linq;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Algorithm
{
    public static class ThMEPLineExtension
    {
        public static List<Line> LineSimplifier(DBObjectCollection dBObjectCollection, double ArcChord, double DistGap2Extend, double DistGap2Merge, double AngleTolerance)
        {
            // 将多段线炸开，保留所有Line(长度大于10.0mm)
            var curves = ExplodeCurves(dBObjectCollection);
            var lines = new List<Line>();
            var arcs = new List<Arc>();
            curves.ForEach(o => 
            {
                if(o is Line l)
                {
                    lines.Add(l);
                } 
                else
                {
                    arcs.Add(o as Arc);
                }
            });

            // z归零
            var lines_zTo0 = ProjectToXY(lines);

            // 处理可以merge的直线
            // similar - 相同
            // contain - 包含
            // overlap - 重叠
            // join - 相连
            var mergedLines = LineMerge(lines_zTo0, DistGap2Extend, DistGap2Merge, AngleTolerance);

            // 处理交叉直线（延长、打断至相交）
            // cross - 交叉（未延伸）
            // cross-extend - 延伸后交叉
            var crossedLines = LineCross(mergedLines, DistGap2Extend, AngleTolerance);

            var results = new List<Line>();
            results.AddRange(crossedLines);
            // arc打成多段线
            arcs.ForEach(o => 
            {
                
                var polyline = o.TessellateArcWithChord(ArcChord);
                var entitySet = new DBObjectCollection();
                polyline.Explode(entitySet);
                foreach(var obj in entitySet)
                {
                    results.Add(obj as Line);
                }
            });
            return results;
        }

        // 将输入的DBObjectCollection的线炸开，只提取出所有line和arc
        public static List<Curve> ExplodeCurves(DBObjectCollection curves)
        {
            var objs = new List<Curve>();
            foreach (Curve curve in curves)
            {
                const double lengthThreshold = 10.0;
                if (curve is Line line)
                {
                    // 剔除过短线段
                    if (line.Length > lengthThreshold)
                    {
                        objs.Add(line.WashClone() as Line);
                    }
                }
                else if (curve is Polyline polyline)
                {
                    var entitySet = new DBObjectCollection();
                    polyline.Explode(entitySet);
                    objs.AddRange(ExplodeCurves(entitySet));
                }
                else if (curve is Arc arc)
                {
                    if (arc.Length > lengthThreshold)
                    {
                        objs.Add(arc);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }

        // 直线拍平到z=0的平面
        public static List<Line> ProjectToXY(List<Line> lines)
        {
            var lines_z0 = new List<Line>();
            lines.ForEach(o =>
            {
                var direction = new Vector3d(0, 0, 1);
                var plane = new Plane(new Point3d(0.0, 0.0, 0.0), direction);
                lines_z0.Add(o.GetProjectedCurve(plane,direction) as Line);
            });
            return lines_z0; 
        }

        /// <summary>
        /// 判断线与线之间的连接关系（重合、共线、相交等）
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="DistGap2Extend">零碎共线对象图元径向gap</param>
        /// <param name="DistGap2Merge">重叠（部分重叠/完全重叠）共线对象法向gap</param>
        /// <param name="AngleTolerance">共线图元径向角度gap（rad）</param>
        /// <returns></returns>
        private static List<Tuple<int, int, string>> LineRelationship(List<Line> lines, double DistGap2Extend, double DistGap2Merge, double AngleTolerance)
        {
            var lists = new List<Tuple<int, int, string>>();
            lines.ForEach(o =>
            {
                var near = lines.FindAll(x => (o.GetDistToPoint(x.StartPoint,false) <= Math.Sqrt(2.0) * DistGap2Extend) || 
                    (o.GetDistToPoint(x.EndPoint,false) <= Math.Sqrt(2.0) * DistGap2Extend));
                near.Remove(o);
                var collinear = new List<Line>();
                var cross = new List<Line>();
                near.ForEach(x =>
                {
                    if (x.Delta.GetAngleTo(o.Delta) <= AngleTolerance || x.Delta.GetAngleTo(o.Delta) >= Math.PI - AngleTolerance)
                    {
                        if((o.GetDistToPoint(x.StartPoint, false) <= DistGap2Extend) || (o.GetDistToPoint(x.EndPoint, false) <= DistGap2Extend))
                        {
                            collinear.Add(x);
                        }
                    }
                    else
                    {
                        cross.Add(x);
                    }
                });
                // 共线情况：相同(similar)，包含(contain)，重叠(overlap)，端点相连(join)
                collinear.ForEach(y =>
                {
                    var linePair = new List<int>();
                    linePair.Add(lines.IndexOf(o));
                    linePair.Add(lines.IndexOf(y));
                    if ((y.StartPoint.DistanceTo(o.StartPoint) < DistGap2Merge && y.EndPoint.DistanceTo(o.EndPoint) < DistGap2Merge) ||
                        (y.EndPoint.DistanceTo(o.StartPoint) < DistGap2Merge && y.StartPoint.DistanceTo(o.EndPoint) < DistGap2Merge))
                    {
                        lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "similar"));
                    }
                    else
                    {
                        var Dist_Start = o.GetDistToPoint(y.StartPoint, false);
                        var Dist_End = o.GetDistToPoint(y.EndPoint, false);
                        if (Dist_Start <= DistGap2Merge && Dist_End <= DistGap2Merge)
                        {
                            lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "contain"));
                        }
                        else if (Dist_Start <= DistGap2Merge || Dist_End <= DistGap2Merge)
                        {
                            lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "overlap/join"));
                        }
                        else if(Dist_Start <= DistGap2Extend || Dist_End <= DistGap2Extend)
                        {
                            lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "join"));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                });
                // 相交情况：相交-无需延伸(cross)，相交-需延伸(cross-extend)
                cross.ForEach(z =>
                {
                    var linePair = new List<int>();
                    linePair.Add(lines.IndexOf(o));
                    linePair.Add(lines.IndexOf(z));
                    var Dist_Start = o.GetDistToPoint(z.StartPoint, false);
                    var Dist_End = o.GetDistToPoint(z.EndPoint, false);
                    if (Dist_Start <= Math.Sqrt(2.0) * DistGap2Extend || Dist_End <= Math.Sqrt(2.0) * DistGap2Extend)
                    {
                        if (o.Intersect(z, Intersect.OnBothOperands).Count == 0)
                        {
                            lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "cross-extend"));
                        }
                        else
                        {
                            lists.Add(Tuple.Create(linePair.Min(), linePair.Max(), "cross"));
                        }
                    }
                });
            });
            var list_filter = new List<Tuple<int, int, string>>();
            lists.ForEach(t =>
            {
                if ((!list_filter.Contains(t)) && list_filter.FindAll(o => o.Item1 == t.Item1 && o.Item2 == t.Item2).Count == 0)
                {
                    list_filter.Add(t);
                }
            });
            return list_filter;
        }

        // 支持输入一堆line，寻找tolerance下similar,overlap,join的line并merge
        public static List<Line> LineMerge(List<Line> lines, double DistGap2Extend, double DistGap2Merge, double AngleTolerance)
        {
            var lists = LineRelationship(lines, DistGap2Extend, DistGap2Merge, AngleTolerance);
            var lines2remove = new List<int>();
            for (int i = 0; i < lists.Count; i++)
            {
                var tuple = lists[i];
                if (lists.Find(o => lists.IndexOf(o) < i && o == tuple) == null) // 判断是否有重复操作的直线对
                {
                    var pts = new List<Point3d>();
                    pts.Add(lines[tuple.Item1].StartPoint);
                    pts.Add(lines[tuple.Item1].EndPoint);
                    pts.Add(lines[tuple.Item2].StartPoint);
                    pts.Add(lines[tuple.Item2].EndPoint);
                    switch (tuple.Item3)
                    {
                        case "similar": // 端点距离在DistGap2Merge范围内
                        case "contain":
                        case "overlap/join":
                        case "join":
                            var linePts = MaxDistancePts(pts);
                            lines[tuple.Item1] = new Line(linePts.Item1, linePts.Item2);
                            // lists 不做remove，只做更新
                            lists = UpdateRelationship(lists, tuple.Item1, tuple.Item2);
                            lines2remove.Add(tuple.Item2);
                            break;
                        default:
                            break;
                    }
                }
            }
            lines2remove.OrderByDescending(o => o).ToList().ForEach(x => lines.RemoveAt(x));
            return lines;
        }

        // 支持输入一堆line，寻找tolerance下cross,cross-extend的line并merge
        public static List<Line> LineCross(List<Line> lines, double DistGap2Extend, double AngleTolerance)
        {
            var lists = LineRelationship(lines, DistGap2Extend, 0.0, AngleTolerance);
            for (int i = 0; i < lists.Count; i++)
            {
                var tuple = lists[i];
                if (lists.Find(o => lists.IndexOf(o) < i && o == tuple) == null)
                {
                    switch (tuple.Item3)
                    {
                        case "cross":
                            var intersectPts = lines[tuple.Item1].Intersect(lines[tuple.Item2], Intersect.OnBothOperands);
                            if (intersectPts.Count != 1)
                            {
                                throw new NotSupportedException();
                            }
                            else
                            {
                                lines[tuple.Item1] = BreakAtIntersect(lines[tuple.Item1], intersectPts.First(), DistGap2Extend);
                                lines[tuple.Item2] = BreakAtIntersect(lines[tuple.Item2], intersectPts.First(), DistGap2Extend);
                            }
                            break;
                        case "cross-extend":
                            intersectPts = lines[tuple.Item1].Intersect(lines[tuple.Item2], Intersect.ExtendBoth);
                            if (intersectPts.Count != 1)
                            {
                                throw new NotSupportedException();
                            }
                            else
                            {
                                lines[tuple.Item1] = BreakAtIntersect(lines[tuple.Item1], intersectPts.First(), DistGap2Extend);
                                lines[tuple.Item2] = BreakAtIntersect(lines[tuple.Item2], intersectPts.First(), DistGap2Extend);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return lines;
        }

        // 对于linemerge，寻找最远的两个点，构建成新的直线
        private static Tuple<Point3d, Point3d> MaxDistancePts(List<Point3d> pts)
        {
            // 列表去重
            var points = new List<Point3d>();
            pts.ForEach(o =>
            {
                if (!points.Contains(o))
                {
                    points.Add(o);
                }
            });
            // 找到距离最远的两个点
            var dist_Max = Tuple.Create(0.0, 0, 0);
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    var dist = points[i].DistanceTo(points[j]);
                    if (dist > dist_Max.Item1)
                    {
                        dist_Max = Tuple.Create(dist, i, j);
                    }
                }
            }
            return Tuple.Create(points[dist_Max.Item2], points[dist_Max.Item3]);
        }

        // 对于原本线的配对关系，在对某一对线进行更新后，其他的配对关系也更新
        // 例：原配对关系{(0,1，overlap),(1,2,cross)}->处理第一对linemerge/linecross更新后{(0,0,overlap),(0,2,cross)}
        private static List<Tuple<int, int, string>> UpdateRelationship(List<Tuple<int, int, string>> lists, int index2retain, int index2remove)
        {
            var lists_update = new List<Tuple<int, int, string>>();
            lists.ForEach(t =>
            {
                if (t.Item1 == index2remove)
                {
                    lists_update.Add(Tuple.Create(index2retain, t.Item2, t.Item3));
                }
                else if (t.Item2 == index2remove)
                {
                    lists_update.Add(Tuple.Create(t.Item1, index2retain, t.Item3));
                }
                else
                {
                    lists_update.Add(t);
                }
            });
            return lists_update;
        }

        // 对于linecross，延长至交点/在交点处打断
        private static Line BreakAtIntersect(Line line, Point3d point, double DistTolerance)
        {
            var dist_S = line.StartPoint.DistanceTo(point);
            var dist_E = line.EndPoint.DistanceTo(point);
            // 交点在直线上
            if (dist_S + dist_E - line.Length <= 1e-10)
            {
                if (dist_S <= DistTolerance)
                {
                    return new Line(point, line.EndPoint);
                }
                else if (dist_E <= DistTolerance)
                {
                    return new Line(line.StartPoint, point);
                }
                else
                {
                    return new Line(line.StartPoint, line.EndPoint);
                }
            }
            // 交点在直线外（需延伸）
            else
            {
                if (dist_S >= line.Length)
                {
                    return new Line(line.StartPoint, point);
                }
                else if (dist_E >= line.Length)
                {
                    return new Line(point, line.EndPoint);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}



