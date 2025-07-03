namespace Autodesk.AutoCAD.Geometry
{
    /// <summary>
    ///
    /// </summary>
    public static class Point3dExtension
    {
        /// <summary>
        /// To the point2 d.
        /// </summary>
        /// <param name="pnt">The PNT.</param>
        /// <returns></returns>
        public static Point2d ToPoint2D(this Point3d pnt)
        {
            return new Point2d(pnt.X, pnt.Y);
        }
    }
}