namespace AcadLib.Geometry
{
    using System;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a triangle in the 3d space. It can be viewed as a structure consisting of three Point3d.
    /// </summary>
    public class Triangle3d : Triangle<Point3d>
    {
        /// <summary>
        /// Initializes a new instance of Triangle3d; that is empty.
        /// </summary>
        public Triangle3d()
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle3d that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The Point3d array whose elements are copied to the new Triangle3d.</param>
        public Triangle3d([NotNull] Point3d[] pts) : base(pts)
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle3d that contains the specified elements.
        /// </summary>
        /// <param name="a">The first vertex of the new Triangle3d (origin).</param>
        /// <param name="b">The second vertex of the new Triangle3d (2nd vertex).</param>
        /// <param name="c">The third vertex of the new Triangle3d (3rd vertex).</param>
        public Triangle3d(Point3d a, Point3d b, Point3d c) : base(a, b, c)
        {
        }

        /// <summary>
        /// Initializes a new instance of Triangle3d according to an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public Triangle3d(Point3d org, Vector3d v1, Vector3d v2)
        {
            _pts[0] = _pt0 = org;
            _pts[0] = _pt1 = org + v1;
            _pts[0] = _pt2 = org + v2;
        }

        /// <summary>
        /// Gets the triangle area.
        /// </summary>
        public double Area => Math.Abs(
            ((_pt1.X - _pt0.X) * (_pt2.Y - _pt0.Y) -
             (_pt2.X - _pt0.X) * (_pt1.Y - _pt0.Y)) / 2.0);

