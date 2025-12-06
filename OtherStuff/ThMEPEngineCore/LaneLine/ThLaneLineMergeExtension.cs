using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.LaneLine
{
    public class ThLaneLineMergeExtension : ThLaneLineEngine
    {
        public static DBObjectCollection Merge(DBObjectCollection curves)
        {
            var objs = new DBObjectCollection();
            var parallelLines = GroupParallelLines(curves);
            parallelLines.ForEach(l =>
            {
                if (l.Count == 1)
                {
                    objs.Add(l[0] as Line);
                }
                else
                {
                    objs.Add(MergeLines(l.Cast<Line>().ToList()));
                }
            });
            return objs;
        }

        public static DBObjectCollection Merge(DBObjectCollection theCurves, DBObjectCollection otherCurves)
        {
            return Merge(theCurves.Cast<DBObject>().Union(otherCurves.Cast<DBObject>()).ToCollection());
        }

        private static Line MergeLines(List<Line> lines)
        {
            var polygons = lines.Select(o => Expand(o, extend_distance, collinear_gap_distance)).Select(o => o.ToNTSPolygon());
            var multiPolygon = ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(polygons.ToArray());
            var centerline =  CenterLine(multiPolygon.Union());
            var direction = centerline.LineDirection();
            return new Line(centerline.StartPoint + direction * extend_distance, centerline.EndPoint - direction * extend_distance);
        }
    }
}
