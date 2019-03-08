namespace AcadLib.Geometry
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the CircularArc2dType
    /// </summary>
    [PublicAPI]
    public static class CircularArc3dExtensions
    {
        [NotNull]
        public static Polyline ConvertToPolyline([NotNull] this CircularArc3d arc)
        {
            var poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y), GetBulge(arc), 0, 0);
            poly.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
            return poly;
        }

        [NotNull]
        public static Polyline ConvertToPolyline([NotNull] this CircularArc2d arc)
        {
            var poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y), GetBulge(arc, arc.IsClockWise), 0, 0);
            poly.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
            return poly;
        }

        /// <summary>
        /// Функция возвращает кривизну дуги (bulge) или 0.0
        /// </summary>
        public static double GetBulge([NotNull] this CircularArc2d arc, bool clockWise = false)
        {
            var bulge = Math.Tan(Math.Abs(arc.StartAngle - arc.EndAngle) * 0.25);
            return clockWise ? -bulge : bulge;
        }

        /// <summary>
        /// Функция возвращает кривизну дуги (bulge) или 0.0
        /// </summary>
        public static double GetBulge([NotNull] this CircularArc3d arc, bool clockWise = false)
        {
            var bulge = Math.Tan(Math.Abs(arc.StartAngle - arc.EndAngle) * 0.25);
            return clockWise ? -bulge : bulge;
        }

        /// <summary>
        /// Returns the tangents between the active CircularArc3d instance complete circle and a point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument.
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center
        /// to the point before the one on the right side.
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="pt">The Point3d to which tangents are searched</param>
        /// <returns>An array of LineSegement3d representing the tangents (2) or null if there is none.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        [CanBeNull]
        public static LineSegment3d[] GetTangentsTo([NotNull] this CircularArc3d arc, Point3d pt)
        {
            // check if arc and point lies on the plane
            var normal = arc.Normal;
            var WCS2OCS = Matrix3d.WorldToPlane(normal);
            var elevation = arc.Center.TransformBy(WCS2OCS).Z;
            if (Math.Abs(elevation - pt.TransformBy(WCS2OCS).Z) < Tolerance.Global.EqualPoint)
                throw new Autodesk.AutoCAD.Runtime.Exception(
                    Autodesk.AutoCAD.Runtime.ErrorStatus.NonCoplanarGeometry);

            var plane = new Plane(Point3d.Origin, normal);
            var ca2d = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            var lines2d = ca2d.GetTangentsTo(pt.Convert2d(plane));

            if (lines2d == null)
                return null;

            var result = new LineSegment3d[lines2d.Length];
            for (var i = 0; i < lines2d.Length; i++)
            {
                var ls2d = lines2d[i];
                result[i] = new LineSegment3d(ls2d.StartPoint.Convert3d(normal, elevation),
                    ls2d.EndPoint.Convert3d(normal, elevation));
            }

            return result;
        }

        /// <summary>
        /// Returns the tangents between the active CircularArc3d instance complete circle and another one.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument.
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both,
        /// the tangent on the left side of the line from this circular arc center to the other one before the one on the right side.
        /// </remarks>
        /// <param name="arc">The instance to which this method applies.</param>
        /// <param name="other">The CircularArc3d to which searched for tangents.</param>
        /// <param name="flags">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment3d representing the tangents (maybe 2 or 4) or null if there is none.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNonCoplanarGeometry is thrown if the objects do not lies on the same plane.</exception>
        [CanBeNull]
        public static LineSegment3d[] GetTangentsTo(
            [NotNull] this CircularArc3d arc,
            [NotNull] CircularArc3d other,
            TangentType flags)
        {
            // check if circles lies on the same plane
            var normal = arc.Normal;
            var WCS2OCS = Matrix3d.WorldToPlane(normal);
            var elevation = arc.Center.TransformBy(WCS2OCS).Z;
            if (!(normal.IsParallelTo(other.Normal) &&
                  Math.Abs(elevation - other.Center.TransformBy(WCS2OCS).Z) < Tolerance.Global.EqualPoint))
                throw new Autodesk.AutoCAD.Runtime.Exception(
                    Autodesk.AutoCAD.Runtime.ErrorStatus.NonCoplanarGeometry);

            var plane = new Plane(Point3d.Origin, normal);
            var ca2d1 = new CircularArc2d(arc.Center.Convert2d(plane), arc.Radius);
            var ca2d2 = new CircularArc2d(other.Center.Convert2d(plane), other.Radius);
            var lines2d = ca2d1.GetTangentsTo(ca2d2, flags);

            if (lines2d == null)
                return null;

            var result = new LineSegment3d[lines2d.Length];
            for (var i = 0; i < lines2d.Length; i++)
            {
                var ls2d = lines2d[i];
                result[i] = new LineSegment3d(ls2d.StartPoint.Convert3d(normal, elevation),
                    ls2d.EndPoint.Convert3d(normal, elevation));
            }

            return result;
        }
    }
}