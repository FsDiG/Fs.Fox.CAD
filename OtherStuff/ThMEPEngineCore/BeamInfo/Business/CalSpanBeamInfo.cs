using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.BeamInfo.Model;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public class CalSpanBeamInfo
    {
        public void FindBeamOfCentralizeMarking(ref List<Beam> allBeams, out List<Beam> divisionBeams)
        {
            //找到带有集中标注的梁，并将他们按照先y后x排序。
            List<Beam> cMarkBeams = allBeams.Where(x => x.CentralizeMarkings != null && x.CentralizeMarkings.Count > 0)
                .Where(x => x.CentralizeMarkings.Where(y => (y.Marking as DBText).TextString.Contains("L")).Count() > 0)
                .OrderByDescending(x => x.CentralizeMarkings.First(y => (y.Marking as DBText).TextString.Contains("L")).AlignmentPoint.Y)
                .ThenBy(x => x.CentralizeMarkings.First(y => (y.Marking as DBText).TextString.Contains("L")).AlignmentPoint.X)
                .ToList();
            List<Beam> noCMarkBeams = allBeams.Except(cMarkBeams).ToList();
            List<Beam> mergeBeams = new List<Beam>();
            divisionBeams = new List<Beam>();
            foreach (var beam in cMarkBeams)
            {
                string text = beam.CentralizeMarkings.Select(x => x.Marking as DBText).First(x => x.TextString.Contains("L")).TextString;
                if (text.Contains("(") && text.Contains(")"))
                {
                    string inText = Regex.Replace(text, @"(.*\()(.*)(\).*)", "$2");
                    int num = int.Parse(Regex.Replace(inText, @"[^0-9]+", ""));
                    bool hasLetter = Regex.Matches(inText, "[a-zA-Z]").Count > 0;
                    if (hasLetter)
                    {
                        num = num + 1;
                    }

                    Beam curBeam = beam;
                    int index = 1;
                    while (index < num)  //因为排序的时候是按照从左往右排序集中标注的，所以先往左边寻找梁，以免遗漏
                    {
                        Beam matchBeam = FindMatchBeam(curBeam, noCMarkBeams, allBeams, true);
                        if (matchBeam != null)
                        {
                            if (inText.Contains("A") && index == num - 1)
                            {
                                if (matchBeam.OverhaningType != BeamOverhangingType.OneOverhangingBeam)
                                {
                                    break;
                                }
                            }
                            if (inText.Contains("B") && index == num - 1)
                            {
                                if (matchBeam.OverhaningType != BeamOverhangingType.TwoOverhangingBeam)
                                {
                                    break;
                                }
                            }
                            noCMarkBeams.Remove(matchBeam);
                            matchBeam.CentralizeMarkings = beam.CentralizeMarkings;
                            curBeam = matchBeam;
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    curBeam = beam;
                    while (index < num)  //向右找
                    {
                        Beam matchBeam = FindMatchBeam(curBeam, noCMarkBeams, allBeams, false);
                        if (matchBeam != null)
                        {
                            if (inText.Contains("A") && index == num - 1)
                            {
                                if (matchBeam.OverhaningType != BeamOverhangingType.OneOverhangingBeam)
                                {
                                    break;
                                }
                            }
                            if (inText.Contains("B") && index == num - 1)
                            {
                                if (matchBeam.OverhaningType != BeamOverhangingType.TwoOverhangingBeam)
                                {
                                    break;
                                }
                            }
                            noCMarkBeams.Remove(matchBeam);
                            matchBeam.CentralizeMarkings = beam.CentralizeMarkings;
                            curBeam = matchBeam;
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (index == 1 && num > 1)
                    {
                        divisionBeams.Add(beam);
                    }
                }
                else
                {
                    if (beam.BeamType == BeamStandardsType.SecondaryBeam)
                    {
                        mergeBeams.Add(beam);
                    }
                }
            }

            //合并应该合并的梁
            List<Beam> secBeams = noCMarkBeams.Where(x => x.BeamType == BeamStandardsType.SecondaryBeam).ToList();
            foreach (var beam in mergeBeams)
            {
                List<Beam> matchBeams = CalMatchMergeBeams(secBeams, allBeams, beam);
                if (matchBeams.Count > 0)
                {
                    Beam mergeBeam = MergeBeams(matchBeams, beam);
                    allBeams.Remove(beam);
                    allBeams = allBeams.Except(matchBeams).ToList();
                    allBeams.Add(mergeBeam);
                }
            }
        }

        #region 合并梁
        /// <summary>
        /// 找出符合条件的能被合并的梁
        /// </summary>
        /// <param name="secBeams"></param>
        /// <param name="allBeams"></param>
        /// <param name="beam"></param>
        /// <returns></returns>
        private List<Beam> CalMatchMergeBeams(List<Beam> secBeams, List<Beam> allBeams, Beam beam)
        {
            List<Beam> matchBeams = new List<Beam>();
            Beam curBeam = beam;
            while (true)  //因为排序的时候是按照从左往右排序集中标注的，所以先往左边寻找梁，以免遗漏
            {
                Beam matchBeam = FindMatchBeam(curBeam, secBeams, allBeams, true);
                if (matchBeam != null)
                {
                    secBeams.Remove(matchBeam);
                    matchBeams.Add(matchBeam);
                    curBeam = matchBeam;
                }
                else
                {
                    break;
                }
            }

            curBeam = beam;
            while (true)  //向右找
            {
                Beam matchBeam = FindMatchBeam(curBeam, secBeams, allBeams, false);
                if (matchBeam != null)
                {
                    secBeams.Remove(matchBeam);
                    matchBeams.Add(matchBeam);
                    curBeam = matchBeam;
                }
                else
                {
                    break;
                }
            }

            return matchBeams;
        }

        /// <summary>
        /// 合并梁
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="cBeam"></param>
        /// <returns></returns>
        private MergeLineBeam MergeBeams(List<Beam> beams, Beam cBeam)
        {
            MergeLineBeam mergeBeam = new MergeLineBeam(beams);
            mergeBeam.mergeBeams.Add(cBeam);
            mergeBeam.UpBeamLines.Add(cBeam.UpBeamLine);
            mergeBeam.DownBeamLines.Add(cBeam.DownBeamLine);
            mergeBeam.BeamNormal = cBeam.BeamNormal;
            mergeBeam.CentralizeMarkings = cBeam.CentralizeMarkings;
            mergeBeam.OriginMarkings = beams.SelectMany(x => x.OriginMarkings).ToList();

            foreach (var beam in beams)
            {
                if (mergeBeam.UpBeamLines.Contains(beam.UpBeamLine))
                {
                    mergeBeam.DownBeamLines.Add(beam.DownBeamLine);
                    continue;
                }
                if (mergeBeam.UpBeamLines.Contains(beam.DownBeamLine))
                {
                    mergeBeam.DownBeamLines.Add(beam.UpBeamLine);
                    continue;
                }
                if (mergeBeam.DownBeamLines.Contains(beam.UpBeamLine))
                {
                    mergeBeam.UpBeamLines.Add(beam.DownBeamLine);
                    continue;
                }
                if (mergeBeam.DownBeamLines.Contains(beam.DownBeamLine))
                {
                    mergeBeam.UpBeamLines.Add(beam.UpBeamLine);
                    continue;
                }

                Point3d sp = mergeBeam.UpBeamLines.First().StartPoint;
                double dis1 = beam.UpBeamLine.StartPoint.DistanceTo(sp);
                double dis2 = beam.UpBeamLine.EndPoint.DistanceTo(sp);
                double upDis = dis1 > dis2 ? dis2 : dis1;
                double dis3 = beam.DownBeamLine.StartPoint.DistanceTo(sp);
                double dis4 = beam.DownBeamLine.EndPoint.DistanceTo(sp);
                double downDis = dis3 > dis4 ? dis4 : dis3;
                if (upDis < downDis)
                {
                    mergeBeam.UpBeamLines.Add(beam.UpBeamLine);
                    mergeBeam.DownBeamLines.Add(beam.DownBeamLine);
                }
                else
                {
                    mergeBeam.UpBeamLines.Add(beam.DownBeamLine);
                    mergeBeam.DownBeamLines.Add(beam.UpBeamLine);
                }
            }

            return mergeBeam;
        }

        /// <summary>
        /// 找到匹配的梁
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="beamsLst"></param>
        /// <param name="allBeams"></param>
        /// <param name="isLeft"></param>
        /// <returns></returns>
        private Beam FindMatchBeam(Beam beam, List<Beam> beamsLst, List<Beam> allBeams, bool isLeft)
        {
            Vector3d normal = beam.BeamNormal;
            Vector3d findXDir = isLeft ? -Vector3d.XAxis : Vector3d.XAxis;
            Vector3d findYDir = isLeft ? Vector3d.YAxis : -Vector3d.YAxis;
            Curve intersectCuv = null;
            if (normal.DotProduct(findXDir) > 0.001 || (-0.001 < normal.DotProduct(findXDir) && findYDir.DotProduct(normal) > 0))
            {
                intersectCuv = beam.EndIntersect != null && beam.EndIntersect.EntityCurve.Count > 0 ? beam.EndIntersect.EntityCurve.First() : null;
            }
            else
            {
                intersectCuv = beam.StartIntersect != null && beam.StartIntersect.EntityCurve.Count > 0 ? beam.StartIntersect.EntityCurve.First() : null;
            }
            if (intersectCuv == null)
            {
                return null;
            }

            List<Beam> findBeam = new List<Beam>();
            if (intersectCuv is Polyline)
            {
                findBeam = beamsLst.Where(x => ((x.StartIntersect != null && x.StartIntersect.EntityCurve.Where(y => y == intersectCuv).Count() > 0) ||
                                   (x.EndIntersect != null && x.EndIntersect.EntityCurve.Where(y => y == intersectCuv).Count() > 0)) &&
                                   x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                   .ToList();
            }
            else
            {
                foreach (var curBeam in allBeams)
                {
                    if (curBeam.UpBeamLine == intersectCuv)
                    {
                        findBeam.AddRange(beamsLst.Where(x => ((x.StartIntersect != null && 
                        x.StartIntersect.EntityCurve.Where(y => y == curBeam.DownBeamLine).Count() > 0) ||
                                             (x.EndIntersect != null && x.EndIntersect.EntityCurve.Where(y => y == curBeam.DownBeamLine).Count() > 0)) &&
                                             x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                             .ToList());
                    }

                    if (curBeam.DownBeamLine == intersectCuv)
                    {
                        findBeam.AddRange(beamsLst.Where(x => ((x.StartIntersect != null && 
                                             x.StartIntersect.EntityCurve.Where(y => y == curBeam.UpBeamLine).Count() > 0) ||
                                             (x.EndIntersect != null && 
                                             x.EndIntersect.EntityCurve.Where(y => y == curBeam.UpBeamLine).Count() > 0)) &&
                                             x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                             .ToList());
                    }
                }
            }

            return FindConnectBeam(findBeam, beam);
        }

        /// <summary>
        /// 找到能够连接的梁
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="beam"></param>
        /// <returns></returns>
        private Beam FindConnectBeam(List<Beam> beams, Beam beam)
        {
            if (beams.Count <= 0)
            {
                return null;
            }

            Beam matchBeam = null;
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = Vector3d.ZAxis.CrossProduct(beam.BeamNormal);
            Matrix3d trans = new Matrix3d(new double[]{
                    beam.BeamNormal.X, yDir.X, zDir.X, 0,
                    beam.BeamNormal.Y, yDir.Y, zDir.Y, 0,
                    beam.BeamNormal.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

            beam.UpBeamLine.TransformBy(trans.Inverse());
            beam.DownBeamLine.TransformBy(trans.Inverse());
            double y1 = beam.UpBeamLine.StartPoint.Y >= beam.DownBeamLine.StartPoint.Y ? beam.UpBeamLine.StartPoint.Y : beam.DownBeamLine.StartPoint.Y;
            double y2 = beam.UpBeamLine.StartPoint.Y < beam.DownBeamLine.StartPoint.Y ? beam.UpBeamLine.StartPoint.Y : beam.DownBeamLine.StartPoint.Y;
            beams.ForEach(x =>
            {
                x.UpBeamLine.TransformBy(trans.Inverse());
                x.DownBeamLine.TransformBy(trans.Inverse());
            });
            var paraBeams = beams.Where(x =>
            {
                double y3 = x.UpBeamLine.StartPoint.Y >= x.DownBeamLine.StartPoint.Y ? x.UpBeamLine.StartPoint.Y : x.DownBeamLine.StartPoint.Y;
                double y4 = x.UpBeamLine.StartPoint.Y < x.DownBeamLine.StartPoint.Y ? x.UpBeamLine.StartPoint.Y : x.DownBeamLine.StartPoint.Y;
                if (y1 >= y3 && y3 > y2)
                {
                    return true;
                }
                else if (y3 >= y1 && y1 > y4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }).ToList();
            if (paraBeams.Count > 0)
            {
                matchBeam = paraBeams.First();
            }

            beam.UpBeamLine.TransformBy(trans);
            beam.DownBeamLine.TransformBy(trans);
            beams.ForEach(x =>
            {
                x.UpBeamLine.TransformBy(trans);
                x.DownBeamLine.TransformBy(trans);
            });
            return matchBeam;
        }
        #endregion

        #region 分割梁
        /// <summary>
        /// 分割梁
        /// </summary>
        /// <param name="divisionBeam"></param>
        /// <param name="allBeams"></param>
        /// <returns></returns>
        public List<Beam> DivisionBeams(Beam divisionBeam, List<Beam> allBeams)
        {
            //搭接在需要被分割梁上的其他梁
            var intersectBeams = FindMatchDivBeam(divisionBeam, allBeams);
            Dictionary<Point3d, Point3d> mpDic = new Dictionary<Point3d, Point3d>();
            foreach (var iBeam in intersectBeams)
            {
                Point3d p1 = divisionBeam.DownBeamLine.GetClosestPointTo(iBeam.DownBeamLine.StartPoint, false);
                Point3d p2 = divisionBeam.DownBeamLine.GetClosestPointTo(iBeam.UpBeamLine.StartPoint, false);
                Point3d p3 = divisionBeam.UpBeamLine.GetClosestPointTo(iBeam.DownBeamLine.StartPoint, false);
                Point3d p4 = divisionBeam.UpBeamLine.GetClosestPointTo(iBeam.UpBeamLine.StartPoint, false);
                Point3d downMP = new Point3d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, 0);
                Point3d upMP = new Point3d((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2, 0);
                mpDic.Add(downMP, upMP);
            }
            mpDic.Add(divisionBeam.DownEndPoint, divisionBeam.UpEndPoint);

            List<Beam> diviBeams = new List<Beam>();
            Point3d downSP = divisionBeam.DownStartPoint, upSP = divisionBeam.UpStartPoint;
            while (mpDic.Count > 0)
            {
                Point3d keyP = mpDic.Keys.OrderBy(x => x.DistanceTo(downSP)).First();
                Point3d downEP = keyP;
                Point3d upEP = mpDic[keyP];
                if (downSP.DistanceTo(downEP) > downSP.DistanceTo(upEP))
                {
                    upEP = keyP;
                    downEP = mpDic[keyP];
                }
                LineBeam beam = new LineBeam(new Line(upSP, upEP), new Line(downSP, downEP));
                beam.DownBeamLine = divisionBeam.DownBeamLine;
                beam.UpBeamLine = divisionBeam.UpBeamLine;
                beam.ThCentralizedMarkingP = divisionBeam.ThCentralizedMarkingP;
                beam.ThOriginMarkingcsP = divisionBeam.ThOriginMarkingcsP;
                diviBeams.Add(beam);
                downSP = downEP;
                upSP = upEP;
                mpDic.Remove(keyP);
            }

            return diviBeams;
        }

        /// <summary>
        /// 找到匹配的分割梁的梁
        /// </summary>
        /// <param name="divisionBeam"></param>
        /// <param name="allBeams"></param>
        /// <returns></returns>
        private List<Beam> FindMatchDivBeam(Beam divisionBeam, List<Beam> allBeams)
        {
            //搭接在需要被分割梁上的其他梁
            var intersectBeams = allBeams.Where(x => x.EndIntersect != null && x.StartIntersect != null)
                                   .Where(x => (x.EndIntersect.EntityCurve.Where(y => y.Id.Handle.Value == divisionBeam.DownBeamLine.Id.Handle.Value ||
                                                                       y.Id.Handle.Value == divisionBeam.UpBeamLine.Id.Handle.Value).Count() > 0 &&
                                               (x.StartIntersect.EntityType == IntersectType.Column ||
                                               x.StartIntersect.EntityType == IntersectType.Wall)) ||
                                               (x.StartIntersect.EntityCurve.Where(z => z.Id.Handle.Value == divisionBeam.DownBeamLine.Id.Handle.Value ||
                                                                       z.Id.Handle.Value == divisionBeam.UpBeamLine.Id.Handle.Value).Count() > 0 &&
                                               (x.EndIntersect.EntityType == IntersectType.Column ||
                                               x.EndIntersect.EntityType == IntersectType.Wall))
                                 ).ToList();

            Vector3d xDir = divisionBeam.BeamNormal;
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = Vector3d.ZAxis.CrossProduct(xDir);
            Matrix3d trans = new Matrix3d(new double[]{
                    xDir.X, yDir.X, zDir.X, 0,
                    xDir.Y, yDir.Y, zDir.Y, 0,
                    xDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});
            double minX = divisionBeam.UpBeamLine.StartPoint.TransformBy(trans.Inverse()).X;
            double maxX = divisionBeam.UpBeamLine.EndPoint.TransformBy(trans.Inverse()).X;
            intersectBeams = intersectBeams.Where(x =>
            {
                Point3d downP = x.DownBeamLine.StartPoint.TransformBy(trans.Inverse());
                Point3d upP = x.UpBeamLine.StartPoint.TransformBy(trans.Inverse());
                if (downP.X > minX && upP.X > minX && downP.X < maxX && upP.X < maxX)
                {
                    return true;
                }
                return false;
            }).OrderBy(x=>x.UpBeamLine.StartPoint.TransformBy(trans.Inverse()).X)
            .ToList();
            return intersectBeams;
        }
        #endregion
    }
}

