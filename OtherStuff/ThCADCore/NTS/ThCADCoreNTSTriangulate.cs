using System;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSTriangulate
    {
        public static GeometryCollection VoronoiDiagram(this Polyline polyline, double distanceTolerance)
        {
            var lineString = polyline.ToNTSLineString();
            var voronoiDiagram = new VoronoiDiagramBuilder();
            voronoiDiagram.SetSites(Densifier.Densify(lineString, distanceTolerance));
            return voronoiDiagram.GetDiagram(ThCADCoreNTSService.Instance.GeometryFactory);
        }

        public static DBObjectCollection VoronoiTriangulation(this Polyline polyline, double distanceTolerance)
        {
            var objs = new DBObjectCollection();
            foreach (var geometry in polyline.VoronoiDiagram(distanceTolerance).Geometries)
            {
                if (geometry is Polygon polygon)
                {
                    objs.Add(polygon.Shell.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }

        public static DBObjectCollection DelaunayTriangulation(this Polyline polyline)
        {
            var objs = new DBObjectCollection();
            var delaunayTriangulation = new DelaunayTriangulationBuilder();
            delaunayTriangulation.SetSites(LineString.Empty.Union(polyline.ToNTSLineString()));
            var triangles = delaunayTriangulation.GetTriangles(ThCADCoreNTSService.Instance.GeometryFactory);
            foreach (var geometry in triangles.Geometries)
            {
                if (geometry is Polygon polygon)
                {
                    objs.Add(polygon.Shell.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }

        public static DBObjectCollection ConformingDelaunayTriangulation(this Point3dCollection points, Polyline polyline)
        {
            var objs = new DBObjectCollection();
            var builder = new ConformingDelaunayTriangulationBuilder();
            var sites = ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPointFromCoords(points.ToNTSCoordinates());
            builder.SetSites(sites);
            builder.Constraints = polyline.ToNTSLineString();
            var triangles = builder.GetTriangles(ThCADCoreNTSService.Instance.GeometryFactory);
            foreach (var geometry in triangles.Geometries)
            {
                if (geometry is Polygon polygon)
                {
                    objs.Add(polygon.Shell.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }

        public static DBObjectCollection DelaunayTriangulation(this Point3dCollection points)
        {
            var objs = new DBObjectCollection();
            var delaunayTriangulation = new DelaunayTriangulationBuilder();
            delaunayTriangulation.SetSites(points.ToNTSCoordinates());
            var triangles = delaunayTriangulation.GetTriangles(ThCADCoreNTSService.Instance.GeometryFactory);
            foreach (var geometry in triangles.Geometries)
            {
                if (geometry is Polygon polygon)
                {
                    objs.Add(polygon.Shell.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }
    }
}
