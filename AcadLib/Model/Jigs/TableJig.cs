namespace AcadLib.Jigs
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class TableJig : EntityJig
    {
        private readonly string _msg;
        private Point3d _position;

        public TableJig([NotNull] Table table, double scale, string msg) : base(table)
        {
            _msg = msg;
            var table1 = table;
            _position = table1.Position;
            table1.TransformBy(Matrix3d.Scaling(scale, table.Position));
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        protected override SamplerStatus Sampler([NotNull] JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                Message = "\n" + _msg
            };
            var res = prompts.AcquirePoint(jigOpts);
            if (res.Status == PromptStatus.OK)
            {
                var curPoint = res.Value;
                if (_position.DistanceTo(curPoint) > 1.0e-2)
                    _position = curPoint;
                else
                    return SamplerStatus.NoChange;
            }

            return res.Status == PromptStatus.Cancel ? SamplerStatus.Cancel : SamplerStatus.OK;
        }

        protected override bool Update()
        {
            var table = (Table)Entity;
            if (table.Position.DistanceTo(_position) > 1.0e-2)
            {
                table.Position = _position;
                return true;
            }

            return false;
        }
    }
}