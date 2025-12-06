using System.Linq;
using System.Collections.Generic;
using NetTopologySuite.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Geometries.Implementation;
using NTSVertex = NetTopologySuite.Triangulate.QuadEdge.Vertex;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSConcaveHull
    {
        private class Vertex
        {
            public int id { get; set; }

            public Coordinate coordinate { get; set; }

            public bool border { get; set; }

            public Vertex(int id, Coordinate coordinate)
            {
                this.id = id;
                this.coordinate = coordinate;
            }

            /**
             * Returns the ID of the vertex.
             * 
             * @return
             * 		the ID of the vertex
             */
            public int getId()
            {
                return this.id;
            }

            /**
             * Defines the ID of the vertex.
             * 
             * @param id
             * 		the ID of the vertex
             */
            public void setId(int id)
            {
                this.id = id;
            }

            /**
             * Returns the coordinate of the vertex.
             * 
             * @return
             * 		the coordinate of the vertex
             */
            public Coordinate getCoordinate()
            {
                return this.coordinate;
            }

            /**
             * Defines the coordinate of the vertex.
             * 
             * @param c
             * 		the coordinate of the vertex
             */
            public void setCoordinate(Coordinate c)
            {
                this.coordinate = c;
            }

            /**
             * Returns true if the vertex is a border vertex
             * of the triangulation framework, false otherwise.
             * 
             * @return
             * 		true if the vertex is a border vertex,
             * 		false otherwise
             */
            public bool isBorder()
            {
                return this.border;
            }

            /**
             * Defines the indicator to know if the edge
             * is a border edge of the triangulation framework.
             * 
             * @param border
             * 		true if the edge is a border edge,
             * 		false otherwise
             */
            public void setBorder(bool border)
            {
                this.border = border;
            }
        }

        private class Triangle
        {
            public int id { get; set; }

            public bool border { get; set; }

            public List<Edge> edges { get; set; } = new List<Edge>();

            public List<Triangle> neighbours { get; set; } = new List<Triangle>();

            public Triangle(int id, bool border)
            {
                this.id = id;
                this.border = border;
            }

            /**
             * Returns the ID of the triangle.
             * 
             * @return
             * 		the ID of the triangle
             */
            public int getId()
            {
                return this.id;
            }

            /**
             * Defines the ID of the triangle.
             * 
             * @param id
             * 		ID of the triangle
             */
            public void setId(int id)
            {
                this.id = id;
            }

            /**
             * Returns true if the triangle is a border triangle
             * of the triangulation framework, false otherwise.
             * 
             * @return
             * 		true if the triangle is a border triangle,
             * 		false otherwise
             */
            public bool isBorder()
            {
                return this.border;
            }

            /**
             * Defines the indicator to know if the triangle
             * is a border triangle of the triangulation framework.
             * 
             * @param border
             * 		true if the triangle is a border triangle,
             * 		false otherwise
             */
            public void setBorder(bool border)
            {
                this.border = border;
            }

            /**
             * Returns the edges which compose the triangle.
             * 
             * @return
             * 		the edges of the triangle which compose the triangle
             */
            public List<Edge> getEdges()
            {
                return this.edges;
            }

            /**
             * Defines the edges which compose the triangle.
             * 
             * @param edges
             * 		the edges which compose the triangle
             */
            public void setEdges(List<Edge> edges)
            {
                this.edges = edges;
            }

            /**
             * Returns the neighbour triangles of the triangle.
             * 
             * @return
             * 		the neighbour triangles of the triangle
             */
            public List<Triangle> getNeighbours()
            {
                return this.neighbours;
            }

            /**
             * Defines the neighbour triangles of the triangle.
             * 
             * @param neighbours
             * 		the neighbour triangles of the triangle
             */
            public void setNeighbours(List<Triangle> neighbours)
            {
                this.neighbours = neighbours;
            }


            /**
             * Add an edge to the triangle.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addEdge(Edge edge)
            {
                getEdges().Add(edge);
            }

            /**
             * Add edges to the triangle.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addEdges(List<Edge> edges)
            {
                getEdges().AddRange(edges);
            }

            /**
             * Remove an edge of the triangle.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeEdge(Edge edge)
            {
                return getEdges().Remove(edge);
            }

            /**
             * Remove edges of the triangle.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeEdges(List<Edge> edges)
            {
                return getEdges().RemoveAll(o => edges.Contains(o)) > 0;
            }


            /**
             * Add a neighbour triangle to the triangle.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addNeighbour(Triangle triangle)
            {
                getNeighbours().Add(triangle);
            }

            /**
             * Add neighbour triangles to the triangle.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addNeighbours(List<Triangle> triangles)
            {
                getNeighbours().AddRange(triangles);
            }

            /**
             * Remove a neighbour triangle of the triangle.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeNeighbour(Triangle triangle)
            {
                return getNeighbours().Remove(triangle);
            }

            /**
             * Remove neighbour triangles of the triangle.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeNeighbours(List<Triangle> triangles)
            {
                return getNeighbours().RemoveAll(o => triangles.Contains(o)) > 0;
            }
        }

        private class Edge
        {
            public int id { get; set; }

            public LineSegment geometry { get; set; }

            public bool border { get; set; }

            public Vertex oV { get; set; }

            public Vertex eV { get; set; }

            public List<Triangle> triangles { get; set; } = new List<Triangle>();

            public List<Edge> incidentEdges { get; set; } = new List<Edge>();

            public Edge(int id, LineSegment geometry, Vertex oV, Vertex eV, bool border)
            {
                this.id = id;
                this.oV = oV;
                this.eV = eV;
                this.border = border;
                this.geometry = geometry;
            }

            /**
             * Returns the ID of the edge.
             * 
             * @return
             * 		the ID of the edge
             */
            public int getId()
            {
                return this.id;
            }

            /**
             * Defines the ID of the edge.
             * 
             * @param id
             * 		ID of the edge
             */
            public void setId(int id)
            {
                this.id = id;
            }

            /**
             * Returns the geometry of the edge.
             * 
             * @return
             * 		the geometry of the edge
             */
            public LineSegment getGeometry()
            {
                return this.geometry;
            }

            /**
             * Defines the geometry of the edge.
             * 
             * @param geometry
             * 		geometry of the edge (segment)
             */
            public void setGeometry(LineSegment geometry)
            {
                this.geometry = geometry;
            }

            /**
             * Returns true if the edge is a border edge
             * of the triangulation framework, false otherwise.
             * 
             * @return
             * 		true if the edge is a border edge,
             * 		false otherwise
             */
            public bool isBorder()
            {
                return this.border;
            }

            /**
             * Defines the indicator to know if the edge
             * is a border edge of the triangulation framework.
             * 
             * @param border
             * 		true if the edge is a border edge,
             * 		false otherwise
             */
            public void setBorder(bool border)
            {
                this.border = border;
            }

            /**
             * Returns the origin vertex of the edge.
             * 
             * @return
             * 		the origin vertex of the edge
             */
            public Vertex getOV()
            {
                return this.oV;
            }

            /**
             * Defines the origin vertex of the edge.
             * 
             * @param oV
             * 		origin vertex of the edge
             */
            public void setOV(Vertex oV)
            {
                this.oV = oV;
            }

            /**
             * Returns the end vertex of the edge.
             * 
             * @return
             * 		the end vertex of the edge
             */
            public Vertex getEV()
            {
                return this.eV;
            }

            /**
             * Defines the end vertex of the edge.
             * 
             * @param eV
             * 		end vertex of the edge
             */
            public void setEV(Vertex eV)
            {
                this.eV = eV;
            }

            /**
             * Returns the triangles in relationship with the edge.
             * 
             * @return
             * 		the triangles in relationship with the edge
             */
            public List<Triangle> getTriangles()
            {
                return this.triangles;
            }

            /**
             * Defines the triangles in relationship with the edge.
             * 
             * @param triangles
             * 		the triangles in relationship with the edge
             */
            public void setTriangles(List<Triangle> triangles)
            {
                this.triangles = triangles;
            }

            /**
             * Returns the edges in relationship with the edge.
             * 
             * @return
             * 		the edges in relationship with the edge
             */
            public List<Edge> getIncidentEdges()
            {
                return this.incidentEdges;
            }

            /**
             * Defines the edges in relationship with the edge.
             * 
             * @param edges
             * 		the edges in relationship with the edge
             */
            public void setIncidentEdges(List<Edge> edges)
            {
                this.incidentEdges = edges;
            }

            /**
             * Add a triangle in relationship with the edge.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addTriangle(Triangle triangle)
            {
                getTriangles().Add(triangle);
            }

            /**
             * Add triangles in relationship with the edge.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addTriangles(List<Triangle> triangles)
            {
                getTriangles().AddRange(triangles);
            }

            /**
             * Remove a triangle in relationship with the edge.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeTriangle(Triangle triangle)
            {
                return getTriangles().Remove(triangle);
            }

            /**
             * Remove triangles in relationship with the edge.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeTriangles(List<Triangle> triangles)
            {
                return getTriangles().RemoveAll(o => triangles.Contains(o)) > 0;
            }

            /**
             * Add an incident edge in relationship with the edge.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addIncidentEdge(Edge edge)
            {
                getIncidentEdges().Add(edge);
            }

            /**
             * Add incident edges in relationship with the edge.
             * 
             * @return
             * 		true if added, false otherwise
             */
            public void addIncidentEdges(List<Edge> edges)
            {
                getIncidentEdges().AddRange(edges);
            }

            /**
             * Remove an incident edge in relationship with the edge.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeIncidentEdge(Edge edge)
            {
                return getIncidentEdges().Remove(edge);
            }

            /**
             * Remove incident edges in relationship with the edge.
             * 
             * @return
             * 		true if removed, false otherwise
             */
            public bool removeAllIncidentEdges(List<Edge> edges)
            {
                return getIncidentEdges().RemoveAll(o => edges.Contains(o)) > 0;
            }
        }

        private GeometryFactory geomFactory;
        private GeometryCollection geometries;
        private double threshold;

        private Dictionary<LineSegment, int> segments = new Dictionary<LineSegment, int>();
        private Dictionary<int, Edge> edges = new Dictionary<int, Edge>();
        private Dictionary<int, Triangle> triangles = new Dictionary<int, Triangle>();
        private SortedDictionary<int, Edge> lengths = new SortedDictionary<int, Edge>();

        private Dictionary<int, Edge> shortLengths = new Dictionary<int, Edge>();

        private Dictionary<Coordinate, int> coordinates = new Dictionary<Coordinate, int>();
        private Dictionary<int, Vertex> vertices = new Dictionary<int, Vertex>();

        public ThCADCoreNTSConcaveHull(Geometry geometry, double threshold)
        {
            this.threshold = threshold;
            this.geomFactory = geometry.Factory;
            this.geometries = transformIntoPointGeometryCollection(geometry);
        }

        private GeometryCollection transformIntoPointGeometryCollection(Geometry geom)
        {
            UniqueCoordinateArrayFilter filter = new UniqueCoordinateArrayFilter();
            geom.Apply(filter);
            Coordinate[] coord = filter.Coordinates;

            Geometry[] geometries = new Geometry[coord.Length];
            for (int i = 0; i < coord.Length; i++)
            {
                Coordinate[] c = new Coordinate[] { coord[i] };
                CoordinateArraySequence cs = new CoordinateArraySequence(c);
                geometries[i] = geomFactory.CreatePoint(cs);
            }

            return geomFactory.CreateGeometryCollection(geometries);
        }


        /**
         * Returns a {@link Geometry} that represents the concave hull of the input
         * geometry according to the threshold.
         * The returned geometry contains the minimal number of points needed to
         * represent the concave hull.
         *
         * @return if the concave hull contains 3 or more points, a {@link Polygon};
         * 2 points, a {@link LineString};
         * 1 point, a {@link Point};
         * 0 points, an empty {@link GeometryCollection}.
         */
        public Geometry getConcaveHull()
        {

            if (this.geometries.NumGeometries == 0)
            {
                return this.geomFactory.CreateGeometryCollection(null);
            }
            if (this.geometries.NumGeometries == 1)
            {
                return this.geometries.GetGeometryN(0);
            }
            if (this.geometries.NumGeometries == 2)
            {
                return this.geomFactory.CreateLineString(this.geometries.Coordinates);
            }

            return concaveHull();
        }


        /**
         * Create the concave hull.
         * 
         * @return
         * 		the concave hull
         */
        private Geometry concaveHull()
        {

            // triangulation: create a DelaunayTriangulationBuilder object	
            ConformingDelaunayTriangulationBuilder cdtb = new ConformingDelaunayTriangulationBuilder();

            // add geometry collection
            cdtb.SetSites(this.geometries);

            QuadEdgeSubdivision qes = cdtb.GetSubdivision();

            IList<QuadEdge> quadEdges = qes.GetEdges();
            IList<QuadEdgeTriangle> qeTriangles = QuadEdgeTriangle.CreateOn(qes);
            IEnumerable<NTSVertex> qeVertices = qes.GetVertices(false);

            int iV = 0;
            foreach (NTSVertex v in qeVertices)
            {
                this.coordinates.Add(v.Coordinate, iV);
                this.vertices.Add(iV, new Vertex(iV, v.Coordinate));
                iV++;
            }

            // border
            List<QuadEdge> qeFrameBorder = new List<QuadEdge>();
            List<QuadEdge> qeFrame = new List<QuadEdge>();
            List<QuadEdge> qeBorder = new List<QuadEdge>();

            foreach (QuadEdge qe in quadEdges)
            {
                if (qes.IsFrameBorderEdge(qe))
                {
                    qeFrameBorder.Add(qe);
                }
                if (qes.IsFrameEdge(qe))
                {
                    qeFrame.Add(qe);
                }
            }

            // border
            for (int j = 0; j < qeFrameBorder.Count; j++)
            {
                QuadEdge q = qeFrameBorder[j];
                if (!qeFrame.Contains(q))
                {
                    qeBorder.Add(q);
                }
            }

            // deletion of exterior edges
            foreach (QuadEdge qe in qeFrame)
            {
                qes.Delete(qe);
            }

            Dictionary<QuadEdge, double> qeDistances = new Dictionary<QuadEdge, double>();
            foreach (QuadEdge qe in quadEdges)
            {
                qeDistances[qe] = qe.ToLineSegment().Length;
            }

            //DoubleComparator dc = new DoubleComparator(qeDistances);
            //TreeMap<QuadEdge, Double> qeSorted = new TreeMap<QuadEdge, Double>(dc);
            //qeSorted.putAll(qeDistances);

            // edges creation
            int i = 0;
            foreach (QuadEdge qe in qeDistances.OrderBy(o => o.Value).Select(o => o.Key))
            {
                LineSegment s = qe.ToLineSegment();
                s.Normalize();

                int idS = this.coordinates[s.P0];
                int idD = this.coordinates[s.P1];
                Vertex oV = this.vertices[idS];
                Vertex eV = this.vertices[idD];

                Edge edge;
                if (qeBorder.Contains(qe))
                {
                    oV.setBorder(true);
                    eV.setBorder(true);
                    edge = new Edge(i, s, oV, eV, true);
                    if (s.Length < this.threshold)
                    {
                        this.shortLengths.Add(i, edge);
                    }
                    else
                    {
                        this.lengths.Add(i, edge);
                    }
                }
                else
                {
                    edge = new Edge(i, s, oV, eV, false);
                }
                this.edges.Add(i, edge);
                this.segments.Add(s, i);
                i++;
            }

            // hm of linesegment and hm of edges // with id as key
            // hm of triangles using hm of ls and connection with hm of edges

            i = 0;
            foreach (QuadEdgeTriangle qet in qeTriangles)
            {
                LineSegment sA = qet.GetEdge(0).ToLineSegment();
                LineSegment sB = qet.GetEdge(1).ToLineSegment();
                LineSegment sC = qet.GetEdge(2).ToLineSegment();
                sA.Normalize();
                sB.Normalize();
                sC.Normalize();

                Edge edgeA = this.edges[this.segments[sA]];
                Edge edgeB = this.edges[this.segments[sB]];
                Edge edgeC = this.edges[this.segments[sC]];

                Triangle triangle = new Triangle(i, qet.IsBorder() ? true : false);
                triangle.addEdge(edgeA);
                triangle.addEdge(edgeB);
                triangle.addEdge(edgeC);

                edgeA.addTriangle(triangle);
                edgeB.addTriangle(triangle);
                edgeC.addTriangle(triangle);

                this.triangles.Add(i, triangle);
                i++;
            }

            // add triangle neighbourood
            foreach (Edge edge in this.edges.Values)
            {
                if (edge.getTriangles().Count != 1)
                {
                    Triangle tA = edge.getTriangles()[0];
                    Triangle tB = edge.getTriangles()[1];
                    tA.addNeighbour(tB);
                    tB.addNeighbour(tA);
                }
            }


            // concave hull algorithm
            int index = 0;
            while (index != -1)
            {
                index = -1;

                Edge e = null;

                // find the max length (smallest id so first entry)
                int si = this.lengths.Count;

                if (si != 0)
                {
                    KeyValuePair<int, Edge> entry = this.lengths.First();
                    int ind = entry.Key;
                    if (entry.Value.getGeometry().Length > this.threshold)
                    {
                        index = ind;
                        e = entry.Value;
                    }
                }

                if (index != -1)
                {
                    Triangle triangle = e.getTriangles()[0];
                    List<Triangle> neighbours = triangle.getNeighbours();
                    // irregular triangle test
                    if (neighbours.Count == 1)
                    {
                        this.shortLengths.Add(e.getId(), e);
                        this.lengths.Remove(e.getId());
                    }
                    else
                    {
                        Edge e0 = triangle.getEdges()[0];
                        Edge e1 = triangle.getEdges()[1];
                        // test if all the vertices are on the border
                        if (e0.getOV().isBorder() && e0.getEV().isBorder()
                                && e1.getOV().isBorder() && e1.getEV().isBorder())
                        {
                            this.shortLengths.Add(e.getId(), e);
                            this.lengths.Remove(e.getId());
                        }
                        else
                        {
                            // management of triangles
                            Triangle tA = neighbours[0];
                            Triangle tB = neighbours[1];
                            tA.setBorder(true); // FIXME not necessarily useful
                            tB.setBorder(true); // FIXME not necessarily useful
                            this.triangles.Remove(triangle.getId());
                            tA.removeNeighbour(triangle);
                            tB.removeNeighbour(triangle);

                            // new edges
                            List<Edge> ee = triangle.getEdges();
                            Edge eA = ee[0];
                            Edge eB = ee[1];
                            Edge eC = ee[2];

                            if (eA.isBorder())
                            {
                                this.edges.Remove(eA.getId());
                                eB.setBorder(true);
                                eB.getOV().setBorder(true);
                                eB.getEV().setBorder(true);
                                eC.setBorder(true);
                                eC.getOV().setBorder(true);
                                eC.getEV().setBorder(true);

                                // clean the relationships with the triangle
                                eB.removeTriangle(triangle);
                                eC.removeTriangle(triangle);

                                if (eB.getGeometry().Length< this.threshold)
                                {
                                    this.shortLengths.Add(eB.getId(), eB);
                                }
                                else
                                {
                                    this.lengths.Add(eB.getId(), eB);
                                }
                                if (eC.getGeometry().Length < this.threshold)
                                {
                                    this.shortLengths.Add(eC.getId(), eC);
                                }
                                else
                                {
                                    this.lengths.Add(eC.getId(), eC);
                                }
                                this.lengths.Remove(eA.getId());
                            }
                            else if (eB.isBorder())
                            {
                                this.edges.Remove(eB.getId());
                                eA.setBorder(true);
                                eA.getOV().setBorder(true);
                                eA.getEV().setBorder(true);
                                eC.setBorder(true);
                                eC.getOV().setBorder(true);
                                eC.getEV().setBorder(true);

                                // clean the relationships with the triangle
                                eA.removeTriangle(triangle);
                                eC.removeTriangle(triangle);

                                if (eA.getGeometry().Length < this.threshold)
                                {
                                    this.shortLengths.Add(eA.getId(), eA);
                                }
                                else
                                {
                                    this.lengths.Add(eA.getId(), eA);
                                }
                                if (eC.getGeometry().Length < this.threshold)
                                {
                                    this.shortLengths.Add(eC.getId(), eC);
                                }
                                else
                                {
                                    this.lengths.Add(eC.getId(), eC);
                                }
                                this.lengths.Remove(eB.getId());
                            }
                            else
                            {
                                this.edges.Remove(eC.getId());
                                eA.setBorder(true);
                                eA.getOV().setBorder(true);
                                eA.getEV().setBorder(true);
                                eB.setBorder(true);
                                eB.getOV().setBorder(true);
                                eB.getEV().setBorder(true);
                                // clean the relationships with the triangle
                                eA.removeTriangle(triangle);
                                eB.removeTriangle(triangle);

                                if (eA.getGeometry().Length < this.threshold)
                                {
                                    this.shortLengths.Add(eA.getId(), eA);
                                }
                                else
                                {
                                    this.lengths.Add(eA.getId(), eA);
                                }
                                if (eB.getGeometry().Length < this.threshold)
                                {
                                    this.shortLengths.Add(eB.getId(), eB);
                                }
                                else
                                {
                                    this.lengths.Add(eB.getId(), eB);
                                }
                                this.lengths.Remove(eC.getId());
                            }
                        }
                    }
                }
            }

            // concave hull creation
            List<LineString> edges = new List<LineString>();
            foreach (Edge e in this.lengths.Values)
            {
                LineString l = e.getGeometry().ToGeometry(this.geomFactory);
                edges.Add(l);
            }

            foreach (Edge e in this.shortLengths.Values)
            {
                LineString l = e.getGeometry().ToGeometry(this.geomFactory);
                edges.Add(l);
            }

            // merge
            LineMerger lineMerger = new LineMerger();
            lineMerger.Add(edges);
            LineString merge = lineMerger.GetMergedLineStrings().First() as LineString;

            if (merge.IsRing)
            {
                var ring = this.geomFactory.CreateLinearRing(merge.Coordinates);
                return this.geomFactory.CreatePolygon(ring);
            }

            return merge;
        }
    }
}
