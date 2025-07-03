using System;
using NFox.Cad;
using ThCADExtension;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSMPolygonExtension
    {
        public static List<Entity> ToDbEntities(this Hatch hatch)
        {
            var results = new List<Entity>();
            var polygons = new List<Polygon>();
            hatch.Boundaries().ForEach(o =>
            {
                if (o is Polyline polyline)
                {
                    polygons.Add(polyline.ToNTSPolygon());
                }
                else if (o is Circle circle)
                {
                    var circlePolygon = circle.ToNTSPolygon();
                    if (circlePolygon != null)
                    {
                        polygons.Add(circlePolygon);
                    }
                }
            });

            MultiPolygon multiPolygon = ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(polygons.ToArray());
            ThCADCoreNTSBuildArea buildArea = new ThCADCoreNTSBuildArea();
            var result = buildArea.Build(multiPolygon);
            foreach (var ploygon in FilterPolygons(result))
            {
                results.Add(ploygon.ToDbEntity());
            }
            return results;
        }
        private static List<Polygon> FilterPolygons(Geometry geometry)
        {
            var objs = new List<Polygon>();
            if (geometry.IsEmpty)
            {
                return objs;
            }
            if (geometry is Polygon polygon)
            {
                objs.Add(polygon);
            }
            else if (geometry is MultiPolygon polygons)
            {
                polygons.Geometries.ForEach(g => objs.AddRange(FilterPolygons(g)));
            }
            else
            {
                throw new NotSupportedException();
            }
            return objs;
        }
    }
}
