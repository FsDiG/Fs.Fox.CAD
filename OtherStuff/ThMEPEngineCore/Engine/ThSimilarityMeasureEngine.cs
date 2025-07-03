using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;

namespace ThMEPEngineCore.Engine
{
    public class ThSimilarityMeasureEngine
    {
        private ThBeamConnectRecogitionEngine BeamConnectRecogitionEngine { get; set; }

        public ThSimilarityMeasureEngine(ThBeamConnectRecogitionEngine thBeamConnectRecogitionEngine)
        {
            BeamConnectRecogitionEngine = thBeamConnectRecogitionEngine;
        }
        public void SimilarityMeasure()
        {
            BeamMeasure();
        }
        private void BeamMeasure()
        {            
            List<ThIfcElement> duplicateCollection = new List<ThIfcElement>();
            BeamConnectRecogitionEngine.BeamEngine.Elements.ForEach(m =>
            {
                if (!duplicateCollection.Where(n => n.Uuid == m.Uuid).Any())
                {
                    DBObjectCollection crossObjs = BeamConnectRecogitionEngine.SpatialIndexManager.
                    BeamSpatialIndex.SelectCrossingPolygon(m.Outline as Polyline);
                    Polyline baseOutline = m.Outline as Polyline;
                    foreach (DBObject crossObj in crossObjs)
                    {
                        ThIfcElement thIfcElement = BeamConnectRecogitionEngine.BeamEngine.FilterByOutline(crossObj);
                        if (thIfcElement.Uuid != m.Uuid)
                        {
                            double measure = baseOutline.SimilarityMeasure(thIfcElement.Outline as Polyline);
                            if (measure >= ThMEPEngineCoreCommon.SIMILARITYMEASURETOLERANCE)
                            {
                                duplicateCollection.Add(thIfcElement);
                            }
                        }
                    }
                }
            });
            BeamConnectRecogitionEngine.BeamEngine.Elements = 
                BeamConnectRecogitionEngine.BeamEngine.Elements.Where(
                    m => !duplicateCollection.Where(n => n.Uuid == m.Uuid).Any()).ToList();            
        }
    }
}
