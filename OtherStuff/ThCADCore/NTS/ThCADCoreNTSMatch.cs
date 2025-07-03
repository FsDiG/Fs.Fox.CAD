using NetTopologySuite.Algorithm.Match;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSMatch
    {
        public static double SimilarityMeasure(this Polyline pline, Polyline other)
        {
            var measure = new HausdorffSimilarityMeasure();
            return measure.Measure(pline.ToNTSPolygon(), other.ToNTSPolygon());
        }
    }
}
