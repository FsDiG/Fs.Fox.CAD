using System;
using NFox.Cad;
using DotNetARX;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThSpaceRecognitionEngine : ThSpatialElementRecognitionEngine
    {
        private List<DBText> SpaceNames { get; set; }
        private List<Curve> SpaceBoundaries { get; set; }
        private Dictionary<DBText, List<Curve>> TextContainer { get; set; }
        private Dictionary<Curve, List<Curve>> AreaContainer { get; set; }
        private Dictionary<Curve, ThIfcSpace> SpaceIndex { get; set; }
        public ThSpaceRecognitionEngine()
        {
            Spaces = new List<ThIfcSpace>();
            SpaceNames = new List<DBText>();
            SpaceBoundaries = new List<Curve>();
            TextContainer = new Dictionary<DBText, List<Curve>>();
            AreaContainer = new Dictionary<Curve, List<Curve>>();
            SpaceIndex = new Dictionary<Curve, ThIfcSpace>();
        }
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                SpaceNames = RecognizeSpaceNameText(database, polygon);
                SpaceBoundaries = RecognizeSpaceBoundary(database, polygon);
                DuplicateRemove(SpaceBoundaries);
                BuildTextContainers();
                BuildAreaContainers();
                CreateSpaceBoundaries();
                SpaceMatchText();
                BuildNestedSpace();
            }
        }
        private void DuplicateRemove(List<Curve> curves)
        {
            if(curves.Count>0)
            {
                var spatialIndex = new ThCADCoreNTSSpatialIndex(curves.ToCollection());
                SpaceBoundaries = spatialIndex.SelectAll().Cast<Curve>().ToList();
            }
        }
        private void BuildTextContainers()
        {
            this.TextContainer = new Dictionary<DBText, List<Curve>>();
            SpaceNames.ForEach(m =>
            {
                try
                {
                    if (m.GeometricExtents != null)
                    {
                        Polyline textBoundary = ThGeometryTool.TextOBB(m);
                        Point3d textCenterPt = ThGeometryTool.GetMidPt(
                            textBoundary.GetPoint3dAt(0),
                            textBoundary.GetPoint3dAt(2));
                        var containers = SelectTextIntersectPolygon(SpaceBoundaries, textBoundary);
                        containers = containers.Where(n => n is Polyline polyline && polyline.Contains(textCenterPt)).ToList();
                        this.TextContainer.Add(m, containers);
                    }
                }
                catch
                {
                    //throw new ArgumentNullException();
                }
            });
        }
        private void BuildAreaContainers()
        {
            this.AreaContainer = new Dictionary<Curve, List<Curve>>();
            SpaceBoundaries.ForEach(m =>
            {
                if (m is Polyline polyline)
                {
                    var containers = SelectPolylineContainers(SpaceBoundaries, polyline);
                    containers.Remove(m);
                    this.AreaContainer.Add(m, containers.Where(o=>(o.Area-m.Area)>10).OrderBy(o => o.Area).ToList());
                }
                else if (m is Circle circle)
                {
                    Polyline polyline1 = circle.Tessellate(50);
                    var containers = SelectPolylineContainers(SpaceBoundaries, polyline1);
                    containers.Remove(m);
                    this.AreaContainer.Add(m, containers.Where(o => (o.Area - m.Area) > 10).OrderBy(o => o.Area).ToList());
                }
                else
                {
                    this.AreaContainer.Add(m, new List<Curve>());
                }
            });
        }
        private List<DBText> RecognizeSpaceNameText(Database database, Point3dCollection polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            using (var spaceNameDbExtension = new ThSpaceNameRecognition(database))
            {
                spaceNameDbExtension.BuildElementTexts();
                List<DBText> dbTexts = new List<DBText>();
                if (polygon.Count > 0)
                {
                    DBObjectCollection dbObjs = new DBObjectCollection();
                    spaceNameDbExtension.Texts.ForEach(o => dbObjs.Add(o));
                    ThCADCoreNTSSpatialIndex columnCurveSpatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                    var pline = new Polyline()
                    {
                        Closed = true,
                    };
                    pline.CreatePolyline(polygon);
                    foreach (var filterObj in columnCurveSpatialIndex.SelectCrossingPolygon(pline))
                    {
                        dbTexts.Add(filterObj as DBText);
                    }
                }
                else
                {
                    dbTexts = spaceNameDbExtension.Texts;
                }
                return dbTexts;
            }
        }
        private List<Curve> RecognizeSpaceBoundary(Database database, Point3dCollection polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            using (var spaceBoundaryDbExtension = new ThSpaceBoundaryRecognition(database))
            {
                spaceBoundaryDbExtension.BuildElementCurves();
                List<Curve> curves = new List<Curve>();
                if (polygon.Count > 0)
                {
                    DBObjectCollection dbObjs = new DBObjectCollection();
                    spaceBoundaryDbExtension.Curves.ForEach(o => dbObjs.Add(o));
                    ThCADCoreNTSSpatialIndex columnCurveSpatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                    var pline = new Polyline()
                    {
                        Closed = true,
                    };
                    pline.CreatePolyline(polygon);
                    foreach (var filterObj in columnCurveSpatialIndex.SelectCrossingPolygon(pline))
                    {
                        curves.Add(filterObj as Curve);
                    }
                }
                else
                {
                    curves = spaceBoundaryDbExtension.Curves;
                }
                return curves;
            }
        }
        private void CreateSpaceBoundaries()
        {
            SpaceBoundaries.ForEach(o => Spaces.Add(new ThIfcSpace { Boundary = o }));
            Spaces.ForEach(o => SpaceIndex.Add(o.Boundary, o));
        }
        private void SpaceMatchText()
        {
            DBObjectCollection dbObjs = new DBObjectCollection();
            Dictionary<Curve, List<string>> dict = new Dictionary<Curve, List<string>>();
            SpaceNames.ForEach(o =>
            {
                try
                {
                    var curves = TextContainer[o];
                    if (curves.Count > 0)
                    {
                        var belonged = curves.Cast<Polyline>().OrderBy(k => k.Area).First();
                        if (!dict.ContainsKey(belonged))
                        {
                            dict.Add(belonged, new List<string> { o.TextString });
                        }
                        else
                        {
                            if (dict[belonged].IndexOf(o.TextString) < 0)
                            {
                                dict[belonged].Add(o.TextString);
                            }
                        }
                    }
                }
                catch
                {

                }
            });
            Spaces.ForEach(o =>
            {
                if (dict.ContainsKey(o.Boundary))
                {
                    o.Tags = dict[o.Boundary];
                }
            });
        }
        private void BuildNestedSpace()
        {
            Spaces = Spaces.OrderBy(o => o.Boundary.Area).ToList();
            Spaces.ForEach(o => BuildNestedSpace(o));
        }
        private void BuildNestedSpace(ThIfcSpace thIfcSpace)
        {
            var objs = AreaContainer[thIfcSpace.Boundary];
            if (objs.Count == 0)
            {
                return;
            }
            foreach (var parentObj in objs)
            {
                if (SpaceIndex.ContainsKey(parentObj))
                {
                    var parent = SpaceIndex[parentObj];
                    if (parent.SubSpaces.IndexOf(thIfcSpace) < 0)
                    {
                        parent.SubSpaces.Add(thIfcSpace);
                    }
                    BuildNestedSpace(parent);
                }
            }
        }
        /// <summary>
        /// Crossing selection
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        private List<Curve> SelectTextIntersectPolygon(List<Curve> curves, Polyline textOBB)
        {
            return curves.Where(o =>
            {
                if (o is Polyline polyline)
                {
                    using (var ov = new ThCADCoreNTSFixedPrecision())
                    {
                        ThCADCoreNTSRelate relation = new ThCADCoreNTSRelate(polyline, textOBB);
                        return relation.IsOverlaps || relation.IsCovers;
                    }
                }
                return false;
            }).ToList();
        }
        /// <summary>
        /// 获取完全包括area的容器
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private List<Curve> SelectPolylineContainers(List<Curve> curves, Polyline son)
        {
            //return curves.Where(o =>
            //{
            //    if (o is Polyline parent)
            //    {
            //        return IsCovers(parent, son);
            //    }
            //    return false;
            //}).ToList();
            var bufferObjs = son.Buffer(-15.0);
            return curves.Where(o =>
            {
                if (o is Polyline parent)
                {
                    return bufferObjs.Cast<Curve>().Where(m =>
                    {
                        if (m is Polyline polyline)
                        {
                            ThCADCoreNTSRelate relation = new ThCADCoreNTSRelate(parent, polyline);
                            return relation.IsCovers;
                        }
                        return false;
                    }).Any();
                }
                return false;
            }).ToList();
        }

        private bool IsCovers(Polyline parent,Polyline son,double tolerance=10.0)
        {            
            if(parent.Equals(son))
            {
                return true;
            }
            else
            {
                bool result = false;
                var relation = new ThCADCoreNTSRelate(parent, son);
                result = relation.IsCovers;
                if (!result)
                {
                    var pts = son.VerticesEx(50.0);
                    foreach (Point3d pt in pts)
                    {
                        if (!parent.Contains(pt))
                        {
                            var closePt = parent.GetClosestPointTo(pt, false);
                            if (closePt.DistanceTo(pt) > tolerance)
                            {
                                return false;
                            }
                        }
                    }
                }
                return result;
            }
        }

        public override void Recognize(List<ThRawIfcSpatialElementData> datas, Point3dCollection polygon)
        {
            throw new NotImplementedException();
        }

        public override void RecognizeMS(Database database, Point3dCollection polygon)
        {
            throw new NotImplementedException();
        }
    }
}
