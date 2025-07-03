using Autodesk.AutoCAD.Geometry;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class Extents3dExtensions
    {
        /// <summary>
        /// Centers the point.
        /// </summary>
        /// <param name="exts">The exts.</param>
        /// <returns></returns>
        public static Point3d CenterPoint(this Extents3d exts)
        {
            return exts.MinPoint + ((exts.MaxPoint - exts.MinPoint) / 2);
        }
    }
}