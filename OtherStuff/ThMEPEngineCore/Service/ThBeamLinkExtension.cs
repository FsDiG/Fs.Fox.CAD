using System;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Engine;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad;
using ThMEPEngineCore.Algorithm;

namespace ThMEPEngineCore.Service
{
    public class ThBeamLinkExtension
    {
        public ThBeamConnectRecogitionEngine ConnectionEngine { get; set; }
        private ThColumnRecognitionEngine ColumnEngine
        {
            get
            {
                return ConnectionEngine.ColumnEngine;
            }
        }
        private ThBuildingElementRecognitionEngine BeamEngine
        {
            get
            {
                return ConnectionEngine.BeamEngine;
            }
        }
        private ThShearWallRecognitionEngine ShearWallEngine
        {
            get
            {
                return ConnectionEngine.ShearWallEngine;
            }
        }
        private ThSpatialIndexManager SpatialIndexManager
        {
            get
            {
                return ConnectionEngine.SpatialIndexManager;
            }
        }

        public ThBeamLinkExtension()
        {
        }

        public ThBeamLink CreateSinglePrimaryBeamLink(ThIfcBeam Beam)
        {
            ThBeamLink beamLink = new ThBeamLink();            
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(Beam);
            beamLink.Start = thSingleBeamLink.StartVerComponents;
            beamLink.End = thSingleBeamLink.EndVerComponents;
            if (JudgePrimaryBeam(beamLink))
            {
                Beam.ComponentType = BeamComponentType.PrimaryBeam;
                beamLink.Beams.Add(Beam);
            }
            return beamLink;
        }
        public bool JudgePrimaryBeam(ThBeamLink thBeamLink)
        {
            var startLink = thBeamLink.Start.Where(o=>o.GetType()==typeof(ThIfcColumn) || o.GetType() == typeof(ThIfcWall));
            var endLink = thBeamLink.End.Where(o => o.GetType() == typeof(ThIfcColumn) || o.GetType() == typeof(ThIfcWall));
            if(startLink.Any() && endLink.Any())
            {
                return true;
            }
            return false;
        }
        public bool JudgeHalfPrimaryBeam(ThBeamLink thBeamLink)
        {
            if (JudgePrimaryBeam(thBeamLink))
            {
                return false;
            }
            var startLinkComponent = thBeamLink.Start.Where(o => o.GetType() == typeof(ThIfcColumn) || o.GetType() == typeof(ThIfcWall));
            if(startLinkComponent.Any())
            {
                var endLinkPrimaryBeam = thBeamLink.End.Where(o => o is ThIfcBeam thIfcBeam && thIfcBeam.ComponentType==BeamComponentType.PrimaryBeam);
                if(endLinkPrimaryBeam.Any())
                {
                    return true;
                }
            }
            var endLinkComponent = thBeamLink.End.Where(o => o is ThIfcColumn || o is ThIfcWall);
            if (endLinkComponent.Any())
            {
                var startLinkPrimaryBeam = thBeamLink.Start.Where(o => o is ThIfcBeam thIfcBeam && thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam);
                if (startLinkPrimaryBeam.Any())
                {
                    return true;
                }
            }
            return false;
        }
        public bool JudgeOverhangingPrimaryBeam(ThBeamLink thBeamLink)
        {
            if (JudgePrimaryBeam(thBeamLink))
            {
                return false;
            }
            var startLinkComponent = thBeamLink.Start.Where(o => o.GetType() == typeof(ThIfcColumn) || o.GetType() == typeof(ThIfcWall));
            if (startLinkComponent.Any()) 
            {
                //起始端有竖向构件,末端非主梁
                return !thBeamLink.End.Where(o => o is ThIfcBeam thIfcBeam && thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam).Any();
            }
            var endLinkComponent = thBeamLink.End.Where(o => o is ThIfcColumn || o is ThIfcWall);
            if (endLinkComponent.Any())
            {
                //末端有竖向构件,起始端非主梁
                return !thBeamLink.Start.Where(o => o is ThIfcBeam thIfcBeam && thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam).Any();
            }
            return false;
        }
        public bool JudgeSecondaryPrimaryBeam(ThBeamLink thBeamLink)
        {
            if (JudgePrimaryBeam(thBeamLink))
            {
                return false;
            }
            var startLinkComponent = thBeamLink.Start.Where(o => o is ThIfcColumn || o is ThIfcWall);
            var endLinkComponent = thBeamLink.End.Where(o => o is ThIfcColumn || o is ThIfcWall);
            if (startLinkComponent.Any() || endLinkComponent.Any())
            {
                return false;
            }
            var startLinkBeam = thBeamLink.Start.Where(o => o is ThIfcBeam thIfcBeam &&
            (thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.HalfPrimaryBeam ||
            thIfcBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam));
            var endLinkBeam = thBeamLink.End.Where(o => o is ThIfcBeam thIfcBeam && 
            (thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.HalfPrimaryBeam ||
            thIfcBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam));
            return startLinkBeam.Any() && endLinkBeam.Any();
        }
        public bool JudgeSubSecondaryPrimaryBeam(ThBeamLink thBeamLink)
        {
            //后期根据规则调整
            if (JudgePrimaryBeam(thBeamLink))
            {
                return false;
            }
            var startLinkComponent = thBeamLink.Start.Where(o => o is ThIfcColumn || o is ThIfcWall);
            var endLinkComponent = thBeamLink.End.Where(o => o is ThIfcColumn || o is ThIfcWall);
            if (startLinkComponent.Any() || endLinkComponent.Any())
            {
                return false;
            }
            var startLinkBeam = thBeamLink.Start.Where(o => o is ThIfcBeam thIfcBeam &&
            (thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.HalfPrimaryBeam ||
            thIfcBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.SecondaryBeam));
            var endLinkBeam = thBeamLink.End.Where(o => o is ThIfcBeam thIfcBeam &&
            (thIfcBeam.ComponentType == BeamComponentType.PrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.HalfPrimaryBeam ||
            thIfcBeam.ComponentType == BeamComponentType.OverhangingPrimaryBeam || thIfcBeam.ComponentType == BeamComponentType.SecondaryBeam));
            return startLinkBeam.Any() && endLinkBeam.Any();
        }

