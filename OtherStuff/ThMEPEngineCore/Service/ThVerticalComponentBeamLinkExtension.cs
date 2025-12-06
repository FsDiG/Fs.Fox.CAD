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
    public class ThVerticalComponentBeamLinkExtension : ThBeamLinkExtension
    {
        private List<ThBeamLink> PrimaryBeamLinks { get; set; }
        private List<ThIfcBuildingElement> UnDefinedBeams = new List<ThIfcBuildingElement>();
        public ThVerticalComponentBeamLinkExtension(List<ThIfcBuildingElement> undefinedBeams, List<ThBeamLink> primaryBeamLinks)
        {
            UnDefinedBeams = undefinedBeams;
            PrimaryBeamLinks = primaryBeamLinks;
        }
        public void CreatePrimaryBeamLink()
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
                thBeamLink.Start = QueryPortLinkElements(linkElements[0], prePt,ThMEPEngineCoreCommon.BeamComponentConnectionTolerance);
                thBeamLink.End = QueryPortLinkElements(linkElements[linkElements.Count - 1], backPt, ThMEPEngineCoreCommon.BeamComponentConnectionTolerance);
                if (JudgePrimaryBeam(thBeamLink))
                {
                    thBeamLink.Beams = linkElements;
                    thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.PrimaryBeam);
                    PrimaryBeamLinks.Add(thBeamLink);
                }
            }
        }
        private Point3d PreFindBeamLink(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[0]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count>0)
            {
                return portPt;
            }
            //端点连接的梁
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
            //从端点连接的梁中过滤只存在于UnDefinedBeams集合里的梁
            linkElements = linkElements
                .Where(m => IsUndefinedBeam(UnDefinedBeams,m))
                .Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count==0)
            {
                return portPt;
            }
            if(beamLink[0] is ThIfcLineBeam currentLineBeam)
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
                if(beamLink.Where(o=>o.Uuid== linkElements[0].Uuid).Any())
                {
                    return portPt;
                }
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
            //端点连接的梁
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
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
                if (beamLink.Where(o => o.Uuid == linkElements[0].Uuid).Any())
                {
                    return portPt;
                }
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
