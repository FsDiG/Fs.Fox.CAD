﻿using System.Collections;
using System.Collections.Generic;
using System;

namespace GeometryExtensions
{
    /// <summary>
    /// Provides methods for the derived classes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the triangle.</typeparam>
    public abstract class Triangle<T> : IEnumerable<T>
    {
        #region Fields

        /// <summary>
        /// The first triangle element (origin).
        /// </summary>
        protected T _pt0 = default!;

        /// <summary>
        /// The second triangle element.
        /// </summary>
        protected T _pt1 = default!;

        /// <summary>
        /// The third triangle element.
        /// </summary>
        protected T _pt2 = default!;

        /// <summary>
        /// An array containing the three triangle elements.
        /// </summary>
        protected T[] _pts = new T[3];

        #endregion

        #region Constructors

 
        /// <summary>
        /// Initializes a new instance of Triangle &lt;T&gt; that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The array whose elements are copied to the new Triangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown if the array do not contains three items.</exception>
        protected internal Triangle(T[] pts)
        {
            if (pts.Length != 3)
            {
                throw new ArgumentOutOfRangeException("The array must contain 3 items");
            }
            _pts[0] = _pt0 = pts[0];
            _pts[1] = _pt1 = pts[1];
            _pts[2] = _pt2 = pts[2];
        }

        /// <summary>
        /// Initializes a new instance of Triangle &lt;T&gt; that contains the specified elements.
        /// </summary>
        /// <param name="a">First element to be copied in the new Triangle.</param>
        /// <param name="b">Second element to be copied in the new Triangle.</param>
        /// <param name="c">Third element to be copied in the new Triangle.</param>
        protected internal Triangle(T a, T b, T c)
        {
            _pt0 = a;
            _pt1 = b;
            _pt2 = c;
            _pts[0] = _pt0;
            _pts[1] = _pt1;
            _pts[2] = _pt2;
        }

        /// <summary>
        /// 
        /// </summary>
        protected Triangle()
        {
            _pt0 = default!;
            _pt1 = default!;
            _pt2 = default!;
        }

        #endregion

        #region Indexor

        /// <summary>
        /// Item
        /// </summary>
        /// <param name="i">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is throw if index is less than 0 or more than 2.</exception>
        public T this[int i]
        {
            get
            {
                if (i > 2 || i < 0)
                {
                    throw new IndexOutOfRangeException("Index out of range");
                }

                return _pts[i];
            }
            set
            {
                if (i > 2 || i < 0)
                {
                    throw new IndexOutOfRangeException("Index out of range");
                }

                _pts[i] = value;
                this.Set(_pts);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified Triangle&lt;T&gt; derived types instances are considered equal.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if every corresponding element in both Triangle&lt;T&gt; are considered equal; otherwise, nil.</returns>
        public override bool Equals(object obj)
        {
            Triangle<T>? trgl = obj as Triangle<T>;
            return
                trgl != null &&
                trgl.GetHashCode() == this.GetHashCode() &&
                trgl[0] != null && trgl[0]!.Equals(_pts[0]) &&
                trgl[1] != null && trgl[1]!.Equals(_pts[1]) &&
                trgl[2] != null && trgl[2]!.Equals(_pts[2]);
        }

        /// <summary>
        /// Serves as a hash function for Triangle&lt;T&gt; derived types.
        /// </summary>
        /// <returns>A hash code for the current Triangle&lt;T&gt;.</returns>
        public override int GetHashCode()
        {
            return _pts[0]!.GetHashCode() ^ _pts[1]!.GetHashCode() ^ _pts[2]!.GetHashCode();
        }

        /// <summary>
        /// Reverses the order without changing the origin (swaps the 2nd and 3rd elements)
        /// </summary>
        public void Inverse()
        {
            this.Set(_pt0, _pt2, _pt1);
        }

        /// <summary>
        /// Sets the elements of the triangle.
        /// </summary>
        /// <param name="pts">The array whose elements are copied to the Triangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown if the array do not contains three items.</exception>
        public void Set(T[] pts)
        {
            if (pts.Length != 3)
            {
                throw new IndexOutOfRangeException("The array must contain 3 items");
            }

            _pts[0] = _pt0 = pts[0];
            _pts[1] = _pt1 = pts[1];
            _pts[2] = _pt2 = pts[2];
        }

        /// <summary>
        /// Sets the elements of the triangle.
        /// </summary>
        /// <param name="a">First element to be copied in the Triangle.</param>
        /// <param name="b">Second element to be copied in the Triangle.</param>
        /// <param name="c">Third element to be copied in the Triangle.</param>
        public void Set(T a, T b, T c)
        {
            _pts[0] = _pt0 = a;
            _pts[1] = _pt1 = b;
            _pts[2] = _pt2 = c;
        }

        /// <summary>
        /// Converts the triangle into an array.
        /// </summary>
        /// <returns>An array of three elements.</returns>
        public T[] ToArray()
        {
            return _pts;
        }

        /// <summary>
        /// Applies ToString() to each element and concatenate the results separted with commas.
        /// </summary>
        /// <returns>A string containing the three elements separated with commas.</returns>
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", _pt0, _pt1, _pt2);
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the triangle.
        /// </summary>
        /// <returns>An IEnumerable&lt;T&gt; enumerator for the Triangle&lt;T&gt;.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in _pts)
            {
                yield return item;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
