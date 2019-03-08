namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class CurveExt
    {
        /// <summary>
        /// Точка в центре, но внутри полилинии
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public static Point3d CentroidIn(this Curve curve)
        {
            var center = curve.Centroid();
            if (curve.IsPointInsidePolylineByRay(center, Tolerance.Global)) return center;
            var ptClosest = curve.GetClosestPointTo(center, Vector3d.ZAxis, false);
            using (var ray = new Ray())
            using (var plane = new Plane())
            {
                ray.BasePoint = center;
                ray.SecondPoint = ptClosest;
                var ptsIntersects = new Point3dCollection();
                curve.IntersectWith(ray, Intersect.OnBothOperands, plane, ptsIntersects, IntPtr.Zero, IntPtr.Zero);
                if (ptsIntersects.Count > 1)
                {
                    return ptsIntersects[0].Center(ptsIntersects[1]);
                }
            }

            return center;
        }

        public static Point3d Centroid([NotNull] this Curve c)
        {
            var pts = c.GetGeCurve().GetSamplePoints(10).Select(s => s.Point).ToList();
            return new Point3d(pts.Average(a => a.X), pts.Average(a => a.Y), 0);
        }

        [Obsolete("Использую GetParameterAtPointTry с допусом.")]
        public static double GetParameterAtPointTry([NotNull] this Curve c, Point3d pt, bool extend = false)
        {
            try
            {
                return c.GetParameterAtPoint(pt);
            }
            catch
            {
                var ptCorrect = c.GetClosestPointTo(pt, Vector3d.ZAxis, extend);
                return c.GetParameterAtPoint(ptCorrect);
            }
        }

        public static double? GetParameterAtPointTry([NotNull] this Curve c, Point3d pt, double delta, bool extend = false)
        {
            try
            {
                return c.GetParameterAtPoint(pt);
            }
            catch
            {
                var ptCorrect = c.GetClosestPointTo(pt, Vector3d.ZAxis, extend);
                if ((ptCorrect - pt).Length <= delta)
                    return c.GetParameterAtPoint(ptCorrect);
            }

            return null;
        }

        [NotNull]
        public static List<Point3d> GetPoints([NotNull] this Curve curve)
        {
            if (curve is Polyline pl)
                return pl.GetPoints().Select(s => s.Convert3d()).ToList();
            var pts = new List<Point3d>();
            for (var i = curve.StartParam; i <= curve.EndParam; i += 0.1)
            {
                var pt = curve.GetPointAtParameter(i);
                pts.Add(pt);
            }

            return pts;
        }

        public static bool IsPointInsidePolylineByRay(this Curve c, Point3d pt, Tolerance tolerance)
        {
            using (var ray = new Ray { BasePoint = pt, UnitDir = Vector3d.YAxis })
            {
                return IsPointInsidePolylineByRay(ray, pt, c, tolerance);
            }
        }

        public static bool IsPointInsidePolylineByRay([NotNull] Ray ray, Point3d pt, Curve c, Tolerance tolerance)
        {
            var pts = new Point3dCollection();
            ray.IntersectWith(c, Intersect.OnBothOperands, new Plane(), pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count > 0 && pts.Cast<Point3d>().All(p => c.IsVertex(p, tolerance.EqualPoint)))
            {
                // Повернуть луч и повторить
                ray.TransformBy(Matrix3d.Rotation(5d.ToRadians(), Vector3d.ZAxis, ray.BasePoint));
                return IsPointInsidePolylineByRay(ray, pt, c, tolerance);
            }

            return pts.Count.IsOdd() || IsPointOnPolyline(c, pt, tolerance);
        }

        public static bool IsPointOnPolyline([NotNull] this Curve c, Point3d pt, Tolerance tolerance)
        {
            var ptPl = c.GetClosestPointTo(pt, Vector3d.ZAxis, false);
            return pt.IsEqualTo(ptPl, tolerance);
        }

        public static bool IsVertex([NotNull] this Curve c, Point3d pt, double tolerance = 0.0001)
        {
            return c.GetParameterAtPointTry(pt).IsWholeNumber(tolerance);
        }
    }
}
