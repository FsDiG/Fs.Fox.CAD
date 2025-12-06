using Autodesk.AutoCAD.Geometry;

namespace ThCADExtension
{
    public static class ThGeExtension
    {
        public static LineSegment3d To3D(this LineSegment2d lineSegment, PlanarEntity plane)
        {
            return new LineSegment3d(
                new Point3d(plane, lineSegment.StartPoint),
                new Point3d(plane, lineSegment.EndPoint));
        }

        public static CircularArc3d To3D(this CircularArc2d circularArc, PlanarEntity plane)
        {
            return new CircularArc3d(
                new Point3d(plane, circularArc.Center),
                plane.Normal,
                new Vector3d(plane, circularArc.ReferenceVector),
                circularArc.Radius,
                circularArc.StartAngle,
                circularArc.EndAngle);
        }

        public static EllipticalArc3d To3D(this EllipticalArc2d ellipticalArc, PlanarEntity plane)
        {
            return new EllipticalArc3d(
                new Point3d(plane, ellipticalArc.Center),
                new Vector3d(plane, ellipticalArc.MajorAxis),
                new Vector3d(plane, ellipticalArc.MinorAxis),
                ellipticalArc.MajorRadius,
                ellipticalArc.MinorRadius,
                ellipticalArc.StartAngle,
                ellipticalArc.EndAngle);
        }

        public static NurbCurve3d To3D(this NurbCurve2d nurbCurve, PlanarEntity plane)
        {
            if (nurbCurve.HasFitData)
            {
                NurbCurve2dFitData n2fd = nurbCurve.FitData;
                return new NurbCurve3d(
                    n2fd.FitPoints.To3dPoints(plane),
                    new Vector3d(plane, n2fd.StartTangent),
                    new Vector3d(plane, n2fd.EndTangent),
                    true,
                    true,
                    n2fd.KnotParam,
                    n2fd.FitTolerance);
            }
            else
            {
                double period = 0;
                bool isPeriodic = nurbCurve.IsPeriodic(out period);
                NurbCurve2dData n2fd = nurbCurve.DefinitionData;
                return new NurbCurve3d(
                    n2fd.Degree,
                    n2fd.Knots,
                    n2fd.ControlPoints.To3dPoints(plane),
                    n2fd.Weights,
                    isPeriodic);
            }
        }

        public static Point3dCollection To3dPoints(this Point2dCollection collection, PlanarEntity plane)
        {
            var points = new Point3dCollection();
            foreach (var point in collection)
            {
                points.Add(new Point3d(plane, point));
            }
            return points;
        }
    }
}
