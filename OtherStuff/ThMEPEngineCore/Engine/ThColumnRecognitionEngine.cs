using NFox.Cad;
using DotNetARX;
using System.Linq;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThColumnExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThColumnExtractionVisitor()
            {
                LayerFilter = ThStructureColumnLayerManager.HatchXrefLayers(database),
            };
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }

    public class ThColumnRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThColumnExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            List<Curve> curves = new List<Curve>();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                ThCADCoreNTSSpatialIndex columnCurveSpatialIndex = new ThCADCoreNTSSpatialIndex(objs);
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
                curves = objs.Cast<Curve>().ToList();
            }
            curves.ToCollection().UnionPolygons().Cast<Curve>()
                .ForEach(o =>
                {
                    if (o is Polyline polyline && polyline.Area > 0.0)
                    {
                        var bufferObjs = polyline.Buffer(ThMEPEngineCoreCommon.ColumnBufferDistance);
                        if (bufferObjs.Count == 1)
                        {
                            var outline = bufferObjs[0] as Polyline;
                            Elements.Add(ThIfcColumn.Create(outline));
                        }
                    }
                });
        }
    }
}
