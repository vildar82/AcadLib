namespace AcadLib.Jigs
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using JetBrains.Annotations;

    [PublicAPI]
    public class DragEntitiesJig : DrawJig
    {
        private readonly IEnumerable<Entity> ents;
        private Point3d basePoint;

        public DragEntitiesJig(IEnumerable<Entity> ents, Point3d basePoint)
        {
            this.basePoint = basePoint;
            this.ents = ents;
        }

        public Point3d DragPoint { get; private set; }

        protected override SamplerStatus Sampler([NotNull] JigPrompts prompts)
        {
            var options = new JigPromptPointOptions("\nТочка вставки: ")
            {
                BasePoint = basePoint,
                UseBasePoint = true,
                Cursor = CursorType.RubberBand,
                UserInputControls = UserInputControls.Accept3dCoordinates
            };
            var ppr = prompts.AcquirePoint(options);
            if (ppr.Value.IsEqualTo(DragPoint))
                return SamplerStatus.NoChange;
            DragPoint = ppr.Value;
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw([NotNull] WorldDraw draw)
        {
            var wGeom = draw.Geometry;
            var disp = Matrix3d.Displacement(basePoint.GetVectorTo(DragPoint));
            wGeom.PushModelTransform(disp);
            foreach (var ent in ents)
            {
                wGeom.Draw(ent);
            }

            wGeom.PopModelTransform();
            return true;
        }
    }
}