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
    public class ThHalfPrimaryBeamLinkExtension : ThBeamLinkExtension
    {
        private List<ThIfcBuildingElement> UnDefinedBeams { get; set; }
        private List<ThBeamLink> PrimaryBeamLinks { get; set; }
        public List<ThBeamLink> HalfPrimaryBeamLinks { get; private set; }
        public ThHalfPrimaryBeamLinkExtension(List<ThIfcBuildingElement> undefinedBeams, List<ThBeamLink> primaryBeamLinks)
        {
            UnDefinedBeams = undefinedBeams;
            PrimaryBeamLinks = primaryBeamLinks;
            HalfPrimaryBeamLinks = new List<ThBeamLink>();
        }        
        public void CreateHalfPrimaryBeamLink()
        {
            for (int i = 0; i < UnDefinedBeams.Count; i++)
            {
                ThIfcBeam currentBeam = UnDefinedBeams[i] as ThIfcBeam;
                if (currentBeam.ComponentType == BeamComponentType.PrimaryBeam)
                {
                    continue;
                }
                ThBeamLink thBeamLink = new ThBeamLink();               
                List<ThIfcBeam> linkElements = new List<ThIfcBeam>() { currentBeam };
                Point3d prePt = PreFindBeamLink(currentBeam.StartPoint, linkElements);
                Point3d backPt = BackFindBeamLink(currentBeam.EndPoint, linkElements);
                ThSingleBeamLink startBeamLink = ConnectionEngine.QuerySingleBeamLink(linkElements[0]);
                thBeamLink.Start = startBeamLink.GetPortVerComponents(prePt);
                ThSingleBeamLink endBeamLink = ConnectionEngine.QuerySingleBeamLink(linkElements[linkElements.Count - 1]);
                thBeamLink.End = endBeamLink.GetPortVerComponents(backPt);
                if (thBeamLink.Start.Count == 0 && thBeamLink.End.Count > 0)
                {
                    // 末端连接竖向构件                    
                    thBeamLink.Start = QueryPortLinkPrimaryBeams(linkElements[0], prePt);
                }
                else if (thBeamLink.Start.Count > 0 && thBeamLink.End.Count == 0)
                {
                    // 起始端连接竖向构件
                    thBeamLink.End = QueryPortLinkPrimaryBeams(linkElements[linkElements.Count - 1], backPt);
                }
                else
                {
                    continue;
                }
                if (JudgeHalfPrimaryBeam(thBeamLink))
                {
                    thBeamLink.Beams = linkElements;
                    thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.HalfPrimaryBeam);
                    HalfPrimaryBeamLinks.Add(thBeamLink);
                }
            }
        }
        private List<ThIfcBuildingElement> QueryPortLinkPrimaryBeams(ThIfcBeam thIfcBeam,Point3d portPt)
        {
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(thIfcBeam); 
            List<ThIfcBeam> thIfcBeams= thSingleBeamLink.GetPortBeams(portPt);
            thIfcBeams= thIfcBeams.Where(o => o.ComponentType == BeamComponentType.PrimaryBeam).ToList();
            if (thIfcBeam is ThIfcLineBeam thIfcLineBeam)
            {
                thIfcBeams.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        return !TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                    }
                    else
                    {
                        return true;
                    }
                });
            }
            return thIfcBeams.Cast<ThIfcBuildingElement>().ToList();
        }
        private Point3d PreFindBeamLink(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[0]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count > 0)
            {
                return portPt;
            }           
            List<ThIfcBeam> linkPrimaryBeams = QueryPortLinkPrimaryBeams(PrimaryBeamLinks,beamLink[0], portPt);
            if(linkPrimaryBeams.Count>0)
            {
                return portPt;
            }
            var linkElements = thSingleBeamLink.GetPortBeams(portPt);
            //从端点连接的梁中过滤只存在于UnDefinedBeams集合里的梁
            linkElements = linkElements
                .Where(m => IsUndefinedBeam(UnDefinedBeams, m))
                .Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
            }
            if (beamLink[0] is ThIfcLineBeam currentLineBeam)
            {
                linkElements = linkElements.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        return TwoBeamCenterLineIsClosed(currentLineBeam, otherLineBeam);
                    }
                    else if (o is ThIfcArcBeam otherArcBeam)
                    {
                        return true;
                    }
                    return false;
                }).ToList();
            }
            if(linkElements.Count==1)
            {
                beamLink.Insert(0, linkElements[0]);
                ThIfcBeam findBeam = linkElements[0] as ThIfcBeam;
                if(findBeam.StartPoint.DistanceTo(portPt)> findBeam.EndPoint.DistanceTo(portPt))
                {
                    return PreFindBeamLink(findBeam.StartPoint, beamLink);
                }
                else
                {
                    return PreFindBeamLink(findBeam.EndPoint, beamLink);
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
            List<ThIfcBeam> linkPrimaryBeams = QueryPortLinkPrimaryBeams(PrimaryBeamLinks,beamLink[beamLink.Count - 1], portPt);
            if (linkPrimaryBeams.Count > 0)
            {
                return portPt;
            }
            var linkElements = thSingleBeamLink.GetPortBeams(portPt); 
            //从端点连接的梁中过滤只存在于UnDefinedBeams集合里的梁
            linkElements = linkElements
                .Where(m => IsUndefinedBeam(UnDefinedBeams, m))
                .Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
            }
            if (beamLink[beamLink.Count-1] is ThIfcLineBeam currentLineBeam)
            {
                linkElements = linkElements.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        return TwoBeamCenterLineIsClosed(currentLineBeam, otherLineBeam);
                    }
                    else if (o is ThIfcArcBeam otherArcBeam)
                    {
                        return true;
                    }
                    return false;
                }).ToList();
            }
            if (linkElements.Count == 1)
            {
                beamLink.Add(linkElements[0]);
                ThIfcBeam findBeam = linkElements[0] as ThIfcBeam;
                if (findBeam.StartPoint.DistanceTo(portPt) > findBeam.EndPoint.DistanceTo(portPt))
                {
                    return BackFindBeamLink(findBeam.StartPoint, beamLink);
                }
                else
                {
                    return BackFindBeamLink(findBeam.EndPoint, beamLink);
                }
            }
            return portPt;
        }
    }
}
