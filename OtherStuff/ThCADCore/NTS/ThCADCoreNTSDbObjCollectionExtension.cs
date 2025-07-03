using System;
using NFox.Cad;
using System.Linq;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Geometries.Utilities;
using NTSDimension = NetTopologySuite.Geometries.Dimension;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSDbObjCollectionExtension
    {
        public static MultiPolygon ToNTSMultiPolygon(this DBObjectCollection objs)
        {
            return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(objs.ToNTSPolygons().ToArray());
        }

        public static List<Polygon> ToNTSPolygons(this DBObjectCollection curves)
        {
            var polygons = new List<Polygon>();
            curves.Cast<Entity>().ForEach(e => polygons.Add(e.ToNTSPolygon()));
            return polygons;
        }

        public static List<Geometry> ToNTSLineStrings(this DBObjectCollection curves)
        {
            var geometries = new List<Geometry>();
            curves.Cast<Entity>().ForEach(e => geometries.Add(e.ToNTSGeometry()));
            return geometries;
        }

        public static Geometry ToNTSNodedLineStrings(this DBObjectCollection curves)
        {
            // UnaryUnionOp.Union()有Robust issue
            // 会抛出"non-noded intersection" TopologyException
            // 为了规避这个问题，这里使用Geometry.Union()
            // https://gis.stackexchange.com/questions/50399/fixing-non-noded-intersection-problem-using-postgis
            var mLineString = ToMultiLineString(curves);
            Geometry nodedLineStrings = ThCADCoreNTSService.Instance.GeometryFactory.CreateEmpty(NTSDimension.Curve);
            mLineString.Geometries.ForEach(o => nodedLineStrings = nodedLineStrings.Union(o));
            return nodedLineStrings;
        }

        public static Geometry UnionGeometries(this DBObjectCollection curves)
        {
            // https://lin-ear-th-inking.blogspot.com/2007/11/fast-polygon-merging-in-jts-using.html
            return curves.ToNTSMultiPolygon().Union();
        }

        public static DBObjectCollection UnionPolygons(this DBObjectCollection curves)
        {
            return curves.UnionGeometries().ToDbCollection();
        }

        public static Polyline GetMinimumRectangle(this DBObjectCollection curves)
        {
            var geometry = curves.Combine();
            var rectangle = MinimumDiameter.GetMinimumRectangle(geometry);
            if (rectangle is Polygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static MultiLineString ToMultiLineString(this DBObjectCollection curves)
        {
            var geometries = curves.Cast<Curve>().Select(o => o.ToNTSGeometry());
            var lineStrings = geometries.Where(o => o is LineString).Cast<LineString>();
            return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiLineString(lineStrings.ToArray());
        }

        public static Geometry Combine(this DBObjectCollection curves)
        {
            var geometries = curves.ToNTSLineStrings();
            return GeometryCombiner.Combine(geometries);
        }

        public static bool Covers(this DBObjectCollection curves, Line line)
        {
            return curves.ToMultiLineString().Covers(line.ToNTSGeometry());
        }

        public static DBObjectCollection ToDbCollection(this Geometry geometry, bool keepHoles = false)
        {
            return geometry.ToDbObjects().ToCollection<DBObject>();
        }
    }
}