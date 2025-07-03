using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADExtension
{
    public static class ThBlockTools
    {
        public static Extents3d  GeometricExtents(this BlockTableRecord btr)
        {
            var extents = new Extents3d();
            extents.AddBlockExtents(btr);
            return extents;
        }
    }
}
