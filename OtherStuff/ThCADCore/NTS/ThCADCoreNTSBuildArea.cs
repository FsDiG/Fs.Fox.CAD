using System;
using System.Linq;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Polygonize;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSPolygonFace : IComparable
    {
        public Polygon Poly { get; set; }
        public Geometry Env { get; set; }
        public double Envarea { get; set; }
        public ThCADCoreNTSPolygonFace Parent { get; set; }

        public uint ParentCount()
        {
            uint count = 0;
            ThCADCoreNTSPolygonFace face = this;
            while (face.Parent != null)
            {
                ++count;
                face = face.Parent;
            }
            return count;
        }

        public static ThCADCoreNTSPolygonFace Create(Polygon polygon)
        {
            return new ThCADCoreNTSPolygonFace()
            {
                Poly = polygon,
                Env = polygon.Envelope,
                Envarea = polygon.Envelope.Area,
            };
        }

        public int CompareTo(object obj)
        {
            // return Less than zero if this object 
            // is less than the object specified by the CompareTo method.

            // return Zero if this object is equal to the object 
            // specified by the CompareTo method.

            // return Greater than zero if this object is greater than 
            // the object specified by the CompareTo method.
            var that = (ThCADCoreNTSPolygonFace)obj;
            return this.Envarea.CompareTo(that.Envarea);
        }
    }

    public class ThCADCoreNTSBuildArea
    {
        public Geometry Build(Geometry geometry)
        {
            var polys = geometry.Polygonize();

            if (polys.Count == 0)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateGeometryCollection();
            }

            if (polys.Count == 1)
            {
                return polys.First();
            }

            var faces = new List<ThCADCoreNTSPolygonFace>();
            foreach (Polygon poly in polys)
            {
                faces.Add(ThCADCoreNTSPolygonFace.Create(poly));
            }

            var polygons = CollectFacesWithEvenAncestors(FindFaceHoles(faces));
            return CascadedPolygonUnion.Union(polygons.Geometries);
        }

        private List<ThCADCoreNTSPolygonFace> FindFaceHoles(List<ThCADCoreNTSPolygonFace> faces)
        {
            var sortedFaces = faces.OrderByDescending(o => o.Envarea).ToList();
            for (int i = 0; i < sortedFaces.Count; i++)
            {
                var face = sortedFaces[i];
                int holes = face.Poly.NumInteriorRings;
                for (int h = 0; h < holes; h++)
                {
                    var hole = face.Poly.GetInteriorRingN(h);
                    for (int j = i + 1; j < faces.Count; j++)
                    {
                        var face2 = sortedFaces[j];
                        if (face2.Parent != null)
                        {
                            continue;
                        }

                        var face2Exterior = face2.Poly.ExteriorRing;
                        if (face2Exterior.Equals(hole))
                        {
                            face2.Parent = face;
                            break;
                        }
                    }
                }
            }
            return sortedFaces;
        }

        private MultiPolygon CollectFacesWithEvenAncestors(List<ThCADCoreNTSPolygonFace> faces)
        {
            var geoms = new List<Polygon>();
            foreach (var face in faces)
            {
                if ((face.ParentCount() % 2) != 0)
                {
                    continue;
                }
                geoms.Add(face.Poly);
            }
            return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(geoms.ToArray());
        }
    }

}