        public List<ThIfcBuildingElement> QueryPortLinkElements(ThIfcBeam thIfcBeam, Point3d portPt,double beamConnectionTolerance)
        {
            List<ThIfcBuildingElement> links = new List<ThIfcBuildingElement>();
            DBObjectCollection linkObjs = new DBObjectCollection();
            if (thIfcBeam is ThIfcLineBeam thIfcLineBeam)
            {
                Polyline portSearchEnvelope = GetLineBeamPortSearchEnvelope(thIfcLineBeam, portPt,
                    ThMEPEngineCoreCommon.BeamExtensionRatio, beamConnectionTolerance);
                var preparedEnvelope = new ThCADCoreNTSPreparedPolygon(portSearchEnvelope);
                // 先判断是否搭接在柱上
                linkObjs = SpatialIndexManager.ColumnSpatialIndex.SelectCrossingPolygon(portSearchEnvelope);
                if (linkObjs.Count > 0)
                {
                    // 确保梁的延伸和柱是“重叠(Overlap)”的
                    var overlapObjs = linkObjs.Cast<Polyline>().Where(o => preparedEnvelope.Intersects(o));
                    foreach (DBObject dbObj in overlapObjs)
                    {
                        links.Add(ColumnEngine.FilterByOutline(dbObj));
                    }
                }
                if (links.Count > 0)
                {
                    return links;
                }

                // 再判断是否搭接在剪力墙上
                linkObjs = SpatialIndexManager.WallSpatialIndex.SelectCrossingPolygon(portSearchEnvelope);
                if (linkObjs.Count > 0)
                {
                    // 确保梁的延伸和剪力墙是“重叠(Overlap)”的
                    var overlapObjs = linkObjs.Cast<Entity>().Where(o =>
                    {
                        if(o is Curve curve)
                        {
                            return preparedEnvelope.Intersects(curve);
                        }
                        else if(o is MPolygon mPolygon)
                        {
                            return preparedEnvelope.Intersects(mPolygon);
                        }
                        else
                        {
                            return false;
                        }
                    });
                    foreach (DBObject dbObj in overlapObjs)
                    {
                        links.Add(ShearWallEngine.FilterByOutline(dbObj));
                    }
                }
                return links;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        protected Polyline CreatePortEnvelope(Vector3d dir,Point3d portPt,double width, double distance)
        {
            Vector3d perpendicularVector = dir.GetPerpendicularVector();
            Point3d pt1 = portPt + perpendicularVector.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt4 = portPt - perpendicularVector.GetNormal().MultiplyBy(width / 2.0);
            Point3d pt2 = pt1 + dir.GetNormal().MultiplyBy(distance);
            Point3d pt3 = pt4 + dir.GetNormal().MultiplyBy(distance);
            Point3dCollection pts = new Point3dCollection() { pt1, pt2, pt3, pt4 };
            return CreatePolyline(pts);
        }
        protected Polyline CreatePolyline(Point3dCollection pts)
        {
            Polyline polyline = new Polyline()
            {
                Closed = true
            };
            for(int i=0;i<pts.Count;i++)
            {
                polyline.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0.0, 0.0, 0.0);
            }
            return polyline;
        }
        protected double GetPolylineWidth(Polyline polyline,Point3d pt)
        {
            List<LineSegment3d> segments = new List<LineSegment3d>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                LineSegment3d lineSegment = polyline.GetLineSegmentAt(i);
                if (lineSegment != null)
                {
                    segments.Add(lineSegment);
                }
            }
            return segments.OrderBy(o => o.MidPoint.DistanceTo(pt)).FirstOrDefault().Length;
        }
        public bool TwoBeamIsParallel(ThIfcLineBeam firstBeam, ThIfcLineBeam secondBeam)
        {
            return firstBeam.Direction.IsParallelToEx(secondBeam.Direction);
        }
        protected bool TwoBeamCenterLineIsClosed(ThIfcLineBeam firstBeam, ThIfcLineBeam secondBeam, double tolerance = 10.0)
        {
            if (TwoBeamIsParallel(firstBeam, secondBeam))
            {
                var projectPt = secondBeam.StartPoint.GetProjectPtOnLine(firstBeam.StartPoint, firstBeam.EndPoint);
                return secondBeam.StartPoint.DistanceTo(projectPt) <= tolerance;
            }
            return false;
        }
        public bool TwoBeamIsCollinear(ThIfcLineBeam firstBeam, ThIfcLineBeam secondBeam)
        {
            return ThMEPNTSExtension.IsLooseCollinear(firstBeam.StartPoint,firstBeam.EndPoint,
                secondBeam.StartPoint,secondBeam.EndPoint);
        }
        public List<ThIfcBeam> QueryPortLinkBeams(ThIfcBeam thIfcBeam, Point3d portPt,double beamExtensionRatio,double beamIntervalTolerance)
        {
            Polyline portSearchEnvelope = null;
            if (thIfcBeam is ThIfcLineBeam thIfcLineBeam)
            {               
                portSearchEnvelope = GetLineBeamPortSearchEnvelope(thIfcLineBeam, portPt,
                    beamExtensionRatio, beamIntervalTolerance);
            }
            else if (thIfcBeam is ThIfcArcBeam thIfcArcBeam)
            {               
                portSearchEnvelope = GetArcBeamPortSearchEnvelope(thIfcArcBeam, portPt,
                    beamExtensionRatio, beamIntervalTolerance);
            }
            else
            {
                throw new NotSupportedException();
            }
            var preparedEnvelope = new ThCADCoreNTSPreparedPolygon(portSearchEnvelope);
            var linkObjs = SpatialIndexManager.BeamSpatialIndex.SelectCrossingPolygon(portSearchEnvelope);
            var overlapObjs = linkObjs.Cast<Polyline>()
                .Where(o => preparedEnvelope.Intersects(o))
                .ToCollection();
            return BeamEngine.Elements
                .Where(o => overlapObjs.Contains(o.Outline))
                .Where(o => o.Uuid != thIfcBeam.Uuid)
                .Cast<ThIfcBeam>()
                .ToList();
        }
        protected Polyline GetLineBeamPortEnvelope(ThIfcLineBeam thIfcLineBeam, Point3d portPt,
            double beamExtendRadio,double beamExtendLength)
        {
            double beamWidth = GetPolylineWidth(thIfcLineBeam.Outline as Polyline, portPt);
            //在梁宽和柱宽完全一致且完美贴合在一起的情况下，找不到连接的柱子
            //这里暂时我们采用一个"Workaround"，将梁宽扩大后，就可以找到了。
            beamWidth *= beamExtendRadio;
            if (portPt.DistanceTo(thIfcLineBeam.StartPoint) < portPt.DistanceTo(thIfcLineBeam.EndPoint))
            {
                return CreatePortEnvelope(thIfcLineBeam.Direction.Negate(), portPt, beamWidth, beamExtendLength);
            }
            else
            {
                return CreatePortEnvelope(thIfcLineBeam.Direction, portPt, beamWidth, beamExtendLength);
            }
        }
        protected Polyline GetLineBeamPortSearchEnvelope(ThIfcLineBeam thIfcLineBeam, Point3d portPt,double beamExtendRaio,double beamExtendDis)
        {
            double beamWidth = GetPolylineWidth(thIfcLineBeam.Outline as Polyline, portPt);
            //在梁宽和柱宽完全一致且完美贴合在一起的情况下，找不到连接的柱子
            //这里暂时我们采用一个"Workaround"，将梁宽扩大后，就可以找到了。
            beamWidth *= beamExtendRaio;
            if (beamExtendDis > thIfcLineBeam.Length / 2.0)
            {
                beamExtendDis = thIfcLineBeam.Length / 2.0;
            }
            Vector3d direction = thIfcLineBeam.Direction; 
            if(portPt.DistanceTo(thIfcLineBeam.StartPoint)<=1.0)
            {
                Point3d sp = portPt + direction.GetNormal().MultiplyBy(beamExtendDis * 0.5);
                Point3d ep = portPt - direction.GetNormal().MultiplyBy(beamExtendDis);
                return CreatePortEnvelope(sp.GetVectorTo(ep), sp, beamWidth, sp.DistanceTo(ep));
            }
            else
            {
                Point3d sp = portPt - direction.GetNormal().MultiplyBy(beamExtendDis * 0.5);
                Point3d ep = portPt + direction.GetNormal().MultiplyBy(beamExtendDis);
                return CreatePortEnvelope(sp.GetVectorTo(ep), sp, beamWidth, sp.DistanceTo(ep));
            }
        }
        protected Polyline GetArcBeamPortEnvelope(ThIfcArcBeam thIfcArcBeam, Point3d portPt,double beamExtendDis)
        {
            double beamWidth = GetPolylineWidth(thIfcArcBeam.Outline as Polyline, portPt);
            //在梁宽和柱宽完全一致且完美贴合在一起的情况下，找不到连接的柱子
            //这里暂时我们采用一个"Workaround"，将梁宽扩大后，就可以找到了。
            beamWidth *= ThMEPEngineCoreCommon.BeamExtensionRatio;
            if (portPt.DistanceTo(thIfcArcBeam.StartPoint) < portPt.DistanceTo(thIfcArcBeam.EndPoint))
            {
                return CreatePortEnvelope(thIfcArcBeam.StartTangent.Negate(), portPt, beamWidth, beamExtendDis);
            }
            else
            {
                return CreatePortEnvelope(thIfcArcBeam.EndTangent.Negate(), portPt, beamWidth, beamExtendDis);
            }
        }
        protected Polyline GetArcBeamPortSearchEnvelope(ThIfcArcBeam thIfcArcBeam, Point3d portPt,double extendRatio,double beamExtendDis)
        {
            double beamWidth = GetPolylineWidth(thIfcArcBeam.Outline as Polyline, portPt);
            //在梁宽和柱宽完全一致且完美贴合在一起的情况下，找不到连接的柱子
            //这里暂时我们采用一个"Workaround"，将梁宽扩大后，就可以找到了。
            beamWidth *= extendRatio;
            Vector3d direction;
            if (thIfcArcBeam.StartPoint.DistanceTo(portPt)<=1.0)
            {
                direction = thIfcArcBeam.StartTangent;
            }
            else
            {
                direction = thIfcArcBeam.EndTangent;
            }
            Point3d sp = portPt - direction.GetNormal().MultiplyBy(beamExtendDis);
            Point3d ep = portPt + direction.GetNormal().MultiplyBy(beamExtendDis);
            return CreatePortEnvelope(sp.GetVectorTo(ep), sp, beamWidth, sp.DistanceTo(ep)); ;
        }
        protected List<ThIfcBeam> QueryPortLinkPrimaryBeams(List<ThBeamLink> PrimaryBeamLinks,ThIfcBeam currentBeam, Point3d portPt,bool? isParallel = false)
        {
            //查找端点处连接的梁
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
            //端点处连接的梁中是否含有主梁
            List<ThIfcBeam> primaryBeams = linkElements.Where(m => PrimaryBeamLinks.Where(n => n.Beams.Where(k => k.Uuid == m.Uuid).Any()).Any()).ToList();
            //后续根据需要是否要对主梁进行方向筛选
            if (currentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                primaryBeams = primaryBeams.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        if (isParallel == true)
                        {
                            return TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else if(isParallel==false)
                        {
                            return !TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }).ToList();
            }
            return primaryBeams;
        }
        protected List<ThIfcBeam> QueryPortLinkHalfPrimaryBeams(List<ThBeamLink> HalfPrimaryBeamLinks,ThIfcBeam currentBeam, Point3d portPt,bool? isParallel = false)
        {
            //查找端点处连接的梁
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);            
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt); 
            List<ThIfcBeam> halfPrimaryBeams = linkElements.Where(m => HalfPrimaryBeamLinks.Where(n => n.Beams.Where(k => k.Uuid == m.Uuid).Any()).Any()).ToList();
            //TODO 后续根据需要是否要对主梁进行方向筛选
            if (currentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                halfPrimaryBeams = halfPrimaryBeams.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        if(isParallel==true)
                        {
                            return TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else if(isParallel == false)
                        {
                            return !TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }).ToList();
            }
            return halfPrimaryBeams;
        }
        protected List<ThIfcBeam> QueryPortLinkOverhangingPrimaryBeams(List<ThBeamLink> OverhangingPrimaryBeamLinks, ThIfcBeam currentBeam, Point3d portPt, bool? isParallel = false)
        {
            //查找端点处连接的梁
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
            List<ThIfcBeam> overhangingPrimaryBeams = linkElements.Where(m => OverhangingPrimaryBeamLinks.Where(n => n.Beams.Where(k => k.Uuid == m.Uuid).Any()).Any()).ToList();
            if (currentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                overhangingPrimaryBeams = overhangingPrimaryBeams.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        if (isParallel == true)
                        {
                            return TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else if (isParallel == false)
                        {
                            return !TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }).ToList();
            }
            return overhangingPrimaryBeams;
        }
        protected List<ThIfcBeam> QueryPortLinkSecondaryBeams(List<ThBeamLink> SecondaryBeams, ThIfcBeam currentBeam, Point3d portPt, bool? isParallel = false)
        {
            //查找端点处连接的梁
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
            List<ThIfcBeam> secondaryBeams = linkElements.Where(m => SecondaryBeams.Where(n => n.Beams.Where(k => k.Uuid == m.Uuid).Any()).Any()).ToList();
            if (currentBeam is ThIfcLineBeam thIfcLineBeam)
            {
                secondaryBeams = secondaryBeams.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        if (isParallel == true)
                        {
                            return TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else if (isParallel == false)
                        {
                            return !TwoBeamIsParallel(thIfcLineBeam, otherLineBeam);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }).ToList();
            }
            return secondaryBeams;
        }
        protected List<ThIfcBeam> QueryPortLinkUndefinedBeams(List<ThIfcBuildingElement> UnDefinedBeams,ThIfcBeam currentBeam, Point3d portPt, bool? isParallel = true)
        {
            //查找端点处连接的梁
            ThSingleBeamLink thSingleBeamLink = ConnectionEngine.QuerySingleBeamLink(currentBeam);
            List<ThIfcBeam> linkElements = thSingleBeamLink.GetPortBeams(portPt);
            //从端点连接的梁中过滤只存在于UnDefinedBeams集合里的梁
            linkElements = linkElements.Where(m => UnDefinedBeams.Where(n => m.Uuid == n.Uuid).Any()).ToList();
            //只收集是非定义的梁
            linkElements = linkElements.Where(o => o is ThIfcBeam thIfcBeam && thIfcBeam.ComponentType == BeamComponentType.Undefined).ToList();
            if (currentBeam is ThIfcLineBeam lineBeam)
            {
                linkElements = linkElements.Where(o =>
                {
                    if (o is ThIfcLineBeam otherLineBeam)
                    {
                        if (isParallel==true)
                        {
                            return TwoBeamCenterLineIsClosed(lineBeam, otherLineBeam);
                        }
                        else if(isParallel == false)
                        {
                            return !TwoBeamIsParallel(lineBeam, otherLineBeam);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else if (o is ThIfcArcBeam otherArcBeam)
                    {
                        return true;
                    }
                    return false;
                }).ToList();
            }
            return linkElements;
        }
        protected bool IsUndefinedBeam(List<ThIfcBuildingElement> UnDefinedBeams,ThIfcBeam beam)
        {
            return UnDefinedBeams.Where(o => beam.Uuid == o.Uuid).Any() &&
                beam.ComponentType == BeamComponentType.Undefined;
        }
    }
}
