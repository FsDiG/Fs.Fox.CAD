using System;
using System.Linq;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPPolygonPartitioner
    {
        public static List<Polyline> PolygonPartition(Polyline polyline)
        {
            // 手动闭合Polyline
            var poly = polyline.Clone() as Polyline;
            if (!polyline.Closed)
            {
                if (polyline.StartPoint.DistanceTo(polyline.EndPoint) <= 1)
                {
                    poly.RemoveVertexAt(poly.NumberOfVertices - 1);
                    poly.AddVertexAt(polyline.NumberOfVertices - 1, polyline.StartPoint.ToPoint2D(), 0, 0, 0);
                }
                else
                {
                    poly.AddVertexAt(polyline.NumberOfVertices, polyline.StartPoint.ToPoint2D(), 0, 0, 0);
                }
                poly.Closed = true;
            }
            else
            {
                if (!polyline.StartPoint.IsEqualTo(polyline.GetPoint3dAt(polyline.NumberOfVertices - 1)))
                {
                    poly.AddVertexAt(polyline.NumberOfVertices, polyline.StartPoint.ToPoint2D(), 0, 0, 0);
                }
            }

            // 剔除无用边缘点（相邻三点是否共线）
            poly = RemoveCollinearPoints(poly);

            // 取出多段线所有边缘
            var poly_Edge = new PolylineSegmentCollection(poly);

            // 定义顶点的凹凸性(目前仅适用于2D点)：
            // 凸点(点的两条边构成的角度，在多边形内部的有90°)  - 记录为 -1
            // 凹点(点的两条边构成的角度，在多边形内部的有270°) - 记录为 1
            var Vertices = GetVerticesProperty(poly);

            // 确定所有点的属性（凸点、凹点，新增内点）
            var points = new List<Tuple<Point2d, int>>();
            foreach (var item in Vertices)
            {
                if (item.Key.Item2 == -1)
                {
                    var intersect = IntersectWith(poly_Edge, Vertices, item.Key.Item1, item.Value);
                    if (intersect.Item3 == 0)
                    {
                        points.Add(Tuple.Create(intersect.Item2, 0));
                    }
                }
                points.Add(Tuple.Create(item.Key.Item1, item.Key.Item2));
            }
            return Points2Rectangles(points);
        }

        // 移除polyline边缘线上的无用顶点
        private static Polyline RemoveCollinearPoints(Polyline poly)
        {
            var index_remove = new List<int>();
            for (int i = 0; i < (poly.NumberOfVertices - 1); i++)
            {
                var pt = poly.GetPoint2dAt(i);
                var pt_prev = (i == 0) ? (poly.GetPoint2dAt(poly.NumberOfVertices - 2)) : (poly.GetPoint2dAt(i - 1));
                var pt_next = poly.GetPoint2dAt(i + 1);
                if (Math.Sin(pt.GetVectorTo(pt_next).GetAngleTo(pt_prev.GetVectorTo(pt))) < 1e-4)
                {
                    index_remove.Add(i);
                }
            }
            index_remove.Reverse();
            foreach (int i in index_remove)
            {
                poly.RemoveVertexAt(i);
            }
            return poly;
        }

        // 判断所有原顶点的属性（凸点/凹点）
        private static Dictionary<Tuple<Point2d, int>, Vector2d> GetVerticesProperty(Polyline polyline)
        {
            var points = new Dictionary<Tuple<Point2d, int>, Vector2d>();
            var convexVertex = -1;
            var concaveVertex = 1;
            for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                var pt = polyline.GetPoint2dAt(i);
                var pt_prev = (i == 0) ? (polyline.GetPoint2dAt(polyline.NumberOfVertices - 2)) : (polyline.GetPoint2dAt(i - 1));
                var pt_next = polyline.GetPoint2dAt(i + 1);

                // 逆时针取点，多边形区域始终在边缘的左侧
                var vector_prev = pt_prev.GetVectorTo(pt);
                var vector_next = pt.GetVectorTo(pt_next);

                // 排除计算精度的影响
                var temp_sin = (vector_prev.X * vector_next.Y - vector_prev.Y * vector_next.X) / (vector_prev.Length * vector_next.Length);
                var temp_cos = vector_prev.DotProduct(vector_next) / (vector_prev.Length * vector_next.Length);
                if ((Math.Abs(temp_sin) > 1) && ((Math.Abs(temp_sin) - 1) < 1e-10)) temp_sin = (temp_sin > 0) ? 1 : -1;
                if ((Math.Abs(temp_cos) > 1) && ((Math.Abs(temp_cos) - 1) < 1e-10)) temp_cos = (temp_cos > 0) ? 1 : -1;
                var rho = Math.Asin(temp_sin);
                var theta = (rho < 0) ? Math.Acos(temp_cos) * (-1) : Math.Acos(temp_cos);

                // 确定每一个凹点的延伸方向（横向延伸）
                var vectorX_prev = vector_prev.GetAngleTo(new Vector2d(1.0, 0.0));
                var vectorY_next = vector_next.GetAngleTo(new Vector2d(1.0, 0.0));
                var vector_extend = (Math.Sin(vectorX_prev) < Math.Sin(vectorY_next)) ? vector_prev / vector_prev.Length : (-vector_next / vector_next.Length);

                // 排除绘制错误的造成的凹点
                var misPlot = ((pt.GetDistanceTo(pt_prev) <= 1) || (pt.GetDistanceTo(pt_next) <= 1)) ? -1 : 1;
                var pt_property = (ThCADCoreNTSDbExtension.IsCCW(polyline) && theta < 0) || (!ThCADCoreNTSDbExtension.IsCCW(polyline) && theta > 0) ?
                    (Tuple.Create(pt, convexVertex * misPlot, vector_extend * 2000)) :
                    (Tuple.Create(pt, concaveVertex, vector_next));
                points.Add(Tuple.Create(pt_property.Item1, pt_property.Item2), pt_property.Item3);
            }
            return points;
        }

        // 求解所有凹点横向延伸后的交点（新增内点）
        private static Tuple<PolylineSegment, Point2d, int> IntersectWith(PolylineSegmentCollection polylineSegments, Dictionary<Tuple<Point2d, int>, Vector2d> Vertices, Point2d pt, Vector2d vector)
        {
            var intersectPoint = new Point2d();
            var intersectLine = new PolylineSegment(pt, (pt + vector));
            var intersectpt_property = 0;

            // 找到凹点延伸线上所有与之相交的交点
            double dist = 2000.0;
            var linesegment = new LineSegment2d(pt, vector);
            var intersectPts = new List<Tuple<PolylineSegment, Point2d>>();
            polylineSegments.ForEach(o =>
            {
                var linesegmentLine = new Line(pt.ToPoint3d(), (pt + vector).ToPoint3d());
                var pts = linesegment.IntersectWith(o.ToLineSegment());
                if (pts != null)
                {
                    for (int j = 0; j < pts.Length; j++)
                    {
                        intersectPts.Add(Tuple.Create(o, pts[j]));
                    }
                }
            });

            // 找到使凹点延伸长度最近的线
            if (intersectPts == null)
            {
                throw new NotSupportedException();
            }
            else
            {
                for (int i = 0; i < intersectPts.Count; i++)
                {
                    if (pt.GetDistanceTo(intersectPts[i].Item2) <= dist &&
                        pt.GetDistanceTo(intersectPts[i].Item2) != 0)
                    {
                        dist = pt.GetDistanceTo(intersectPts[i].Item2);
                        intersectPoint = intersectPts[i].Item2;
                        intersectLine = intersectPts[i].Item1;
                    }
                }
            }

            // 如果交点附近有另一个凹点，则与另一个凹点相连
            Vertices.ForEach(o =>
            {
                var pline = (new PolylineSegmentCollection(new PolylineSegment(pt, intersectPoint))).ToPolyline();
                if ((o.Key.Item1.GetDistanceTo(intersectPoint) <= 1) ||
                ((pline.Distance(o.Key.Item1.ToPoint3d()) <= 1) && (pline.Distance(o.Key.Item1.ToPoint3d()) > 0)))
                {
                    intersectPoint = o.Key.Item1;
                    intersectpt_property = -1;
                }
            });
            return Tuple.Create(intersectLine, intersectPoint, intersectpt_property);
        }

        // 用得到的所有点构建矩形
        private static List<Polyline> Points2Rectangles(List<Tuple<Point2d, int>> pts)
        {
            var polylines = new List<Polyline>();
            while (pts.Count >= 4)
            {
                pts = pts.OrderBy(o => Math.Floor(o.Item1.Y)).ThenBy(o => Math.Floor(o.Item1.X)).ToList();
                var pt_ld = pts.First();
                // 排除误绘点
                if (pts.Find(o => (Math.Abs(o.Item1.X - pt_ld.Item1.X) < 5.0) && (o.Item1.Y - pt_ld.Item1.Y) >= 1.0) == null ||
                    pts.Find(o => (Math.Abs(o.Item1.Y - pt_ld.Item1.Y) < 5.0) && (o.Item1.X - pt_ld.Item1.X) >= 0.0) == null)
                {
                    pts.Remove(pt_ld);
                    continue;
                }

                var pt_rd_list = pts.FindAll(o => (Math.Abs(o.Item1.Y - pt_ld.Item1.Y) < 5.0) && ((o.Item1.X - pt_ld.Item1.X) != 0.0));
                pt_rd_list = pt_rd_list.OrderBy(o => Math.Floor(o.Item1.Y)).ThenBy(o => Math.Floor(o.Item1.X)).ToList();
                while (pts.Find(o => (Math.Abs(o.Item1.X - pt_rd_list.First().Item1.X) < 5.0) && (o.Item1.Y - pt_rd_list.First().Item1.Y > 1.0)) == null)
                {
                    pts.Remove(pt_rd_list.First());
                    pt_rd_list.Remove(pt_rd_list.First());
                }
                var pt_rd = pt_rd_list.First();

                var pt_ru_list = pts.FindAll(o => (Math.Abs(o.Item1.X - pt_rd.Item1.X) < 5.0) && ((o.Item1.Y - pt_rd.Item1.Y) != 0.0));
                pt_ru_list = pt_ru_list.OrderBy(o => Math.Floor(o.Item1.Y)).ThenBy(o => Math.Floor(o.Item1.X)).ToList();
                while (pts.Find(o => (Math.Abs(o.Item1.Y - pt_ru_list.First().Item1.Y) < 5.0) && (Math.Abs(o.Item1.X - pt_ld.Item1.X) < 5.0)) == null)
                {
                    pts.Remove(pt_ru_list.First());
                    pt_ru_list.Remove(pt_ru_list.First());
                }
                var pt_ru = pt_ru_list.First();

                var pt_lu = pts.Find(o => (Math.Abs(o.Item1.Y - pt_ru.Item1.Y) < 5.0) && (Math.Abs(o.Item1.X - pt_ld.Item1.X) < 5.0));

                // 构建polygon分割后的矩形
                Polyline pline = new Polyline()
                {
                    Closed = true
                };
                pline.AddVertexAt(0, pt_ld.Item1, 0.0, 0.0, 0.0);
                pline.AddVertexAt(1, pt_rd.Item1, 0.0, 0.0, 0.0);
                pline.AddVertexAt(2, pt_ru.Item1, 0.0, 0.0, 0.0);
                pline.AddVertexAt(3, pt_lu.Item1, 0.0, 0.0, 0.0);
                polylines.Add(pline);

                // 移除已经使用过的凸点和凹点
                pts.Remove(pt_ld);
                pts.Remove(pt_rd);
                if (pt_ru.Item2 != 0) pts.Remove(pt_ru);
                if (pt_lu.Item2 != 0) pts.Remove(pt_lu);
            }
            return polylines;
        }
    }
}
