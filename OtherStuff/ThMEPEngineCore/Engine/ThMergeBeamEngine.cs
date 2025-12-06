using System;
using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.Model;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThMergeBeamEngine : ThBeamPreprocessEngine
    { 
        private ThBeamConnectRecogitionEngine BeamConnectRecogitionEngine { get; set; }

        private ThCADCoreNTSSpatialIndex SpatialIndex
        {
            get
            {
                return BeamConnectRecogitionEngine.SpatialIndexManager.BeamSpatialIndex;
            }
        }

        private ThBuildingElementRecognitionEngine BeamEngine
        {
            get
            {
                return BeamConnectRecogitionEngine.BeamEngine;
            }
        }

        private IEnumerable<ThIfcLineBeam> LineBeams
        {
            get
            {
                return BeamEngine.Elements.Where(o => o is ThIfcLineBeam).Cast<ThIfcLineBeam>();
            }
        }

        public ThMergeBeamEngine(ThBeamConnectRecogitionEngine thBeamConnectRecogitionEngine)
        {
            BeamConnectRecogitionEngine = thBeamConnectRecogitionEngine;
        }

        public void Merge()
        {
            var beamElements = new List<ThIfcBuildingElement>();
            foreach (var beam in LineBeams)
            {
                var poly = beam.Outline as Polyline;
                var objs = SpatialIndex.SelectCrossingPolygon(poly);
                if (objs.Count == 1)
                {
                    continue;
                }
                var beams = BeamEngine.FilterByOutline(objs)
                    .Cast<ThIfcLineBeam>()
                    .Where(o => SameWidth(beam, o) && SameHeight(beam, o))
                    .Where(o => IsCollinear(beam, o))
                    .Where(o => IsOverlap(beam, o));
                if (beams.Count() == 1)
                {
                    continue;
                }
                var tagBeams = beams.Where(o => SpatialIndex.Tag(o.Outline) != null);
                if (tagBeams.Count() == 0)
                {
                    var tag = Guid.NewGuid().ToString();
                    beams.ForEach(o => SpatialIndex.AddTag(o.Outline, tag));
                }
                else
                {
                    var tag = SpatialIndex.Tag(tagBeams.First().Outline);
                    beams.ForEach(o => SpatialIndex.AddTag(o.Outline, tag));
                }
            }
            var groups = LineBeams.GroupBy(o => SpatialIndex.Tag(o.Outline));
            foreach (var group in groups)
            {
                if (group.Key == null)
                {
                    group.ForEach(o => beamElements.Add(o));
                }
                else
                {
                    beamElements.AddRange(MergeBeams(group.ToList()));
                }
            }
            BeamEngine.Elements = beamElements;
        }
    }
}
