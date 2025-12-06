using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSCenterlineBuilder
    {
        public static DBObjectCollection Centerline(Polyline polyline, double interpolationDistance)
        {
            var geometry = polyline.ToNTSPolygon();
            var lineStrings = new List<Geometry>();
            foreach (Polygon polygon in polyline.VoronoiDiagram(interpolationDistance).Geometries)
            {
                var iterator = new LinearIterator(polygon.Shell);
                for (; iterator.HasNext();iterator.Next())
                {
                    if (!iterator.IsEndOfLine)
                    {
                        var line = CreateLineString(iterator);
                        if (line.Within(geometry))
                        {
                            lineStrings.Add(line);
                        }
                    }
                }
            }
            var centerlines = new DBObjectCollection();
            foreach(LineString lineString in lineStrings)
            {
                centerlines.Add(lineString.ToDbPolyline());
            }
            return centerlines;
        }

        private static Geometry CreateLineString(LinearIterator iterator)
        {
            return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(new Coordinate[]
            {
                iterator.SegmentStart,
                iterator.SegmentEnd
            });
        }
    }
}
