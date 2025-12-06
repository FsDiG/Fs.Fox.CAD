using System;
using DotNetARX;
using ThCADExtension;
using Dreambuild.AutoCAD;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Utilities;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSDbExtension
    {
        public static Polyline ToDbPolyline(this LineString lineString)
        {
            var pline = new Polyline()
            {
                Closed = lineString.IsClosed,
            };
            pline.CreatePolyline(lineString.Coordinates.ToAcGePoint3ds());
            return pline;
        }

        public static Line ToDbline(this LineString lineString)
        {
            var line = new Line
            {
                StartPoint = lineString.StartPoint.ToAcGePoint3d(),
                EndPoint = lineString.EndPoint.ToAcGePoint3d()
            };
            return line;
        }

        public static List<Polyline> ToDbPolylines(this Polygon polygon)
        {
            var plines = new List<Polyline>();
            plines.Add(polygon.Shell.ToDbPolyline());
            foreach (LinearRing hole in polygon.Holes)
            {
                plines.Add(hole.ToDbPolyline());
            }
            return plines;
        }

        public static MPolygon ToDbMPolygon(this Polygon polygon)
        {
            List<Curve> holes = new List<Curve>();
            var shell = polygon.Shell.ToDbPolyline();
            polygon.Holes.ForEach(o => holes.Add(o.ToDbPolyline()));
            return ThMPolygonTool.CreateMPolygon(shell, holes);
        }

        public static Entity ToDbEntity(this Polygon polygon)
        {
            if (polygon.NumInteriorRings > 0)
            {
                return polygon.ToDbMPolygon();
            }
            else
            {
                return polygon.Shell.ToDbPolyline();
            }
        }

        public static List<Polyline> ToDbPolylines(this MultiLineString geometries)
        {
            var plines = new List<Polyline>();
            foreach (var geometry in geometries.Geometries)
            {
                if (geometry is LineString lineString)
                {
                    plines.Add(lineString.ToDbPolyline());
                }
                else if (geometry is LinearRing linearRing)
                {
                    plines.Add(linearRing.ToDbPolyline());
                }
                else if (geometry is Polygon polygon)
                {
                    plines.AddRange(polygon.ToDbPolylines());
                }
                else if (geometry is MultiLineString multiLineString)
                {
                    plines.AddRange(multiLineString.ToDbPolylines());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return plines;
        }

        public static Line ToDbLine(this LineSegment segment)
        {
            return new Line()
            {
                StartPoint = segment.P0.ToAcGePoint3d(),
                EndPoint = segment.P1.ToAcGePoint3d()
            };
        }

        public static List<Region> ToDbRegions(this MultiPolygon mPolygon)
        {
            var regions = new List<Region>();
            foreach (Polygon polygon in mPolygon.Geometries)
            {
                regions.Add(polygon.ToDbRegion());
            }
            return regions;
        }

        public static List<Polyline> ToDbPolylines(this MultiPolygon mPolygon)
        {
            var plines = new List<Polyline>();
            foreach (Polygon polygon in mPolygon.Geometries)
            {
                plines.Add(polygon.Shell.ToDbPolyline());
            }
            return plines;
        }

        public static List<DBObject> ToDbObjects(this Geometry geometry, bool keepHoles = false)
        {
            var objs = new List<DBObject>();
            if (geometry.IsEmpty)
            {
                return objs;
            }
            if (geometry is LineString lineString)
            {
                objs.Add(lineString.ToDbPolyline());
            }
            else if (geometry is LinearRing linearRing)
            {
                objs.Add(linearRing.ToDbPolyline());
            }
            else if (geometry is Polygon polygon)
            {
                if (keepHoles)
                {
                    objs.Add(polygon.ToDbMPolygon());
                }
                else
                {
                    objs.AddRange(polygon.ToDbPolylines());
                }
            }
            else if (geometry is MultiLineString lineStrings)
            {
                lineStrings.Geometries.ForEach(g => objs.AddRange(g.ToDbObjects(keepHoles)));
            }
            else if (geometry is MultiPolygon polygons)
            {
                polygons.Geometries.ForEach(g => objs.AddRange(g.ToDbObjects(keepHoles)));
            }
            else if (geometry is GeometryCollection geometries)
            {
                geometries.Geometries.ForEach(g => objs.AddRange(g.ToDbObjects(keepHoles)));
            }
            else if (geometry is Point point) 
            {
                objs.Add(point.ToDbPoint());
            }
            else
            {
                throw new NotSupportedException();
            }
            return objs;
        }

        public static LineString ToNTSLineString(this Polyline poly)
        {
            var points = new List<Coordinate>();
            var arcLength = ThCADCoreNTSService.Instance.ArcTessellationLength;
            var polyLine = poly.HasBulges ? poly.TessellatePolylineWithArc(arcLength) : poly;
            for (int i = 0; i < polyLine.NumberOfVertices; i++)
            {
                points.Add(polyLine.GetPoint3dAt(i).ToNTSCoordinate());
            }

            // 对于处于“闭合”状态的多段线，要保证其首尾点一致
            if (polyLine.Closed && !points[0].Equals(points[points.Count - 1]))
            {
                points.Add(points[0]);
            }

            if (points[0].Equals(points[points.Count - 1]))
            {
                // 三个点，其中起点和终点重合
                // 多段线退化成一根线段
                if (points.Count == 3)
                {
                    return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
                }

                // 二个点，其中起点和终点重合
                // 多段线退化成一个点
                if (points.Count == 2)
                {
                    return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString();
                }

                // 一个点
                // 多段线退化成一个点
                if (points.Count == 1)
                {
                    return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString();
                }

                // 首尾端点一致的情况
                // LinearRings are the fundamental building block for Polygons.
                // LinearRings may not be degenerate; that is, a LinearRing must have at least 3 points.
                // Other non-degeneracy criteria are implied by the requirement that LinearRings be simple. 
                // For instance, not all the points may be collinear, and the ring may not self - intersect.
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateLinearRing(points.ToArray());
            }
            else
            {
                // 首尾端点不一致的情况
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
            }
        }

        public static LineString ToNTSLineString(this Polyline2d poly2d)
        {
            var poly = new Polyline();
            poly.ConvertFrom(poly2d, false);
            return poly.ToNTSLineString();
        }

        public static Polygon ToNTSPolygon(this Polyline polyLine)
        {
            var geometry = polyLine.ToNTSLineString();
            if (geometry is LinearRing ring)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon(ring);
            }
            else
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon();
            }
        }

        public static Polygon ToNTSPolygon(this MPolygon mPolygon)
        {
            Polyline shell = null;
            List<Polyline> holes = new List<Polyline>();
            for (int i = 0; i < mPolygon.NumMPolygonLoops; i++)
            {
                LoopDirection direction = mPolygon.GetLoopDirection(i);
                MPolygonLoop mPolygonLoop = mPolygon.GetMPolygonLoopAt(i);
                Polyline polyline = new Polyline()
                {
                    Closed = true
                };
                for (int j = 0; j < mPolygonLoop.Count; j++)
                {
                    var bulgeVertex = mPolygonLoop[j];
                    polyline.AddVertexAt(j, bulgeVertex.Vertex, bulgeVertex.Bulge, 0, 0);
                }
                if(LoopDirection.Exterior == direction)
                {
                    shell = polyline;
                }
                else if (LoopDirection.Interior == direction)
                {
                    holes.Add(polyline);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            if(shell == null && holes.Count == 1)
            {
                return holes[0].ToNTSPolygon();
            }
            else if(shell != null && holes.Count == 0)
            {
                return shell.ToNTSPolygon();
            }
            else if(shell != null && holes.Count > 0)
            {
                List<LinearRing> holeRings = new List<LinearRing>();
                holes.ForEach(o => 
                {
                    holeRings.Add(o.ToNTSLineString() as LinearRing);
                });
                LinearRing shellLinearRing = shell.ToNTSLineString() as LinearRing;
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon(shellLinearRing, holeRings.ToArray());
            }
            else
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon();
            }
        }

        public static Polygon ToNTSPolygon(this Circle circle)
        {
            var length = ThCADCoreNTSService.Instance.ArcTessellationLength;
            var circum = 2 * Math.PI * circle.Radius;
            int num = (int)Math.Ceiling(circum / length);
            if (num >= 3)
            {
                return circle.ToNTSPolygon(num);
            }
            else
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon();
            }
        }

        public static Polygon ToNTSPolygon(this Circle circle, int numPoints)
        {
            // 获取圆的外接矩形
            var shapeFactory = new GeometricShapeFactory(ThCADCoreNTSService.Instance.GeometryFactory)
            {
                NumPoints = numPoints,
                Size = 2 * circle.Radius,
                Centre = circle.Center.ToNTSCoordinate(),
            };
            return shapeFactory.CreateCircle();
        }

        public static LineString ToNTSLineString(this Arc arc)
        {
            var arcLength = ThCADCoreNTSService.Instance.ArcTessellationLength;
            return arc.TessellateArcWithArc(arcLength).ToNTSLineString();
        }

        public static LineString ToNTSLineString(this Line line)
        {
            var points = new List<Coordinate>
            {
                line.StartPoint.ToNTSCoordinate(),
                line.EndPoint.ToNTSCoordinate()
            };
            return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
        }

        public static LineSegment ToNTSLineSegment(this Line line)
        {
            return new LineSegment(line.StartPoint.ToNTSCoordinate(), line.EndPoint.ToNTSCoordinate());
        }

        public static Point ToNTSPoint(this DBPoint point)
        {
            return ThCADCoreNTSService.Instance.GeometryFactory.CreatePoint(point.Position.ToNTSCoordinate());
        }

        public static DBPoint ToDbPoint(this Point point)
        {
            return new DBPoint(point.ToAcGePoint3d());
        }

        public static bool IsCCW(this Polyline pline)
        {
            return Orientation.IsCCW(pline.ToNTSLineString().Coordinates);
        }
    }
}
