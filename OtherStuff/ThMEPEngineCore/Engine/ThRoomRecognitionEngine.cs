using System.Linq;
using NFox.Cad;
using DotNetARX;
using ThCADCore.NTS;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomExtractionEngine : ThSpatialElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThRoomExtractionVisitor()
            {
                LayerFilter = ThRoomLayerManager.CurveXrefLayers(database),
            };
            var extractor = new ThSpatialElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results.AddRange(visitor.Results);
        }

        public override void ExtractFromMS(Database database)
        {
            var visitor = new ThRoomExtractionVisitor()
            {
                LayerFilter = ThSpaceBoundarLayerManager.CurveXrefLayers(database),
            };
            var extractor = new ThSpatialElementExtractor();
            extractor.Accept(visitor);
            extractor.ExtractFromMS(database);
            Results.AddRange(visitor.Results);
        }
    }
    public class ThRoomRecognitionEngine : ThSpatialElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThRoomExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void RecognizeMS(Database database, Point3dCollection polygon)
        {
            var engine = new ThRoomExtractionEngine();
            engine.ExtractFromMS(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcSpatialElementData> datas, Point3dCollection polygon)
        {
            var results = new List<ThRawIfcSpatialElementData>();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                var pline = new Polyline()
                {
                    Closed = true,
                };
                pline.CreatePolyline(polygon);
                var filterObjs = spatialIndex.SelectCrossingPolygon(pline);
                results = datas.Where(o => filterObjs.Contains(o.Geometry as Curve)).ToList();
            }
            else
            {
                results = datas;
            }
            results.ForEach(o =>
            {
                if (o.Geometry is Polyline polyline && polyline.Area > 0.0)
                {
                    var room = ThIfcRoom.Create(polyline);
                    var properties = ThPropertySet.CreateWithHyperlink2(o.Data as string);
                    if (properties.Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY))
                    {
                        room.Name = properties.Properties[ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY];
                    }
                    Elements.Add(room);
                }
            });
        }
    }
}
