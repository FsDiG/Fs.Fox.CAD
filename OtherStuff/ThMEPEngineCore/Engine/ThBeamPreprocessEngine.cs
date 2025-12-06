using System;
using NFox.Cad;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Algorithm;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThBeamPreprocessEngine : ThBuildingElementPreprocessEngine
    {
        protected bool IsCollinear(ThIfcLineBeam beam, ThIfcLineBeam other)
        {
            return (beam.Uuid == other.Uuid) ||
                ThMEPNTSExtension.IsLooseCollinear(
                beam.StartPoint,
                beam.EndPoint,
                other.StartPoint,
                other.EndPoint);
        }

        protected bool IsOverlap(ThIfcLineBeam beam, ThIfcLineBeam other)
        {
            return (beam.Uuid == other.Uuid) ||
                    ThMEPNTSExtension.IsLooseOverlap(
                    beam.StartPoint,
                    beam.EndPoint,
                    other.StartPoint,
                    other.EndPoint);
        }

        protected bool SameWidth(ThIfcLineBeam beam, ThIfcLineBeam other)
        {
            return Math.Abs(beam.Width - other.Width) <= ThMEPEngineCoreCommon.GEOMETRY_TOLERANCE.EqualPoint;
        }
        protected bool SameHeight(ThIfcLineBeam beam, ThIfcLineBeam other)
        {
            return Math.Abs(beam.Height - other.Height) <= ThMEPEngineCoreCommon.GEOMETRY_TOLERANCE.EqualPoint;
        }
        protected List<ThIfcLineBeam> MergeBeams(List<ThIfcLineBeam> beams)
        {
            var results = new List<ThIfcLineBeam>();
            var height = beams.Max(o => o.Height);
            beams.Select(o => o.Outline)
                .ToCollection()
                .LooseUnion()
                .Cast<Polyline>()
                .ForEachDbObject(o =>
                {
                    var rectangle = o.GetMinimumRectangle();
                    results.Add(ThIfcLineBeam.Create(rectangle, height));
                });
            return results;
        }
    }
}
