using System;
using DotNetARX;
using System.Linq;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    /// <summary>
    /// EarCut算法
    /// https://github.com/mapbox/earcut.hpp
    /// </summary>
    public class ThCADCoreNTSEarCutNode
    {
        public int i;
        public int z;
        public double x, y;
        public bool steiner;
        public ThCADCoreNTSEarCutNode prev, next;
        public ThCADCoreNTSEarCutNode prevZ, nextZ;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_i"></param>
        /// <param name="_pt"></param>
        public ThCADCoreNTSEarCutNode(int _i, double _x, double _y)
        {
            i = _i;
            x = _x;
            y = _y;
        }
    }

    public class ThCADCoreNTSEarCutTriangulationBuilder
    {
        bool hashing;
        double minX, maxX;
        double minY, maxY;
        double inv_size = 0;
        int vertices = 0;
        List<int> indices;

        public ThCADCoreNTSEarCutTriangulationBuilder()
        {
            indices = new List<int>();
        }

        public DBObjectCollection EarCut(Polyline poly, DBObjectCollection holes)
        {
            var pointsCollection = new List<List<Point2d>>();
            pointsCollection.Add(Vertices(poly));
            pointsCollection.AddRange(Vertices(holes));
            Earcut(pointsCollection);
            var objs = new DBObjectCollection();
            var points = pointsCollection.SelectMany(o => o).ToList();
            for (int i = 0; i < indices.Count; i += 3)
            {
                var vertices = new Point2d[]
                {
                    points[indices[i]],
                    points[indices[i + 1]],
                    points[indices[i + 2]],
                };
                var triangle = new Polyline()
                {
                    Closed = true,
                };
                triangle.CreatePolyline(vertices);
                objs.Add(triangle);
            }
            return objs;
        }

        private List<Point2d> Vertices(Polyline poly)
        {
            var points = new List<Point2d>();
            poly.Vertices().Cast<Point3d>().ForEach(o => points.Add(o.ToPoint2D()));
            return points;
        }

        private List<List<Point2d>> Vertices(DBObjectCollection holes)
        {
            var points = new List<List<Point2d>>();
            holes.Cast<Polyline>().ForEach(o => points.Add(Vertices(o)));
            return points;
        }

        private void RemoveNode(ThCADCoreNTSEarCutNode p)
        {
            p.next.prev = p.prev;
            p.prev.next = p.next;

            if (p.prevZ != null) p.prevZ.nextZ = p.nextZ;
            if (p.nextZ != null) p.nextZ.prevZ = p.prevZ;
        }

        private ThCADCoreNTSEarCutNode InsertNode(int i, Point2d pt, ThCADCoreNTSEarCutNode last)
        {
            var p = new ThCADCoreNTSEarCutNode(i, pt.X, pt.Y);

            if (last == null)
            {
                p.prev = p;
                p.next = p;
            }
            else
            {
                p.next = last.next;
                p.prev = last;
                last.next.prev = p;
                last.next = p;
            }
            return p;
        }

        private ThCADCoreNTSEarCutNode SplitPolygon(ThCADCoreNTSEarCutNode a, ThCADCoreNTSEarCutNode b)
        {
            var a2 = new ThCADCoreNTSEarCutNode(a.i, a.x, a.y);
            var b2 = new ThCADCoreNTSEarCutNode(b.i, b.x, b.y);
            var an = a.next;
            var bp = b.prev;

            a.next = b;
            b.prev = a;

            a2.next = an;
            an.prev = a2;

            b2.next = a2;
            a2.prev = b2;

            bp.next = b2;
            b2.prev = bp;

            return b2;
        }

        private bool MiddleInside(ThCADCoreNTSEarCutNode a, ThCADCoreNTSEarCutNode b)
        {
            var p = a;
            bool inside = false;
            double px = (a.x + b.x) / 2;
            double py = (a.y + b.y) / 2;
            do
            {
                if (((p.y > py) != (p.next.y > py))
                    && p.next.y != p.y
                    && (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x))
                {
                    inside = !inside;
                }
                p = p.next;
            } while (p != a);

            return inside;
        }

        private bool LocallyInside(ThCADCoreNTSEarCutNode a, ThCADCoreNTSEarCutNode b)
        {
            return Area(a.prev, a, a.next) < 0 ?
                Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 :
                Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
        }

        private bool IntersectsPolygon(ThCADCoreNTSEarCutNode a, ThCADCoreNTSEarCutNode b)
        {
            ThCADCoreNTSEarCutNode p = a;
            do
            {
                if (p.i != a.i
                    && p.next.i != a.i
                    && p.i != b.i
                    && p.next.i != b.i
                    && Intersects(p, p.next, a, b))
                {
                    return true;
                }
                p = p.next;
            } while (p != a);

            return false;
        }

        private int Sign(double val)
        {
            return Convert.ToInt32(0.0 < val) - Convert.ToInt32(val < 0.0);
        }

        private bool OnSegment(ThCADCoreNTSEarCutNode p, ThCADCoreNTSEarCutNode q, ThCADCoreNTSEarCutNode r)
        {
            return q.x <= Math.Max(p.x, r.x)
                && q.x >= Math.Max(p.x, r.x)
                && q.y <= Math.Max(p.y, r.y)
                && q.y >= Math.Max(p.y, r.y);
        }

        private bool Intersects(ThCADCoreNTSEarCutNode p1, ThCADCoreNTSEarCutNode q1, ThCADCoreNTSEarCutNode p2, ThCADCoreNTSEarCutNode q2)
        {
            int o1 = Sign(Area(p1, q1, p2));
            int o2 = Sign(Area(p1, q1, q2));
            int o3 = Sign(Area(p2, q2, p1));
            int o4 = Sign(Area(p2, q2, q1));

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == 0 && OnSegment(p1, p2, q1)) return true; // p1, q1 and p2 are collinear and p2 lies on p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true; // p1, q1 and q2 are collinear and q2 lies on p1q1
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true; // p2, q2 and p1 are collinear and p1 lies on p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true; // p2, q2 and q1 are collinear and q1 lies on p2q2

            return false;
        }

        private bool Equals(ThCADCoreNTSEarCutNode p1, ThCADCoreNTSEarCutNode p2)
        {
            var pt1 = new Point2d(p1.x, p1.y);
            var pt2 = new Point2d(p2.x, p2.y);
            return pt1.IsEqualTo(pt2);
        }

        private double Area(ThCADCoreNTSEarCutNode p, ThCADCoreNTSEarCutNode q, ThCADCoreNTSEarCutNode r)
        {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }

        private bool IsValidDiagonal(ThCADCoreNTSEarCutNode a, ThCADCoreNTSEarCutNode b)
        {
            return a.next.i != b.i
                && a.prev.i != b.i
                && !IntersectsPolygon(a, b)
                && (
                    (LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b) && (Area(a.prev, a, b.prev) != 0.0 || Area(a, b.prev, b) != 0.0))
                    || (Equals(a, b) && Area(a.prev, a, a.next) > 0 && Area(b.prev, b, b.next) > 0)
                   );
        }

        private bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
        {
            return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0
                && (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0
                && (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
        }

        private ThCADCoreNTSEarCutNode GetLeftmost(ThCADCoreNTSEarCutNode start)
        {
            ThCADCoreNTSEarCutNode p = start;
            ThCADCoreNTSEarCutNode leftmost = start;
            do
            {
                if (p.x < leftmost.x || (p.x == leftmost.x && p.y < leftmost.y))
                {
                    leftmost = p;
                }
                p = p.next;
            } while (p != start);

            return leftmost;
        }

        private int ZOrder(double x_, double y_)
        {
            int x = (int)(32767.0 * (x_ - minX) * inv_size);
            int y = (int)(32767.0 * (y_ - minY) * inv_size);

            x = (x | (x << 8)) & 0x00FF00FF;
            x = (x | (x << 4)) & 0x0F0F0F0F;
            x = (x | (x << 2)) & 0x33333333;
            x = (x | (x << 1)) & 0x55555555;

            y = (y | (y << 8)) & 0x00FF00FF;
            y = (y | (y << 4)) & 0x0F0F0F0F;
            y = (y | (y << 2)) & 0x33333333;
            y = (y | (y << 1)) & 0x55555555;

            return x | (y << 1);
        }

        private ThCADCoreNTSEarCutNode SortLinked(ThCADCoreNTSEarCutNode list)
        {
            ThCADCoreNTSEarCutNode p;
            ThCADCoreNTSEarCutNode q;
            ThCADCoreNTSEarCutNode e;
            ThCADCoreNTSEarCutNode tail;
            int i, numMerges, pSize, qSize;
            int inSize = 1;

            for (; ; )
            {
                p = list;
                list = null;
                tail = null;
                numMerges = 0;
                while (p != null)
                {
                    numMerges++;
                    q = p;
                    pSize = 0;
                    for (i = 0; i < inSize; i++)
                    {
                        pSize++;
                        q = q.nextZ;
                        if (q == null) break;
                    }

                    qSize = inSize;

                    while (pSize > 0 || (qSize > 0 && q != null))
                    {

                        if (pSize == 0)
                        {
                            e = q;
                            q = q.nextZ;
                            qSize--;
                        }
                        else if (qSize == 0 || q == null)
                        {
                            e = p;
                            p = p.nextZ;
                            pSize--;
                        }
                        else if (p.z <= q.z)
                        {
                            e = p;
                            p = p.nextZ;
                            pSize--;
                        }
                        else
                        {
                            e = q;
                            q = q.nextZ;
                            qSize--;
                        }

                        if (tail != null) tail.nextZ = e;
                        else list = e;

                        e.prevZ = tail;
                        tail = e;
                    }

                    p = q;
                }

                tail.nextZ = null;

                if (numMerges <= 1) return list;

                inSize *= 2;
            }
        }

        private void IndexCurve(ThCADCoreNTSEarCutNode start)
        {
            ThCADCoreNTSEarCutNode p = start;

            do
            {
                p.z = p.z != 0 ? p.z : ZOrder(p.x, p.y);
                p.prevZ = p.prev;
                p.nextZ = p.next;
                p = p.next;
            } while (p != start);

            p.prevZ.nextZ = null;
            p.prevZ = null;

            SortLinked(p);
        }

        private bool SectorContainsSector(ThCADCoreNTSEarCutNode m, ThCADCoreNTSEarCutNode p)
        {
            return Area(m.prev, m, p.prev) < 0 && Area(p.next, m, m.next) < 0;
        }

        private ThCADCoreNTSEarCutNode FindHoleBridge(ThCADCoreNTSEarCutNode hole, ThCADCoreNTSEarCutNode outerNode)
        {
            ThCADCoreNTSEarCutNode p = outerNode;
            double hx = hole.x;
            double hy = hole.y;
            double qx = double.MinValue;
            ThCADCoreNTSEarCutNode m = null;

            do
            {
                if (hy <= p.y && hy >= p.next.y && p.next.y != p.y)
                {
                    double x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);
                    if (x <= hx && x > qx)
                    {
                        qx = x;
                        if (x == hx)
                        {
                            if (hy == p.y) return p;
                            if (hy == p.next.y) return p.next;
                        }
                        m = p.x < p.next.x ? p : p.next;
                    }
                }
                p = p.next;
            } while (p != outerNode);

            if (m == null) return null;

            if (hx == qx) return m;

            ThCADCoreNTSEarCutNode stop = m;
            double tanMin = double.MinValue;
            double tanCur = 0;

            p = m;
            double mx = m.x;
            double my = m.y;

            do
            {
                if (hx >= p.x && p.x >= mx && hx != p.x &&
                    PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
                {

                    tanCur = Math.Abs(hy - p.y) / (hx - p.x);

                    if (LocallyInside(p, hole) &&
                        (tanCur < tanMin || (tanCur == tanMin && (p.x > m.x || SectorContainsSector(m, p)))))
                    {
                        m = p;
                        tanMin = tanCur;
                    }
                }

                p = p.next;
            } while (p != stop);

            return m;
        }

        private void EliminateHole(ThCADCoreNTSEarCutNode hole, ThCADCoreNTSEarCutNode outerNode)
        {
            outerNode = FindHoleBridge(hole, outerNode);
            if (outerNode != null)
            {
                ThCADCoreNTSEarCutNode b = SplitPolygon(outerNode, hole);
                FilterPoints(outerNode, outerNode.next);
                FilterPoints(b, b.next);
            }
        }

        private ThCADCoreNTSEarCutNode EliminateHoles(List<List<Point2d>> points, ThCADCoreNTSEarCutNode outerNode)
        {
            int len = points.Count;
            List<ThCADCoreNTSEarCutNode> queue = new List<ThCADCoreNTSEarCutNode>();
            for (int i = 1; i < len; i++)
            {
                ThCADCoreNTSEarCutNode list = LinkedList(points[i], false);
                if (list != null)
                {
                    if (list == list.next) list.steiner = true;
                    queue.Add(GetLeftmost(list));
                }
            }

            queue.OrderBy(o => o.x);

            for (int i = 0; i < queue.Count; i++)
            {
                EliminateHole(queue[i], outerNode);
                outerNode = FilterPoints(outerNode, outerNode.next);
            }

            return outerNode;
        }

        private void SplitEarcut(ThCADCoreNTSEarCutNode start)
        {
            ThCADCoreNTSEarCutNode a = start;
            do
            {
                ThCADCoreNTSEarCutNode b = a.next.next;
                while (b != a.prev)
                {
                    if (a.i != b.i && IsValidDiagonal(a, b))
                    {
                        ThCADCoreNTSEarCutNode c = SplitPolygon(a, b);
                        a = FilterPoints(a, a.next);
                        c = FilterPoints(c, c.next);
                        EarcutLinked(a);
                        EarcutLinked(c);
                        return;
                    }
                    b = b.next;
                }
                a = a.next;
            } while (a != start);
        }

        private ThCADCoreNTSEarCutNode CureLocalIntersections(ThCADCoreNTSEarCutNode start)
        {
            ThCADCoreNTSEarCutNode p = start;
            do
            {
                ThCADCoreNTSEarCutNode a = p.prev;
                ThCADCoreNTSEarCutNode b = p.next.next;

                if (!Equals(a, b) && Intersects(a, p, p.next, b) && LocallyInside(a, b) && LocallyInside(b, a))
                {
                    indices.Add(a.i);
                    indices.Add(p.i);
                    indices.Add(b.i);

                    RemoveNode(p);
                    RemoveNode(p.next);

                    p = start = b;
                }
                p = p.next;
            } while (p != start);

            return FilterPoints(p);
        }

        private bool IsEarHashed(ThCADCoreNTSEarCutNode ear)
        {
            ThCADCoreNTSEarCutNode a = ear.prev;
            ThCADCoreNTSEarCutNode b = ear;
            ThCADCoreNTSEarCutNode c = ear.next;

            if (Area(a, b, c) >= 0) return false;

            double minTX = Math.Min(a.x, Math.Min(b.x, c.x));
            double minTY = Math.Min(a.y, Math.Min(b.y, c.y));
            double maxTX = Math.Max(a.x, Math.Max(b.x, c.x));
            double maxTY = Math.Max(a.y, Math.Max(b.y, c.y));

            int minZ = ZOrder(minTX, minTY);
            int maxZ = ZOrder(maxTX, maxTY);

            ThCADCoreNTSEarCutNode p = ear.nextZ;

            while (p != null && p.z <= maxZ)
            {
                if (p != ear.prev && p != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0)
                    return false;
                p = p.nextZ;
            }

            p = ear.prevZ;

            while (p != null && p.z >= minZ)
            {
                if (p != ear.prev && p != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0)
                    return false;
                p = p.prevZ;
            }

            return true;
        }

        private bool IsEar(ThCADCoreNTSEarCutNode ear)
        {
            ThCADCoreNTSEarCutNode a = ear.prev;
            ThCADCoreNTSEarCutNode b = ear;
            ThCADCoreNTSEarCutNode c = ear.next;

            if (Area(a, b, c) >= 0) return false;

            ThCADCoreNTSEarCutNode p = ear.next.next;

            while (p != ear.prev)
            {
                if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0) return false;
                p = p.next;
            }

            return true;
        }

        private void EarcutLinked(ThCADCoreNTSEarCutNode ear, int pass = 0)
        {
            if (ear == null) return;

            if (pass == 0 && hashing) IndexCurve(ear);

            ThCADCoreNTSEarCutNode stop = ear;
            ThCADCoreNTSEarCutNode prev;
            ThCADCoreNTSEarCutNode next;

            int iterations = 0;

            while (ear.prev != ear.next)
            {
                iterations++;
                prev = ear.prev;
                next = ear.next;

                if (hashing ? IsEarHashed(ear) : IsEar(ear))
                {
                    indices.Add(prev.i);
                    indices.Add(ear.i);
                    indices.Add(next.i);

                    RemoveNode(ear);

                    ear = next.next;
                    stop = next.next;

                    continue;
                }

                ear = next;

                if (ear == stop)
                {
                    if (pass == 0) EarcutLinked(FilterPoints(ear), 1);

                    else if (pass == 1)
                    {
                        ear = CureLocalIntersections(FilterPoints(ear));
                        EarcutLinked(ear, 2);

                    }
                    else if (pass == 2) SplitEarcut(ear);

                    break;
                }
            }
        }

        private ThCADCoreNTSEarCutNode FilterPoints(ThCADCoreNTSEarCutNode start, ThCADCoreNTSEarCutNode end = null)
        {
            if (end == null) end = start;

            ThCADCoreNTSEarCutNode p = start;
            bool again;
            do
            {
                again = false;

                if (!p.steiner && (Equals(p, p.next) || Area(p.prev, p, p.next) == 0))
                {
                    RemoveNode(p);
                    p = end = p.prev;

                    if (p == p.next) break;
                    again = true;

                }
                else
                {
                    p = p.next;
                }
            } while (again || p != end);

            return end;
        }

        private ThCADCoreNTSEarCutNode LinkedList(List<Point2d> points, bool clockwise)
        {
            double sum = 0;
            int len = points.Count;
            int i, j;
            ThCADCoreNTSEarCutNode last = null;

            for (i = 0, j = len > 0 ? len - 1 : 0; i < len; j = i++)
            {
                var p1 = points[i];
                var p2 = points[j];
                double p20 = p2.X;
                double p10 = p1.X;
                double p11 = p1.Y;
                double p21 = p2.Y;
                sum += (p20 - p10) * (p11 + p21);
            }

            if (clockwise == (sum > 0))
            {
                for (i = 0; i < len; i++)
                {
                    last = InsertNode(vertices + i, points[i], last);
                }
            }
            else
            {
                for (i = len; i-- > 0;)
                {
                    last = InsertNode(vertices + i, points[i], last);
                }
            }

            if (last != null && Equals(last, last.next))
            {
                RemoveNode(last);
                last = last.next;
            }

            vertices += len;

            return last;
        }

        private void Earcut(List<List<Point2d>> points)
        {
            indices.Clear();
            vertices = 0;
            if (points.Count == 0) return;

            double x;
            double y;
            int threshold = 80;
            int len = 0;

            for (int i = 0; threshold >= 0 && i < points.Count; i++)
            {
                threshold -= points[i].Count;
                len += points[i].Count;
            }

            ThCADCoreNTSEarCutNode outerNode = LinkedList(points[0], true);
            if (outerNode == null || outerNode.prev == outerNode.next) return;

            if (points.Count > 1) outerNode = EliminateHoles(points, outerNode);

            hashing = threshold < 0;
            if (hashing)
            {
                ThCADCoreNTSEarCutNode p = outerNode.next;
                minX = maxX = outerNode.x;
                minY = maxY = outerNode.y;
                do
                {
                    x = p.x;
                    y = p.y;
                    minX = (double)Math.Min(minX, x);
                    minY = (double)Math.Min(minY, y);
                    maxX = (double)Math.Max(maxX, x);
                    maxY = (double)Math.Max(maxY, y);
                    p = p.next;
                }
                while (p != outerNode);

                inv_size = (double)Math.Max(maxX - minX, maxY - minY);
                inv_size = inv_size != .0 ? (1.0 / inv_size) : .0;
            }

            EarcutLinked(outerNode);
        }
    }
}
