using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Engine
{
    public class ThSplitBeamEngine : ThBeamPreprocessEngine
    {
        private ThBeamConnectRecogitionEngine BeamConnectRecogitionEngine { get; set; }

        public ThSplitBeamEngine(ThBeamConnectRecogitionEngine thBeamConnectRecogitionEngine)
        {
            BeamConnectRecogitionEngine = thBeamConnectRecogitionEngine;           
        }
        public void Split()
        {
            //处理梁穿过剪力墙竖向构件
            SplitBeamPassWalls();

            //处理梁穿过柱竖向构件
            SplitBeamPassColumns();

            //将梁长度过小的排除
            FilterSmallBeams();
        }
        public void Split(BeamComponentType beamComponentType)
        {
            //处理梁穿过主梁
            SplitBeamPassBeams(beamComponentType);

            //将梁长度过小的排除
            FilterSmallBeams();
        }

        private void SplitBeamPassBeams(BeamComponentType beamComponentType)
        {
            var primaryBeams = BeamConnectRecogitionEngine.BeamEngine.Elements.Cast<ThIfcBeam>().ToList().Where(o =>
            {
                return o.ComponentType == beamComponentType ? true : false;
            }).ToList();
            var UndefinedBeams = BeamConnectRecogitionEngine.BeamEngine.Elements.Cast<ThIfcBeam>().ToList().Where(o =>
            {
                return o.ComponentType == BeamComponentType.Undefined ? true : false;
            }).ToList();
            List<ThIfcBeam> removeBeams = new List<ThIfcBeam>();
            List<ThIfcBeam> addBeams = new List<ThIfcBeam>();
            UndefinedBeams.ForEach(o =>
            {
                Polyline outline = o.Outline as Polyline;
                if (beamComponentType == BeamComponentType.OverhangingPrimaryBeam)
                {
                    outline = ThLineBeamOutliner.Extend(o, 0, ThMEPEngineCoreCommon.BeamIntersectExtentionTolerance);
                }
                DBObjectCollection passComponents = BeamConnectRecogitionEngine.SpatialIndexManager.
                BeamSpatialIndex.SelectCrossingPolygon(outline);
                var passBeams = BeamConnectRecogitionEngine.BeamEngine.FilterByOutline(passComponents)
                .Where(m => primaryBeams.Where(n => n.Uuid == m.Uuid).Any())
                .Cast<ThIfcBeam>().ToList();
                var splitBeams = Split(o, passBeams.ToList());
                if (splitBeams.Count > 0)
                {
                    removeBeams.Add(o);
                    addBeams.AddRange(splitBeams);
                }
            });
            removeBeams.ForEach(o => BeamConnectRecogitionEngine.BeamEngine.Elements.Remove(o));
            BeamConnectRecogitionEngine.BeamEngine.Elements.AddRange(addBeams);
            BeamConnectRecogitionEngine.SyncBeamSpatialIndex();
            BeamConnectRecogitionEngine.UpdateSingleBeamLink(addBeams, removeBeams);
        }

        private List<ThIfcBeam> Split(ThIfcBeam thIfcBeam,List<ThIfcBeam> beams)
        {
            if(thIfcBeam is ThIfcLineBeam lineBeam)
            {
                return Split(lineBeam, beams);
            }
            else if (thIfcBeam is ThIfcArcBeam arcBeam)
            {
                return Split(arcBeam, beams);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private List<ThIfcBeam> Split(ThIfcLineBeam lineBeam, List<ThIfcBeam> beams)
        {
            beams=beams.Where(o =>
            {
                if(o is ThIfcLineBeam otherBeam)
                {
                    return !ThGeometryTool.IsParallelToEx(lineBeam.Direction, otherBeam.Direction);
                }
                else
                {
                    return true;
                }
            }).ToList();
            if(beams.Count==0)
            {
                return new List<ThIfcBeam>();
            }
            List<Entity> passBeams = new List<Entity>();
            if (beams[0].ComponentType==BeamComponentType.OverhangingPrimaryBeam)
            {
                beams.ForEach(o => passBeams.Add(ThLineBeamOutliner.ExtendBoth(o, 2 * lineBeam.Width, 2 * lineBeam.Width)));
            }
            else
            {
                beams.ForEach(o => passBeams.Add(o.Outline as Polyline));
            }
            passBeams=passBeams.Where(o =>
            {
                var intersectPts = ThGeometryTool.IntersectWithEx(lineBeam.Outline, o);
                return intersectPts.Count == 4;
            }).ToList();
            ThLinealBeamSplitter thSplitBeam = new ThLinealBeamSplitter(lineBeam);
            thSplitBeam.Split(passBeams);
            return thSplitBeam.SplitBeams;
        }        
        private List<ThIfcBeam> Split(ThIfcArcBeam arcBeam, List<ThIfcBeam> beams)
        {
            throw new NotSupportedException();
        }
        private void FilterSmallBeams()
        {
            BeamConnectRecogitionEngine.BeamEngine.Elements=
                BeamConnectRecogitionEngine.BeamEngine.Elements.Where(o =>
            {
                if (o is ThIfcLineBeam thIfcLinearBeam && thIfcLinearBeam.Length < ThMEPEngineCoreCommon.BeamMinimumLength)
                {
                    return false;
                }
                return true;
            }).ToList();
        }
        private void SplitBeamPassWalls()
        {
            List<string> removeUuids = new List<string>();
            List<ThIfcBeam> addBeams = new List<ThIfcBeam>();
            BeamConnectRecogitionEngine.BeamEngine.Elements.ForEach(o =>
                {
                    List<Entity> passWalls = BeamCrossWallOutlines(o as ThIfcBeam);
                    ThBeamSplitter thSplitBeam = null;
                    if (o is ThIfcLineBeam thIfcLineBeam)
                    {
                        thSplitBeam = new ThLinealBeamSplitter(thIfcLineBeam);
                    }
                    else if (o is ThIfcArcBeam thIfcArcBeam)
                    {
                        thSplitBeam = new ThCurveBeamSplitter(thIfcArcBeam);
                    }
                    thSplitBeam.Split(passWalls);
                    if (thSplitBeam.SplitBeams.Count > 0)
                    {
                        removeUuids.Add(o.Uuid);
                        addBeams.AddRange(thSplitBeam.SplitBeams);
                    }
                });
            if(addBeams.Count>0)
            {
                BeamConnectRecogitionEngine.BeamEngine.Elements =
                BeamConnectRecogitionEngine.BeamEngine.Elements.Where(o => removeUuids.IndexOf(o.Uuid) < 0).ToList();
                BeamConnectRecogitionEngine.BeamEngine.Elements.AddRange(addBeams);
                BeamConnectRecogitionEngine.SyncBeamSpatialIndex();
            }
        }        
        private void SplitBeamPassColumns()
        {
            List<string> removeUuids = new List<string>();
            List<ThIfcBeam> addBeams = new List<ThIfcBeam>();
            BeamConnectRecogitionEngine.BeamEngine.Elements.ForEach(o =>
            {
                var passColumns = BeamCrossColumnOutlines(o as ThIfcBeam);
                ThBeamSplitter thSplitBeam = null;
                if (o is ThIfcLineBeam thIfcLineBeam)
                {
                    thSplitBeam = new ThLinealBeamSplitter(thIfcLineBeam);
                }
                else if (o is ThIfcArcBeam thIfcArcBeam)
                {
                    thSplitBeam = new ThCurveBeamSplitter(thIfcArcBeam);
                }
                thSplitBeam.Split(passColumns);
                if (thSplitBeam.SplitBeams.Count > 1)
                {
                    removeUuids.Add(o.Uuid);
                    addBeams.AddRange(thSplitBeam.SplitBeams);
                }
            });
            if(addBeams.Count>0)
            {
                BeamConnectRecogitionEngine.BeamEngine.Elements =
                BeamConnectRecogitionEngine.BeamEngine.Elements.Where(o => removeUuids.IndexOf(o.Uuid) < 0).ToList();
                BeamConnectRecogitionEngine.BeamEngine.Elements.AddRange(addBeams);
                BeamConnectRecogitionEngine.SyncBeamSpatialIndex();
            }
        }
        private List<Entity> BeamCrossWallOutlines(ThIfcBeam thIfcBeam)
        {
            List<Entity> passWalls = new List<Entity>();
            //搜索与梁相交的墙段
            Polyline outline = ThLineBeamOutliner.Extend(thIfcBeam as ThIfcLineBeam, 0.0, -ThMEPEngineCoreCommon.BeamBufferDistance);
            DBObjectCollection wallComponents = BeamConnectRecogitionEngine.SpatialIndexManager.WallSpatialIndex.SelectCrossingPolygon(outline);
            if (wallComponents.Count > 0)
            {
                var intersectShearWalls = BeamConnectRecogitionEngine.ShearWallEngine.FilterByOutline(wallComponents).ToList();
                intersectShearWalls.ForEach(m => passWalls.Add(m.Outline));
            }
            return passWalls;
        }
        private List<Entity> BeamCrossColumnOutlines(ThIfcBeam thIfcBeam)
        {            
            List<Entity> passColumns = new List<Entity>();
            Polyline outline = ThLineBeamOutliner.Extend(thIfcBeam as ThIfcLineBeam, 0.0, -ThMEPEngineCoreCommon.BeamBufferDistance);
            DBObjectCollection columnComponents = BeamConnectRecogitionEngine.SpatialIndexManager.ColumnSpatialIndex.SelectCrossingPolygon(outline);
            if (columnComponents.Count > 0)
            {
                var intersectColumns = BeamConnectRecogitionEngine.ColumnEngine.FilterByOutline(columnComponents).ToList();
                intersectColumns.ForEach(o =>
                {
                    passColumns.Add(o.Outline as Polyline);
                });
            }
            return passColumns;
        }
    }
}
