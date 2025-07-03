using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThShearWallExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThShearWallExtractionVisitor()
            {
                LayerFilter = ThStructureShearWallLayerManager.HatchXrefLayers(database),
            };
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }

    public class ThShearWallRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThShearWallExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            List<Entity> ents = new List<Entity>();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                ThCADCoreNTSSpatialIndex shearwallCurveSpatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                foreach (var filterObj in shearwallCurveSpatialIndex.SelectCrossingPolygon(polygon))
                {
                    ents.Add(filterObj as Entity);
                }
            }
            else
            {
                ents = objs.Cast<Entity>().ToList();
            }
            ents.ForEach(o =>
            {
                if (o is Polyline polyline && polyline.Area > 0.0)
                {
                    var bufferObjs = polyline.Buffer(ThMEPEngineCoreCommon.ShearWallBufferDistance);
                    if (bufferObjs.Count == 1)
                    {
                        var outline = bufferObjs[0] as Polyline;
                        Elements.Add(ThIfcWall.Create(outline));
                    }
                }
            });
        }
    }
}
