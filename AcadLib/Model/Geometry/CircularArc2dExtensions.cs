using NetLib;

namespace AcadLib.Geometry
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the CircularArc2dType
    /// </summary>
    public static class CircularArc2dExtensions
    {
        /// <summary>
        /// Gets the algebraic (signed) area of the circular arc.
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The algebraic area.</returns>
        public static double AlgebricArea([NotNull] this CircularArc2d arc)
        {
            var rad = arc.Radius;
            var ang = arc.IsClockWise ? arc.StartAngle - arc.EndAngle : arc.EndAngle - arc.StartAngle;
            return rad * rad * (ang - Math.Sin(ang)) / 2.0;
        }

        /// <summary>
        /// Gets the centroid of the circular arc.
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The centroid of the arc.</returns>
        public static Point2d Centroid([NotNull] this CircularArc2d arc)
        {
            var start = arc.StartPoint;
            var end = arc.EndPoint;
            var area = arc.AlgebricArea();
            var chord = start.GetDistanceTo(end);
            var angle = (end - start).Angle;
            return arc.Center.Polar(angle - Math.PI / 2.0, chord * chord * chord / (12.0 * area));
        }

        [NotNull]
        public static Circle CreateCircle([NotNull] this CircularArc2d arc)
        {
            return new Circle(arc.Center.Convert3d(), Vector3d.ZAxis, arc.Radius);
        }

        /// <summary>
        /// Returns the tangents between the active CircularArc2d instance complete circle and a point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument.
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center
        /// to the point before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="pt">The Point2d to which tangents are searched</param>
        /// <returns>An array of LineSegement2d representing the tangents (2) or null if there is none.</returns>
        [CanBeNull]
        public static LineSegment2d[] GetTangentsTo([NotNull] this CircularArc2d arc, Point2d pt)
        {
            // check if the point is inside the circle
            var center = arc.Center;
            if (pt.GetDistanceTo(center) <= arc.Radius)
                return null;

            var vec = center.GetVectorTo(pt) / 2.0;
            var tmp = new CircularArc2d(center + vec, vec.Length);
            var inters = arc.IntersectWith(tmp);
            if (inters == null)
                return null;
            var result = new LineSegment2d[2];
            var v1 = inters[0] - center;
            var i = vec.X * v1.Y - vec.Y - v1.X > 0 ? 0 : 1;
            var j = i ^ 1;
            result[i] = new LineSegment2d(inters[0], pt);
            result[j] = new LineSegment2d(inters[1], pt);
            return result;
        }

        /// <summary>
        /// Returns the tangents between the active CircularArc2d instance complete circle and another one.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument.
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both,
        /// the tangent on the left side of the line from this circular arc center to the other one before the other one.
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="other">The CircularArc2d to which searched for tangents.</param>
        /// <param name="flags">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment2d representing the tangents (maybe 2 or 4) or null if there is none.</returns>
        [CanBeNull]
        public static LineSegment2d[] GetTangentsTo(
            [NotNull] this CircularArc2d arc,
            [NotNull] CircularArc2d other,
            TangentType flags)
        {
            // check if a circle is inside the other
            var dist = arc.Center.GetDistanceTo(other.Center);
            if (dist - Math.Abs(arc.Radius - other.Radius) <= Tolerance.Global.EqualPoint)
                return null;

            // check if circles overlap
            var overlap = arc.Radius + other.Radius >= dist;
            if (overlap && flags == TangentType.Inner)
                return null;

            CircularArc2d tmp1;
            Point2d[] inters;
            Vector2d vec1, vec2, vec = other.Center - arc.Center;
            int i, j;
            var result = new LineSegment2d[(int)flags == 3 && !overlap ? 4 : 2];

            // outer tangents
            if ((flags & TangentType.Outer) > 0)
            {
                if (Math.Abs(arc.Radius - other.Radius) < 0.001)
                {
                    var perp = new Line2d(arc.Center, vec.GetPerpendicularVector());
                    inters = arc.IntersectWith(perp);
                    if (inters == null)
                        return null;
                    vec1 = (inters[0] - arc.Center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(inters[0], inters[0] + vec);
                    result[j] = new LineSegment2d(inters[1], inters[1] + vec);
                }
                else
                {
                    var center = arc.Radius < other.Radius ? other.Center : arc.Center;
                    tmp1 = new CircularArc2d(center, Math.Abs(arc.Radius - other.Radius));
                    var tmp2 = new CircularArc2d(arc.Center + vec / 2.0, dist / 2.0);
                    inters = tmp1.IntersectWith(tmp2);
                    if (inters == null)
                        return null;
                    vec1 = (inters[0] - center).GetNormal();
                    vec2 = (inters[1] - center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius, other.Center + vec1 * other.Radius);
                    result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius, other.Center + vec2 * other.Radius);
                }
            }

            // inner tangents
            if ((flags & TangentType.Inner) > 0 && !overlap)
            {
                var ratio = arc.Radius / (arc.Radius + other.Radius) / 2.0;
                tmp1 = new CircularArc2d(arc.Center + vec * ratio, dist * ratio);
                inters = arc.IntersectWith(tmp1);
                if (inters == null)
                    return null;
                vec1 = (inters[0] - arc.Center).GetNormal();
                vec2 = (inters[1] - arc.Center).GetNormal();
                i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 2 : 3;
                j = i == 2 ? 3 : 2;
                result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius, other.Center + vec1.Negate() * other.Radius);
                result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius, other.Center + vec2.Negate() * other.Radius);
            }

            return result;
        }

        public static bool IsCircle([NotNull] this CircularArc2d arc)
        {
            return Math.Abs(Math.Abs(arc.EndAngle - arc.StartAngle) - MathExt.PI2) < 0.00001;
        }
    }
}