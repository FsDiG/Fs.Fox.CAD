using NFox.Cad;
using DotNetARX;
using System.Linq;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Engine
{
    public class ThSlabExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThSlabExtractionVisitor();
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }

    public class ThSlabRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThSlabExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }
        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            List<Curve> curves = new List<Curve>();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                var pline = new Polyline()
                {
                    Closed = true,
                };
                pline.CreatePolyline(polygon);
                foreach (var filterObj in spatialIndex.SelectCrossingPolygon(pline))
                {
                    curves.Add(filterObj as Curve);
                }
            }
            else
            {
                curves = objs.Cast<Curve>().ToList();
            }
            curves.ForEach(o =>
                {
                    if (o is Polyline polyline && polyline.Area > 0.0)
                    {
                        Elements.Add(ThIfcSlab.Create(polyline));
                    }
                });
        }
    }
}