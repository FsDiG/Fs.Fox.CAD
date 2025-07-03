using NFox.Cad;
using DotNetARX;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomMarkExtractionEngine : ThAnnotationElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThRoomMarkExtractionVisitor
            {
                LayerFilter = ThSpaceNameLayerManager.TextXrefLayers(database)
            };
            var extractor = new ThAnnotationElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }

        public override void ExtractFromMS(Database database)
        {
            var visitor = new ThRoomMarkExtractionVisitor
            {
                LayerFilter = ThSpaceNameLayerManager.TextXrefLayers(database)
            };
            var extractor = new ThAnnotationElementExtractor();
            extractor.Accept(visitor);
            extractor.ExtractFromMS(database);
            Results = visitor.Results;
        }
    }
    public class ThRoomMarkRecognitionEngine : ThAnnotationElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThRoomMarkExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void RecognizeMS(Database database, Point3dCollection polygon)
        {
            var engine = new ThRoomMarkExtractionEngine();
            engine.ExtractFromMS(database);
            Recognize(engine.Results, polygon);
        }        

        public override void Recognize(List<ThRawIfcAnnotationElementData> datas, Point3dCollection polygon)
        {
            var objs = datas.Select(o => o.Geometry).ToCollection();
            var filterDatas = new List<ThRawIfcAnnotationElementData>();
            if (polygon.Count > 0)
            {
                var spatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                var pline = new Polyline()
                {
                    Closed = true,
                };
                pline.CreatePolyline(polygon);
                var filterObjs = spatialIndex.SelectCrossingPolygon(pline);
                filterDatas = datas.Where(o => filterObjs.Contains(o.Geometry)).ToList();
            }
            else
            {
                filterDatas = datas;
            }
            filterDatas.ForEach(o => Elements.Add(ThIfcTextNote.Create(GetContent(o.Data), o.Geometry as Polyline)));
        }
        private string GetContent(object data)
        {
            string text = "";
            if (data is DBText dbText)
            {
                text = dbText.TextString;
            }
            else if (data is MText mText)
            {
                text = mText.Text;
            }
            else if (data is string str)
            {
                text = str;
            }
            return text;
        }
    }
}
