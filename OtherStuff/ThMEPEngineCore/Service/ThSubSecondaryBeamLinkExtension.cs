using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.BeamInfo.Business;
using ThMEPEngineCore.Engine;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Service
{
    public class ThSubSecondaryBeamLinkExtension : ThSecondaryBeamLinkExtension
    {
        public ThSubSecondaryBeamLinkExtension(List<ThIfcBuildingElement> undefinedBeams, List<ThBeamLink> primaryBeamLinks,
            List<ThBeamLink> halfPrimaryBeamLinks, List<ThBeamLink> overhangingPrimaryBeamLinks, List<ThBeamLink> secondaryBeamLinks)
            :base(undefinedBeams, primaryBeamLinks, halfPrimaryBeamLinks, overhangingPrimaryBeamLinks)
        {
            SecondaryBeamLinks = secondaryBeamLinks;
        }   
        /// <summary>
        /// 次次梁
        /// </summary>
        public void CreateSubSecondaryBeamLink()
        {
            for (int i = 0; i < UnDefinedBeams.Count; i++)
            {
                ThIfcBeam currentBeam = UnDefinedBeams[i] as ThIfcBeam;
                if (currentBeam.ComponentType == BeamComponentType.SecondaryBeam)
                {
                    continue;
                }
                ThBeamLink thBeamLink = new ThBeamLink();
                List<ThIfcBeam> linkElements = new List<ThIfcBeam>() { currentBeam };
                Point3d prePt = PreFindBeamLink(currentBeam.StartPoint, linkElements);
                Point3d backPt = BackFindBeamLink(currentBeam.EndPoint, linkElements);
                ThSingleBeamLink startLink = ConnectionEngine.QuerySingleBeamLink(linkElements[0]);
                ThSingleBeamLink endLink = ConnectionEngine.QuerySingleBeamLink(linkElements[linkElements.Count - 1]);
                if (startLink.GetPortVerComponents(prePt).Count == 0 && endLink.GetPortVerComponents(backPt).Count == 0)
                {
                    thBeamLink.Start.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[0], prePt));
                    thBeamLink.Start.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[0], prePt));
                    thBeamLink.Start.AddRange(QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, linkElements[0], prePt));
                    thBeamLink.Start.AddRange(QueryPortLinkSecondaryBeams(SecondaryBeamLinks, linkElements[0], prePt));

                    thBeamLink.End.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[linkElements.Count - 1], backPt));
                    thBeamLink.End.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[linkElements.Count - 1], backPt));
                    thBeamLink.End.AddRange(QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, linkElements[linkElements.Count - 1], backPt));
                    thBeamLink.End.AddRange(QueryPortLinkSecondaryBeams(SecondaryBeamLinks, linkElements[linkElements.Count - 1], backPt));
                }
                else
                {
                    continue;
                }
                if (JudgeSubSecondaryPrimaryBeam(thBeamLink))
                {
                    thBeamLink.Beams = linkElements;
                    thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.SecondaryBeam);
                    SecondaryBeamLinks.Add(thBeamLink);
                }
            }
        }
        public void FindRestBeamLink()
        {
            for (int i = 0; i < UnDefinedBeams.Count; i++)
            {
                ThIfcBeam currentBeam = UnDefinedBeams[i] as ThIfcBeam;
                if (currentBeam.ComponentType == BeamComponentType.SecondaryBeam)
                {
                    continue;
                }
                ThBeamLink thBeamLink = new ThBeamLink();
                List<ThIfcBeam> linkElements = new List<ThIfcBeam>() { currentBeam };
                thBeamLink.Start.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[0], currentBeam.StartPoint));
                thBeamLink.Start.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[0], currentBeam.StartPoint));
                thBeamLink.Start.AddRange(QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, linkElements[0], currentBeam.StartPoint));
                thBeamLink.Start.AddRange(QueryPortLinkSecondaryBeams(SecondaryBeamLinks, linkElements[0], currentBeam.StartPoint));

                thBeamLink.End.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[linkElements.Count - 1], currentBeam.EndPoint));
                thBeamLink.End.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[linkElements.Count - 1], currentBeam.EndPoint));
                thBeamLink.End.AddRange(QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, linkElements[linkElements.Count - 1], currentBeam.EndPoint));
                thBeamLink.End.AddRange(QueryPortLinkSecondaryBeams(SecondaryBeamLinks, linkElements[linkElements.Count - 1], currentBeam.EndPoint));
                thBeamLink.Beams = linkElements;
                thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.SecondaryBeam);
                SecondaryBeamLinks.Add(thBeamLink);
            }
        }
        private Point3d PreFindBeamLink(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[0]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count > 0)
            {
                return portPt;
            }
            //梁端部连接的主梁、半主梁、悬挑主梁、次梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks, beamLink[0], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, beamLink[0], portPt).Count > 0 ||
               QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, beamLink[0], portPt).Count > 0)
            {
                return portPt;
            }
            //查找平行于当前梁的未定义梁
            List<ThIfcBeam> linkElements = QueryPortLinkUndefinedBeams(UnDefinedBeams, beamLink[0], portPt);
            //过滤不在beamLink中的梁
            linkElements = linkElements.Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
            }
            if (linkElements.Count == 1)
            {
                beamLink.Insert(0, linkElements[0]);
                if (linkElements[0].StartPoint.DistanceTo(portPt) > linkElements[0].EndPoint.DistanceTo(portPt))
                {
                    return PreFindBeamLink(linkElements[0].StartPoint, beamLink);
                }
                else
                {
                    return PreFindBeamLink(linkElements[0].EndPoint, beamLink);
                }
            }
            return portPt;
        }
        private Point3d BackFindBeamLink(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[beamLink.Count - 1]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count > 0)
            {
                return portPt;
            }
            //梁端部连接的主梁或半主梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0 ||
               QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0)
            {
                return portPt;
            }
            List<ThIfcBeam> linkElements = QueryPortLinkUndefinedBeams(UnDefinedBeams, beamLink[beamLink.Count - 1], portPt);
            //过滤不存在于beamLink中的梁
            linkElements = linkElements.Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
            }
            if (linkElements.Count == 1)
            {
                beamLink.Add(linkElements[0]);
                if (linkElements[0].StartPoint.DistanceTo(portPt) > linkElements[0].EndPoint.DistanceTo(portPt))
                {
                    return BackFindBeamLink(linkElements[0].StartPoint, beamLink);
                }
                else
                {
                    return BackFindBeamLink(linkElements[0].EndPoint, beamLink);
                }
            }
            return portPt;
        }
    }
}
