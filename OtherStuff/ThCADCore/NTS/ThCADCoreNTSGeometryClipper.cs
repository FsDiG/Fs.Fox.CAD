using NFox.Cad;
using System.Linq;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;
using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSGeometryClipper
    {
        private Polygon Clipper { get; set; }

        public ThCADCoreNTSGeometryClipper(AcPolygon polygon)
        {
            Clipper = polygon.ToNTSPolygon();
        }

        public static DBObjectCollection Clip(AcPolygon polygon, Curve curve, bool inverted = false)
        {
            var clipper = new ThCADCoreNTSGeometryClipper(polygon);
            return clipper.Clip(curve, inverted);
        }

        public static DBObjectCollection Clip(AcPolygon polygon, DBObjectCollection curves, bool inverted = false)
        {
            var clipper = new ThCADCoreNTSGeometryClipper(polygon);
            return clipper.Clip(curves, inverted);
        }

        public DBObjectCollection Clip(Curve curve, bool inverted = false)
        {
            return Clip(curve.ToNTSGeometry(), inverted).ToDbCollection();
        }

        public DBObjectCollection Clip(DBObjectCollection curves, bool inverted = false)
        {
            return Clip(curves.ToNTSNodedLineStrings(), inverted).ToDbCollection();
        }

        private Geometry Clip(Geometry other, bool inverted = false)
        {
            return inverted ? other.Difference(Clipper) : Clipper.Intersection(other);
        }
    }
}
