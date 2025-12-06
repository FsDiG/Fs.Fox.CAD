using ThCADCore.NTS;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Geometries.Prepared;
using Autodesk.AutoCAD.Geometry;
using ThCADExtension;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPXClipInfo
    {
        public Polyline Polygon { get; set; }
        public bool Inverted { get; set; }

        public bool IsValid
        {
            get
            {
                return Polygon != null;
            }
        }

        public bool Contains(Point3d point)
        {
            var dbpoint = new DBPoint(point);
            return PreparedPolygon.Contains(dbpoint.ToNTSPoint());
        }

        public bool Contains(Curve curve)
        {
            return PreparedPolygon.Contains(curve.ToNTSGeometry());
        }

        public bool Contains(MPolygon mPolygon)
        {
            var polygon = mPolygon.ToNTSPolygon();
            return PreparedPolygon.Contains(polygon);
        }
        public bool Intersects(Curve curve)
        {
            return PreparedPolygon.Intersects(curve.ToNTSGeometry());
        }

        private IPreparedGeometry preparedPolygon;
        private IPreparedGeometry PreparedPolygon
        {
            get
            {
                if (preparedPolygon == null)
                {
                    preparedPolygon = PreparedGeometryFactory.Prepare(Polygon.ToNTSPolygon());
                }
                return preparedPolygon;
            }
        }

        public void TransformBy(Matrix3d mat)
        {
            if (IsValid)
            {
                Polygon.TransformBy(mat);
            }
        }
    }
}
