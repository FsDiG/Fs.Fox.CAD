using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the Region type.
    /// </summary>
    public static class RegionExtensions
    {
        /// <summary>
        /// Gets the centroid of the region.
        /// </summary>
        /// <param name="reg">The instance to which the method applies.</param>
        /// <returns>The centroid of the region (WCS coordinates).</returns>
        public static Point3d Centroid(this Region reg)
        {
            using (Solid3d sol = new Solid3d())
            {
                sol.Extrude(reg, 2.0, 0.0);
                return sol.MassProperties.Centroid - reg.Normal;
            }
        }
    }
}
