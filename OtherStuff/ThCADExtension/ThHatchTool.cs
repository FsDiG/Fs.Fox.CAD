using System;
using NFox.Cad;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThHatchTool
    {
        // https://forums.autodesk.com/t5/net/restore-hatch-boundaries-if-they-have-been-lost-with-net/td-p/3779514
        // https://adndevblog.typepad.com/autocad/2012/04/perimeter-of-a-hatch-using-objectarx-and-autocad-net-api.html
        public static List<Curve> Boundaries(this Hatch hatch,double tolerance=1e-4)
        {
            var curves = new List<Curve>();
            if(hatch==null)
            {
                return curves;
            }
            for (int index = 0; index < hatch.NumberOfLoops; index++)
            {
                var hatchLoop = hatch.GetLoopAt(index);
                if (hatchLoop.IsPolyline)
                {
                    var vertices = hatchLoop.Polyline;
                    var pline = new Polyline(vertices.Count)
                    {
                        Closed = true,
                    };
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        pline.AddVertexAt(i, vertices[i].Vertex, vertices[i].Bulge, 0, 0);
                    }
                    curves.Add(pline);
                }
                else
                {
                    // 单独处理圆的情况
                    if (hatchLoop.Curves.Count == 1
                        && hatchLoop.Curves[0].IsClosed()
                        && hatchLoop.Curves[0] is CircularArc2d circularArc)
                    {
                        curves.Add(circularArc.ToCircle());
                        continue;
                    }

                    // 暂时只处理线和圆弧组合的情况
                    var segments = new PolylineSegmentCollection();
                    foreach (Curve2d cv in hatchLoop.Curves)
                    {
                        LineSegment2d line2d = cv as LineSegment2d;
                        CircularArc2d arc2d = cv as CircularArc2d;
                        EllipticalArc2d ellipse2d = cv as EllipticalArc2d;
                        NurbCurve2d spline2d = cv as NurbCurve2d;
                        if (line2d != null)
                        {
                            segments.Add(new PolylineSegment(line2d));
                        }
                        else if (arc2d != null)
                        {
                            segments.Add(new PolylineSegment(arc2d));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                        //else if (ellipse2d != null)
                        //{
                        //    var eclipse = ellipse2d.ToCurve();
                        //    segments.AddRange(new PolylineSegmentCollection(eclipse));
                        //}
                        //else if (spline2d != null)
                        //{
                        //    var poly = spline2d.ToCurve().ToPolyline() as Polyline;
                        //    segments.AddRange(new PolylineSegmentCollection(poly));
                        //}
                        //else
                        //{
                        //    throw new NotSupportedException();
                        //}
                    }
                    segments.Join();
                    var newPoly = segments.ToPolyline();
                    if (newPoly.StartPoint.IsEqualTo(newPoly.EndPoint,new Tolerance(tolerance, tolerance)))
                    {
                        newPoly.Closed = true;
                    }
                    curves.Add(newPoly);
                }
            }
            return curves;
        }
    }
}
