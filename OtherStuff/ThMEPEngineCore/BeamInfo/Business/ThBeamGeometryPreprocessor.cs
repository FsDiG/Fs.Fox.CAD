using System;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;
using System.Collections.Generic;

namespace ThMEPEngineCore.BeamInfo.Business
{
    public static class ThBeamGeometryPreprocessor
    {
        /// <summary>
        /// 分解曲线
        /// </summary>
        /// <param name="curves"></param>
        public static DBObjectCollection ExplodeCurves(DBObjectCollection curves)
        {
            var objs = new DBObjectCollection();
            foreach (Curve curve in curves)
            {
                if (curve is Line line)
                {
                    objs.Add(line);
                }
                else if (curve is Arc arc)
                {
                    objs.Add(arc);
                }
                else if (curve is Polyline polyline)
                {
                    using (var entitySet = new DBObjectCollection())
                    {
                        polyline.Explode(entitySet);
                        foreach (Entity entity in entitySet)
                        {
                            objs.Add(entity);
                        }
                    }
                }
            }
            return objs;
        }

        /// <summary>
        /// 投影曲线到XY平面
        /// </summary>
        /// <param name="curves"></param>
        /// https://spiderinnet1.typepad.com/blog/2013/12/autocad-net-matrix-transformations-project-entity-to-plane.html
        public static DBObjectCollection ProjectXYCurves(DBObjectCollection curves)
        {
            var objs = new DBObjectCollection();
            Plane XYPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            Matrix3d matrix = Matrix3d.Projection(XYPlane, XYPlane.Normal);
            foreach (Curve curve in curves)
            {
                if (curve is Line line)
                {
                    if (line.Normal.IsParallelTo(Vector3d.ZAxis))
                    {
                        objs.Add(line);
                    }
                    else
                    {
                        objs.Add(new Line(line.StartPoint.TransformBy(matrix), line.EndPoint.TransformBy(matrix)));
                    }
                }
                else if (curve is Arc arc)
                {
                    if (arc.Normal.IsParallelTo(Vector3d.ZAxis))
                    {
                        objs.Add(arc);
                    }
                    else
                    {
                        objs.Add(new Arc(arc.Center.TransformBy(matrix), arc.Radius, arc.StartAngle, arc.EndAngle));
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }

        /// <summary>
        /// Z值归0
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static void Z0Curves(ref DBObjectCollection curves)
        {
            foreach (Curve curve in curves)
            {
                curve.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, 1e99)));
                curve.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -1e99)));
            }
        }

        /// <summary>
        /// 过滤曲线
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static DBObjectCollection FilterCurves(DBObjectCollection curves)
        {
            DBObjectCollection objs = new DBObjectCollection();
            foreach (Curve curve in curves)
            {
                if (curve.GetLength() > ThMEPEngineCoreCommon.BeamMinimumLength)
                {
                    objs.Add(curve);
                }
            }

            return objs;
        }

        public static DBObjectCollection MergeConnectCurves(this DBObjectCollection curves, Tolerance tolerance)
        {
            DBObjectCollection resCurves = new DBObjectCollection();
            while (curves.Count > 0)
            {
                var firCurve = curves[0] as Line;
                curves.Remove(firCurve);

                for (int i = 0; i < curves.Count; i++)
                {
                    Line tempLine = curves[i] as Line;
                    if (tempLine.StartPoint.IsEqualTo(firCurve.StartPoint) ||
                        tempLine.StartPoint.IsEqualTo(firCurve.EndPoint) ||
                        tempLine.EndPoint.IsEqualTo(firCurve.StartPoint) ||
                        tempLine.EndPoint.IsEqualTo(firCurve.EndPoint))
                    {
                        List<Point3d> points = new List<Point3d>();
                        points.Add(firCurve.StartPoint);
                        points.Add(firCurve.EndPoint);
                        points.Add(tempLine.StartPoint);
                        points.Add(tempLine.EndPoint);
                        points = points.OrderBy(x => x.X + x.Y).ToList();
                        Point3d sp = points.First();
                        Point3d ep = points.Last();

                        firCurve.StartPoint = sp;
                        firCurve.EndPoint = ep;

                        curves.Remove(curves[i]);
                        i = -1;
                    }
                }

                if (firCurve.Length > 0)
                {
                    resCurves.Add(firCurve);
                }
            }

            return resCurves;
        }
    }
}
