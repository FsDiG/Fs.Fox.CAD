using System;
using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Engine
{
    public class ThSnapBeamEngine : ThBeamPreprocessEngine
    {
        private ThBeamConnectRecogitionEngine BeamConnectRecogitionEngine { get; set; }
        private ThBeamLinkExtension ThBeamLinkEx;
        private List<ThIfcBeam> adds;
        private List<ThIfcBeam> removes;
        public ThSnapBeamEngine(ThBeamConnectRecogitionEngine thBeamConnectRecogitionEngine)
        {
            BeamConnectRecogitionEngine = thBeamConnectRecogitionEngine;
            ThBeamLinkEx = new ThBeamLinkExtension
            {
                ConnectionEngine = BeamConnectRecogitionEngine
            };
        }
        public void Snap()
        {
            SnapToComponent();
            SnapToBeam();
        }

        private void SnapToComponent()
        {
            adds = new List<ThIfcBeam>();
            removes = new List<ThIfcBeam>();
            BeamConnectRecogitionEngine.BeamEngine.Elements.ForEach(o => SnapToComponent(o as ThIfcBeam));
            if (adds.Count > 0)
            {
                BeamConnectRecogitionEngine.BeamEngine.Elements.RemoveAll(o => removes.Contains(o));
                BeamConnectRecogitionEngine.BeamEngine.Elements.AddRange(adds);
                BeamConnectRecogitionEngine.SyncBeamSpatialIndex();
            }
        }
        private void SnapToBeam()
        {
            adds = new List<ThIfcBeam>();
            removes = new List<ThIfcBeam>();
            BeamConnectRecogitionEngine.BeamEngine.Elements.ForEach(o => SnapToBeam(o as ThIfcBeam));
            if (adds.Count > 0)
            {
                BeamConnectRecogitionEngine.BeamEngine.Elements.RemoveAll(o => removes.Contains(o));
                BeamConnectRecogitionEngine.BeamEngine.Elements.AddRange(adds);
                BeamConnectRecogitionEngine.SyncBeamSpatialIndex();
            }
        }
        private void SnapToComponent(ThIfcBeam thifcBeam)
        {
            if (thifcBeam is ThIfcLineBeam thIfcLineBeam)
            {
                SnapToComponent(thIfcLineBeam);
            }
            else if (thifcBeam is ThIfcArcBeam thIfcArcBeam)
            {
                SnapToComponent(thIfcArcBeam);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private void SnapToBeam(ThIfcBeam thifcBeam)
        {
            if (thifcBeam is ThIfcLineBeam thIfcLineBeam)
            {
                SnapToBeam(thIfcLineBeam);
            }
            else if (thifcBeam is ThIfcArcBeam thIfcArcBeam)
            {
                SnapToBeam(thIfcArcBeam);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private void SnapToComponent(ThIfcLineBeam thIfcLineBeam)
        {
            ThBeamLink thBeamLink = new ThBeamLink();
            thBeamLink.Beams.Add(thIfcLineBeam);
            thBeamLink.Start = GetPortLinkObjs(thIfcLineBeam, thIfcLineBeam.StartPoint);
            thBeamLink.End = GetPortLinkObjs(thIfcLineBeam, thIfcLineBeam.EndPoint);
            var instance = ThBeamLinkSnapService.Snap(thBeamLink);
            adds.AddRange(instance.Adds);
            removes.AddRange(instance.Removes);
        }
        private void SnapToComponent(ThIfcArcBeam thIfcArcBeam)
        {
            throw new NotSupportedException();
        }
        private void SnapToBeam(ThIfcLineBeam thIfcLineBeam)
        {
            ThBeamLink thBeamLink = new ThBeamLink();
            thBeamLink.Beams.Add(thIfcLineBeam);
            thBeamLink.Start = GetPortUnParallelBeams(thIfcLineBeam, thIfcLineBeam.StartPoint);
            thBeamLink.End = GetPortUnParallelBeams(thIfcLineBeam, thIfcLineBeam.EndPoint);
            var instance = ThBeamLinkSnapService.Snap(thBeamLink);
            adds.AddRange(instance.Adds);
            removes.AddRange(instance.Removes);
        }
        private void SnapToBeam(ThIfcArcBeam thIfcArcBeam)
        {
            throw new NotSupportedException();
        }
        private List<ThIfcBuildingElement> GetPortLinkObjs(ThIfcBeam thIfcBeam, Point3d portPt)
        {
            List<ThIfcBuildingElement> results = new List<ThIfcBuildingElement>();
            var linkBeams = GetPortLinkCollinearBeams(thIfcBeam, portPt);
            if (linkBeams.Count == 0)
            {
                var linkComponents = ThBeamLinkEx.QueryPortLinkElements(thIfcBeam,
                portPt, ThMEPEngineCoreCommon.BeamComponentConnectionTolerance);
                linkComponents.ForEach(o => results.Add(o));
            }
            return results;
        }
        private List<ThIfcBuildingElement> GetPortUnParallelBeams(ThIfcBeam thIfcBeam, Point3d portPt)
        {
            List<ThIfcBuildingElement> results = new List<ThIfcBuildingElement>();
            var linkBeams = GetPortLinkCollinearBeams(thIfcBeam, portPt);
            if (linkBeams.Count == 0)
            {
                var linkComponents = ThBeamLinkEx.QueryPortLinkElements(thIfcBeam,
                portPt, ThMEPEngineCoreCommon.BeamComponentConnectionTolerance);
                if (linkComponents.Count > 0)
                {
                    return results;
                }
            }
            //如果端点没有竖向构件，查找不连接的，类似于T型连接的梁
            results.AddRange(GetPortLinkTTypeBeams(thIfcBeam, portPt));
            return results;
        }
        private List<ThIfcBeam> GetPortLinkCollinearBeams(ThIfcBeam thIfcBeam, Point3d portPt)
        {
            if (thIfcBeam is ThIfcLineBeam lineBeam)
            {
                return GetPortLinkCollinearBeams(lineBeam, portPt);
            }
            else if (thIfcBeam is ThIfcArcBeam arcBeam)
            {
                return GetPortLinkCollinearBeams(arcBeam, portPt);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private List<ThIfcBeam> GetPortLinkCollinearBeams(ThIfcLineBeam thIfcLineBeam, Point3d portPt)
        {
            var linkBeams = ThBeamLinkEx.QueryPortLinkBeams(thIfcLineBeam,
                   portPt, 0.5, ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance);
            return linkBeams.Where(o =>
            {
                if (o is ThIfcLineBeam lineBeam)
                {
                    return ThStructureBeamUtils.IsLooseCollinear(thIfcLineBeam, lineBeam);
                }
                else
                {
                    return ThStructureBeamUtils.IsLooseCollinear(thIfcLineBeam, portPt, o as ThIfcArcBeam);
                }
            }).ToList();
        }

        private List<ThIfcBeam> GetPortLinkCollinearBeams(ThIfcArcBeam thIfcArcBeam, Point3d portPt)
        {
            throw new NotSupportedException();
        }
        private List<ThIfcBeam> GetPortLinkTTypeBeams(ThIfcBeam thIfcBeam, Point3d portPt)
        {
            if (thIfcBeam is ThIfcLineBeam lineBeam)
            {
                return GetPortLinkTTypeBeams(lineBeam, portPt);
            }
            else if (thIfcBeam is ThIfcArcBeam arcBeam)
            {
                return GetPortLinkTTypeBeams(arcBeam, portPt);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private List<ThIfcBeam> GetPortLinkTTypeBeams(ThIfcLineBeam thIfcLineBeam, Point3d portPt)
        {
            var linkBeams = ThBeamLinkEx.QueryPortLinkBeams(thIfcLineBeam,
                   portPt, 1.0, ThMEPEngineCoreCommon.BeamIntervalMaximumTolerance);
            return linkBeams.Where(o =>
            {
                if (o is ThIfcLineBeam lineBeam)
                {
                    return ThStructureBeamUtils.IsSpacedTType(thIfcLineBeam, lineBeam);
                }
                else
                {
                    return ThStructureBeamUtils.IsSpacedTType(thIfcLineBeam, o as ThIfcArcBeam);
                }
            }).ToList();
        }

        private List<ThIfcBeam> GetPortLinkTTypeBeams(ThIfcArcBeam thIfcArcBeam, Point3d portPt)
        {
            throw new NotSupportedException();
        }
    }
}
