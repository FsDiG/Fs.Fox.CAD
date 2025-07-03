using System;
using System.Linq;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThCurveCollectionExtension
    {
        public static DBObjectCollection ExplodeCurves(this DBObjectCollection curves)
        {
            var lines = new DBObjectCollection();
            curves.Cast<Curve>().ForEach(c =>
            {
                if (c is Line line)
                {
                    lines.Add(line.Clone() as Line);
                }
                else if (c is Polyline polyline)
                {
                    var items = new DBObjectCollection();
                    polyline.Explode(items);
                    items.Cast<Curve>().ForEach(o => lines.Add(o));
                }
                else
                {
                    throw new NotSupportedException();
                }
            });
            return lines;
        }
    }
}
