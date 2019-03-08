namespace Autodesk.AutoCAD.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Point2d type.
    /// </summary>
    [PublicAPI]
    public static class Point2dExtensions
    {
        public static Point2d Move(this Point2d pt, double x, double y)
        {
            return pt + new Vector2d(x, y);
        }

        public static Point2d Center(this Point2d pt, Point2d other)
        {
            return new Point2d(
                pt.X + (other.X - pt.X) * 0.5,
                pt.Y + (other.Y - pt.Y) * 0.5);
        }

        /// <summary>
        /// Converts a 2d point into a 3d point with Z coodinate equal to 0.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <returns>The corresponding 3d point.</returns>
        public static Point3d Convert3d(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0.0);
        }

        /// <summary>
        /// Converts a 2d point into a 3d point according to the specified plane.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="plane">The plane which the point lies on.</param>
        /// <returns>The corresponding 3d point</returns>
        public static Point3d Convert3d(this Point2d pt, Plane plane)
        {
            return new Point3d(pt.X, pt.Y, 0.0).TransformBy(Matrix3d.PlaneToWorld(plane));
        }

        /// <summary>
        /// Converts a 2d point into a 3d point according to the plane defined by
        /// the specified normal vector and elevation.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="normal">The normal vector of the plane which the point lies on.</param>
        /// <param name="elevation">The elevation of the plane which the point lies on.</param>
        /// <returns>The corresponding 3d point</returns>
        public static Point3d Convert3d(this Point2d pt, Vector3d normal, double elevation)
        {
            return new Point3d(pt.X, pt.Y, elevation).TransformBy(Matrix3d.PlaneToWorld(normal));
        }

        /// <summary>
        /// Отсеивание одинаковых точек
        /// </summary>
        /// <param name="points"></param>
        [NotNull]
        public static List<Point2d> DistinctPoints([NotNull] this List<Point2d> points)
        {
            // Отсеивание одинаковых точек
            return points.Distinct(new AcadLib.Comparers.Point2dEqualityComparer()).ToList();
        }

        /// <summary>
        /// Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="pt">The point 2d to project.</param>
        /// <param name="normal">The normal vector of the entity which owns the point 2d.</param>
        /// <returns>The transformed Point2d.</returns>
        public static Point2d Flatten(this Point2d pt, Vector3d normal)
        {
            return new Point3d(pt.X, pt.Y, 0.0)
                .TransformBy(Matrix3d.PlaneToWorld(normal))
                .Convert2d(new Plane());
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is on the segment defined by two points.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="p1">The segment start point.</param>
        /// <param name="p2">The segment end point.</param>
        /// <returns>true if the point is on the segment; otherwise, false.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2)
        {
            return p1.GetVectorTo(pt).GetNormal().Equals(pt.GetVectorTo(p2).GetNormal());
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is on the segment defined by two points.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="p1">The segment start point.</param>
        /// <param name="p2">The segment end point.</param>
        /// <param name="tol">The tolerance used in comparisons.</param>
        /// <returns>true if the point is on the segment; otherwise, false.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2, Tolerance tol)
        {
            return p1.GetVectorTo(pt).GetNormal(tol).Equals(pt.GetVectorTo(p2).GetNormal(tol));
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is inside the extents.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="extents">The extents 2d supposed to contain the point.</param>
        /// <returns>true if the point is inside the extents; otherwise, false.</returns>
        public static bool IsInside(this Point2d pt, Extents2d extents)
        {
            return
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y;
        }

        /// <summary>
        /// Defines a point with polar coordinates from an origin point.
        /// </summary>
        /// <param name="org">The instance to which the method applies.</param>
        /// <param name="angle">The angle about the X axis.</param>
        /// <param name="distance">The distance from the origin</param>
        /// <returns>The new 2d point.</returns>
        public static Point2d Polar(this Point2d org, double angle, double distance)
        {
            return new Point2d(
                org.X + distance * Math.Cos(angle),
                org.Y + distance * Math.Sin(angle));
        }

        [NotNull]
        public static string ToStringEx(this Point2d pt)
        {
            return pt.ToString("0.00", CultureInfo.CurrentCulture);
        }
    }
}