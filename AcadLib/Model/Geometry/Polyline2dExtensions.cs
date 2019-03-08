namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using AcRx = Autodesk.AutoCAD.Runtime;

    /// <summary>
    /// Provides extension methods for the Polyline2d type.
    /// </summary>
    [PublicAPI]
    public static class Polyline2dExtensions
    {
        /// <summary>
        /// Gets the centroid of the polyline 2d.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The centroid of the polyline 2d (WCS coordinates).</returns>
        public static Point3d Centroid([NotNull] this Polyline2d pl)
        {
            var vertices = pl.GetVertices().ToArray();
            var last = vertices.Length - 1;
            var vertex = vertices[0];
            var p0 = vertex.Position.Convert2d();
            var cen = new Point2d(0.0, 0.0);
            var area = 0.0;
            var bulge = vertex.Bulge;
            double tmpArea;
            Point2d tmpPt;
            var tri = new Triangle2d();
            CircularArc2d arc;
            if (Math.Abs(bulge) > 0.0001)
            {
                arc = pl.GetArcSegment2dAt(0);
                tmpArea = arc.AlgebricArea();
                tmpPt = arc.Centroid();
                area += tmpArea;
                cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }

            for (var i = 1; i < last; i++)
            {
                var p1 = vertices[i].Position.Convert2d();
                var p2 = vertices[i + 1].Position.Convert2d();
                tri.Set(p0, p1, p2);
                tmpArea = tri.AlgebricArea;
                area += tmpArea;
                cen += (tri.Centroid * tmpArea).GetAsVector();
                bulge = vertices[i].Bulge;
                if (Math.Abs(bulge) > 0.0001)
                {
                    arc = pl.GetArcSegment2dAt(i);
                    tmpArea = arc.AlgebricArea();
                    tmpPt = arc.Centroid();
                    area += tmpArea;
                    cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
                }
            }

            bulge = vertices[last].Bulge;
            if (Math.Abs(bulge) > 0.0001 && pl.Closed)
            {
                arc = pl.GetArcSegment2dAt(last);
                tmpArea = arc.AlgebricArea();
                tmpPt = arc.Centroid();
                area += tmpArea;
                cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }

            cen = cen.DivideBy(area);
            return new Point3d(cen.X, cen.Y, pl.Elevation).TransformBy(Matrix3d.PlaneToWorld(pl.Normal));
        }

        /// <summary>
        /// Gets the arc 2d segment of the polyline 2d at specified index.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>A copy of the segment (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown the index is out of range.</exception>
        [NotNull]
        public static CircularArc2d GetArcSegment2dAt([NotNull] this Polyline2d pl, int index)
        {
            try
            {
                var WCS2ECS = pl.Ecs.Inverse();
                return new CircularArc2d(
                    pl.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 0.5).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the arc 3d segment of the polyline 2d at specified index.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>A copy of the segment (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown the index is out of range.</exception>
        [NotNull]
        public static CircularArc3d GetArcSegmentAt([NotNull] this Polyline2d pl, int index)
        {
            try
            {
                return new CircularArc3d(
                    pl.GetPointAtParameter(index),
                    pl.GetPointAtParameter(index + 0.5),
                    pl.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the linear 2d segment of the polyline 2d at specified index.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>A copy of the segment (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown the index is out of range.</exception>
        [NotNull]
        public static LineSegment2d GetLineSegment2dAt([NotNull] this Polyline2d pl, int index)
        {
            try
            {
                var WCS2ECS = pl.Ecs.Inverse();
                return new LineSegment2d(
                    pl.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the linear 3d segment of the polyline 2d at specified index.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>A copy of the segment (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException is thrown the index is out of range.</exception>
        [NotNull]
        public static LineSegment3d GetLineSegmentAt([NotNull] this Polyline2d pl, int index)
        {
            try
            {
                return new LineSegment3d(
                    pl.GetPointAtParameter(index),
                    pl.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline2d along the given plane.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <returns>The projected polyline</returns>
        [CanBeNull]
        public static Polyline GetOrthoProjectedPolyline(this Polyline2d pline, [NotNull] Plane plane)
        {
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline2d parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        [CanBeNull]
        public static Polyline GetProjectedPolyline(this Polyline2d pline, [NotNull] Plane plane, Vector3d direction)
        {
            var tol = new Tolerance(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (pline.Normal.IsPerpendicularTo(direction, tol))
            {
                var dirPlane = new Plane(Point3d.Origin, direction);
                if (!pline.IsWriteEnabled) pline = pline.UpgradeOpenTr();
                pline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
                var extents = pline.GeometricExtents;
                pline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
                return GeomExt.ProjectExtents(extents, plane, direction, dirPlane);
            }

            return GeomExt.ProjectPolyline(pline, plane, direction);
        }

        public static Extents2d GetRectangleFromCenter(this Point2d center, double side)
        {
            var hs = side * 0.5;
            return new Extents2d(new Point2d(center.X - hs, center.Y - hs),
                new Point2d(center.X + hs, center.Y + hs));
        }

        /// <summary>
        /// Gets the vertices list of the polyline 2d.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The vertices list.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNoActiveTransactions is thrown if the method is not called form a Transaction.</exception>
        [NotNull]
        public static List<Vertex2d> GetVertices([NotNull] this Polyline2d pl)
        {
            var tr = pl.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new AcRx.Exception(AcRx.ErrorStatus.NoActiveTransactions);

            var vertices = new List<Vertex2d>();
            foreach (ObjectId id in pl)
            {
                var vx = (Vertex2d)tr.GetObject(id, OpenMode.ForRead);
                if (vx.VertexType != Vertex2dType.SplineControlVertex)
                    vertices.Add(vx);
            }

            return vertices;
        }
    }
}