        /// <summary>
        /// Gets the triangle centroid.
        /// </summary>
        public Point3d Centroid => (_pt0 + _pt1.GetAsVector() + _pt2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        [CanBeNull]
        public CircularArc3d CircumscribedCircle
        {
            get
            {
                var ca2d = Convert2d().CircumscribedCircle;
                return ca2d == null ? null : new CircularArc3d(ca2d.Center.Convert3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Gets the triangle plane elevation.
        /// </summary>
        public double Elevation => _pt0.TransformBy(Matrix3d.WorldToPlane(Normal)).Z;

        /// <summary>
        /// Gets the unit vector of the triangle plane greatest slope.
        /// </summary>
        public Vector3d GreatestSlope
        {
            get
            {
                var norm = Normal;
                if (norm.IsParallelTo(Vector3d.ZAxis))
                    return new Vector3d(0.0, 0.0, 0.0);
                return Math.Abs(norm.Z) < 0.0001
                    ? Vector3d.ZAxis.Negate()
                    : new Vector3d(-norm.Y, norm.X, 0.0).CrossProduct(norm).GetNormal();
            }
        }

        /// <summary>
        /// Gets the unit horizontal vector of the triangle plane.
        /// </summary>
        public Vector3d Horizontal
        {
            get
            {
                var norm = Normal;
                return norm.IsParallelTo(Vector3d.ZAxis) ? Vector3d.XAxis : new Vector3d(-norm.Y, norm.X, 0.0).GetNormal();
            }
        }

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        [CanBeNull]
        public CircularArc3d InscribedCircle
        {
            get
            {
                var ca2d = Convert2d().InscribedCircle;
                return ca2d == null ? null : new CircularArc3d(ca2d.Center.Convert3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the triangle plane is horizontal.
        /// </summary>
        public bool IsHorizontal => Math.Abs(_pt0.Z - _pt1.Z) < 0.0001 && Math.Abs(_pt0.Z - _pt2.Z) < 0.0001;

        /// <summary>
        /// Gets the normal vector of the triangle plane.
        /// </summary>
        public Vector3d Normal => (_pt1 - _pt0).CrossProduct(_pt2 - _pt0).GetNormal();

        /// <summary>
        /// Gets the percent slope of the triangle plane.
        /// </summary>
        public double SlopePerCent
        {
            get
            {
                var norm = Normal;
                return Math.Abs(norm.Z) < 0.0001
                    ? double.PositiveInfinity
                    : Math.Abs(100.0 * Math.Sqrt(Math.Pow(norm.X, 2.0) + Math.Pow(norm.Y, 2.0)) / norm.Z);
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
                var origin = Centroid;
                var zaxis = Normal;
                var xaxis = Horizontal;
                var yaxis = zaxis.CrossProduct(xaxis).GetNormal();
                return new Matrix3d(new[]
                {
                    xaxis.X,
                    yaxis.X,
                    zaxis.X,
                    origin.X,
                    xaxis.Y,
                    yaxis.Y,
                    zaxis.Y,
                    origin.Y,
                    xaxis.Z,
                    yaxis.Z,
                    zaxis.Z,
                    origin.Z,
                    0.0,
                    0.0,
                    0.0,
                    1.0
                });
            }
        }

        /// <summary>
        /// Converts a Triangle3d into a Triangle2d according to the Triangle3d plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        [NotNull]
        public Triangle2d Convert2d()
        {
            var plane = GetPlane();
            return new Triangle2d(
                Array.ConvertAll(_pts, x => x.Convert2d(plane)));
        }

        /// <summary>
        /// Projects a Triangle3d on the WCS XY plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        [NotNull]
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
        [NotNull]
        public BoundedPlane GetBoundedPlane()
        {
            return new BoundedPlane(this[0], this[1], this[2]);
        }

        /// <summary>
        /// Gets the unbounded plane defined by the triangle.
        /// </summary>
        /// <returns>The unbouned plane.</returns>
        [NotNull]
        public Plane GetPlane()
        {
            var normal = Normal;
            var origin =
                new Point3d(0.0, 0.0, Elevation).TransformBy(Matrix3d.PlaneToWorld(normal));
            return new Plane(origin, normal);
        }

        /// <summary>
        /// Gets the segment at specified index.
        /// </summary>
        /// <param name="index">The segment index.</param>
        /// <returns>The segment 3d</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is throw if index is less than 0 or more than 2.</exception>
        [NotNull]
        public LineSegment3d GetSegmentAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment3d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the Tolerance.Global.EqualPoint value.
        /// </summary>
        /// <param name="t3d">The triangle3d to compare.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo([NotNull] Triangle3d t3d)
        {
            return IsEqualTo(t3d, Tolerance.Global);
        }

        /// <summary>
        /// Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the Tolerance.EqualPoint value of the specified tolerance.
        /// </summary>
        /// <param name="t3d">The triangle3d to compare.</param>
        /// <param name="tol">The tolerance used in points comparisons.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo([NotNull] Triangle3d t3d, Tolerance tol)
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
            var tol = new Tolerance(1e-9, 1e-9);
            var v1 = pt.GetVectorTo(_pt0).CrossProduct(pt.GetVectorTo(_pt1)).GetNormal();
            var v2 = pt.GetVectorTo(_pt1).CrossProduct(pt.GetVectorTo(_pt2)).GetNormal();
            var v3 = pt.GetVectorTo(_pt2).CrossProduct(pt.GetVectorTo(_pt0)).GetNormal();
            return v1.IsEqualTo(v2, tol) && v2.IsEqualTo(v3, tol);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is on a triangle segment.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is on a segment; otherwise, false.</returns>
        public bool IsPointOn(Point3d pt)
        {
            var tol = new Tolerance(1e-9, 1e-9);
            var v0 = new Vector3d(0.0, 0.0, 0.0);
            var v1 = pt.GetVectorTo(_pt0).CrossProduct(pt.GetVectorTo(_pt1));
            var v2 = pt.GetVectorTo(_pt1).CrossProduct(pt.GetVectorTo(_pt2));
            var v3 = pt.GetVectorTo(_pt2).CrossProduct(pt.GetVectorTo(_pt0));
            return v1.IsEqualTo(v0, tol) || v2.IsEqualTo(v0, tol) || v3.IsEqualTo(v0, tol);
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
            _pts = new[] { _pt0, _pt1, _pt2 };
        }

        /// <summary>
        /// Transforms a Triangle3d with a transformation matrix
        /// </summary>
        /// <param name="mat">The 3d transformation matrix.</param>
        /// <returns>The new Triangle3d.</returns>
        [NotNull]
        public Triangle3d Transformby(Matrix3d mat)
        {
            return new Triangle3d(Array.ConvertAll(
                _pts, p => p.TransformBy(mat)));
        }
    }
}