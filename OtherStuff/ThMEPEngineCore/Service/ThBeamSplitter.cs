using System;
using System.Linq;
using System.Collections.Generic;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Model.Segment;

namespace ThMEPEngineCore.Service
{
    public abstract class ThBeamSplitter
    {
        public List<ThIfcBeam> SplitBeams { get; protected set; }
        protected ThBeamSplitter()
        {
            SplitBeams = new List<ThIfcBeam>();
        }
        public abstract void Split(List<Entity> outlines);
        public abstract void SplitTType(List<ThIfcBeam> beams);
    }
}
