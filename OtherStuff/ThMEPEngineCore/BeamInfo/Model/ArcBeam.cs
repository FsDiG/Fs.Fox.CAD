using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.BeamInfo.Utils;

namespace ThMEPEngineCore.BeamInfo.Model
{
    public class ArcBeam : Beam
    {
        public ArcBeam(Curve upLine, Curve downLine)
        {
            if ((upLine as Arc).Length > (downLine as Arc).Length)
            {
                UpBeamLine = upLine;
                DownBeamLine = downLine;
            }
            else
            {
                UpBeamLine = downLine;
                DownBeamLine = upLine;
            }

            UpStartPoint = UpBeamLine.StartPoint;
            UpEndPoint = UpBeamLine.EndPoint;
            if (UpStartPoint.DistanceTo(DownBeamLine.StartPoint) < UpStartPoint.DistanceTo(DownBeamLine.EndPoint))
            {
                DownStartPoint = DownBeamLine.StartPoint;
                DownEndPoint = DownBeamLine.EndPoint;
            }
            else
            {
                DownStartPoint = DownBeamLine.EndPoint;
                DownEndPoint = DownBeamLine.StartPoint;
            }

            CenterPoint = (upLine as Arc).Center;
            Vector3d proV = Vector3d.ZAxis.CrossProduct(UpEndPoint - UpStartPoint).GetNormal();
            Point3d proP1 = upLine.GetClosestPointTo(CenterPoint, proV, false);
            Point3d proP2 = downLine.GetClosestPointTo(CenterPoint, proV, false);
            if (CenterPoint.DistanceTo(proP1) > CenterPoint.DistanceTo(proP2))
            {
                MiddlePoint = proP1;
            }
            else
            {
                MiddlePoint = proP2;
            }
            BeamNormal = (MiddlePoint - CenterPoint).GetNormal();

            BeamSPointSolid = CreatePolyline(UpStartPoint, DownStartPoint, BeamNormal, 10);
            BeamEPointSolid = CreatePolyline(UpEndPoint, DownEndPoint, BeamNormal, 10);
        }

        public override Polyline BeamBoundary
        {
            get
            {
                var arc_1 = UpBeamLine as Arc;
                var arc_2 = DownBeamLine as Arc;
                return ThArcBeamOutliner.Outline(arc_1, arc_2);
            }
        }

        public Point3d MiddlePoint { get; set; }

        public Point3d CenterPoint { get; set; }
    }
}
