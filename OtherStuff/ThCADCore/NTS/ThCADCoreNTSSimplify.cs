using System;
using NetTopologySuite.Simplify;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSSimplify
    {
        public static Polyline VWSimplify(this Polyline pline, double distanceTolerance)
        {
            var simplifier = new VWLineSimplifier(pline.ToNTSLineString().Coordinates, distanceTolerance);
            var result = ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(simplifier.Simplify());
            return result.ToDbPolyline();
        }

        public static Polyline DPSimplify(this Polyline pline, double distanceTolerance)
        {
            var simplifier = new DouglasPeuckerLineSimplifier(pline.ToNTSLineString().Coordinates)
            {
                DistanceTolerance = distanceTolerance,
            };
            var result = ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(simplifier.Simplify());
            return result.ToDbPolyline();
        }

        public static Polyline TPSimplify(this Polyline pline, double distanceTolerance)
        {
            var result = TopologyPreservingSimplifier.Simplify(pline.ToNTSLineString(), distanceTolerance);
            if (result is LineString lineString)
            {
                return lineString.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
