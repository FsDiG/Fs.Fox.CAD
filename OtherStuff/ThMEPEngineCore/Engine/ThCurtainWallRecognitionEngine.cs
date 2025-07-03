using System.Linq;
using System.Collections.Generic;
using NFox.Cad;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThCurtainWallExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThCurtainWallExtractionVisitor()
            {
                LayerFilter = ThCurtainWallLayerManager.CurveXrefLayers(database),
            };
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }
    public class ThCurtainWallRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThCurtainWallExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            var curves = new DBObjectCollection();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                foreach (var filterObj in spatialIndex.SelectCrossingPolygon(polygon))
                {
                    curves.Add(filterObj as Curve);
                }
            }
            else
            {
                curves = objs;
            }
            curves.Cast<Curve>().ForEach(o =>
            {
                if (o is Polyline polyline && polyline.Area > 0.0)
                {
                    Elements.Add(ThIfcCurtainWall.Create(o));
                }
            });
        }        
    }
}
