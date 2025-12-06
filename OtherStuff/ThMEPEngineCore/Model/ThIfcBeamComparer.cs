using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ThMEPEngineCore.Model
{
    public class ThIfcBeamComparer : IComparer<ThIfcBeam>
    {
        private ThPoint3dComparer PointComparer { get; set; }

        public ThIfcBeamComparer(Point3d center)
        {
            PointComparer = new ThPoint3dComparer(center);
        }

        public int Compare(ThIfcBeam x, ThIfcBeam y)
        {
            return PointComparer.Compare(x.Outline.GetCenter(), y.Outline.GetCenter());
        }
    }
}
