namespace AcadLib.Geometry
{
    using System;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a Polyline segment.
    /// </summary>
    [PublicAPI]
    public class PolylineSegment
    {
        private Point2d _endPoint;
        private Point2d _startPoint;

        /// <summary>
        /// Creates a new instance of PolylineSegment from two points.
        /// </summary>
        /// <param name="startPoint">The start point of the segment.</param>
        /// <param name="endPoint">The end point of the segment.</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            Bulge = 0.0;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment from two points and a bulge.
        /// </summary>
        /// <param name="startPoint">The start point of the segment.</param>
        /// <param name="endPoint">The end point of the segment.</param>
        /// <param name="bulge">The bulge of the segment.</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint, double bulge)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            Bulge = bulge;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment from two points, a bulge and a constant width.
        /// </summary>
        /// <param name="startPoint">The start point of the segment.</param>
        /// <param name="endPoint">The end point of the segment.</param>
        /// <param name="bulge">The bulge of the segment.</param>
        /// <param name="constantWidth">The constant width of the segment.</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint, double bulge, double constantWidth)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            Bulge = bulge;
            StartWidth = constantWidth;
            EndWidth = constantWidth;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment from two points, a bulge, a start width and an end width.
        /// </summary>
        /// <param name="startPoint">The start point of the segment.</param>
        /// <param name="endPoint">The end point of the segment.</param>
        /// <param name="bulge">The bulge of the segment.</param>
        /// <param name="startWidth">The start width of the segment.</param>
        /// <param name="endWidth">The end width of the segment.</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint, double bulge, double startWidth, double endWidth)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            Bulge = bulge;
            StartWidth = startWidth;
            EndWidth = endWidth;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment from a LineSegment2d
        /// </summary>
        /// <param name="line">A LineSegment2d instance.</param>
        public PolylineSegment([NotNull] LineSegment2d line)
        {
            _startPoint = line.StartPoint;
            _endPoint = line.EndPoint;
            Bulge = 0.0;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment from a CircularArc2d
        /// </summary>
        /// <param name="arc">A CircularArc2d instance.</param>
        public PolylineSegment([NotNull] CircularArc2d arc)
        {
            _startPoint = arc.StartPoint;
            _endPoint = arc.EndPoint;
            Bulge = Math.Tan((arc.EndAngle - arc.StartAngle) / 4.0);
            if (arc.IsClockWise)
                Bulge = -Bulge;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        /// <summary>
        /// Gets or sets the segment bulge.
        /// </summary>
        public double Bulge { get; set; }

        /// <summary>
        /// Gets or sets the segment end point.
        /// </summary>
        public Point2d EndPoint
        {
            get => _endPoint;
            set => _endPoint = value;
        }

        /// <summary>
        /// Gets or sets the segment end width.
        /// </summary>
        public double EndWidth { get; set; }

        /// <summary>
        /// Gets true if the segment is linear.
        /// </summary>
        public bool IsLinear => Math.Abs(Bulge) < 0.0001;

        /// <summary>
        /// Gets or sets the segment start point.
        /// </summary>
        public Point2d StartPoint
        {
            get => _startPoint;
            set => _startPoint = value;
        }

        /// <summary>
        /// Gets or sets the segment start width.
        /// </summary>
        public double StartWidth { get; set; }

        /// <summary>
        /// Returns a copy of the PolylineSegment
        /// </summary>
        /// <returns>A new PolylineSegment instance which is a copy of the instance this method applies to.</returns>
        [NotNull]
        public PolylineSegment Clone()
        {
            return new PolylineSegment(StartPoint, EndPoint, Bulge, StartWidth, EndWidth);
        }

        /// <summary>
        /// Determines whether the specified PolylineSegment instances are considered equal.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if the objects are considered equal; otherwise, nil.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is PolylineSegment seg))
                return false;
            if (seg.GetHashCode() != GetHashCode())
                return false;
            if (!_startPoint.IsEqualTo(seg.StartPoint))
                return false;
            if (!_endPoint.IsEqualTo(seg.EndPoint))
                return false;
            if (Math.Abs(Bulge - seg.Bulge) > 0.0001)
                return false;
            if (Math.Abs(StartWidth - seg.StartWidth) > 0.0001)
                return false;
            return !(Math.Abs(EndWidth - seg.EndWidth) > 0.0001);
        }

        /// <summary>
        /// Serves as a hash function for the PolylineSegment type.
        /// </summary>
        /// <returns>A hash code for the current PolylineSegemnt.</returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _startPoint.GetHashCode() ^

                   // ReSharper disable once NonReadonlyMemberInGetHashCode
                   _endPoint.GetHashCode() ^

                   // ReSharper disable once NonReadonlyMemberInGetHashCode
                   Bulge.GetHashCode() ^

                   // ReSharper disable once NonReadonlyMemberInGetHashCode
                   StartWidth.GetHashCode() ^

                   // ReSharper disable once NonReadonlyMemberInGetHashCode
                   EndWidth.GetHashCode();
        }

