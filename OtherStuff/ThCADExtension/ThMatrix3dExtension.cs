using Autodesk.AutoCAD.Geometry;

namespace ThCADExtension
{
    public static class ThMatrix3dExtension
    {
        /// <summary>
        /// Gets the transformation matrix that transforms from the 
        /// given CoordinateSystem3d to the coordinate system of the 
        /// matrix which the method is invoked on.
        /// </summary>
        public static Matrix3d GetTransformFrom(this Matrix3d dest, CoordinateSystem3d origin)
        {
            CoordinateSystem3d to = dest.CoordinateSystem3d;
            if (to == origin)
                return dest;
            return Matrix3d.AlignCoordinateSystem(to.Origin, to.Xaxis, to.Yaxis, to.Zaxis,
               origin.Origin, origin.Xaxis, origin.Yaxis, origin.Zaxis);
        }

        /// <summary>
        /// Gets the transformation matrix that transforms from the 
        /// coordinate system of the given Matrix3d to the coordinate 
        /// system of the Matrix3d which the method is invoked on.
        /// </summary>
        public static Matrix3d GetTransformFrom(this Matrix3d dest, Matrix3d origin)
        {
            if (dest == origin)
                return dest;
            CoordinateSystem3d from = dest.CoordinateSystem3d;
            CoordinateSystem3d to = origin.CoordinateSystem3d;
            return Matrix3d.AlignCoordinateSystem(from.Origin, from.Xaxis, from.Yaxis, from.Zaxis,
               to.Origin, to.Xaxis, to.Yaxis, to.Zaxis);
        }

        /// <summary>
        /// Returns the Matrix3d that tranforms from the given coordinate
        /// system to the world coordinate system.
        /// </summary>
        public static Matrix3d ToWorld(this CoordinateSystem3d ucs)
        {
            return Matrix3d.Identity.GetTransformFrom(ucs);
        }
    }
}
