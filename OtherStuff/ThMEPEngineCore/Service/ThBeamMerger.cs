using System;
using System.Linq;
using System.Collections.Generic;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service
{
    public class ThBeamMerger
    {
        public ThBeamLink BeamLink { get; private set; }
        public ThBeamMerger(ThBeamLink thBeamLink)
        {
            BeamLink = thBeamLink;
        }
        public static ThBeamMerger Merge(ThBeamLink thBeamLink)
        {
            ThBeamMerger thBeamMerger = new ThBeamMerger(thBeamLink);
            thBeamMerger.Merge();
            return thBeamMerger;
        }
        private void Merge()
        {
            //if (BeamLink.Beams.Count <= 1)
            //{
            //    return;
            //}
            ////目前支持直梁的合并
            //if (!BeamLink.Beams.Where(o => o is ThIfcArcBeam).Any())
            //{
            //    double maxW = BeamLink.Beams.Select(o => o.Width).OrderByDescending(o => o).FirstOrDefault();
            //    double maxH = BeamLink.Beams.Select(o => o.Height).OrderByDescending(o => o).FirstOrDefault();
            //    var beamOutLine = BeamLink.CreateExtendBeamOutline(0.0);
            //    ThIfcLineBeam thIfcLineBeam = new ThIfcLineBeam()
            //    {
            //        Uuid = Guid.NewGuid().ToString(),
            //        StartPoint = beamOutLine.Item2,
            //        EndPoint = beamOutLine.Item3,
            //        Outline = beamOutLine.Item1,
            //        Width = maxW,
            //        Height = maxH,
            //        ComponentType = BeamLink.Beams[0].ComponentType
            //    };                
            //    BeamLink.Beams = new List<ThIfcBeam>();
            //    BeamLink.Beams.Add(thIfcLineBeam);
            //}
        }
    }
}
