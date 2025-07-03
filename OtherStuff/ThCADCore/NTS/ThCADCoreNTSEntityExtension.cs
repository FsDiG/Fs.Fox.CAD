using System;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSEntityExtension
    {
        public static Geometry ToNTSGeometry(this Entity obj)
        {
            if (obj is DBPoint point)
            {
                return point.ToNTSPoint();
            }
            else if (obj is Line line)
            {
                return line.ToNTSLineString();
            }
            else if (obj is Polyline polyline)
            {
                return polyline.ToNTSLineString();
            }
            else if (obj is Polyline2d poly2d)
            {
                return poly2d.ToNTSLineString();
            }
            else if (obj is Circle circle)
            {
                return circle.ToNTSPolygon();
            }
            else if (obj is Arc arc)
            {
                return arc.ToNTSLineString();
            }
            else if(obj is MPolygon mPolygon)
            {
                return mPolygon.ToNTSPolygon();
            }
            else if (obj is Region region)
            {
                return region.ToNTSPolygon();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Polygon ToNTSPolygon(this Entity entity)
        {
            if (entity is Polyline polyline)
            {
                return polyline.ToNTSPolygon();
            }
            else if (entity is Circle circle)
            {
                return circle.ToNTSPolygon();
            }
            else if (entity is MPolygon mPolygon)
            {
                return mPolygon.ToNTSPolygon();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
