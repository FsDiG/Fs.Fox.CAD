using System.Linq;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using System.Text.RegularExpressions;
using ThMEPEngineCore.BeamInfo.Model;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public class FillBeamInfo
    {
        public void FillMarkingInfo(List<Beam> allBeams)
        {
            foreach (var beam in allBeams)
            {
                FillCentralizeMarking(beam);
                FillOriginMarking(beam);
            }
        }

        /// <summary>
        /// 为梁填充集中标注
        /// </summary>
        /// <param name="beam"></param>
        private void FillCentralizeMarking(Beam beam)
        {
            List<DBText> cInfo = beam.CentralizeMarkings.Select(x => (x.Marking as DBText)).ToList();
            if (cInfo.Count <= 0)
            {
                return;
            }

            beam.ThCentralizedMarkingP = new ThCentralizedMarking();
            for (int i = 0; i < cInfo.Count; i++)
            {
                string text = cInfo[i].TextString;
                switch (i)
                {
                    case 0:
                        string[] strAry = text.Split(' ');
                        beam.ThCentralizedMarkingP.BeamNum = strAry[0];
                        if (strAry.Count() > 1)
                        {
                            beam.ThCentralizedMarkingP.SectionSize = strAry[1];
                        }
                        break;
                    case 1:
                        beam.ThCentralizedMarkingP.Hooping = text;
                        break;
                    case 2:
                        beam.ThCentralizedMarkingP.ExposedReinforcement = text;
                        break;
                    case 3:
                        beam.ThCentralizedMarkingP.TwistedSteel = text;
                        break;
                    case 4:
                        beam.ThCentralizedMarkingP.LevelDValue = text;
                        break;
                }
            }
        }

        /// <summary>
        /// 为梁添加原位标注
        /// </summary>
        /// <param name="beam"></param>
        private void FillOriginMarking(Beam beam)
        {
            if (beam.OriginMarkings.Count <= 0)
            {
                return;
            }

            beam.ThOriginMarkingcsP = new ThOriginMarkingcs();
            if (beam is LineBeam)
            {
                (beam as LineBeam).GetOrderPoints(out Point3d upLP, out Point3d upMP, out Point3d upRP, out Point3d downLP, out Point3d downMP, out Point3d downRP);
                foreach (var marking in beam.OriginMarkings)
                {
                    DBText oMark = marking.Marking as DBText;
                    string text = oMark.TextString;
                    oMark.GetTextBoxCorners(out Point3d pt1, out Point3d pt2, out Point3d pt3, out Point3d pt4);
                    Point3d mPoint = new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, 0);

                    List<Point3d> allBeamPoints = new List<Point3d>();
                    if (text.Contains("x") || text.Contains("×"))
                    {
                        allBeamPoints = new List<Point3d>() { upMP, downMP, };
                    }
                    else
                    {
                        allBeamPoints = new List<Point3d>() { upLP, upRP, downLP, downRP, };
                        double upDis = upLP.DistanceTo(upMP) / 2, downDis = pt1.DistanceTo(pt2) / 3;
                        if (upDis > downDis)
                        {
                            downDis = upLP.DistanceTo(upMP) / 2;
                            upDis = pt1.DistanceTo(pt2) / 3;
                        }
                        if (mPoint.DistanceTo(upMP) < upDis)
                        {
                            allBeamPoints.Add(upMP);
                        }
                        if (mPoint.DistanceTo(downMP) < downDis)
                        {
                            allBeamPoints.Add(downMP);
                        }
                    }
                    

                    Point3d nearest = allBeamPoints.OrderBy(x => x.DistanceTo(mPoint)).First();
                    if (nearest == upLP)
                    {
                        beam.ThOriginMarkingcsP.UpLeftSteel = text;
                    }
                    else if (nearest == upRP)
                    {
                        beam.ThOriginMarkingcsP.UpRightSteel = text;
                    }
                    else if (nearest == downMP)
                    {
                        if (text.Contains("x") || text.Contains("×"))
                        {
                            beam.ThOriginMarkingcsP.SectionSize = text;
                        }
                        else
                        {
                            beam.ThOriginMarkingcsP.DownErectingBar = text;
                        }
                    }
                    else if (nearest == upMP)
                    {
                        if (text.Contains("x") || text.Contains("×"))
                        {
                            beam.ThOriginMarkingcsP.SectionSize = text;
                        }
                        else if (text.Contains("@"))
                        {
                            beam.ThOriginMarkingcsP.Hooping = text;
                        }
                        else if (text.Contains("G"))
                        {
                            beam.ThOriginMarkingcsP.TwistedSteel = text;
                        }
                        else if (text.First() == '(' && text.Last() == ')')
                        {
                            beam.ThOriginMarkingcsP.LevelDValue = text;
                        }
                        else
                        {
                            beam.ThOriginMarkingcsP.UpErectingBar = text;
                        }
                    }
                }
            }
        }
    }
}
