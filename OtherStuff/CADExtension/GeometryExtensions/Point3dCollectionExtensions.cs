using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the Point3dCollection type (and IEnumerable&lt;Point3d&gt;).
    /// </summary>
    public static class Point3dCollectionExtensions
    {
        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source)
        {
            return source.Cast<Point3d>().RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection according to the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of disitnct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source, Tolerance tolerance)
        {
            return source.Cast<Point3d>().Distinct(new Point3dComparer());
        }

        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source)
        {
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection according to the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of disitnct points.</returns>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source, Tolerance tolerance)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Point3dCollectionExtensions.tolerance = tolerance;
            return source.Distinct(new Point3dComparer());
        }

        /// <summary>
        /// Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains(this Point3dCollection source, Point3d pt)
        {
            return source.Contains(pt, Tolerance.Global);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains(this Point3dCollection source, Point3d pt, Tolerance tol)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            for (int i = 0; i < source.Count; i++)
            {
                if (pt.IsEqualTo(source[i], tol))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        /// ArgumentException is thrown if the collection is null or empty.</exception>
        public static Extents3d ToExtents3d(this Point3dCollection pts)
        {
            return pts.Cast<Point3d>().ToExtents3d();
        }

        /// <summary>
        /// Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        /// ArgumentException is thrown if the sequence is null or empty.</exception>
        public static Extents3d ToExtents3d(this IEnumerable<Point3d> pts)
        {
            if (pts == null || !pts.Any())
                throw new ArgumentException("Null or empty sequence");
            
            return pts.Aggregate(new Extents3d(), (e, p) => { e.AddPoint(p); return e; });
        }

        private static Tolerance tolerance;

        class Point3dComparer : IEqualityComparer<Point3d>
        {
            public bool Equals(Point3d a, Point3d b)
            {
                return a.IsEqualTo(b, Point3dCollectionExtensions.tolerance);
            }

            public int GetHashCode(Point3d pt)
            {
                return new Point3d(Round(pt.X), Round(pt.Y), Round(pt.Z)).GetHashCode();
            }

            private double prec = Point3dCollectionExtensions.tolerance.EqualPoint * 10.0;

            private double Round(double num)
            {
                return prec == 0.0 ? num : Math.Floor(num / prec + 0.5) * prec;
            }
        }
    }
}
