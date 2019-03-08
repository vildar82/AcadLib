namespace AcadLib.Jigs
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Запрос точки вставки с висящим на курсоре прямоугольником.
    /// Точка вставки - нижний левый угол
    /// </summary>
    [PublicAPI]
    public class RectangleJig : EntityJig
    {
        public RectangleJig(double length, double height) : base(new Polyline())
        {
            var pl = (Polyline)Entity;
            var pt1 = Position.Convert2d();
            pl.AddVertexAt(0, pt1, 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(pt1.X, pt1.Y + height), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(pt1.X + length, pt1.Y + height), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(pt1.X + length, pt1.Y), 0, 0, 0);
            pl.Closed = true;
        }

        public Point3d Position { get; set; }

        protected override SamplerStatus Sampler([NotNull] JigPrompts prompts)
        {
            var res = prompts.AcquirePoint("\nТочка вставки:");
            if (res.Status != PromptStatus.OK)
                throw new OperationCanceledException();
            var status = SamplerStatus.NoChange;
            if (!Position.IsEqualTo(res.Value, Tolerance.Global))
            {
                status = SamplerStatus.OK;
            }

            Position = res.Value; // TransformBy(ed.CurrentUserCoordinateSystem);
            return status;
        }

        protected override bool Update()
        {
            try
            {
                var pl = (Polyline)Entity;
                var plPt1 = pl.GetPoint2dAt(0).Convert3d();
                var vec = Position - plPt1;
                var trans = Matrix3d.Displacement(vec);
                pl.TransformBy(trans);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}