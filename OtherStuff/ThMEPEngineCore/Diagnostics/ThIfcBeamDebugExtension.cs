using System.Diagnostics;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;

namespace ThMEPEngineCore.Diagnostics
{
    public static class ThIfcBeamDiagnosticsExtension
    {
        public static void Debug(this ThIfcBeam beam, Point3d startPt, Point3d endPt)
        {
            if ((beam.StartPoint.DistanceTo(startPt) <= 10.0 && beam.EndPoint.DistanceTo(endPt) <= 10.0) ||
                (beam.StartPoint.DistanceTo(endPt) <= 10.0 && beam.EndPoint.DistanceTo(startPt) <= 10.0))
            {
                Debugger.Break();
            }
        }
    }
}
