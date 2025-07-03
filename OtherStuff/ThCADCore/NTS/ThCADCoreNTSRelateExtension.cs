using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Relate;
using NTSDimension = NetTopologySuite.Geometries.Dimension;
using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSRelate
    {
        private IntersectionMatrix Matrix { get; set; }

        public ThCADCoreNTSRelate(AcPolygon poly0, AcPolygon poly2)
        {
            Matrix = RelateOp.Relate(poly0.ToNTSPolygon(), poly2.ToNTSPolygon());
        }

        public ThCADCoreNTSRelate(Polygon poly0, Polygon poly2)
        {
            Matrix = RelateOp.Relate(poly0, poly2);
        }

        public bool IsCovers
        {
            get
            {
                // Geometry A covers Geometry B if no points of B lie in the exterior of A
                // Covers vs contains:
                //      http://lin-ear-th-inking.blogspot.be/2007/06/subtleties-of-ogc-covers-spatial.html
                return Matrix.IsCovers();
            }
        }

        public bool IsContains
        {
            get
            {
                // Geometry A contains Geometry B if no points of B lie in the exterior of A, 
                // and at least one point of the interior of B lies in the interior of A
                return Matrix.IsContains();
            }
        }

        public bool IsIntersects
        {
            get
            {
                // INTERSECT returns a feature if any spatial relationship is found. 
                // Applies to all shape type combinations.
                // Intersect is true if the intersection of the two polygons is a point, a line or a polygon of any shape. 
                // It includes all types of spatial relationships (except "disjoint", of course)
                return Matrix.IsIntersects();
            }
        }

        public bool IsOverlaps
        {
            get
            {
                // OVERLAP returns a feature if the intersection of the two shapes results in an object of the same dimension, 
                // but different from both of the shapes.
                // Overlap is true if the intersection of two polygons is a polygon (does not include "touch") 
                // and the shape of this intersection is different from both shapes(does not include "identical", "contains" or "within"). 
                // Overlap is thus far more restrictive.
                return Matrix.IsOverlaps(NTSDimension.Surface, NTSDimension.Surface);
            }
        }

        public bool IsWithIn
        {
            get
            {
                return Matrix.IsWithin();
            }
        }
    }
}
