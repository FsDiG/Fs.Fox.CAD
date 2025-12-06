using AcHelper;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using Autodesk.AutoCAD.Colors;
using DotNetARX;

namespace ThCADCore.Test
{
    public class ThCADCoreTestApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "ThMBB", CommandFlags.Modal)]
        public void ThMBB()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.MinimumBoundingBox());
            }
        }

        [CommandMethod("TIANHUACAD", "ThMBC", CommandFlags.Modal)]
        public void ThMBC()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.MinimumBoundingCircle());
            }
        }

        [CommandMethod("TIANHUACAD", "ThOBB", CommandFlags.Modal)]
        public void ThOBB()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                acadDatabase.ModelSpace.Add(objs.GetMinimumRectangle());
            }
        }

        [CommandMethod("TIANHUACAD", "ThConvexHull", CommandFlags.Modal)]
        public void ThConvexHull()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.ConvexHull());
            }
        }

        [CommandMethod("TIANHUACAD", "ThEnvelope", CommandFlags.Modal)]
        public void ThEnvelope()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }



                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.GetOctagonalEnvelope());
            }
        }

        private List<Point2d> Vertices(Polyline poly)
        {
            var points = new List<Point2d>();
            poly.Vertices().Cast<Point3d>().ForEach(o => points.Add(o.ToPoint2D()));
            return points;
        }

        private List<List<Point2d>> Vertices(DBObjectCollection holes)
        {
            var points = new List<List<Point2d>>();
            holes.Cast<Polyline>().ForEach(o => points.Add(Vertices(o)));
            return points;
        }

        [CommandMethod("TIANHUACAD", "ThOutline", CommandFlags.Modal)]
        public void ThOutline()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (var obj in objs.Outline())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThDifference", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThDifference()
        {
            using (var ov = new ThCADCoreNTSFixedPrecision())
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("\n请选择框线");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Polyline>(obj));
                }

                var frame = result2.ObjectId;
                var frameObj = acadDatabase.Element<Polyline>(frame);
                foreach (Entity obj in frameObj.Difference(objs))
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThPolygonizer", CommandFlags.Modal)]
        public void ThPolygonizer()
        {
            using (var ov = new ThCADCoreNTSFixedPrecision())
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (var obj in objs.Polygons())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThUnaryUnionOp", CommandFlags.Modal)]
        public void ThUnion()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                var geometry = objs.ToNTSNodedLineStrings();

                foreach (Entity obj in geometry.ToDbCollection())
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThOverlayUnion", CommandFlags.Modal)]
        public void ThOverlayUnion()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }


                var geometrys = objs.ToNTSLineStrings();

                var overlapUnion = OverlapUnion.Union(geometrys.First(), geometrys.Last());
                foreach (Entity obj in overlapUnion.ToDbCollection())
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThPolygonIntersect", CommandFlags.Modal)]
        public void ThPolygonIntersect()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var singleResult = Active.Editor.GetEntity("请选择对象");
                if (singleResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var frame = singleResult.ObjectId;
                var frameObj = acadDatabase.Element<Polyline>(frame);
                
                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (Polygon obj in objs.Polygonize())
                {
                    // 洞
                    //obj.SymmetricDifference(frameObj.ToNTSPolygon());
                    //var ntsObj = obj.Difference(new DBObjectCollection() { frameObj }.UnionGeometries());
                    var ntsObj =  obj.Union(frameObj.ToNTSPolygon());
                    foreach (Entity entity in ntsObj.ToDbCollection())
                    {
                        entity.ColorIndex = 2;
                        acadDatabase.ModelSpace.Add(entity);
                    }
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThSnapIfNeededOverlayOp", CommandFlags.Modal)]
        public void ThOverlay()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                var geometrys = objs.ToNTSLineStrings();

                var snapGeometry = SnapIfNeededOverlayOp.Overlay(geometrys.First(), geometrys.Last(), SpatialFunction.Union);
                foreach (Entity obj in snapGeometry.ToDbCollection())
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThSnapIntersection", CommandFlags.Modal)]
        public void ThSnapIfNeededIntersection()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                var geometrys = objs.ToNTSLineStrings();
                // 求交点
                var snapGeometry = SnapIfNeededOverlayOp.Overlay(geometrys.First(), geometrys.Last(), SpatialFunction.Intersection);
                foreach (Entity obj in snapGeometry.ToDbCollection())
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThSnapDifference", CommandFlags.Modal)]
        public void ThThDifference()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                //var objs = new DBObjectCollection();
                //foreach (var obj in result.Value.GetObjectIds())
                //{
                //    objs.Add(acadDatabase.Element<Entity>(obj));
                //}
                //var geometrys = objs.ToNTSLineStrings();

                //var snapGeometry = SnapIfNeededOverlayOp.Overlay(geometrys.First(), geometrys.Last(), SpatialFunction.Difference);
                //foreach (Entity obj in snapGeometry.ToDbCollection())
                //{
                //    obj.ColorIndex = 1;
                //    acadDatabase.ModelSpace.Add(obj);
                //}

                var polys = new List<Polyline>();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    polys.Add(acadDatabase.Element<Polyline>(obj));
                }

                var snapGeometry = SnapIfNeededOverlayOp.Overlay(polys.First().ToNTSPolygon(), polys.Last().ToNTSPolygon(), SpatialFunction.Union);
                foreach (Entity obj in snapGeometry.ToDbCollection())
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }


        [CommandMethod("TIANHUACAD", "ThVoronoiDiagram", CommandFlags.Modal)]
        public void ThVoronoiDiagram()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                foreach (Entity diagram in pline.VoronoiTriangulation(pline.Length / 50.0))
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THDT", CommandFlags.Modal)]
        public void ThDelaunayTriangulation()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var points = new Point3dCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    points.Add(acadDatabase.Element<Entity>(obj).GeometricExtents.CenterPoint());
                }
                foreach (Entity diagram in points.DelaunayTriangulation())
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THCDT", CommandFlags.Modal)]
        public void ThConformingDelaunayTriangulation()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result1 = Active.Editor.GetSelection();
                if (result1.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择对象");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var points = new Point3dCollection();
                foreach (var obj in result1.Value.GetObjectIds())
                {
                    points.Add(acadDatabase.Element<Entity>(obj).GeometricExtents.CenterPoint());
                }
                var pline = acadDatabase.Element<Polyline>(result2.ObjectId);
                foreach (Entity diagram in points.ConformingDelaunayTriangulation(pline))
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THLineMerger", CommandFlags.Modal)]
        public void THLineMerger()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result1 = Active.Editor.GetSelection();
                if (result1.Status != PromptStatus.OK)
                {
                    return;
                }

                var dbLst = new DBObjectCollection();
                foreach (var obj in result1.Value.GetObjectIds())
                {
                    dbLst.Add(acadDatabase.Element<Entity>(obj));
                }

                foreach (Entity diagram in dbLst.LineMerge())
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THPileGroup", CommandFlags.Modal)]
        public void THPileGroup()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result1 = Active.Editor.GetSelection();
                if (result1.Status != PromptStatus.OK)
                {
                    return;
                }

                var dbLst1 = new List<Curve>();
                foreach (var obj in result1.Value.GetObjectIds())
                {
                    dbLst1.Add(acadDatabase.Element<Curve>(obj));
                }

                var totalIds = new ObjectIdList();
                var ids = DrawProfile(dbLst1, "dbLst");
                totalIds.AddRange(ids);
                var groupId = GroupTools.CreateGroup(acadDatabase.Database, "d", totalIds);

                var dbLst2 = new List<Curve>();
                var result2 = Active.Editor.GetEntity("select");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                dbLst2.Add(acadDatabase.Element<Curve>(result2.ObjectId));

                var ids2 = DrawProfile(dbLst2, "dbLst2");
                var totalIds2 = new ObjectIdList();
                totalIds2.AddRange(ids2);

                var groupId2 = GroupTools.CreateGroup(acadDatabase.Database, "d2", totalIds2);

                var totalIds3 = new ObjectIdList();
                totalIds3.AddRange(totalIds2);
                totalIds3.AddRange(totalIds);

                var groupId3 = GroupTools.CreateGroup(acadDatabase.Database, "totalIds3", totalIds3);

            }
        }

        public static List<ObjectId> DrawProfile(List<Curve> curves, string LayerName, Color color = null)
        {
            var objectIds = new List<ObjectId>();
            if (curves == null || curves.Count == 0)
                return objectIds;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var curve in curves)
                {
                    var clone = curve.Clone() as Curve;
                    clone.Layer = LayerName;
                    objectIds.Add(db.ModelSpace.Add(clone));
                }
            }

            return objectIds;
        }

        public static ObjectId CreateLayer(string aimLayer, Color color)
        {
            LayerTableRecord layerRecord = null;
            using (var db = AcadDatabase.Active())
            {
                foreach (var layer in db.Layers)
                {
                    if (layer.Name.Equals(aimLayer))
                    {
                        layerRecord = db.Layers.Element(aimLayer);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(aimLayer);
                    layerRecord.Color = color;
                    layerRecord.IsPlottable = false;
                }
                else
                {
                    if (!layerRecord.Color.Equals(color))
                    {
                        layerRecord.UpgradeOpen();
                        layerRecord.Color = color;
                        layerRecord.IsPlottable = false;
                        layerRecord.DowngradeOpen();
                    }
                }
            }

            return layerRecord.ObjectId;
        }

        [CommandMethod("TIANHUACAD", "ThTrim", CommandFlags.Modal)]
        public void ThTrim()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result1 = Active.Editor.GetEntity("\n请选择框线");
                if (result1.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetSelection();
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new List<DBObject>();
                var frame = acadDatabase.Element<Polyline>(result1.ObjectId);
                var clipper = new ThCADCoreNTSFastGeometryClipper(frame.ToNTSPolygon().EnvelopeInternal);
                foreach (var obj in result2.Value.GetObjectIds())
                {
                    var curve = acadDatabase.Element<Curve>(obj);
                    objs.AddRange(clipper.clip(curve.ToNTSGeometry(), true).ToDbObjects());
                }
                foreach (Entity obj in objs)
                {
                    obj.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(obj);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThPlTrim", CommandFlags.Modal)]
        public void ThPlTrim()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择框线");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var curve = acadDatabase.Element<Polyline>(result.ObjectId);
                var frame = acadDatabase.Element<Polyline>(result2.ObjectId);

                foreach (Entity diagram in curve.ToNTSGeometry().Intersection(frame.ToNTSGeometry()).ToDbCollection())
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThClosePoint", CommandFlags.Modal)]
        public void ThClosePoint()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择框线");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var poly = acadDatabase.Element<Polyline>(result.ObjectId);
                var circle = acadDatabase.Element<Circle>(result2.ObjectId);
                var closestPt = poly.GetClosestPointTo(circle.Center, true);

                var verticalLine = new Line(closestPt, circle.Center);

                acadDatabase.ModelSpace.Add(verticalLine);
            }
        }

        [CommandMethod("TIANHUACAD", "ThPBuffer", CommandFlags.Modal)]
        public void ThBuffer()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("\n请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetDistance("\n请输入距离");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                double distanceTolerance = result2.Value;
                var obj = acadDatabase.Element<Polyline>(result.ObjectId);
                foreach (Polyline pl in obj.BufferPL(20))
                {
                    pl.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(pl);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThSingleBuffer", CommandFlags.Modal)]
        public void ThSingleBuffer()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("\n请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetDistance("\n请输入距离");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                double distanceTolerance = result2.Value;
                var obj = acadDatabase.Element<Entity>(result.ObjectId);
                var dbCol = new DBObjectCollection();
                dbCol.Add(obj);
                foreach (Polyline pl in dbCol.SingleSidedBuffer(distanceTolerance))
                {
                    pl.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(pl);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThOrientation", CommandFlags.Modal)]
        public void ThOrientation()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                if (pline.IsCCW())
                {
                    Active.Editor.WriteLine("It's oriented counter-clockwise.");
                }
                else
                {
                    Active.Editor.WriteLine("It's oriented clockwise.");
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThSpatialIndex", CommandFlags.Modal)]
        public void ThSpatialIndex()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择框线");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var options = new PromptKeywordOptions("\n请指定选择方式")
                {
                    AllowNone = true
                };
                options.Keywords.Add("Window", "Window", "Window(W)");
                options.Keywords.Add("Crossing", "Crossing", "Crossing(C)");
                options.Keywords.Add("Fence", "Fence", "Fence(F)");
                options.Keywords.Default = "Window";
                var result3 = Active.Editor.GetKeywords(options);
                if (result3.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }

                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                var frame = acadDatabase.Element<Polyline>(result2.ObjectId);
                if (result3.StringResult == "Window")
                {
                    spatialIndex.SelectWindowPolygon(frame).Cast<Entity>().ForEachDbObject(o => o.Highlight());
                }
                else if (result3.StringResult == "Crossing")
                {
                    spatialIndex.SelectCrossingPolygon(frame).Cast<Entity>().ForEachDbObject(o => o.Highlight());
                }
                else if (result3.StringResult == "Fence")
                {
                    spatialIndex.SelectFence(frame).Cast<Entity>().ForEachDbObject(o => o.Highlight());
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThNeighbour", CommandFlags.Modal)]
        public void ThNeighbour()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择查询对象");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var result3 = Active.Editor.GetInteger("请输入邻居个数");
                if (result3.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }

                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                var host = acadDatabase.Element<Curve>(result2.ObjectId);
                var nearestNeighbours = spatialIndex.NearestNeighbours(host, result3.Value);
                foreach (Entity neighbour in nearestNeighbours)
                {
                    neighbour.UpgradeOpen();
                    neighbour.ColorIndex = 1;
                    neighbour.DowngradeOpen();
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThPolylineTessellate", CommandFlags.Modal)]
        public void ThPolylineTessellate()
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
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (var item in objs)
                {
                    if (item is Polyline poly)
                    {
                        var polyline_Chord = poly.TessellatePolylineWithChord(100);
                        polyline_Chord.ColorIndex = 2;
                        acadDatabase.ModelSpace.Add(polyline_Chord);
                        var polyline_Arc = poly.TessellatePolylineWithArc(100);
                        polyline_Arc.ColorIndex = 1;
                        acadDatabase.ModelSpace.Add(polyline_Arc);
                    }
                    else if (item is Arc arc)
                    {
                        var arc_Chord = arc.TessellateArcWithChord(100);
                        arc_Chord.ColorIndex = 2;
                        acadDatabase.ModelSpace.Add(arc_Chord);
                        var arc_Arc = arc.TessellateArcWithArc(100);
                        arc_Arc.ColorIndex = 1;
                        acadDatabase.ModelSpace.Add(arc_Arc);
                    }
                }
            }
        }
    }
}
