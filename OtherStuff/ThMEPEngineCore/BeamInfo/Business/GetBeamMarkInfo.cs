using System;
using Linq2Acad;
using System.Linq;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.BeamInfo.Utils;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public class GetBeamMarkInfo
    {
        private Document _doc;
        private AcadDatabase _acdb;
        private readonly string layerName = "S_BEAM_TEXT_HORZ";
        //private readonly string layerName = "S_BEAM_TEXT_HORZ," +
        //    "__覆盖_S20-平面_TEN25CUZ_设计区$0$TH-STYLE1," +
        //    "S_BEAM_TEXT_VERT," +
        //    "S_BEAM_WALL_TEXT," +
        //    "__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM_TEXT_VERT," +
        //    "__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM_TEXT_HORZ";
        private MarkingService markingService;
        private List<MarkingInfo> allMarking;
        private List<Beam> allBeams;

        public GetBeamMarkInfo(Document doc, AcadDatabase acdb)
        {
            _doc = doc;
            _acdb = acdb;
            markingService = new MarkingService(_doc, _acdb, layerName);
        }

        public void FillBeamInfo(List<Beam> beams)
        {
            allBeams = beams;
            allMarking = markingService.GetAllMarking();
            foreach (var beam in allBeams)
            {
                beam.CentralizeMarkings = GetBeamCentralizeMarking(beam);
            }

            foreach (var beam in allBeams)
            {
                beam.OriginMarkings = GetBeamOriginMarking(beam);
            }
        }

        public void DeleteMarking(List<MarkingInfo> markings)
        {
            allMarking.RemoveAll(x => markings.Where(y => y.Marking.Id.Handle == x.Marking.Id.Handle).Count() > 0);
        }

        #region 集中标注
        /// <summary>
        /// 获取梁集中标注(先获取集中标注再获取原位标注)
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        private List<MarkingInfo> GetBeamCentralizeMarking(Beam beam)
        {
            List<MarkingInfo> centralizeMark = new List<MarkingInfo>();
            List<MarkingInfo> removeMark = new List<MarkingInfo>();
            if (beam is LineBeam)
            {
                List<MarkingInfo> lineMarks = markingService.GetMarking(beam.UpStartPoint, beam.DownEndPoint, beam.BeamNormal, 20, MarkingType.Line);
                lineMarks = lineMarks.Where(x => allMarking.Where(y => y.Marking.Id.Handle == x.Marking.Id.Handle).Count() > 0).ToList();
                foreach (var mark in lineMarks)
                {
                    Line line = mark.Marking as Line;
                    int res = CheckFlagLine(beam, line, 20);
                    if (res != 0)
                    {
                        Vector3d lineDir = line.LineDirection();
                        if (Math.Abs(lineDir.DotProduct(beam.BeamNormal)) < 0.01)
                        {
                            var textMarksOfLine = markingService.GetMarking(line.StartPoint, line.EndPoint, lineDir, 100, MarkingType.Text)
                                .Where(x => Math.Abs(x.MarkingNormal.DotProduct(lineDir)) < 0.001).ToList();
                            centralizeMark = CheckFlagTextMarkings(lineDir, textMarksOfLine);
                            centralizeMark = CheckEachFlagMarkings(centralizeMark);
                            removeMark.AddRange(centralizeMark);
                            removeMark.Add(mark);
                            break;
                        }
                        else
                        {
                            var lineResLineMark = markingService.GetMarking(line.StartPoint, line.EndPoint, lineDir, 10, MarkingType.Line)
                                .Where(x => x.Marking.Id != mark.Marking.Id).ToList();
                            if (lineResLineMark.Count > 0)
                            {
                                Line secLine = lineResLineMark.First().Marking as Line;
                                Vector3d secLineDir = secLine.LineDirection();
                                var textMarksOfLine = markingService.GetMarking(secLine.StartPoint, secLine.EndPoint, secLineDir, 200, MarkingType.Text)
                                        .Where(x => Math.Abs(x.MarkingNormal.DotProduct(secLineDir)) < Tolerance.Global.EqualPoint).ToList();
                                centralizeMark = CheckFlagTextMarkings(secLineDir, textMarksOfLine);
                                centralizeMark = CheckEachFlagMarkings(centralizeMark);
                                if (centralizeMark.Count > 0)
                                {
                                    removeMark.AddRange(centralizeMark);
                                    removeMark.Add(mark);
                                    removeMark.Add(lineResLineMark.First());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (beam is ArcBeam)   //弧梁
            {
                #region 暂不处理弧梁
                //ArcBeam arcBeam = beam as ArcBeam;
                //Point3d pt1 = arcBeam.DownStartPoint;
                //Point3d pt2 = arcBeam.DownEndPoint + arcBeam.BeamNormal * arcBeam.MiddlePoint.DistanceTo(arcBeam.CenterPoint);
                //List<MarkingInfo> resMarks = markingService.GetMarking(pt1, pt2, beam.BeamNormal, 0, MarkingType.Line);
                //resMarks = resMarks.Where(x => allMarking.Where(y => y.Marking.Id.Handle == x.Marking.Id.Handle).Count() > 0).ToList();
                //if (resMarks.Count > 0)
                //{
                //    Line line = resMarks.First().Marking as Line;
                //    Vector3d lineDir = line.Delta.GetNormal();

                //    centralizeMark = markingService.GetMarking(line.StartPoint, line.EndPoint, lineDir, 100, MarkingType.Text)
                //            .Where(x => Math.Abs(x.MarkingNormal.DotProduct(lineDir)) < Tolerance.Global.EqualPoint).ToList();
                //}
                #endregion
            }

            DeleteMarking(removeMark);  //从所有标注中删除已经找到的标注，避免有的标注被重复使用

            return centralizeMark;
        }

        /// <summary>
        /// 检查小旗子的引线是否合法(0.不合法  1.合法,起点从梁开始延申  2.合法,终点从梁开始延申)
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="flagLine"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int CheckFlagLine(Beam beam, Line flagLine, double offset)
        {
            Point3d sP = flagLine.StartPoint;
            Point3d eP = flagLine.EndPoint;
            Polyline beamPLine = beam.BeamBoundary;
            if (offset != 0 && !(beam is ArcBeam))
            {
                beamPLine = GetObjectUtils.ExpansionPolyline(beamPLine, offset);
            }

            int res = GetObjectUtils.CheckPointInPolyline(beamPLine, sP, Tolerance.Global.EqualPoint);
            if (res == 0 || res == 1)
            {
                return 1;
            }

            res = GetObjectUtils.CheckPointInPolyline(beamPLine, eP, Tolerance.Global.EqualPoint);
            if (res == 0 || res == 1)
            {
                return 2;
            }

            return 0;
        }

        /// <summary>
        /// 过滤出合格的集中标注
        /// </summary>
        /// <param name="lineDir"></param>
        /// <param name="markings"></param>
        /// <returns></returns>
        private List<MarkingInfo> CheckFlagTextMarkings(Vector3d lineDir, List<MarkingInfo> markings)
        {
            List<MarkingInfo> textMarks = new List<MarkingInfo>();
            List<MarkingInfo> oMarking = new List<MarkingInfo>(markings);
            var resLMark = oMarking.Where(x => (x.Marking as DBText).TextString.Contains("L")).ToList();
            if (resLMark.Count <= 0)
            {
                return textMarks;
            }
            MarkingInfo lMark = resLMark.First();
            oMarking.Remove(lMark);
            textMarks.Add(lMark);
            oMarking = oMarking.OrderBy(x => x.Position.DistanceTo(lMark.Position))
                .Where(x => (x.Position - lMark.Position).GetNormal().IsParallelTo(lineDir, new Tolerance(0.2, 0.2)))
                .ToList();
            if (oMarking.Count > 0)
            {
                double height = lMark.Position.DistanceTo(oMarking.First().Position);
                MarkingInfo firMark = lMark;
                foreach (var mark in oMarking)
                {
                    if (mark.Position.DistanceTo(firMark.Position) < height * 3 / 2)
                    {
                        textMarks.Add(mark);
                        firMark = mark;
                    }
                    else
                    {
                        return textMarks;
                    }
                }
            }

            return textMarks;
        }

        /// <summary>
        /// 检查每一条集中标注合法性（每一行集中标注都有规则）
        /// </summary>
        /// <param name="markings"></param>
        /// <returns></returns>
        private List<MarkingInfo> CheckEachFlagMarkings(List<MarkingInfo> markings)
        {
            List<MarkingInfo> resMarking = new List<MarkingInfo>();
            for (int i = 0; i < markings.Count; i++)
            {
                string text = (markings[i].Marking as DBText).TextString;
                switch (i)
                {
                    case 0:
                        if (!text.Contains("L"))
                        {
                            return resMarking;
                        }
                        break;
                    case 1:
                        if (!text.Contains("@"))
                        {
                            return resMarking;
                        }
                        break;
                    case 2:
                        break;
                    case 3:
                        if (!(text.Contains("G") || text.Contains("N")))
                        {
                            return resMarking;
                        }
                        break;
                    case 4:
                        if (!(text.Contains("(") && text.Contains(")")))
                        {
                            return resMarking;
                        }
                        break;
                }
                resMarking.Add(markings[i]);
            }
            return resMarking;
        }
        #endregion

        #region 原位标注
        /// <summary>
        /// 获取原位标注
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        private List<MarkingInfo> GetBeamOriginMarking(Beam beam)
        {
            List<MarkingInfo> originMarking = new List<MarkingInfo>();
            List<MarkingInfo> resMarkings = markingService.GetMarking(beam.UpStartPoint, beam.DownEndPoint, beam.BeamNormal, 500, MarkingType.Text)
                    .Where(x => x.MarkingNormal.IsParallelTo(beam.BeamNormal, new Tolerance(0.001, 0.001))).ToList();
            resMarkings = CheckTextMarkings(beam as LineBeam, resMarkings);

            foreach (var mark in resMarkings)
            {
                //防止两个梁都搜索到了同一个标注，需要根据规则计算标注到底属于哪个梁
                var serchMarking = allMarking.Where(x => x.Marking.Id.Handle == mark.Marking.Id.Handle).ToList();
                if (serchMarking.Count() > 0)
                {
                    originMarking.Add(mark);
                    allMarking.Remove(serchMarking.First());
                }
                else
                {
                    bool res = CheckTextMarkingPosition(beam as LineBeam, mark, 400);
                    if (res == true)
                    {
                        var resBeam = allBeams.Where(x => x.OriginMarkings != null && x.OriginMarkings.Where(y => y.Marking.Id.Handle == mark.Marking.Id.Handle).Count() > 0).ToList();
                        if (resBeam.Count > 0)
                        {
                            if (CheckMarkingPositionOfTwoBeam(beam, resBeam.First(), mark))
                            {
                                originMarking.Add(mark);
                                resBeam.First().OriginMarkings.RemoveAll(y => y.Marking.Id.Handle == mark.Marking.Id.Handle);
                            }
                        }
                    }
                }
            }

            return originMarking;
        }

        /// <summary>
        /// 去掉带字母的原位标注
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="markings"></param>
        /// <returns></returns>
        private List<MarkingInfo> CheckTextMarkings(LineBeam beam, List<MarkingInfo> markings)
        {
            List<MarkingInfo> resMarking = new List<MarkingInfo>();
            foreach (var mark in markings)
            {
                var textMark = mark.Marking as DBText;
                if (Regex.Matches(textMark.TextString, "[a-zA-Z]").Count > 0)
                {
                    continue;
                }

                resMarking.Add(mark);
            }
            return resMarking;
        }

        /// <summary>
        /// 检查原位标注位置的合法性
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="marking"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private bool CheckTextMarkingPosition(LineBeam beam, MarkingInfo marking, double tol)
        {
            Point3d upMP = new Point3d((beam.UpStartPoint.X + beam.UpEndPoint.X) / 2,
               (beam.UpStartPoint.Y + beam.UpEndPoint.Y) / 2, 0);
            Point3d downMP = new Point3d((beam.DownStartPoint.X + beam.DownEndPoint.X) / 2,
               (beam.DownStartPoint.Y + beam.DownEndPoint.Y) / 2, 0);
            Vector3d moveDir = (upMP - downMP).GetNormal();
            if (moveDir.Y < 0)
            {
                Point3d tempP = upMP;
                upMP = downMP;
                downMP = upMP;
            }

            double downDis = marking.AlignmentPoint.DistanceTo(downMP);
            if (downDis < marking.AlignmentPoint.DistanceTo(upMP)
                && downDis < tol)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断当前标注到底属于哪个梁
        /// </summary>
        /// <param name="thisBeam"></param>
        /// <param name="secBeam"></param>
        /// <param name="marking"></param>
        /// <returns></returns>
        private bool CheckMarkingPositionOfTwoBeam(Beam thisBeam, Beam secBeam, MarkingInfo marking)
        {
            List<double> firBeamDis = new List<double>()
            {
                thisBeam.UpStartPoint.DistanceTo(marking.Position),
                thisBeam.DownStartPoint.DistanceTo(marking.Position),
                thisBeam.UpEndPoint.DistanceTo(marking.Position),
                thisBeam.DownEndPoint.DistanceTo(marking.Position)
            };

            List<double> secBeamDis = new List<double>()
            {
                secBeam.UpStartPoint.DistanceTo(marking.Position),
                secBeam.DownStartPoint.DistanceTo(marking.Position),
                secBeam.UpEndPoint.DistanceTo(marking.Position),
                secBeam.DownEndPoint.DistanceTo(marking.Position)
            };

            if (firBeamDis.Min() < secBeamDis.Min())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
