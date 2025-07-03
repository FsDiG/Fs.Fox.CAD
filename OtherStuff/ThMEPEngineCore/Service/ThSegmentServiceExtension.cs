using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using ThCADExtension;
using ThMEPEngineCore.CAD;

namespace ThMEPEngineCore.Service
{
    public class ThSegmentServiceExtension: ThSegmentService
    {
        public List<Tuple<Point3d, Point3d>> LinearPairPts { get; set; }
        public List<Tuple<Point3d, Point3d>> ArcPairPts { get; set; }
        public ThSegmentServiceExtension(Polyline polyline):base(polyline)
        {
            LinearPairPts = new List<Tuple<Point3d, Point3d>>();
            ArcPairPts = new List<Tuple<Point3d, Point3d>>();
        }
        public void FindPairSegment(Point3dCollection pts)
        {
            var curves = FindPointSegment(pts);
            var linearSegments = curves.Where(o => o.Item1 is LineSegment3d).ToList();
            var arcSegments = curves.Where(o => o.Item1 is CircularArc3d).ToList();
            LinearPairPts=HandleLinearSegments(linearSegments);
            ArcPairPts = HandleArcSegments(arcSegments);
        }
        private List<Tuple<Point3d, Point3d>> HandleLinearSegments(List<Tuple<Curve3d, Point3d>> linearSegments)
        {
            List<Tuple<Point3d, Point3d>> pairPts = new List<Tuple<Point3d, Point3d>>();
            while(linearSegments.Count>0)
            {
                var firstSeg = linearSegments[0];
                linearSegments.RemoveAt(0);
                LinearEntity3d firstLinear = firstSeg.Item1 as LinearEntity3d;
                for (int i = 0; i < linearSegments.Count; i++)
                {
                    LinearEntity3d secondLinear = linearSegments[i].Item1 as LinearEntity3d;
                    if (firstLinear.Direction.IsParallelToEx(secondLinear.Direction) &&
                        !firstLinear.IsColinearTo(secondLinear))
                    {
                        pairPts.Add(Tuple.Create(firstSeg.Item2, linearSegments[i].Item2));
                    }
                }
            }
            return pairPts;
        }       
        private List<Tuple<Point3d, Point3d>> HandleArcSegments(List<Tuple<Curve3d, Point3d>> arcSegments)
        {
            List<Tuple<Point3d, Point3d>> pairPts = new List<Tuple<Point3d, Point3d>>();
            while (arcSegments.Count > 0)
            {
                var firstSeg = arcSegments[0];
                arcSegments.RemoveAt(0);
                for (int i = 0; i < arcSegments.Count; i++)
                {
                    if (IsParallelUncollinear(firstSeg.Item1 as CircularArc3d, arcSegments[i].Item1 as CircularArc3d))
                    {
                        pairPts.Add(Tuple.Create(firstSeg.Item2, arcSegments[i].Item2));
                    }
                }
            }
            return pairPts;
        }
        private List<Tuple<Curve3d, Point3d>> FindPointSegment(Point3dCollection pts)
        {
            List<Tuple<Curve3d, Point3d>> results = new List<Tuple<Curve3d, Point3d>>();
            List<Curve3d> curveSegments = new List<Curve3d>();
            foreach (Point3d pt in pts)
            {
                var curves = GetSegmentByPoint(Outline, pt);
                curves.ForEach(o => results.Add(Tuple.Create(o, pt)));                
            }
            return results;
        }
        private bool IsParallelUncollinear(CircularArc3d firstEnt, CircularArc3d secondEnt)
        {
            //TODO
            return false;
        }
        /// <summary>
        /// 组成弧Segment的两根平行弧
        /// </summary>
        /// <param name="firstArc"></param>
        /// <param name="secondArc"></param>
        /// <returns></returns>
        private bool ParallelToArc(Arc firstArc, Arc secondArc)
        {
            bool isParallel = false;
            if (firstArc.Normal.IsParallelToEx(secondArc.Normal))
            {
                double oneDegree = 1.0 / Math.PI;
                Vector3d vec = firstArc.Center.GetVectorTo(firstArc.GetMidpoint());
                Matrix3d mt1 = Matrix3d.Rotation(oneDegree, firstArc.Normal, firstArc.Center);
                Matrix3d mt2 = Matrix3d.Rotation(oneDegree * -1.0, firstArc.Normal, firstArc.Center);
                Vector3d vec1 = vec.TransformBy(mt1);
                Vector3d vec2 = vec.TransformBy(mt2);

                Point3d line1Ep = firstArc.Center + vec1.GetNormal().MultiplyBy(firstArc.Radius + secondArc.Radius);
                Point3d line2Ep = firstArc.Center + vec2.GetNormal().MultiplyBy(firstArc.Radius + secondArc.Radius);

                Line line1 = new Line(firstArc.Center, line1Ep);
                Line line2 = new Line(firstArc.Center, line2Ep);

                Point3dCollection line1FirstArcPts = new Point3dCollection();
                Point3dCollection line1SecondArcPts = new Point3dCollection();
                line1.IntersectWith(firstArc, Intersect.OnBothOperands, line1FirstArcPts, IntPtr.Zero, IntPtr.Zero);
                line1.IntersectWith(firstArc, Intersect.OnBothOperands, line1SecondArcPts, IntPtr.Zero, IntPtr.Zero);

                Point3dCollection line2FirstArcPts = new Point3dCollection();
                Point3dCollection line2SecondArcPts = new Point3dCollection();
                line2.IntersectWith(firstArc, Intersect.OnBothOperands, line2FirstArcPts, IntPtr.Zero, IntPtr.Zero);
                line2.IntersectWith(secondArc, Intersect.OnBothOperands, line2SecondArcPts, IntPtr.Zero, IntPtr.Zero);

                if (line1FirstArcPts.Count == 1 && line1SecondArcPts.Count == 1 &&
                    line2FirstArcPts.Count == 1 && line2SecondArcPts.Count == 1)
                {
                    if (line1FirstArcPts[0].DistanceTo(line1SecondArcPts[0]) -
                         line2FirstArcPts[0].DistanceTo(line2SecondArcPts[0]) <= 1.0 &&
                         line1FirstArcPts[0].DistanceTo(line1SecondArcPts[0])>0.0)
                    {
                        isParallel = true;
                    }
                }
                line1.Dispose();
                line2.Dispose();
            }
            return isParallel;
        }
    }
}
