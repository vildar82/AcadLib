namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a triangle in a 2d plane. It can be viewed as a structure consisting of three Point2d.
    /// </summary>
    public class Triangle2d : Triangle<Point2d>
    {
        /// <summary>
        /// Initializes a new instance of Triangle2d that is empty.
        /// </summary>
        public Triangle2d()
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle2d that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The Point2d array whose elements are copied to the new Triangle2d.</param>
        public Triangle2d([NotNull] Point2d[] pts) : base(pts)
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle2d that contains the specified elements.
        /// </summary>
        /// <param name="a">The first vertex of the new Triangle2d (origin).</param>
        /// <param name="b">The second vertex of the new Triangle2d (2nd vertex).</param>
        /// <param name="c">The third vertex of the new Triangle2d (3rd vertex).</param>
        public Triangle2d(Point2d a, Point2d b, Point2d c) : base(a, b, c)
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle2d according to an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle2d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public Triangle2d(Point2d org, Vector2d v1, Vector2d v2)
        {
            _pts[0] = _pt0 = org;
            _pts[1] = _pt1 = org + v1;
            _pts[2] = _pt2 = org + v2;
        }

        /// <summary>
        /// Gets the triangle algebraic (signed) area.
        /// </summary>
        public double AlgebricArea => ((_pt1.X - _pt0.X) * (_pt2.Y - _pt0.Y) -
                                       (_pt2.X - _pt0.X) * (_pt1.Y - _pt0.Y)) / 2.0;

        /// <summary>
        /// Gets the triangle centroid.
        /// </summary>
        public Point2d Centroid => (_pt0 + _pt1.GetAsVector() + _pt2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        [CanBeNull]
        public CircularArc2d CircumscribedCircle
        {
            get
            {
                var l1 = GetSegmentAt(0).GetBisector();
                var l2 = GetSegmentAt(1).GetBisector();
                var inters = l1.IntersectWith(l2);
                return inters == null ? null : new CircularArc2d(inters[0], inters[0].GetDistanceTo(_pt0));
            }
        }

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        [CanBeNull]
        public CircularArc2d InscribedCircle
        {
            get
            {
                var v1 = _pt0.GetVectorTo(_pt1).GetNormal();
                var v2 = _pt0.GetVectorTo(_pt2).GetNormal();
                var v3 = _pt1.GetVectorTo(_pt2).GetNormal();
                if (v1.IsEqualTo(v2) || v2.IsEqualTo(v3))
                    return null;
                var l1 = new Line2d(_pt0, v1 + v2);
                var l2 = new Line2d(_pt1, v1.Negate() + v3);
                var inters = l1.IntersectWith(l2);
                return new CircularArc2d(inters[0], GetSegmentAt(0).GetDistanceTo(inters[0]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the triangle vertices are clockwise.
        /// </summary>
        public bool IsClockwise => AlgebricArea < 0.0;

        /// <summary>
        /// Converts the triangle into a Triangle3d according to the specified plane.
        /// </summary>
        /// <param name="plane">Plane of the Triangle3d.</param>
        /// <returns>The new Triangle3d.</returns>
        [NotNull]
        public Triangle3d Convert3d(Plane plane)
        {
            return new Triangle3d(Array.ConvertAll(_pts, x => x.Convert3d(plane)));
        }

        /// <summary>
        /// Converts the triangle into a Triangle3d according to the plane defined by its normal and elevation.
        /// </summary>
        /// <param name="normal">The normal vector of the plane.</param>
        /// <param name="elevation">The elevation of the plane.</param>
        /// <returns>The new Triangle3d.</returns>
        [NotNull]
        public Triangle3d Convert3d(Vector3d normal, double elevation)
        {
            return new Triangle3d(Array.ConvertAll(_pts, x => x.Convert3d(normal, elevation)));
        }

        /// <summary>
        /// Gets the angle between the two segments at specified vertex.
        /// </summary>.
        /// <param name="index">The vertex index.</param>
        /// <returns>The angle expressed in radians.</returns>
        public double GetAngleAt(int index)
        {
            const double pi = 3.141592653589793;
            var ang =
                this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                    this[index].GetVectorTo(this[(index + 2) % 3]));
            if (ang > pi * 2)
                return pi * 2 - ang;
            return ang;
        }

        /// <summary>
        /// Gets the segment at specified index.
        /// </summary>
        /// <param name="index">The segment index.</param>
        /// <returns>The segment 3d.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if index is less than 0 or more than 2.</exception>
        [NotNull]
        public LineSegment2d GetSegmentAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment2d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Gets the intersection points between the triangle and the line.
        /// </summary>
        /// <param name="le2d">The line with which intersections are searched.</param>
        /// <returns>The intersection points list (an empty list if none).</returns>
        [NotNull]
        public List<Point2d> IntersectWith(LinearEntity2d le2d)
        {
            var result = new List<Point2d>();
            for (var i = 0; i < 3; i++)
            {
                var inters = le2d.IntersectWith(GetSegmentAt(i));
                if (inters != null && inters.Length != 0 && !result.Contains(inters[0]))
                    result.Add(inters[0]);
            }

            return result;
        }

        /// <summary>
        /// Gets the intersection points between the triangle and the line.
        /// </summary>
        /// <param name="le2d">The line with which intersections are searched.</param>
        /// <param name="tol">The tolerance used in comparisons.</param>
        /// <returns>The intersection points list (an empty list if none).</returns>
        [NotNull]
        public List<Point2d> IntersectWith(LinearEntity2d le2d, Tolerance tol)
        {
            var result = new List<Point2d>();
            for (var i = 0; i < 3; i++)
            {
                var inters = le2d.IntersectWith(GetSegmentAt(i), tol);
                if (inters != null && inters.Length != 0 && !result.Contains(inters[0]))
                    result.Add(inters[0]);
            }

            return result;
        }

        /// <summary>
        /// Checks if the distance between every respective Point2d in both Triangle2d is less than or equal to the Tolerance.Global.EqualPoint value.
        /// </summary>
        /// <param name="t2d">The triangle2d to compare.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo([NotNull] Triangle2d t2d)
        {
            return IsEqualTo(t2d, Tolerance.Global);
        }

        /// <summary>
        /// Checks if the distance between every respective Point2d in both Triangle2d is less than or equal to the Tolerance.EqualPoint value of the specified tolerance.
        /// </summary>
        /// <param name="t2d">The triangle2d to compare.</param>
        /// <param name="tol">The tolerance used in points comparisons.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo([NotNull] Triangle2d t2d, Tolerance tol)
        {
            return t2d[0].IsEqualTo(_pt0, tol) && t2d[1].IsEqualTo(_pt1, tol) && t2d[2].IsEqualTo(_pt2, tol);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is strictly inside the triangle.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is inside; otherwise, false.</returns>
        public bool IsPointInside(Point2d pt)
        {
            if (IsPointOn(pt))
                return false;
            var inters = IntersectWith(new Ray2d(pt, Vector2d.XAxis));
            if (inters.Count != 1)
                return false;
            var p = inters[0];
            return !p.IsEqualTo(this[0]) && !p.IsEqualTo(this[1]) && !p.IsEqualTo(this[2]);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is on a triangle segment.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>Ttrue if the point is on a segment; otherwise, false.</returns>
        public bool IsPointOn(Point2d pt)
        {
            return
                pt.IsEqualTo(this[0]) ||
                pt.IsEqualTo(this[1]) ||
                pt.IsEqualTo(this[2]) ||
                pt.IsBetween(this[0], this[1]) ||
                pt.IsBetween(this[1], this[2]) ||
                pt.IsBetween(this[2], this[0]);
        }

        /// <summary>
        /// Sets the elements of the triangle using an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public void Set(Point2d org, Vector2d v1, Vector2d v2)
        {
            _pts[0] = _pt0 = org;
            _pts[1] = _pt1 = org + v1;
            _pts[2] = _pt2 = org + v2;
        }

        /// <summary>
        /// Transforms a Triangle2d with a transformation matrix
        /// </summary>
        /// <param name="mat">The 2d transformation matrix.</param>
        /// <returns>The new Triangle2d.</returns>
        [NotNull]
        public Triangle2d TransformBy(Matrix2d mat)
        {
            return new Triangle2d(Array.ConvertAll(
                _pts, p => p.TransformBy(mat)));
        }
    }
}