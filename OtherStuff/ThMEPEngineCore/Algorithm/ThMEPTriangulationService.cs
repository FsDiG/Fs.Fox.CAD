#if ACAD2016
using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;
using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;
using CLI;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPTriangulationService
    {
        public class Triangle
        {
            public int[] vertices { get; set; }

            public Triangle(int v0, int v1, int v2)
            {
                vertices = new int[]
                {
                    v0, v1, v2,
                };
            }

            public int[] getSharedVertices(Triangle other)
            {
                int count = 0;
                bool[] shared = new bool[3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (vertices[i] == other.vertices[j])
                        {
                            count++;
                            shared[i] = true;
                        }
                    }
                }
                int[] common = null;
                if (count > 0)
                {
                    common = new int[count];
                    for (int i = 0, k = 0; i < 3; i++)
                    {
                        if (shared[i])
                        {
                            common[k++] = vertices[i];
                        }
                    }
                }
                return common;
            }
        }

        public class EdgeFlipper
        {
            public List<Coordinate> shellCoords { get; set; }

            public EdgeFlipper(Coordinate[] coordinates)
            {
                shellCoords = new List<Coordinate>(coordinates);
            }

            public bool flip(Triangle t0, Triangle t1)
            {
                return flip(t0, t1, t0.getSharedVertices(t1));
            }

            public bool flip(Triangle ear0, Triangle ear1, int[] sharedVertices)
            {
                if (sharedVertices == null || sharedVertices.Length != 2)
                {
                    return false;
                }

                Coordinate shared0 = shellCoords[sharedVertices[0]];
                Coordinate shared1 = shellCoords[sharedVertices[1]];

                /*
                 * Find the unshared vertex of each ear
                 */
                int[] vertices = ear0.vertices;
                int i = 0;
                while (vertices[i] == sharedVertices[0] || vertices[i] == sharedVertices[1])
                {
                    i++;
                }
                int v0 = vertices[i];
                Coordinate c0 = shellCoords[v0];

                i = 0;
                vertices = ear1.vertices;
                while (vertices[i] == sharedVertices[0] || vertices[i] == sharedVertices[1])
                {
                    i++;
                }
                int v1 = vertices[i];
                Coordinate c1 = shellCoords[v1];

                /*
                 * The candidate new edge is from v0 to v1. First check if this
                 * is inside the quadrilateral
                 */
                Coordinate[] quadRing = { c0, shared0, c1, shared1 };

                int dir0 = (int)Orientation.Index(c0, c1, shared0);
                int dir1 = (int)Orientation.Index(c0, c1, shared1);
                if (dir0 == -dir1)
                {
                    // The candidate edge is inside. Compare its length to
                    // the current shared edge and swap them if the candidate
                    // is shorter.
                    if (c0.Distance(c1) < shared0.Distance(shared1))
                    {
                        ear0.vertices = new int[] { sharedVertices[0], v0, v1 };
                        ear1.vertices = new int[] { v1, v0, sharedVertices[1] };
                        return true;
                    }
                }

                return false;
            }
        }

        public static DBObjectCollection EarCut(AcPolygon shell, AcPolygon[] holes)
        {
            var indices = new List<int>();
            var points = new List<double>();
            points.AddRange(shell.Coordinates2D());
            holes.ForEach(o =>
            {
                indices.Add(points.Count / 2 - 1);
                points.AddRange(o.Coordinates2D());
            });
            var shellCoords = new List<Coordinate>();
            shellCoords.AddRange(shell.Vertices().ToNTSCoordinates());
            holes.ForEach(o =>
            {
                shellCoords.AddRange(o.Vertices().ToNTSCoordinates());
            });

            // EarCut:
            //  https://github.com/mapbox/earcut.hpp
            var objs = new DBObjectCollection();
            var triangles = new List<Triangle>();
            var builder = new ThEarCutTriangulationBuilder();
            var results = builder.EarCut(points.ToArray(), points.Count / 2, indices.ToArray(), indices.Count);
            for (int i = 0; i < results.Count(); i += 3)
            {
                triangles.Add(new Triangle(results[i], results[i+1], results[i+2]));
            }

            // Refinement:
            //  https://github.com/metsfan/jts_earclipper
            //  http://lin-ear-th-inking.blogspot.com/2011/04/polygon-triangulation-via-ear-clipping.html
            EdgeFlipper ef = new EdgeFlipper(shellCoords.ToArray());
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < triangles.Count() - 1 && !changed; i++)
                {
                    Triangle ear0 = triangles[i];
                    for (int j = i + 1; j < triangles.Count() && !changed; j++)
                    {
                        Triangle ear1 = triangles[j];
                        int[] sharedVertices = ear0.getSharedVertices(ear1);
                        if (sharedVertices != null && sharedVertices.Length == 2)
                        {
                            if (ef.flip(ear0, ear1, sharedVertices))
                            {
                                changed = true;
                            }
                        }
                    }
                }
            } while (changed);

            // Convert to Polyline
            for (int i = 0; i < triangles.Count(); i++)
            {
                var triangle = ThPolylineExtension.CreateTriangle(
                    new Point2d(points[2 * triangles[i].vertices[0]], points[2 * triangles[i].vertices[0] + 1]),
                    new Point2d(points[2 * triangles[i].vertices[1]],points[2 * triangles[i].vertices[1] + 1]),
                    new Point2d(points[2 * triangles[i].vertices[2]], points[2 * triangles[i].vertices[2] + 1]));
                objs.Add(triangle);
            }
            return objs;
        }
    }
}
#endif
