using System.Linq;
using System.Collections.Generic;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThOverhangingPrimaryBeamLinkExtension : ThBeamLinkExtension
    {
        private List<ThIfcBuildingElement> UnDefinedBeams { get; set; }
        private List<ThBeamLink> PrimaryBeamLinks { get; set; }
        private List<ThBeamLink> HalfPrimaryBeamLinks { get; set; }
        public List<ThBeamLink> OverhangingPrimaryBeamLinks { get; private set; }
        public ThOverhangingPrimaryBeamLinkExtension(List<ThIfcBuildingElement> undefinedBeams,
            List<ThBeamLink> primaryBeamLinks, List<ThBeamLink> halfPrimaryBeamLinks)
        {
            UnDefinedBeams = undefinedBeams;
            PrimaryBeamLinks = primaryBeamLinks;
            HalfPrimaryBeamLinks = halfPrimaryBeamLinks;
            OverhangingPrimaryBeamLinks = new List<ThBeamLink>();
        }
        public void CreateOverhangingPrimaryBeamLink()
        {
            HandleSingleOverhangingPrimaryBeamLink();
            HandleMultiOverhangingPrimaryBeamLink();
            ExtendOverhangingPrimaryBeamLink();
        }
        private void HandleSingleOverhangingPrimaryBeamLink()
        {
            for (int i = 0; i < UnDefinedBeams.Count; i++)
            {
                ThIfcBeam currentBeam = UnDefinedBeams[i] as ThIfcBeam;
                if (currentBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam)
                {
                    continue;
                }
                ThSingleBeamLink beamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);
                ThBeamLink thBeamLink = new ThBeamLink();
                thBeamLink.Start = beamLink.GetPortVerComponents(currentBeam.StartPoint);
                thBeamLink.End = beamLink.GetPortVerComponents(currentBeam.EndPoint);
                List<ThIfcBeam> parallelUndefinedBeams = new List<ThIfcBeam>();
                if (thBeamLink.Start.Count == 0 && thBeamLink.End.Count > 0)
                {
                    // 末端连接竖向构件
                    thBeamLink.Start.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, currentBeam, currentBeam.StartPoint));
                    if (thBeamLink.Start.Count == 0)
                    {                       
                        parallelUndefinedBeams = QueryPortLinkUndefinedBeams(UnDefinedBeams, currentBeam, currentBeam.StartPoint, true);
                    }
                }
                else if (thBeamLink.Start.Count > 0 && thBeamLink.End.Count == 0)
                {
                    thBeamLink.End.AddRange(QueryPortLinkPrimaryBeams(PrimaryBeamLinks, currentBeam, currentBeam.EndPoint));
                    // 起始端连接竖向构件
                    if (thBeamLink.End.Count == 0)
                    {
                        parallelUndefinedBeams = QueryPortLinkUndefinedBeams(UnDefinedBeams, currentBeam, currentBeam.EndPoint, true);
                    }
                }
                else
                {
                    continue;
                }
                if (JudgeOverhangingPrimaryBeam(thBeamLink) && parallelUndefinedBeams.Count==0)
                {
                    thBeamLink.Beams = new List<ThIfcBeam> { currentBeam };
                    thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.OverhangingPrimaryBeam);
                    OverhangingPrimaryBeamLinks.Add(thBeamLink);
                }
            }
            UnDefinedBeams = UnDefinedBeams.Where(o => o is ThIfcBeam thIfcBeam &&
            thIfcBeam.ComponentType == BeamComponentType.Undefined).ToList();
        }
        private void HandleMultiOverhangingPrimaryBeamLink()
        {
            for (int i = 0; i < UnDefinedBeams.Count; i++)
            {
                ThIfcBeam currentBeam = UnDefinedBeams[i] as ThIfcBeam;
                if (currentBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam)
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
                    List<ThIfcBeam> linkPrimaryBeams = QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[0], prePt);
                    if (linkPrimaryBeams.Count == 0)
                    {
                        thBeamLink.Start.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[0], prePt));
                        thBeamLink.Start.AddRange(QueryPortLinkUndefinedBeams(UnDefinedBeams, linkElements[0], prePt, false).ToList());
                    }
                    else
                    {
                        thBeamLink.Start.AddRange(linkPrimaryBeams);
                    }
                }
                else if (thBeamLink.Start.Count > 0 && thBeamLink.End.Count == 0)
                {
                    List<ThIfcBeam> linkPrimaryBeams = QueryPortLinkPrimaryBeams(PrimaryBeamLinks, linkElements[linkElements.Count - 1], backPt);
                    // 起始端连接竖向构件
                    if (linkPrimaryBeams.Count == 0)
                    {
                        thBeamLink.End.AddRange(QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, linkElements[linkElements.Count - 1], backPt));
                        thBeamLink.End.AddRange(QueryPortLinkUndefinedBeams(UnDefinedBeams, linkElements[linkElements.Count - 1], backPt, false).ToList());
                    }
                    else
                    {
                        thBeamLink.End.AddRange(linkPrimaryBeams);
                    }
                }
                else
                {
                    continue;
                }
                if (JudgeOverhangingPrimaryBeam(thBeamLink))
                {
                    thBeamLink.Beams = linkElements;
                    thBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.OverhangingPrimaryBeam);
                    OverhangingPrimaryBeamLinks.Add(thBeamLink);
                }
            }
            UnDefinedBeams = UnDefinedBeams.Where(o => o is ThIfcBeam thIfcBeam &&
            thIfcBeam.ComponentType == BeamComponentType.Undefined).ToList();
        }
        private Point3d PreFindBeamLink(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[0]);
            if(thSingleBeamLink.GetPortVerComponents(portPt).Count>0)
            {
                return portPt;
            }
            //梁端部连接的主梁或半主梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks,beamLink[0], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, beamLink[0], portPt).Count > 0 ||
               QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, beamLink[0], portPt).Count>0)
            {
                return portPt;
            }
            //查找平行于当前梁的未定义梁
            List<ThIfcBeam> linkElements = QueryPortLinkUndefinedBeams(UnDefinedBeams,beamLink[0], portPt);
            //过滤不在beamLink中的梁
            linkElements = linkElements.Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
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
            //梁端部连接的主梁或半主梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks,beamLink[beamLink.Count - 1], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks,beamLink[beamLink.Count - 1], portPt).Count > 0 ||
               QueryPortLinkOverhangingPrimaryBeams(OverhangingPrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0)
            {
                return portPt;
            }
            List<ThIfcBeam> linkElements = QueryPortLinkUndefinedBeams(UnDefinedBeams,beamLink[beamLink.Count - 1], portPt);
            //过滤不存在于beamLink中的梁
            linkElements = linkElements.Where(m => !beamLink.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            if (linkElements.Count == 0)
            {
                return portPt;
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
        private Point3d PreFindBeamLinkExtension(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[0]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count > 0)
            {
                return portPt;
            }
            //梁端部连接的主梁或半主梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks, beamLink[0], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, beamLink[0], portPt).Count > 0)
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
                    return PreFindBeamLinkExtension(linkElements[0].StartPoint, beamLink);
                }
                else
                {
                    return PreFindBeamLinkExtension(linkElements[0].EndPoint, beamLink);
                }
            }
            return portPt;
        }
        private Point3d BackFindBeamLinkExtension(Point3d portPt, List<ThIfcBeam> beamLink)
        {
            //端点连接竖向构件则返回
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(beamLink[beamLink.Count - 1]);
            if (thSingleBeamLink.GetPortVerComponents(portPt).Count > 0)
            {
                return portPt;
            }
            //梁端部连接的主梁或半主梁，则停止查找
            if (QueryPortLinkPrimaryBeams(PrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0 ||
               QueryPortLinkHalfPrimaryBeams(HalfPrimaryBeamLinks, beamLink[beamLink.Count - 1], portPt).Count > 0 
              )
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
                    return BackFindBeamLinkExtension(linkElements[0].StartPoint, beamLink);
                }
                else
                {
                    return BackFindBeamLinkExtension(linkElements[0].EndPoint, beamLink);
                }
            }
            return portPt;
        }
        private void ExtendOverhangingPrimaryBeamLink()
        {
            var portLinkedUndefinedBeams=FilterPortLinkedUndefinedBeams();
            UnDefinedBeams = UnDefinedBeams.Where(m => !portLinkedUndefinedBeams.Where(n => n.Uuid == m.Uuid).Any()).ToList();
            for (int i = 0; i < this.OverhangingPrimaryBeamLinks.Count;i++)
            {
                var currentBeamLink = this.OverhangingPrimaryBeamLinks[i];
                ThIfcBeam findBeam;
                if(currentBeamLink.StartHasVerticalComponent)
                {
                    findBeam = currentBeamLink.Beams[currentBeamLink.Beams.Count - 1];
                    Point3d backPt = BackFindBeamLinkExtension(findBeam.EndPoint, currentBeamLink.Beams);
                    var lastBeam = currentBeamLink.Beams[currentBeamLink.Beams.Count - 1];                   
                    currentBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.OverhangingPrimaryBeam);
                    currentBeamLink.End.AddRange(QueryPortLinkOverhangingPrimaryBeams
                        (OverhangingPrimaryBeamLinks, lastBeam, backPt));
                    currentBeamLink.End.AddRange(QueryPortLinkUndefinedBeams
                        (UnDefinedBeams, lastBeam, backPt, false));
                    currentBeamLink.End.AddRange(QueryPortLinkUndefinedBeams
                        (portLinkedUndefinedBeams, lastBeam, backPt,false));
                }
                else
                {
                    findBeam = currentBeamLink.Beams[0];
                    Point3d prePt = PreFindBeamLinkExtension(findBeam.StartPoint, currentBeamLink.Beams);                   
                    currentBeamLink.Beams.ForEach(o => o.ComponentType = BeamComponentType.OverhangingPrimaryBeam);
                    currentBeamLink.Start.AddRange(QueryPortLinkOverhangingPrimaryBeams
                        (OverhangingPrimaryBeamLinks, currentBeamLink.Beams[0], prePt));
                    currentBeamLink.Start.AddRange(QueryPortLinkUndefinedBeams
                        (UnDefinedBeams, currentBeamLink.Beams[0], prePt, false));
                    currentBeamLink.Start.AddRange(QueryPortLinkUndefinedBeams
                        (portLinkedUndefinedBeams, currentBeamLink.Beams[0], prePt, false));
                }
            }            
        }
        /// <summary>
        /// 过滤两边都有连接或任意一端连接竖向构件的未定义梁
        /// </summary>
        private List<ThIfcBuildingElement> FilterPortLinkedUndefinedBeams()
        {
            //在剩余非定义梁中，把任意一端连有竖向构件、主梁、非主梁、或两端都是悬挑主梁的去掉
            return UnDefinedBeams.Where(o =>
            {
                var beam = o as ThIfcBeam;
                ThSingleBeamLink beamLink = ConnectionEngine.QuerySingleBeamLink(beam);
                if (beamLink.GetPortVerComponents(beam.StartPoint).Count > 0 ||
                beamLink.GetPortVerComponents(beam.EndPoint).Count > 0)
                {
                    return true;
                }
                var startBeams = beamLink.GetPortBeams(beam.StartPoint);
                var endBeams = beamLink.GetPortBeams(beam.EndPoint);
                bool startHasPrimryBeam = startBeams.Where(n => n.ComponentType == BeamComponentType.PrimaryBeam ||
                  n.ComponentType == BeamComponentType.HalfPrimaryBeam ||
                  n.ComponentType == BeamComponentType.OverhangingPrimaryBeam).Any();
                bool endHasPrimryBeam = endBeams.Where(n => n.ComponentType == BeamComponentType.PrimaryBeam ||
                  n.ComponentType == BeamComponentType.HalfPrimaryBeam ||
                  n.ComponentType == BeamComponentType.OverhangingPrimaryBeam).Any();
                if (startHasPrimryBeam && endHasPrimryBeam)
                {
                    return true;
                }
                return false;
            }).ToList();
        }
    }
}
