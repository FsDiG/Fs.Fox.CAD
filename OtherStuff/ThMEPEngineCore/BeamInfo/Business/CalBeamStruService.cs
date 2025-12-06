using System;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using GeometryExtensions;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Interface;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.BeamInfo.Utils;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public class CalBeamStruService : ICalculateBeam
    {
        /// <summary>
        /// 将梁线按照规则分类成很多类
        /// </summary>
        /// <param name="dBObjects"></param>
        /// <returns></returns>
        public List<Beam> GetBeamInfo(DBObjectCollection dBObjects)
        {
            List<Beam> allBeam = new List<Beam>();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                List<Arc> arcs = new List<Arc>();
                Dictionary<Vector3d, List<Line>> groupDic = new Dictionary<Vector3d, List<Line>>();
                foreach (DBObject obj in dBObjects)
                {
                    if (obj is Line line)
                    {
                        // 忽略Z值不为零的情况
                        var lNormal = line.LineDirection();
                        if (!lNormal.IsEqualTo(new Vector3d(lNormal.X, lNormal.Y, 0.0)))
                        {
                            continue;
                        }

                        var norComp = groupDic.Keys.Where(x => x.IsParallelToEx(lNormal)).ToList();
                        if (norComp.Count > 0)
                        {
                            groupDic[norComp.First()].Add(line);
                        }
                        else
                        {
                            groupDic.Add(lNormal, new List<Line>() { line });
                        }
                    }
                    else if (obj is Arc arcLine)
                    {
                        arcs.Add(arcLine);
                    }
                }

                // TODO: Z归零应该在前面预处理时完成
                //将所有线的法相z值归零（不为0构建坐标系会出错
                groupDic = groupDic.ToDictionary(x => x.Key.Z == 0 ? x.Key : new Vector3d(x.Key.X, x.Key.Y, 0), k => k.Value);

                // 根据组内所有的线段识别出所有梁
                foreach (var lineDic in groupDic)
                {
                    //合并同组内重叠的线
                    var objs = new DBObjectCollection();
                    lineDic.Value.ForEachDbObject(o => objs.Add(o));
                    var results = objs.GetMergeOverlappingCurves(new Tolerance(1, 1));
                    results = results.MergeConnectCurves(new Tolerance(1, 1));
                    //过滤极短的线段，这些“碎线”可能是由于合并重叠线段时产出的
                    var filters = ThBeamGeometryPreprocessor.FilterCurves(results);
                    if (filters.Count > 0)
                    {
                        var res = GetLineBeamObject(filters.Cast<Line>().ToList(), lineDic.Key, 100);
                        allBeam.AddRange(res);
                    }
                }

                // 处理弧梁
                //allBeam.AddRange(GetArcBeamObject(arcs, 1500, 0.5));
            }

            return allBeam;
        }

        /// <summary>
        /// 获取直线梁
        /// </summary>
        /// <param name="linDic"></param>
        /// <param name="lineDir"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private List<LineBeam> GetLineBeamObject(List<Line> linList, Vector3d lineDir, double tolerance)
        {
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = Vector3d.ZAxis.CrossProduct(lineDir);
            Matrix3d trans = new Matrix3d(new double[]{
                    lineDir.X, yDir.X, zDir.X, 0,
                    lineDir.Y, yDir.Y, zDir.Y, 0,
                    lineDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

            //将所有线转到自建坐标系,方便比较
            linList = linList.OrderBy(x =>
            {
                x.TransformBy(trans.Inverse());
                return x.StartPoint.Y;
            }).ToList();

            var linePair = linList.First();
            List<LineBeam> beamLst = new List<LineBeam>();
            while (linList.Count > 0)
            {
                Line firLine = linePair;
                linList.Remove(linePair);
                double lMaxX = firLine.StartPoint.X;
                double lMinX = firLine.EndPoint.X;
                if (firLine.StartPoint.X < firLine.EndPoint.X)
                {
                    lMaxX = firLine.EndPoint.X;
                    lMinX = firLine.StartPoint.X;
                }
                var paraLines = linList.Where(x =>
                {
                    double xMaxX = x.StartPoint.X;
                    double xMinX = x.EndPoint.X;
                    if (x.StartPoint.X < x.EndPoint.X)
                    {
                        xMaxX = x.EndPoint.X;
                        xMinX = x.StartPoint.X;
                    }

                    //两根线距离太宽或者太近也不认为是一组梁
                    if (Math.Abs(firLine.StartPoint.Y - x.StartPoint.Y) > 1500 || Math.Abs(firLine.StartPoint.Y - x.StartPoint.Y) < 10)
                    {
                        return false;
                    }

                    //  两根线有重叠关系(大部分重叠)
                    if (xMinX <= lMaxX && xMaxX >= lMinX)
                    {
                        if (firLine.Length > x.Length)
                        {
                            if ((xMaxX + xMinX) / 2 <= lMaxX && (xMaxX + xMinX) / 2 >= lMinX)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if ((lMinX + lMaxX) / 2 <= xMaxX && (lMinX + lMaxX) / 2 >= xMinX)
                            {
                                return true;
                            }
                        }
                    }

                    // 不可能配成梁的线
                    return false;
                }).ToList();

                if (paraLines.Count > 0)
                {
                    if (paraLines.First().Length > firLine.Length)  //如果梁下边线长度大于上边线，那么就会有其他上边线遗漏
                    {
                        linList.Add(linePair);
                        linList = linList.OrderBy(x => x.StartPoint.Y).ToList();
                        linePair = paraLines.First();
                        continue;
                    }

                    firLine.TransformBy(trans);
                    double sum = 0;
                    List<Line> matchLines = new List<Line>();
                    foreach (var plineDic in paraLines)
                    {
                        var thisLine = plineDic;
                        sum += thisLine.Length;
                        if ((sum > firLine.Length && Math.Abs(sum - firLine.Length) > tolerance))
                        {
                            break;
                        }

                        var tempMacthNum = matchLines.Where(x =>
                        {
                            double xMaxX = x.StartPoint.X;
                            double xMinX = x.EndPoint.X;
                            if (x.StartPoint.X < x.EndPoint.X)
                            {
                                xMaxX = x.EndPoint.X;
                                xMinX = x.StartPoint.X;
                            }

                            double mMaxX = thisLine.StartPoint.X;
                            double mMinX = thisLine.EndPoint.X;
                            if (thisLine.StartPoint.X < thisLine.EndPoint.X)
                            {
                                mMaxX = thisLine.EndPoint.X;
                                mMinX = thisLine.StartPoint.X;
                            }

                            if (mMaxX <= xMinX || mMinX >= xMaxX)
                            {
                                return false;
                            }
                            return true;
                        });
                        if (tempMacthNum.Count() > 0)
                        {
                            break;
                        }

                        matchLines.Add(thisLine.Clone() as Line);
                        thisLine.TransformBy(trans);
                        LineBeam beam = new LineBeam(firLine, thisLine)
                        {
                            UpBeamLine = linePair,
                            DownBeamLine = plineDic
                        };
                        if (beam.StartPoint.DistanceTo(beam.EndPoint) >= 1.0)
                        {
                            beamLst.Add(beam);
                        }
                        linList.Remove(plineDic);
                    }
                }
                else
                {
                    firLine.TransformBy(trans);
                }

                if (linList.Count > 0)
                {
                    linePair = linList.First();
                }
            }

            return beamLst;
        }

        private List<Tuple<Arc, Arc>> ArcBeamPairsExtract(List<Arc> arcs, double DistThreshold, double overlapAngleThreshold)
        {
            var arcSegments = new List<Tuple<Arc, Arc>>();

            // 过滤过短的Arc
            foreach (var item in arcs)
            {
                if (item.Length <= 10) arcs.Remove(item);
            }

            // 如果出现重复弧线，则删除重复段，防止出现重复弧梁
            for (int i = 0; i < arcs.Count; i++)
            {
                for (int j = i + 1; j < arcs.Count; j++)
                {
                    if (IsRepeteArcs(arcs[i], arcs[j]))
                    {
                        arcs[i] = (arcs[i].Length >= arcs[j].Length) ? (arcs[i].ArcMerge(arcs[j])) : (arcs[i] = arcs[j].ArcMerge(arcs[i]));
                        arcs.RemoveAt(j);
                    }
                }
            }

            // 检测两条Arc是否构成Arc对
            for (int i = 0; i < arcs.Count; i++)
            {
                // 将Arc_i转换成polyline，并取出每一个端点
                var polyline = arcs[i].TessellateArcWithChord(arcs[i].Radius * (Math.Sin(Math.PI / 1440.0)));
                var polylineSegments = new PolylineSegmentCollection(polyline);
                var pt1 = new List<Point2d>();
                foreach (var segment in polylineSegments)
                {
                    pt1.Add(segment.StartPoint);
                }
                pt1.Add(polylineSegments.EndPoint);
                for (int j = i + 1; j < arcs.Count; j++)
                {
                    // 计算两个Arc间的距离（近似）
                    var dist = pt1.Min(pt => arcs[j].GetDistToPoint(pt.ToPoint3d()));
                    var overlapEstimate = arcs[i].OverlapAngle(arcs[j]);
                    //判断两条曲边是否构成同一曲梁 => 判定条件： 曲边间距 <= DistThreshold，曲边不相交，扇形区域有重合且重合范围大于小段弧长的overlapAngleThreshold
                    if ((dist <= DistThreshold) && (dist > arcs[i].Radius * (Math.Sin(Math.PI / 1440.0))) && overlapEstimate.Item1 &&
                        overlapEstimate.Item4 >= overlapAngleThreshold * Math.Min(arcs[i].TotalAngle, arcs[j].TotalAngle))
                    {
                        arcSegments.Add(Tuple.Create(arcs[i], arcs[j]));
                    }
                }
            }
            // 合并可合并的Arcs
            return MergeArcs(arcSegments);
        }

        // 检测Arc对中，是否有可合并的两对Arc
        private List<Tuple<Arc, Arc>> MergeArcs(List<Tuple<Arc, Arc>> arcs)
        {
            for (int i = 0; i < arcs.Count; i++)
            {
                for (int j = i + 1; j < arcs.Count; j++)
                {
                    if (arcs[i].Item1 == arcs[j].Item1
                        && arcs[i].Item2.Center.DistanceTo(arcs[j].Item2.Center) <= 1e-4
                        && (arcs[i].Item2.Radius - arcs[j].Item2.Radius) <= 1e-4
                        && arcs[i].Item2.OverlapAngle(arcs[j].Item2).Item1)
                    {
                        arcs[i] = Tuple.Create(arcs[i].Item1, arcs[i].Item2.ArcMerge(arcs[j].Item2));
                        arcs.Remove(arcs[j]);
                    }
                    else if (arcs[i].Item1 == arcs[j].Item2
                        && arcs[i].Item2.Center.DistanceTo(arcs[j].Item1.Center) <= 1e-4
                        && (arcs[i].Item2.Radius - arcs[j].Item1.Radius) <= 1e-4
                        && arcs[i].Item2.OverlapAngle(arcs[j].Item1).Item1)
                    {
                        arcs[i] = Tuple.Create(arcs[i].Item1, arcs[i].Item2.ArcMerge(arcs[j].Item1));
                        arcs.Remove(arcs[j]);
                    }
                    else if (arcs[i].Item2 == arcs[j].Item1
                        && arcs[i].Item1.Center.DistanceTo(arcs[j].Item2.Center) <= 1e-4
                        && (arcs[i].Item1.Radius - arcs[j].Item2.Radius) <= 1e-4
                        && arcs[i].Item1.OverlapAngle(arcs[j].Item2).Item1)
                    {
                        arcs[i] = Tuple.Create(arcs[i].Item1.ArcMerge(arcs[j].Item2), arcs[i].Item2);
                        arcs.Remove(arcs[j]);
                    }
                    else if (arcs[i].Item2 == arcs[j].Item2
                        && arcs[i].Item1.Center.DistanceTo(arcs[j].Item1.Center) <= 1e-4
                        && (arcs[i].Item1.Radius - arcs[j].Item1.Radius) <= 1e-4
                        && arcs[i].Item1.OverlapAngle(arcs[j].Item1).Item1)
                    {
                        arcs[i] = Tuple.Create(arcs[i].Item1.ArcMerge(arcs[j].Item1), arcs[i].Item2);
                        arcs.Remove(arcs[j]);
                    }
                }
            }
            return arcs;
        }

        // 判断Arcs是否重复
        private bool IsRepeteArcs(Arc arc1, Arc arc2)
        {
            var overlapEstimate = arc1.OverlapAngle(arc2);
            var startAngle = overlapEstimate.Item2;
            var endAngle = overlapEstimate.Item3;

            // 排除两段Arc没有重叠区域的情况
            if (!overlapEstimate.Item1) return false;

            // 排除两段Arc仅仅首尾相接的情况
            if (overlapEstimate.Item4 == 0.0 && startAngle == endAngle) return false;

            // 计算两段Arc最小重叠范围
            var arc1_new = new Arc(arc1.Center, arc1.Radius, startAngle, endAngle);
            var arc2_new = new Arc(arc2.Center, arc2.Radius, startAngle, endAngle);

            // 创建弧形与弧的圆心构成的扇形
            double arcBulge1 = arc1_new.BulgeFromCurve(false);
            PolylineSegment arcSegment_1 = new PolylineSegment(arc1_new.StartPoint.ToPoint2D(), arc1_new.EndPoint.ToPoint2D(), arcBulge1);
            var segmentCollection_1 = new PolylineSegmentCollection()
            {
                arcSegment_1,
                new PolylineSegment(arc1_new.EndPoint.ToPoint2D(),arc1_new.Center.ToPoint2D()),
                new PolylineSegment(arc1_new.Center.ToPoint2D(),arc1_new.StartPoint.ToPoint2D()),
            };
            var sector_1 = segmentCollection_1.ToPolyline().ToNTSPolygon();

            double arcBulge2 = arc2_new.BulgeFromCurve(false);
            PolylineSegment arcSegment_2 = new PolylineSegment(arc2_new.StartPoint.ToPoint2D(), arc2_new.EndPoint.ToPoint2D(), arcBulge2);
            var segmentCollection_2 = new PolylineSegmentCollection()
            {
                arcSegment_2,
                new PolylineSegment(arc2_new.EndPoint.ToPoint2D(),arc2_new.Center.ToPoint2D()),
                new PolylineSegment(arc2_new.Center.ToPoint2D(),arc2_new.StartPoint.ToPoint2D()),
            };
            var sector_2 = segmentCollection_2.ToPolyline().ToNTSPolygon();

            // 挖去弧的圆心，防止对后续求交造成影响
            var line1_2start = new Line(arc1_new.Center.GetMidPt(arc2_new.StartPoint), arc2_new.StartPoint);
            var line1_2end = new Line(arc1_new.Center.GetMidPt(arc2_new.EndPoint), arc2_new.EndPoint);
            var line2_1start = new Line(arc2_new.Center.GetMidPt(arc1_new.StartPoint), arc1_new.StartPoint);
            var line2_1end = new Line(arc2_new.Center.GetMidPt(arc1_new.EndPoint), arc1_new.EndPoint);

            var startAngle_1 = startAngle;
            var endAngle_1 = endAngle;
            var startAngle_2 = startAngle;
            var endAngle_2 = endAngle;

            // 判断扇形与线是否有交点
            if (sector_1.Intersects(line1_2start.ToNTSGeometry()))
            {
                var line1_start = new Line(arc1_new.Center, arc1_new.StartPoint);
                startAngle_1 = startAngle + line1_2start.LineDirection().GetAngleTo(line1_start.LineDirection());
                startAngle_1 = (startAngle_1 > 8 * Math.Atan(1)) ? (startAngle_1 - 8 * Math.Atan(1)) : startAngle_1;
                startAngle_1 = (startAngle_1 < 0) ? (startAngle_1 + 8 * Math.Atan(1)) : startAngle_1;
            }
            if (sector_1.Intersects(line1_2end.ToNTSGeometry()))
            {
                var line1_end = new Line(arc1_new.Center, arc1_new.EndPoint);
                endAngle_1 = endAngle - line1_2end.LineDirection().GetAngleTo(line1_end.LineDirection());
                endAngle_1 = (endAngle_1 > 8 * Math.Atan(1)) ? (endAngle_1 - 8 * Math.Atan(1)) : endAngle_1;
                endAngle_1 = (endAngle_1 < 0) ? (endAngle_1 + 8 * Math.Atan(1)) : endAngle_1;
            }
            arc1_new = new Arc(arc1.Center, arc1.Radius, startAngle_1, endAngle_1);

            if (sector_2.Intersects(line2_1start.ToNTSGeometry()))
            {
                var line2_start = new Line(arc2_new.Center, arc2_new.StartPoint);
                startAngle_2 = startAngle + line2_1start.LineDirection().GetAngleTo(line2_start.LineDirection());
                startAngle_2 = (startAngle_2 > 8 * Math.Atan(1)) ? (startAngle_2 - 8 * Math.Atan(1)) : startAngle_2;
                startAngle_2 = (startAngle_2 < 0) ? (startAngle_2 + 8 * Math.Atan(1)) : startAngle_2;
            }
            if (sector_2.Intersects(line2_1end.ToNTSGeometry()))
            {
                var line2_end = new Line(arc2_new.Center, arc2_new.EndPoint);
                endAngle_2 = endAngle - line2_1end.LineDirection().GetAngleTo(line2_end.LineDirection());
                endAngle_2 = (endAngle_2 > 8 * Math.Atan(1)) ? (endAngle_2 - 8 * Math.Atan(1)) : endAngle_2;
                endAngle_2 = (endAngle_2 < 0) ? (endAngle_2 + 8 * Math.Atan(1)) : endAngle_2;
            }
            arc2_new = new Arc(arc2.Center, arc2.Radius, startAngle_2, endAngle_2);

            // 排除重叠长度过小的情况
            if (arc1_new.Length <= 10 || arc2_new.Length <= 10) return false;

            // 计算两段Arc重叠范围内的最大距离
            var polyline = arc1_new.TessellateArcWithChord(arc1_new.Radius * (Math.Sin(Math.PI / 1440.0)));
            var polylineSegments = new PolylineSegmentCollection(polyline);
            var pt1 = new List<Point2d>();
            foreach (var segment in polylineSegments)
            {
                pt1.Add(segment.StartPoint);
            }
            pt1.Add(polylineSegments.EndPoint);
            var dist_max = pt1.Max(pt => arc2_new.GetDistToPoint(pt.ToPoint3d()));
            if (dist_max <= Math.Max(arc1_new.Radius * (Math.Sin(Math.PI / 1440.0)), 20.0))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取弧梁（扩展）
        /// </summary>
        /// <param name="arcs"></param>
        /// <returns></returns>
        private List<ArcBeam> GetArcBeamObject(List<Arc> arcs, double distThreshold, double overlapAngleThreshold)
        {
            List<ArcBeam> beam = new List<ArcBeam>();
            var arcPairs = ArcBeamPairsExtract(arcs, distThreshold, overlapAngleThreshold);
            arcPairs.ForEach(o => beam.Add(new ArcBeam(o.Item1, o.Item2)));
            return beam;
        }
    }
}
