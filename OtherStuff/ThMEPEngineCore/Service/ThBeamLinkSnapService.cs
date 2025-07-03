using System;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Extension;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Service
{
    public class ThBeamLinkSnapService
    {
        public ThBeamLink BeamLink { get; private set; }
        private const double extraExtendDistance = 5.0;
        public List<ThIfcBeam> Adds { get; set; }
        public List<ThIfcBeam> Removes { get; set; }
        public ThBeamLinkSnapService(ThBeamLink thBeamLink)
        {
            BeamLink = thBeamLink;
            Adds = new List<ThIfcBeam>();
            Removes = new List<ThIfcBeam>();
        }
        public static ThBeamLinkSnapService Snap(ThBeamLink thBeamLink)
        {
            ThBeamLinkSnapService thBeamLinkSnapService = new ThBeamLinkSnapService(thBeamLink);
            thBeamLinkSnapService.Snap();
            return thBeamLinkSnapService;
        }
        private void Snap()
        {
            for (int i = 0; i < BeamLink.Beams.Count; i++)
            {
                double startExtendDis = 0.0;
                double endExtendDis = 0.0;
                var currentBeam = BeamLink.Beams[i];
                if (i == 0)
                {
                    startExtendDis = SnapTo(currentBeam, currentBeam.StartPoint, BeamLink.Start);
                    if (BeamLink.Beams.Count == 1)
                    {
                        endExtendDis = SnapTo(currentBeam, currentBeam.EndPoint,BeamLink.End);
                    }
                    else
                    {
                        endExtendDis = SnapTo(currentBeam, currentBeam.EndPoint, 
                            new List<ThIfcBuildingElement> { BeamLink.Beams[i + 1] });
                    }
                }
                else if (i == BeamLink.Beams.Count - 1)
                {
                    endExtendDis = SnapTo(currentBeam, currentBeam.EndPoint,BeamLink.End);
                    startExtendDis = SnapTo(currentBeam, currentBeam.StartPoint,
                        new List<ThIfcBuildingElement> { BeamLink.Beams[i - 1] });
                }
                else
                {
                    startExtendDis = SnapTo(currentBeam, currentBeam.StartPoint,
                        new List<ThIfcBuildingElement> { BeamLink.Beams[i - 1] });
                    endExtendDis = SnapTo(currentBeam, currentBeam.EndPoint,
                        new List<ThIfcBuildingElement> { BeamLink.Beams[i + 1] });
                }
                if (startExtendDis > 0 || endExtendDis > 0)
                {
                    if(currentBeam is ThIfcLineBeam lineBeam)
                    {
                        Adds.Add(ThIfcLineBeam.Create(lineBeam, startExtendDis, endExtendDis));
                        Removes.Add(currentBeam);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }                    
                }
            }
        }
        private double SnapTo(ThIfcBeam currentBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            if (currentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                return SnapTo(thIfcLineBeam, portPt, neighbors);
            }
            else if (currentBeam is ThIfcArcBeam thIfcArcBeam)
            {
                return SnapTo(thIfcArcBeam, portPt, neighbors);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private bool Intersects(ThIfcLineBeam lineBeam, ThIfcBuildingElement neighbor)
        {
            var rectangle = lineBeam.Outline as Polyline;
            return rectangle.Intersects(neighbor.Outline as Entity);
        }

        private double SnapTo(ThIfcLineBeam lineBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            double extendDis = 0.0;
            ThIfcBuildingElement neighbor = null;
            if(neighbors.Where(o => o is ThIfcColumn || o is ThIfcWall).Any())
            {
                neighbor = FindClosestNeighborComponent(lineBeam, portPt, neighbors);
            }
            else if(neighbors.Where(o => o is ThIfcBeam).Any())
            {
                neighbor = FindClosestNeighborBeam(lineBeam, portPt, neighbors);
            }
            else
            {
                return extendDis;
            }
            if (neighbor != null)
            {
                Point3dCollection intersectPts = new Point3dCollection();
                Line centerLine = new Line(lineBeam.StartPoint, lineBeam.EndPoint);
                centerLine.IntersectWith(neighbor.Outline, Intersect.ExtendThis, intersectPts, IntPtr.Zero, IntPtr.Zero);
                if (intersectPts.Count > 0)
                {
                    var closestPt = intersectPts.Cast<Point3d>()
                        .OrderBy(o => portPt.DistanceTo(o)).First();
                    extendDis = portPt.DistanceTo(closestPt);
                    extendDis += extraExtendDistance;
                }
                else
                {
                    extendDis = 0.0;
                }
            }
            return extendDis;
        }

        private double SnapTo(ThIfcArcBeam arcBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            throw new NotSupportedException();
        }
        private ThIfcBuildingElement FindClosestNeighborComponent(ThIfcBeam thIfcBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            if (thIfcBeam is ThIfcLineBeam thIfcLineBeam)
            {
                return FindClosestNeighborComponent(thIfcLineBeam, portPt, neighbors);
            }
            else if(thIfcBeam is ThIfcArcBeam thIfcArcBeam)
            {
                return FindClosestNeighborComponent(thIfcArcBeam, portPt, neighbors);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private ThIfcBuildingElement FindClosestNeighborComponent(ThIfcLineBeam thIfcLineBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            if (neighbors.Where(o => Intersects(thIfcLineBeam, o)).Any())
            {
                return null;
            }
            else
            {
                var closedElements = GetLineClosedElements(neighbors,
                    new Line(thIfcLineBeam.StartPoint, thIfcLineBeam.EndPoint), portPt);
                return closedElements.Count > 0 ? closedElements.OrderBy(o => portPt.DistanceTo(o.Item2)).First().Item1 : null;
            }
        }
        private ThIfcBuildingElement FindClosestNeighborComponent(ThIfcArcBeam thIfcArcBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            return null;
        }
        private ThIfcBuildingElement FindClosestNeighborBeam(ThIfcLineBeam thIfcLineBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            if (neighbors.Where(o => Intersects(thIfcLineBeam, o)).Any())
            {
                return null;
            }
            else
            {
                bool isStartPort = portPt.DistanceTo(thIfcLineBeam.StartPoint) <= 1.0;
                Polyline outline = thIfcLineBeam.Outline as Polyline;
                Point3d firstSp = outline.GetPoint3dAt(0);
                Point3d firstEp = outline.GetPoint3dAt(3);
                Point3d firstPt = isStartPort ? firstSp : firstEp;
                var firstClosedElements = GetLineClosedElements(neighbors, new Line(firstSp, firstEp), firstPt);
                Point3d secondSp = outline.GetPoint3dAt(1);
                Point3d secondEp = outline.GetPoint3dAt(2);
                Point3d secondPt = isStartPort ? secondSp : secondEp;
                var secondeClosedElements = GetLineClosedElements(neighbors, new Line(secondSp, secondEp), secondPt);
                var firstClosedElement = firstClosedElements.Count > 0 ? firstClosedElements.OrderBy(o => firstPt.DistanceTo(o.Item2)).First() : null;
                var secondClosedElement = secondeClosedElements.Count > 0 ? secondeClosedElements.OrderBy(o => secondPt.DistanceTo(o.Item2)).First() : null;
                if (firstClosedElement != null && secondClosedElement != null)
                {
                    if(firstClosedElement.Item2.DistanceTo(firstPt) < ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance ||
                        secondClosedElement.Item2.DistanceTo(secondPt) < ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)
                    {
                        return null;
                    }
                    var firstProjectPt = firstClosedElement.Item2.GetProjectPtOnLine(thIfcLineBeam.StartPoint, thIfcLineBeam.EndPoint);
                    var secondProjectPt = secondClosedElement.Item2.GetProjectPtOnLine(thIfcLineBeam.StartPoint, thIfcLineBeam.EndPoint);
                    if (firstProjectPt.DistanceTo(portPt) < secondProjectPt.DistanceTo(portPt))
                    {
                        return firstClosedElement.Item1;
                    }
                    else
                    {
                        return secondClosedElement.Item1;
                    }
                }
                else if (firstClosedElement != null)
                {                    
                    return (firstClosedElement.Item2.DistanceTo(firstPt) < 
                        ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance)?null: firstClosedElement.Item1;
                }
                else if (secondClosedElement != null)
                {
                    return (secondClosedElement.Item2.DistanceTo(secondPt) <
                        ThMEPEngineCoreCommon.BeamIntervalMinimumTolerance) ? null : secondClosedElement.Item1;
                }
                else
                {
                    return null;
                }
            }
        }
        private ThIfcBuildingElement FindClosestNeighborBeam(ThIfcArcBeam thIfcArcBeam, Point3d portPt, List<ThIfcBuildingElement> neighbors)
        {
            return null;
        }
        /// <summary>
        /// 获取梁两边长线与邻居,由近及远依次相交的构件
        /// </summary>
        /// <param name="neighbors"></param>
        /// <param name="line"></param>
        /// <param name="isStart"></param>
        /// <returns></returns>
        private List<Tuple<ThIfcBuildingElement, Point3d>> GetLineClosedElements(List<ThIfcBuildingElement> neighbors, Line line, Point3d portPt)
        {
            List<Tuple<ThIfcBuildingElement, Point3d>> intersects = new List<Tuple<ThIfcBuildingElement, Point3d>>();
            neighbors.ForEach(o =>
            {
                Point3dCollection intersectPts = new Point3dCollection();
                line.IntersectWith(o.Outline, Intersect.ExtendThis, intersectPts, IntPtr.Zero, IntPtr.Zero);
                if (intersectPts.Count > 0)
                {
                    var closestPt = intersectPts.Cast<Point3d>()
                        .OrderBy(m => portPt.DistanceTo(m)).First();
                    intersects.Add(Tuple.Create(o, closestPt));
                }
            });
            return intersects;
        }
    }
}
