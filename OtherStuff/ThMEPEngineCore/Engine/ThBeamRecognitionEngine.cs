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
    public class ThBeamExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            var visitor = new ThBeamExtractionVisitor()
            {
                LayerFilter = ThBeamLayerManager.AnnotationXrefLayers(database),
            };
            var extractor = new ThBuildingElementExtractor();
            extractor.Accept(visitor);
            extractor.Extract(database);
            Results = visitor.Results;
        }
    }

    public class ThBeamRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThBeamExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            datas.ForEach(o => Elements.Add(ThIfcLineBeam.Create(o.Data as ThIfcBeamAnnotation)));
            if (polygon.Count > 0)
            {
                DBObjectCollection dbObjs = new DBObjectCollection();
                Elements.ForEach(o => dbObjs.Add(o.Outline));
                ThCADCoreNTSSpatialIndex beamSpatialIndex = new ThCADCoreNTSSpatialIndex(dbObjs);
                var pline = new Polyline()
                {
                    Closed = true,
                };
                pline.CreatePolyline(polygon);
                var filterObjs = beamSpatialIndex.SelectCrossingPolygon(pline);
                Elements = Elements.Where(o => filterObjs.Contains(o.Outline)).ToList();
            }

        }
    }
}
