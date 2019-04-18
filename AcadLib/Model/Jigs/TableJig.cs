using System;
using System.Linq;

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
        private readonly Action<string> _keywordInput;
        private readonly string[] _keywords;
        private Point3d _position;

        public TableJig([NotNull] Table table, double scale, string msg)
            : this(table, scale, msg, null)
        {
        }

        public TableJig([NotNull] Table table, double scale, string msg, Action<string> keywordInput, params string[] keywords)
            : base(table)
        {
            _msg      = msg;
            _keywordInput = keywordInput;
            _keywords = keywords;
            _position = table.Position;
            table.TransformBy(Matrix3d.Scaling(scale, table.Position));
            table.TransformBy(AcadHelper.Doc.Editor.CurrentUserCoordinateSystem);
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
            if (_keywords?.Any() == true) foreach (var keyword in _keywords) jigOpts.Keywords.Add(keyword);
            var res = prompts.AcquirePoint(jigOpts);
            switch (res.Status)
            {
                case PromptStatus.OK:
                {
                    var curPoint = res.Value;
                    if (_position.DistanceTo(curPoint) > 1.0e-2)
                        _position = curPoint;
                    else
                        return SamplerStatus.NoChange;
                    break;
                }

                case PromptStatus.Keyword:
                {
                    _keywordInput(res.StringResult);
                    return SamplerStatus.OK;
                }
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
