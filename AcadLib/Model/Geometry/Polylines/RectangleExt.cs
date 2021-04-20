namespace AcadLib.Geometry.Polylines
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public static class RectangleExt
    {
        public static Polyline? CreateRectangle(this Point3d pt, double length, double width, CellAlignment alignment,
            Vector3d dir)
        {
            return CreateRectangle(pt.Convert2d(), length, width, alignment, dir.Convert2d());
        }

        public static Polyline? CreateRectangle(this Point2d pt, double length, double width, CellAlignment alignment, Vector2d dir)
        {
            if (length < 0 || width < 0)
                return null;
            var pl = PlRel.Start(pt).Add(0, width).Add(length, 0).Add(0, -width).Create();
            Vector3d vec = default;
            switch (alignment)
            {
                case CellAlignment.BottomLeft:
                    vec = new Vector3d();
                    break;
                case CellAlignment.TopLeft:
                    vec = new Vector3d(0, -width, 0);
                    break;
                case CellAlignment.TopCenter:
                    vec = new Vector3d(-length * 0.5, -width, 0);
                    break;
                case CellAlignment.TopRight:
                    vec = new Vector3d(-length, -width, 0);
                    break;
                case CellAlignment.MiddleLeft:
                    vec = new Vector3d(0, -width * 0.5, 0);
                    break;
                case CellAlignment.MiddleCenter:
                    vec = new Vector3d(-length * 0.5, -width * 0.5, 0);
                    break;
                case CellAlignment.MiddleRight:
                    vec = new Vector3d(-length, -width * 0.5, 0);
                    break;
                case CellAlignment.BottomCenter:
                    vec = new Vector3d(-length * 0.5, 0, 0);
                    break;
                case CellAlignment.BottomRight:
                    vec = new Vector3d(-length, 0, 0);
                    break;
            }

            var move = Matrix3d.Displacement(vec);
            var rotate = Matrix3d.Rotation(dir.Angle, Vector3d.ZAxis, pt.Convert3d() + vec);
            var matrix = move.PostMultiplyBy(rotate);
            pl.TransformBy(matrix);
            return pl;
        }
    }
}