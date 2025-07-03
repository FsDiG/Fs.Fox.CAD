#if ACAD2016
using DotNetARX;
using System.Linq;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using AcPolygon = Autodesk.AutoCAD.DatabaseServices.Polyline;
using CLI;

namespace ThMEPEngineCore.Algorithm
{
    public class ThMEPPolyDecomposer
    {
        public static DBObjectCollection Decompose(AcPolygon poly)
        {
            var points = new List<double>();
            points.AddRange(poly.Coordinates2D());
            var decomposer = new ThPolygonDecomposer();
            var results = decomposer.Decompose(points.ToArray(), points.Count / 2);
            var objs = new DBObjectCollection();
            foreach (var polygon in results)
            {
                var vectices = new List<Point2d>();
                for (int i = 0; i < polygon.Count(); i += 2)
                {
                    vectices.Add(new Point2d(polygon[i], polygon[i + 1]));
                }
                var item = new Polyline()
                {
                    Closed = true,
                };
                item.CreatePolyline(vectices.ToArray());
                objs.Add(item);
            }
            return objs;
        }
    }
}

#endif
