using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides extension methods for the Point2dCollection type (and IEnumerable&lt;Point2d&gt;).
    /// </summary>
    public static class Point2dCollectionExtensions
    {
        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source)
        {
            return source.Cast<Point2d>().RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection according to the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of disitnct points.</returns>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source, Tolerance tolerance)
        {
            return source.Cast<Point2d>().Distinct(new Point2dComparer());
        }

        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source)
        {
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection according to the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of disitnct points.</returns>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source, Tolerance tolerance)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Point2dCollectionExtensions.tolerance = tolerance;
            return source.Distinct(new Point2dComparer());
        }

        /// <summary>
        /// Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="source">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains(this Point2dCollection source, Point2d pt)
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
        public static bool Contains(this Point2dCollection source, Point2d pt, Tolerance tol)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (pt.IsEqualTo(source[i], tol))
                {
                    return true;
                }
            }
            return false;
        }

        private static Tolerance tolerance;

        class Point2dComparer : IEqualityComparer<Point2d>
        {
            public bool Equals(Point2d a, Point2d b)
            {
                return a.IsEqualTo(b, Point2dCollectionExtensions.tolerance);
            }

            public int GetHashCode(Point2d pt)
            {
                return new Point2d(Round(pt.X), Round(pt.Y)).GetHashCode();
            }

            private double prec = Point2dCollectionExtensions.tolerance.EqualPoint * 10.0;

            private double Round(double num)
            {
                return prec == 0.0 ? num : Math.Floor(num / prec + 0.5) * prec;
            }
        }
    }
}
