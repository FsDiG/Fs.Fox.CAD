using System;
using AcHelper;
using Linq2Acad;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Engine;
using ThMEPEngineCore.Service;
using ThMEPEngineCore.BeamInfo;
using ThMEPEngineCore.Algorithm;
using ThMEPEngineCore.BeamInfo.Utils;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.LaneLine;

namespace ThMEPEngineCore.Test
{
    public class ThMEPEngineCoreTestApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        /// <summary>
        /// 提取指定区域内的梁信息
        /// </summary>
        [CommandMethod("TIANHUACAD", "THGETBEAMINFO", CommandFlags.Modal)]
        public void THGETBEAMINFO()
        {
            // 选择楼层区域
            // 暂时只支持矩形区域
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                DBObjectCollection curves = new DBObjectCollection();
                var selRes = Active.Editor.GetSelection();
                foreach (ObjectId objId in selRes.Value.GetObjectIds())
                {
                    curves.Add(acadDatabase.Element<Curve>(objId));
                }
                var spatialIndex = new ThCADCoreNTSSpatialIndex(curves);
                Point3d pt1 = Active.Editor.GetPoint("select left down point: ").Value;
                Point3d pt2 = Active.Editor.GetPoint("select right up point: ").Value;
                DBObjectCollection filterCurves = spatialIndex.SelectCrossingWindow(pt1, pt2);
                ThDistinguishBeamInfo thDisBeamInfo = new ThDistinguishBeamInfo();
                var beams = thDisBeamInfo.CalBeamStruc(filterCurves);
                foreach (var beam in beams)
                {
                    acadDatabase.ModelSpace.Add(beam.BeamBoundary);
                }
            }
        }
        /// <summary>
        /// 提取所选图元的梁信息
        /// </summary>
        [CommandMethod("TIANHUACAD", "THGETBEAMINFO2", CommandFlags.Modal)]
        public void THGETBEAMINFO2()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };

                // 梁线的图元类型
                var dxfNames = new string[]
                {
                    RXClass.GetClass(typeof(Arc)).DxfName,
                    RXClass.GetClass(typeof(Line)).DxfName,
                    RXClass.GetClass(typeof(Polyline)).DxfName,
                };
                // 梁线的图元图层
                var layers = ThBeamLayerManager.GeometryLayers(acdb.Database);
                var filter = ThSelectionFilterTool.Build(dxfNames, layers.ToArray());
                var entSelected = Active.Editor.GetSelection(options, filter);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    var entity = acdb.Element<Entity>(obj);
                    dBObjects.Add(entity.GetTransformedCopy(Matrix3d.Identity));
                }

                ThDistinguishBeamInfo thDisBeamCommand = new ThDistinguishBeamInfo();
                var beams = thDisBeamCommand.CalBeamStruc(dBObjects);
                using (var acadDatabase = AcadDatabase.Active())
                {
                    foreach (var beam in beams)
                    {
                        acadDatabase.ModelSpace.Add(beam.BeamBoundary);
                    }
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THBE", CommandFlags.Modal)]
        public void ThBuildElement()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var hyperlinks = acadDatabase.Element<Entity>(result.ObjectId).Hyperlinks;
                var buildElement = ThPropertySet.CreateWithHyperlink(hyperlinks[0].Description);
            }
        }

        [CommandMethod("TIANHUACAD", "ThArcBeamOutline", CommandFlags.Modal)]
        public void ThArcBeamOutline()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new List<Arc>();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Arc>(obj));
                }

                Polyline polyline = ThArcBeamOutliner.TessellatedOutline(objs[0], objs[1]);
                polyline.ColorIndex = 1;
                acadDatabase.ModelSpace.Add(polyline);
            }
        }

        [CommandMethod("TIANHUACAD", "TestFrame", CommandFlags.Modal)]
        public void TestFrame()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("\n请选择框线");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                var engine = ThBeamConnectRecogitionEngine.ExecuteRecognize(
                    acadDatabase.Database, new Point3dCollection());
                var frameService = new ThMEPFrameService(engine);
                var frame = acadDatabase.Element<Polyline>(result.ObjectId);
                foreach (Entity item in frameService.RegionsFromFrame(frame))
                {
                    item.ColorIndex = 2;
                    acadDatabase.ModelSpace.Add(item);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THPOLYGONPARTITION", CommandFlags.Modal)]
        public void THPolygonPartition()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Polyline>(obj));
                }
                foreach (Polyline item in objs)
                {
                    var polylines = ThMEPPolygonPartitioner.PolygonPartition(item);
                    //polylines.ColorIndex = 1;
                    //acadDatabase.ModelSpace.Add(polylines);
                    foreach (var obj in polylines)
                    {
                        obj.ColorIndex = 1;
                        acadDatabase.ModelSpace.Add(obj);
                    }
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THLineSimplifer", CommandFlags.Modal)]
        public void THLineSimplifer()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Curve>(obj));
                }
                var lines = ThMEPLineExtension.LineSimplifier(objs, 5.0, 20.0, 2.0, Math.PI / 180.0);
                foreach (var obj in lines)
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THHatchPrint", CommandFlags.Modal)]
        public void THHatchPrint()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var hatchRes = Active.Editor.GetEntity("\nselect a hatch");
                Hatch hatch = acadDatabase.Element<Hatch>(hatchRes.ObjectId);
                hatch.Boundaries().ForEach(o => acadDatabase.ModelSpace.Add(o));
            }
        }
        [CommandMethod("TIANHUACAD", "THLineMergeTest", CommandFlags.Modal)]
        public void THLineMergeTest()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var lineRes = Active.Editor.GetSelection();
                if (lineRes.Status != PromptStatus.OK)
                {
                    return;
                }
                List<Line> lines = new List<Line>();
                lineRes.Value.GetObjectIds().ForEach(o => lines.Add(acadDatabase.Element<Line>(o)));
                var newLines = ThLineMerger.Merge(lines);
                newLines.ForEach(o =>
                {
                    o.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(o);
                });
            }
        }
        [CommandMethod("TIANHUACAD", "THTestIsCollinear", CommandFlags.Modal)]
        public void THTestIsCollinear()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var line1Res = Active.Editor.GetEntity("\nselect first line");
                var line2Res = Active.Editor.GetEntity("\nselect second line");
                Line line1 = acadDatabase.Element<Line>(line1Res.ObjectId);
                Line line2 = acadDatabase.Element<Line>(line2Res.ObjectId);
                if (ThGeometryTool.IsCollinearEx(
                    line1.StartPoint, line1.EndPoint, line2.StartPoint, line2.EndPoint))
                {
                    Active.Editor.WriteMessage("共线");
                }
                else
                {
                    Active.Editor.WriteMessage("不共线");
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THRemoveDangles", CommandFlags.Modal)]
        public void THRemoveDangles()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var objs = Active.Editor.GetSelection();
                if (objs.Status != PromptStatus.OK)
                {
                    return;
                }
                var result = new DBObjectCollection();
                foreach (var obj in objs.Value.GetObjectIds())
                {
                    result.Add(acadDatabase.Element<Curve>(obj));
                }
                var lines = ThLaneLineSimplifier.RemoveDangles(result, 100);
                foreach (var obj in lines)
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }
    }
}