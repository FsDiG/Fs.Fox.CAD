using System;
using ThCADCore.NTS;
using ThCADExtension;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.BeamInfo.Utils
{
    public class ThArcBeamOutliner
    {
        public static Polyline TessellatedOutline(Arc arc1, Arc arc2)
        {
            var overlapEstimate = arc1.OverlapAngle(arc2);
            var startAngle = overlapEstimate.Item2;
            var endAngle = overlapEstimate.Item3;

            //将输入的两段arc转换为PolylineSegmentCollection
            var arc_1 = new Arc(arc1.Center, arc1.Radius, startAngle, endAngle);
            var arc_2 = new Arc(arc2.Center, arc2.Radius, startAngle, endAngle);
            var arcPolyLine1 = arc_1.TessellateArcWithChord(arc_1.Radius * (Math.Sin(Math.PI / 360.0)));
            var arcPolyLine2 = arc_2.TessellateArcWithChord(arc_2.Radius * (Math.Sin(Math.PI / 360.0)));

            // 获取两段新的arc的端点形成两段PolylineSegement
            PolylineSegment lineSegment_1 = new PolylineSegment(arcPolyLine1.EndPoint.ToPoint2D(), arcPolyLine2.EndPoint.ToPoint2D());
            PolylineSegment lineSegment_2 = new PolylineSegment(arcPolyLine2.StartPoint.ToPoint2D(), arcPolyLine1.StartPoint.ToPoint2D());

            // 用多段线封闭成一个封闭区域
            var segmentCollection = new PolylineSegmentCollection();
            var lineSegments_1 = new PolylineSegmentCollection(arcPolyLine1);
            foreach (var segment in lineSegments_1)
            {
                segmentCollection.Add(segment);
            }
            segmentCollection.Add(lineSegment_1);
            arcPolyLine2.ReverseCurve();
            var lineSegments_2 = new PolylineSegmentCollection(arcPolyLine2);
            foreach (var segment in lineSegments_2)
            {
                segmentCollection.Add(segment);
            }
            segmentCollection.Add(lineSegment_2);
            return segmentCollection.ToPolyline();
        }

        public static Polyline Outline(Arc arc1, Arc arc2)
        {
            var overlapEstimate = arc1.OverlapAngle(arc2);
            var startAngle = overlapEstimate.Item2;
            var endAngle = overlapEstimate.Item3;

            //将输入的两段arc转换为PolylineSegment
            Arc arc_1 = new Arc(arc1.Center, arc1.Radius, startAngle, endAngle);
            Arc arc_2 = new Arc(arc2.Center, arc2.Radius, startAngle, endAngle);
            double arcBulge1 = arc_1.BulgeFromCurve(false);
            double arcBulge2 = arc_2.BulgeFromCurve(false);
            PolylineSegment arcSegment_1 = new PolylineSegment(arc_1.StartPoint.ToPoint2D(), arc_1.EndPoint.ToPoint2D(), arcBulge1);
            PolylineSegment arcSegment_2 = new PolylineSegment(arc_2.EndPoint.ToPoint2D(), arc_2.StartPoint.ToPoint2D(), -arcBulge2);

            // 获取两段新的arc的端点形成两段PolylineSegement
            PolylineSegment lineSegment_1 = new PolylineSegment(arcSegment_1.EndPoint, arcSegment_2.StartPoint);
            PolylineSegment lineSegment_2 = new PolylineSegment(arcSegment_2.EndPoint, arcSegment_1.StartPoint);

            // 合并所有得到的PolylineSegment
            var segmentCollection = new PolylineSegmentCollection()
            {
                arcSegment_1,
                lineSegment_1,
                arcSegment_2,
                lineSegment_2
            };

            // 返回多段线
            return segmentCollection.ToPolyline();
        }
    }
}
