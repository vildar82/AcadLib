namespace AcadLib.Geometry
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class LineExt
    {
        public static Point3d Center([NotNull] this Line line)
        {
            return line.StartPoint.Center(line.EndPoint);
        }

        public static LinearEntity2d IsOverlapping(this LineSegment2d line1, LineSegment2d line2, Tolerance tolerance)
        {
            return line1.Overlap(line2, tolerance);
        }

        public static LineSegment2d CreateLine2d(this Point2d pt1, Point2d pt2)
        {
            return new LineSegment2d(pt1, pt2);
        }

        public static LineSegment2d CreateLine2d(this Point3d pt1, Point3d pt2)
        {
            return new LineSegment2d(pt1.Convert2d(), pt2.Convert2d());
        }
    }
}