        /// <summary>
        /// Returns the parameter value of point.
        /// </summary>
        /// <param name="pt">The Point 2d whose get the PolylineSegment parameter at.</param>
        /// <returns>A double between 0.0 and 1.0, or -1.0 if the point does not lie on the segment.</returns>
        public double GetParameterOf(Point2d pt)
        {
            if (IsLinear)
            {
                var line = ToLineSegment();
                return line != null && line.IsOn(pt) ? _startPoint.GetDistanceTo(pt) / line.Length : -1.0;
            }

            var arc = ToCircularArc();
            return arc != null && arc.IsOn(pt)
                ? arc.GetLength(arc.GetParameterOf(_startPoint), arc.GetParameterOf(pt)) /
                  arc.GetLength(arc.GetParameterOf(_startPoint), arc.GetParameterOf(_endPoint))
                : -1.0;
        }

        /// <summary>
        /// Inverses the segment.
        /// </summary>
        public void Inverse()
        {
            var tmpPoint = StartPoint;
            var tmpWidth = StartWidth;
            _startPoint = EndPoint;
            _endPoint = tmpPoint;
            Bulge = -Bulge;
            StartWidth = EndWidth;
            EndWidth = tmpWidth;
        }

        /// <summary>
        /// Converts the PolylineSegment into a CircularArc2d.
        /// </summary>
        /// <returns>A new CircularArc2d instance or null if the PolylineSegment bulge is equal to 0.0.</returns>
        [CanBeNull]
        public CircularArc2d ToCircularArc()
        {
            return IsLinear ? null : new CircularArc2d(_startPoint, _endPoint, Bulge, false);
        }

        /// <summary>
        /// Converts the PolylineSegment into a Curve2d.
        /// </summary>
        /// <returns>A new Curve2d instance.</returns>
        [NotNull]
        public Curve2d ToCurve2d()
        {
            return IsLinear
                ? new LineSegment2d(_startPoint, _endPoint) as Curve2d
                : new CircularArc2d(_startPoint, _endPoint, Bulge, false);
        }

        /// <summary>
        /// Converts the PolylineSegment into a LineSegment2d.
        /// </summary>
        /// <returns>A new LineSegment2d instance or null if the PolylineSegment bulge is not equal to 0.0.</returns>
        [CanBeNull]
        public LineSegment2d ToLineSegment()
        {
            return IsLinear ? new LineSegment2d(_startPoint, _endPoint) : null;
        }

        /// <summary>
        /// Applies ToString() to each property and concatenate the results separted with commas.
        /// </summary>
        /// <returns>A string containing the current PolylineSegemnt properties separated with commas.</returns>
        public override string ToString()
        {
            return $"{_startPoint.ToString()}, {_endPoint.ToString()}, {Bulge}, {StartWidth}, {EndWidth}";
        }
    }
}