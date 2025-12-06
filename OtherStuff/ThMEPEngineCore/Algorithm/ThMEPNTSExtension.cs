using System;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Algorithm
{
    public static class ThMEPNTSExtension
    {
        public static bool IsLooseCollinear(Point3d firstSp, Point3d firstEp,
            Point3d secondSp, Point3d secondEp)
        {
            using (var ov = new ThCADCoreNTSFixedPrecision())
            {
                var first = new Line(firstSp, firstEp);
                var second = new Line(secondSp, secondEp);
                return first.IsCollinear(second);
            }
        }
        public static bool IsLooseOverlap(Point3d firstSp, Point3d firstEp,
            Point3d secondSp, Point3d secondEp)
        {
            using (var ov = new ThCADCoreNTSFixedPrecision())
            {
                var first = new Line(firstSp, firstEp);
                var second = new Line(secondSp, secondEp);
                return first.Overlaps(second);
            }
        }

        public static DBObjectCollection LooseUnion(this DBObjectCollection objs)
        {
            using (var ov = new ThCADCoreNTSFixedPrecision())
            {
                return objs.UnionPolygons();
            }
        }
    }
}
