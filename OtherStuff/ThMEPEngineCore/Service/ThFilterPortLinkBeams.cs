using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.Algorithm;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Service
{
    public class ThFilterPortLinkBeams
    {
        private ThIfcBeam CurrentBeam { set; get; }
        private Point3d PortPt { get; set; }
        public List<ThIfcBeam> LinkedBeams { get; set; }
        private bool IsStartPort { get; set; } = false;
        private ThFilterPortLinkBeams(ThIfcBeam thIfcBeam,Point3d portPt,List<ThIfcBeam> linkedBeams)
        {
            CurrentBeam = thIfcBeam;
            PortPt = portPt;
            LinkedBeams = linkedBeams;
            if(PortPt.DistanceTo(thIfcBeam.StartPoint)<=1.0)
            {
                IsStartPort = true;
            }
        }
        public static List<ThIfcBeam> Filter(ThIfcBeam thIfcBeam, Point3d portPt, List<ThIfcBeam> linkedBeams)
        {
            ThFilterPortLinkBeams portFilter = new ThFilterPortLinkBeams(thIfcBeam, portPt, linkedBeams);
            portFilter.Filter();
            return portFilter.LinkedBeams;
        }
        /// <summary>
        /// 有与此梁端口在一定范围内连接的梁
        /// </summary>
        /// <returns></returns>
        public static bool HasLinkedBeam(ThIfcBeam thIfcBeam, Point3d portPt, List<ThIfcBeam> linkedBeams)
        {
            ThFilterPortLinkBeams portLink = new ThFilterPortLinkBeams(thIfcBeam, portPt, linkedBeams);
            return portLink.HasLinkedBeam();
        }
        private void Filter()
        {
            LinkedBeams = LinkedBeams.Where(o => o.Uuid != CurrentBeam.Uuid).ToList();
            if(CurrentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                FilterLineBeam(thIfcLineBeam);
            }
            else if(CurrentBeam is ThIfcArcBeam thIfcArcBeam)
            {
                FilterCurveBeam(thIfcArcBeam);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private bool HasLinkedBeam()
        {
            bool result = false;
            LinkedBeams = LinkedBeams.Where(o => o.Uuid != CurrentBeam.Uuid).ToList();
            if (CurrentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                result=JudgeLineBeamLink(thIfcLineBeam);
            }
            else if (CurrentBeam is ThIfcArcBeam thIfcArcBeam)
            {
                result = JudgeArcBeamLink(thIfcArcBeam);
            }
            else
            {
                throw new NotSupportedException();
            }
            return result;
        }
        private void FilterLineBeam(ThIfcLineBeam thIfcLineBeam)
        {
            var parallelBeams = LinkedBeams.Where(o => o is ThIfcLineBeam beam &&
            ThMEPNTSExtension.IsLooseCollinear(thIfcLineBeam.StartPoint, thIfcLineBeam.EndPoint,
            beam.StartPoint, beam.EndPoint)).ToList();
            if (IsStartPort)
            {
                parallelBeams = parallelBeams.OrderBy(o => o.EndPoint.DistanceTo(PortPt)).ToList();
            }
            else
            {
                parallelBeams = parallelBeams.OrderBy(o => o.StartPoint.DistanceTo(PortPt)).ToList();
            }
            for (int i = 1; i < parallelBeams.Count; i++)
            {
                LinkedBeams.Remove(parallelBeams[i]);
            }
        }
        private void FilterCurveBeam(ThIfcArcBeam thIfcArcBeam)
        {            
        }
        private bool JudgeLineBeamLink(ThIfcLineBeam thIfcLineBeam)
        {
            return LinkedBeams.Where(o =>
            {
                if (o is ThIfcLineBeam lineBeam)               
                {
                    if (ThMEPNTSExtension.IsLooseCollinear(thIfcLineBeam.StartPoint, thIfcLineBeam.EndPoint, lineBeam.StartPoint, lineBeam.EndPoint))
                    {
                        if (lineBeam.StartPoint.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                        {
                            return true;
                        }
                        if (lineBeam.EndPoint.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                        {
                            return true;
                        }
                    }                   
                }
                else if(o is ThIfcArcBeam arcBeam)
                {
                    Point3d arcPortPt;
                    if (arcBeam.StartPoint.DistanceTo(PortPt) < arcBeam.EndPoint.DistanceTo(PortPt))
                    {
                        arcPortPt = arcBeam.StartPoint;
                    }
                    else
                    {
                        arcPortPt = arcBeam.EndPoint;
                    }
                    if (arcPortPt.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                    {
                        return true;
                    }
                }
                return false;
            }).Any();
        }
        private bool JudgeArcBeamLink(ThIfcArcBeam thIfcLineBeam)
        {
            Point3d extendPt = PortPt;
            if(IsStartPort)
            {
                extendPt= PortPt + thIfcLineBeam.StartTangent.GetNormal().MultiplyBy(10);
            }
            else
            {
                extendPt = PortPt + thIfcLineBeam.EndTangent.GetNormal().MultiplyBy(10);
            }
            return LinkedBeams.Where(o =>
            {
                if (o is ThIfcLineBeam lineBeam)
                {
                    if(ThMEPNTSExtension.IsLooseCollinear(PortPt, extendPt, lineBeam.StartPoint, lineBeam.EndPoint))
                    {
                        if (lineBeam.StartPoint.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                        {
                            return true;
                        }
                        if (lineBeam.EndPoint.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                        {
                            return true;
                        }
                    }  
                }
                else if (o is ThIfcArcBeam arcBeam)
                {
                    Point3d arcPortPt;
                    if (arcBeam.StartPoint.DistanceTo(PortPt) < arcBeam.EndPoint.DistanceTo(PortPt))
                    {
                        arcPortPt = arcBeam.StartPoint;
                    }
                    else
                    {
                        arcPortPt = arcBeam.EndPoint;
                    }    
                    if(arcPortPt.DistanceTo(PortPt) <= ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                    {
                        return true;
                    }
                }
                return false;
            }).Any();
        }
    }
}
