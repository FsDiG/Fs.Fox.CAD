using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

namespace ThCADExtension
{
    public class ThPoint3dComparer : IComparer<Point3d>
    {
        private readonly Point3d center;
        public ThPoint3dComparer(Point3d point)
        {
            center = point;
        }

        protected static bool IsZero(double a)
        {
            return Math.Abs(a) < Tolerance.Global.EqualPoint;
        }

        protected static bool IsEqual(double a, double b)
        {
            return IsZero(b - a);
        }

        public int Compare(Point3d pt1, Point3d pt2)
        {
            // Sort points in clockwise order
            // https://stackoverflow.com/questions/6989100/sort-points-in-clockwise-order
            if (pt1 == pt2)
            {
                return 0;
            }

            if (pt1.X >= 0 && pt2.X < 0)
            {
                return -1;
            }
            else if (pt1.X == 0 && pt2.X == 0)
            {
                return (pt1.Y > pt2.Y) ? -1 : 1;
            }


            double det = (pt1.X - center.X) * (pt2.Y - center.Y) - (pt2.X - center.X) * (pt1.Y - center.Y);
            if (det < 0)
            {
                return -1;
            }
            else if (det > 0)
            {
                return 1;
            }

            double d1 = (pt1.X - center.X) * (pt1.X - center.X) + (pt1.Y - center.Y) * (pt1.Y - center.Y);
            double d2 = (pt2.X - center.X) * (pt2.X - center.X) + (pt2.Y - center.Y) * (pt2.Y - center.Y);
            return (d1 > d2) ? -1 : 1;
        }
    }

    public static class ThPoint3dCollectionExtensions
    {
        public static void Swap(this Point3dCollection collection, int index1, int index2)
        {
            Point3d temp = collection[index1];
            collection[index1] = collection[index2];
            collection[index2] = temp;
        }

        public static Point3d CenterPoint(this Point3dCollection points)
        {
            double sumX = 0, sumY = 0, sumZ = 0;
            for (int i = 0; i < points.Count; i++)
            {
                sumX += points[i].X;
                sumY += points[i].Y;
                sumZ += points[i].Z;
            }

            return new Point3d(sumX / points.Count, sumY / points.Count, sumZ / points.Count);
        }

        // Sort Point3dCollection
        // https://www.keanw.com/2011/01/sorting-an-autocad-point2dcollection-or-point3dcollection-using-net.html
        public static Point3dCollection Sort(this Point3dCollection collection)
        {
            Point3d[] points = new Point3d[collection.Count];
            collection.CopyTo(points, 0);

            Point3d center = collection.CenterPoint();
            Array.Sort(points, new ThPoint3dComparer(center));
            return new Point3dCollection(points);
        }

        public static Point3dCollection TransformBy(this Point3dCollection points, Matrix3d leftSide)
        {
            var newPoints = new Point3dCollection();
            foreach(Point3d point in points)
            {
                newPoints.Add(point.TransformBy(leftSide));
            }
            return newPoints;
        }
    }
}
