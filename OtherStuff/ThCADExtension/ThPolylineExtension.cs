using System;
using DotNetARX;
using System.Linq;
using GeometryExtensions;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThPolylineExtension
    {
        /// <summary>
        /// 多段线顶点集合（不支持圆弧段）
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public static Point3dCollection Vertices(this Polyline pLine)
        {
            //https://keanw.com/2007/04/iterating_throu.html
            Point3dCollection vertices = new Point3dCollection();
            for (int i = 0; i < pLine.NumberOfVertices; i++)
            {
                // 暂时不考虑“圆弧”的情况
                vertices.Add(pLine.GetPoint3dAt(i));
            }

            // 对于处于“闭合”状态的多段线，要保证其首尾点一致
            if (pLine.Closed && !vertices[0].IsEqualTo(vertices[vertices.Count - 1]))
            {
                vertices.Add(vertices[0]);
            }

            return vertices;
        }

        /// <summary>
        /// 多段线顶点集合（支持圆弧段）
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static Point3dCollection VerticesEx(this Polyline poly, double length)
        {
            if (poly.HasBulges)
            {
                return poly.TessellatePolylineWithArc(length).Vertices();
            }
            else
            {
                return poly.Vertices();
            }
        }

        public static double[] Coordinates2D(this Polyline pLine)
        {
            var vertices = (Point3dList)pLine.Vertices();
            return vertices.Select(o => o.ToPoint2d().ToArray()).SelectMany(o => o).ToArray();
        }

        public static Polyline CreateRectangle(Point3d pt1, Point3d pt2, Point3d pt3, Point3d pt4)
        {
            var points = new Point3dCollection()
            {
                pt1,
                pt2,
                pt3,
                pt4
            };
            var pline = new Polyline()
            {
                Closed = true,
            };
            pline.CreatePolyline(points);
            return pline;
        }

        public static Polyline CreateTriangle(Point2d pt1, Point2d pt2, Point2d pt3)
        {
            var points = new Point2dCollection()
            {
                pt1,
                pt2,
                pt3,
            };
            var pline = new Polyline()
            {
                Closed = true,
            };
            pline.CreatePolyline(points);
            return pline;
        }

        public static Polyline ToRectangle(this Extents3d extents)
        {
            Point3d pt1 = extents.MinPoint;
            Point3d pt3 = extents.MaxPoint;
            Point3d pt2 = new Point3d(pt3.X, pt1.Y, pt1.Z);
            Point3d pt4 = new Point3d(pt1.X, pt3.Y, pt1.Z);
            return CreateRectangle(pt1,pt2,pt3,pt4);
        }

        /// <summary>
        /// 根据弦长分割Polyline
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static Polyline TessellatePolylineWithChord(this Polyline poly, double chord)
        {
            var segments = new PolylineSegmentCollection(poly);
            var tessellateSegments = new PolylineSegmentCollection();
            segments.ForEach(s => tessellateSegments.AddRange(s.TessellateSegmentWithChord(chord)));
            return tessellateSegments.ToPolyline();
        }

        /// <summary>
        /// 根据弦长分割Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static Polyline TessellateArcWithChord(this Arc arc, double chord)
        {
            var segment = new PolylineSegment(
                arc.StartPoint.ToPoint2D(),
                arc.EndPoint.ToPoint2D(),
                arc.BulgeFromCurve(false));
            return segment.TessellateSegmentWithChord(chord).ToPolyline();
        }

        /// <summary>
        /// 根据弦长分割PolylineSegment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="chord"></param>
        /// <returns></returns>
        private static PolylineSegmentCollection TessellateSegmentWithChord(this PolylineSegment segment, double chord)
        {
            var segments = new PolylineSegmentCollection();
            if (segment.IsLinear)
            {
                // 分割段是直线
                segments.Add(segment);
            }
            else
            {
                // 分割线是弧线
                var circulararc = new CircularArc2d(segment.StartPoint, segment.EndPoint, segment.Bulge, false);
                // 排除弦长大于弧直径的情况
                if (chord > 2 * circulararc.Radius)
                {
                    segments.Add(new PolylineSegment(segment.StartPoint, segment.EndPoint));
                }
                else
                {
                    var angle = 2 * Math.Asin(chord / (2 * circulararc.Radius));
                    segments.AddRange(segment.DoTessellate(angle));
                }
            }
            return segments;
        }

        /// <summary>
        /// 根据弧长分割Polyline
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Polyline TessellatePolylineWithArc(this Polyline poly, double length)
        {
            var segments = new PolylineSegmentCollection(poly);
            var tessellateSegments = new PolylineSegmentCollection();
            segments.ForEach(s => tessellateSegments.AddRange(s.TessellateSegmentWithArc(length)));
            return tessellateSegments.ToPolyline();
        }

        /// <summary>
        /// 根据弧长分割Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Polyline TessellateArcWithArc(this Arc arc, double length)
        {
            var segment = new PolylineSegment(
                arc.StartPoint.ToPoint2D(),
                arc.EndPoint.ToPoint2D(),
                arc.BulgeFromCurve(false));
            return segment.TessellateSegmentWithArc(length).ToPolyline();
        }

        /// <summary>
        /// 根据弧长分割PolylineSegment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static PolylineSegmentCollection TessellateSegmentWithArc(this PolylineSegment segment, double length)
        {
            var segments = new PolylineSegmentCollection();
            if (segment.IsLinear)
            {
                // 分割线是直线
                segments.Add(segment);
            }
            else
            {
                // 分割线是弧线
                var circulararc = new CircularArc2d(segment.StartPoint, segment.EndPoint, segment.Bulge, false);
                // 排除分割长度大于弧的周长的情况
                if (length >= 2 * Math.PI * circulararc.Radius)
                {
                    segments.Add(new PolylineSegment(segment.StartPoint, segment.EndPoint));
                }
                else
                {
                    var angle = length / circulararc.Radius;
                    segments.AddRange(segment.DoTessellate(angle));
                }
            }
            return segments;
        }

        public static Polyline Tessellate(this Circle circle, double length)
        {
            if (length >= 2*Math.PI*circle.Radius)
            {
                return circle.ToTriangle();
            }
            else
            {
                Plane plane = new Plane(circle.Center, circle.Normal);
                Matrix3d planeToWorld = Matrix3d.PlaneToWorld(plane);
                Arc firstArc = new Arc(Point3d.Origin, circle.Radius, 0.0, Math.PI);
                Arc secondArc = new Arc(Point3d.Origin, circle.Radius, Math.PI, Math.PI*2.0);
                firstArc.TransformBy(planeToWorld);
                secondArc.TransformBy(planeToWorld);
                Polyline firstPolyline = firstArc.TessellateArcWithArc(length);
                Polyline secondPolyline = secondArc.TessellateArcWithArc(length);
                var firstSegmentCollection = new PolylineSegmentCollection(firstPolyline);
                var secondSegmentCollection = new PolylineSegmentCollection(secondPolyline);
                var segmentCollection = new PolylineSegmentCollection();
                firstSegmentCollection.ForEach(o => segmentCollection.Add(o));
                secondSegmentCollection.ForEach(o => segmentCollection.Add(o));
                return segmentCollection.ToPolyline();
            }
        }
        private static Polyline ToTriangle(this Circle circle)
        {
            Plane plane = new Plane(circle.Center, circle.Normal);
            Matrix3d planeToWorld = Matrix3d.PlaneToWorld(plane);
            Point3d firstPt = new Point3d(0, circle.Radius, 0);
            double xLen = circle.Radius * Math.Cos(Math.PI / 6.0);
            double yLen = circle.Radius * Math.Sin(Math.PI / 6.0);
            Point3d secondPt = new Point3d(-xLen, -yLen, 0);
            Point3d thirdPt = new Point3d(xLen, -yLen, 0);
            Polyline polyline = new Polyline()
            {
                Closed = true
            };
            polyline.AddVertexAt(0, new Point2d(firstPt.X, firstPt.Y), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(secondPt.X, secondPt.Y), 0, 0, 0);
            polyline.AddVertexAt(2, new Point2d(thirdPt.X, thirdPt.Y), 0, 0, 0);
            polyline.TransformBy(planeToWorld);
            plane.Dispose();
            return polyline;
        }
        /// <summary>
        /// 根据角度分割弧段(保证起始点终止点不变)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static PolylineSegmentCollection DoTessellate(this PolylineSegment segment, double angle)
        {
            var TessellateArc = new PolylineSegmentCollection();
            var circulararc = new CircularArc2d(segment.StartPoint, segment.EndPoint, segment.Bulge, false);
            var angleRange = 4 * Math.Atan(segment.Bulge);
            // 判断弧线是否是顺时针方向
            int IsClockwise = (segment.Bulge < 0.0) ? -1 : 1;
            if (angle >= (angleRange * IsClockwise))
            {
                TessellateArc.Add(new PolylineSegment(segment.StartPoint, segment.EndPoint));
            }
            else
            {
                // 如果方向向量与y轴正方向的角度 小于等于90° 则方向向量在一三象限或x轴上，此时方向向量与x轴的角度不需要变化，否则需要 2PI - 与x轴角度
                double StartAng = (circulararc.Center.GetVectorTo(segment.StartPoint).GetAngleTo(new Vector2d(0.0, 1.0)) <= Math.PI / 2.0) ?
                    circulararc.Center.GetVectorTo(segment.StartPoint).GetAngleTo(new Vector2d(1.0, 0.0)) :
                    (Math.PI * 2.0 - circulararc.Center.GetVectorTo(segment.StartPoint).GetAngleTo(new Vector2d(1.0, 0.0)));

                double EndAng = (circulararc.Center.GetVectorTo(segment.EndPoint).GetAngleTo(new Vector2d(0.0, 1.0)) <= Math.PI / 2.0) ?
                    circulararc.Center.GetVectorTo(segment.EndPoint).GetAngleTo(new Vector2d(1.0, 0.0)) :
                    (Math.PI * 2.0 - circulararc.Center.GetVectorTo(segment.EndPoint).GetAngleTo(new Vector2d(1.0, 0.0)));
                int num = Convert.ToInt32(Math.Floor(angleRange * IsClockwise / angle)) + 1;

                for (int i = 1; i <= num; i++)
                {
                    var startAngle = StartAng + (i - 1) * angle * IsClockwise;
                    var endAngle = StartAng + i * angle * IsClockwise;
                    if (i == num)
                    {
                        endAngle = EndAng;
                    }
                    startAngle = (startAngle > 8 * Math.Atan(1)) ? startAngle - 8 * Math.Atan(1) : startAngle;
                    startAngle = (startAngle < 0.0) ? startAngle + 8 * Math.Atan(1) : startAngle;
                    endAngle = (endAngle > 8 * Math.Atan(1)) ? endAngle - 8 * Math.Atan(1) : endAngle;
                    endAngle = (endAngle < 0.0) ? endAngle + 8 * Math.Atan(1) : endAngle;
                    // Arc的构建方向是逆时针的，所以如果是顺时针的弧段，需要反向构建
                    if (segment.Bulge < 0.0)
                    {
                        var arc = new Arc(circulararc.Center.ToPoint3d(), circulararc.Radius, endAngle, startAngle);
                        TessellateArc.Add(new PolylineSegment(arc.EndPoint.ToPoint2d(), arc.StartPoint.ToPoint2d()));
                    }
                    else
                    {
                        var arc = new Arc(circulararc.Center.ToPoint3d(), circulararc.Radius, startAngle, endAngle);
                        TessellateArc.Add(new PolylineSegment(arc.StartPoint.ToPoint2d(), arc.EndPoint.ToPoint2d()));
                    }
                }
            }
            return TessellateArc;
        }
    }
}


