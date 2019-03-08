namespace AcadLib.Geometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides methods for the derived classes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the triangle.</typeparam>
    [PublicAPI]
    public abstract class Triangle<T> : IEnumerable<T>
    {
        /// <summary>
        /// The first triangle element (origin).
        /// </summary>
        protected T _pt0;

        /// <summary>
        /// The second triangle element.
        /// </summary>
        protected T _pt1;

        /// <summary>
        /// The third triangle element.
        /// </summary>
        protected T _pt2;

        /// <summary>
        /// An array containing the three triangle elements.
        /// </summary>
        protected T[] _pts = new T[3];

        /// <summary>
        /// Initializes a new instance of Triangle &lt;T&gt; that is empty.
        /// </summary>
        protected internal Triangle()
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle &lt;T&gt; that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The array whose elements are copied to the new Triangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown if the array do not contains three items.</exception>
        protected internal Triangle([NotNull] T[] pts)
        {
            if (pts.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(pts));
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
            _pts[0] = _pt0 = a;
            _pts[1] = _pt1 = b;
            _pts[2] = _pt2 = c;
        }

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
                    throw new IndexOutOfRangeException("Index out of range");
                return _pts[i];
            }
            set
            {
                if (i > 2 || i < 0)
                    throw new IndexOutOfRangeException("Index out of range");
                _pts[i] = value;
                Set(_pts);
            }
        }

        /// <summary>
        /// Determines whether the specified Triangle&lt;T&gt; derived types instances are considered equal.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if every corresponding element in both Triangle&lt;T&gt; are considered equal; otherwise, nil.</returns>
        public override bool Equals(object obj)
        {
            return
                obj is Triangle<T> trgl &&
                trgl.GetHashCode() == GetHashCode() &&
                trgl[0].Equals(_pt0) &&
                trgl[1].Equals(_pt1) &&
                trgl[2].Equals(_pt2);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the triangle.
        /// </summary>
        /// <returns>An IEnumerable&lt;T&gt; enumerator for the Triangle&lt;T&gt;.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _pts)
                yield return item;
        }

        /// <summary>
        /// Serves as a hash function for Triangle&lt;T&gt; derived types.
        /// </summary>
        /// <returns>A hash code for the current Triangle&lt;T&gt;.</returns>
        public override int GetHashCode()
        {
            return _pt0.GetHashCode() ^ _pt1.GetHashCode() ^ _pt2.GetHashCode();
        }

        /// <summary>
        /// Reverses the order without changing the origin (swaps the 2nd and 3rd elements)
        /// </summary>
        public void Inverse()
        {
            Set(_pt0, _pt2, _pt1);
        }

        /// <summary>
        /// Sets the elements of the triangle.
        /// </summary>
        /// <param name="pts">The array whose elements are copied to the Triangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown if the array do not contains three items.</exception>
        public void Set([NotNull] T[] pts)
        {
            if (pts.Length != 3)
                throw new IndexOutOfRangeException("The array must contain 3 items");
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
            return $"{_pt0},{_pt1},{_pt2}";
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}