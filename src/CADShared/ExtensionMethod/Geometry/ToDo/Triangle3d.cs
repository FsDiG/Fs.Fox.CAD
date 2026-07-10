using System;
using Autodesk.AutoCAD.Geometry;

namespace GeometryExtensions
{
    /// <summary>
    /// Represents a triangle in the 3d space. It can be viewed as a structure consisting of three Point3d.
    /// </summary>
    public class Triangle3d : Triangle<Point3d>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of Triangle3d; that is empty.
        /// </summary>
        public Triangle3d() : base() { }


        /// <summary>
        /// Initializes a new instance of Triangle3d that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The Point3d array whose elements are copied to the new Triangle3d.</param>
        public Triangle3d(Point3d[] pts) : base(pts)
        { }

        /// <summary>
        /// Initializes a new instance of Triangle3d that contains the specified elements.
        /// </summary>
        /// <param name="a">The first vertex of the new Triangle3d (origin).</param>
        /// <param name="b">The second vertex of the new Triangle3d (2nd vertex).</param>
        /// <param name="c">The third vertex of the new Triangle3d (3rd vertex).</param>
        public Triangle3d(Point3d a, Point3d b, Point3d c) : base(a, b, c) { }

        /// <summary>
        /// Initializes a new instance of Triangle3d according to an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public Triangle3d(Point3d org, Vector3d v1, Vector3d v2) 
        {
            _pts[0] = _pt0 = org;
            _pts[1] = _pt1 = org + v1;
            _pts[2] = _pt2 = org + v2;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the triangle area.
        /// </summary>
        public double Area
        {
            get
            {
                return Math.Abs(
                    (((_pt1.X - _pt0.X) * (_pt2.Y - _pt0.Y)) -
                    ((_pt2.X - _pt0.X) * (_pt1.Y - _pt0.Y))) / 2.0);
            }
        }

        /// <summary>
        /// Gets the triangle centroid.
        /// </summary>
        public Point3d Centroid
        {
            get { return (_pt0 + _pt1.GetAsVector() + _pt2.GetAsVector()) / 3.0; }
        }

 
        /// <summary>
        /// Gets the triangle plane elevation.
        /// </summary>
        public double Elevation
        {
            get { return _pt0.TransformBy(Matrix3d.WorldToPlane(this.Normal)).Z; }
        }

        /// <summary>
        /// Gets the unit vector of the triangle plane greatest slope.
        /// </summary>
        public Vector3d GreatestSlope
        {
            get
            {
                Vector3d norm = this.Normal;
                if (norm.IsParallelTo(Vector3d.ZAxis))
                {
                    return new Vector3d(0.0, 0.0, 0.0);
                }

                if (norm.Z == 0.0)
                {
                    return Vector3d.ZAxis.Negate();
                }

                return new Vector3d(-norm.Y, norm.X, 0.0).CrossProduct(norm).GetNormal();
            }
        }

        /// <summary>
        /// Gets the unit horizontal vector of the triangle plane.
        /// </summary>
        public Vector3d Horizontal
        {
            get
            {
                Vector3d norm = this.Normal;
                if (norm.IsParallelTo(Vector3d.ZAxis))
                {
                    return Vector3d.XAxis;
                }

                return new Vector3d(-norm.Y, norm.X, 0.0).GetNormal();
            }
        }

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        public CircularArc3d? InscribedCircle
        {
            get
            {
                CircularArc2d? ca2d = this.Convert2d().InscribedCircle;
                if (ca2d == null)
                {
                    return null;
                }

                return new CircularArc3d(ca2d.Center.Convert3d(this.GetPlane()), this.Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the triangle plane is horizontal.
        /// </summary>
        public bool IsHorizontal
        {
            get { return _pt0.Z == _pt1.Z && _pt0.Z == _pt2.Z; }
        }

        /// <summary>
        /// Gets the normal vector of the triangle plane.
        /// </summary>
        public Vector3d Normal
        {
            get { return (_pt1 - _pt0).CrossProduct(_pt2 - _pt0).GetNormal(); }
        }

        /// <summary>
        /// Gets the percent slope of the triangle plane.
        /// </summary>
        public double SlopePerCent
        {
            get
            {
                Vector3d norm = this.Normal;
                if (norm.Z == 0.0)
                {
                    return Double.PositiveInfinity;
                }

                return Math.Abs(100.0 * (Math.Sqrt(Math.Pow(norm.X, 2.0) + Math.Pow(norm.Y, 2.0))) / norm.Z);
            }
        }

        /// <summary>
        /// Gets the triangle coordinates system 
        /// (origin = centroid, X axis = horizontal vector, Y axis = negated geatest slope vector).
        /// </summary>
        public Matrix3d SlopeUCS
        {
            get
            {
                Point3d origin = this.Centroid;
                Vector3d zaxis = this.Normal;
                Vector3d xaxis = this.Horizontal;
                Vector3d yaxis = zaxis.CrossProduct(xaxis).GetNormal();
                return new Matrix3d(new double[]{
                    xaxis.X, yaxis.X, zaxis.X, origin.X,
                    xaxis.Y, yaxis.Y, zaxis.Y, origin.Y,
                    xaxis.Z, yaxis.Z, zaxis.Z, origin.Z,
                    0.0, 0.0, 0.0, 1.0});
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts a Triangle3d into a Triangle2d according to the Triangle3d plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2d Convert2d()
        {
            return new Triangle2d(Array.ConvertAll(_pts, x => x.Convert2d(this.GetPlane())));
        }

        /// <summary>
        /// Projects a Triangle3d on the WCS XY plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2d Flatten()
        {
            return new Triangle2d(
                new Point2d(this[0].X, this[0].Y),
                new Point2d(this[1].X, this[1].Y),
                new Point2d(this[2].X, this[2].Y));
        }

        /// <summary>
        /// Gets the angle between the two segments at specified vertex.
        /// </summary>.
        /// <param name="index">The vertex index.</param>
        /// <returns>The angle expressed in radians.</returns>
        public double GetAngleAt(int index)
        {
            return this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                this[index].GetVectorTo(this[(index + 2) % 3]));
        }

        /// <summary>
        /// Gets the bounded plane defined by the triangle.
        /// </summary>
        /// <returns>The bouned plane.</returns>
        public BoundedPlane GetBoundedPlane()
        {
            return new BoundedPlane(this[0], this[1], this[2]);
        }

        /// <summary>
        /// Gets the unbounded plane defined by the triangle.
        /// </summary>
        /// <returns>The unbouned plane.</returns>
        public Plane GetPlane()
        {
            Point3d origin =
                new Point3d(0.0, 0.0, this.Elevation).TransformBy(Matrix3d.PlaneToWorld(this.Normal));
            return new Plane(origin, this.Normal);
        }

        /// <summary>
        /// Gets the segment at specified index.
        /// </summary>
        /// <param name="index">The segment index.</param>
        /// <returns>The segment 3d</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is throw if index is less than 0 or more than 2.</exception>
        public LineSegment3d GetSegmentAt(int index)
        {
            if (index > 2)
            {
                throw new IndexOutOfRangeException("Index out of range");
            }

            return new LineSegment3d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the Tolerance.Global.EqualPoint value.
        /// </summary>
        /// <param name="t3d">The triangle3d to compare.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo(Triangle3d t3d)
        {
            return this.IsEqualTo(t3d, Tolerance.Global);
        }

        /// <summary>
        /// Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the Tolerance.EqualPoint value of the specified tolerance.
        /// </summary>
        /// <param name="t3d">The triangle3d to compare.</param>
        /// <param name="tol">The tolerance used in points comparisons.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo(Triangle3d t3d, Tolerance tol)
        {
            return t3d[0].IsEqualTo(_pt0, tol) && t3d[1].IsEqualTo(_pt1, tol) && t3d[2].IsEqualTo(_pt2, tol);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is strictly inside the triangle.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is inside; otherwise, false.</returns>
        public bool IsPointInside(Point3d pt)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            Vector3d v1 = pt.GetVectorTo(_pt0).CrossProduct(pt.GetVectorTo(_pt1)).GetNormal();
            Vector3d v2 = pt.GetVectorTo(_pt1).CrossProduct(pt.GetVectorTo(_pt2)).GetNormal();
            Vector3d v3 = pt.GetVectorTo(_pt2).CrossProduct(pt.GetVectorTo(_pt0)).GetNormal();
            return (v1.IsEqualTo(v2, tol) && v2.IsEqualTo(v3, tol));
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is on a triangle segment.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is on a segment; otherwise, false.</returns>
        public bool IsPointOn(Point3d pt)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            Vector3d v0 = new Vector3d(0.0, 0.0, 0.0);
            Vector3d v1 = pt.GetVectorTo(_pt0).CrossProduct(pt.GetVectorTo(_pt1));
            Vector3d v2 = pt.GetVectorTo(_pt1).CrossProduct(pt.GetVectorTo(_pt2));
            Vector3d v3 = pt.GetVectorTo(_pt2).CrossProduct(pt.GetVectorTo(_pt0));
            return (v1.IsEqualTo(v0, tol) || v2.IsEqualTo(v0, tol) || v3.IsEqualTo(v0, tol));
        }

        /// <summary>
        /// Sets the elements of the triangle using an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public void Set(Point3d org, Vector3d v1, Vector3d v2)
        {
            _pt0 = org;
            _pt1 = org + v1;
            _pt2 = org + v2;
            _pts = new Point3d[3] { _pt0, _pt1, _pt2 };
        }

        /// <summary>
        /// Transforms a Triangle3d with a transformation matrix
        /// </summary>
        /// <param name="mat">The 3d transformation matrix.</param>
        /// <returns>The new Triangle3d.</returns>
        public Triangle3d Transformby(Matrix3d mat)
        {
            return new Triangle3d(Array.ConvertAll<Point3d, Point3d>(
                _pts, new Converter<Point3d, Point3d>(p => p.TransformBy(mat))));
        }

        #endregion
    }
}
