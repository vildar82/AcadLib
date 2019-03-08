namespace AcadLib.Jigs
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    public class BlockInsertJig : EntityJig
    {
        private Point3d mCenterPt, mActualPoint;

        public BlockInsertJig([NotNull] BlockReference br) : base(br)
        {
            var ed = AcadHelper.Doc.Editor;
            br.TransformBy(ed.CurrentUserCoordinateSystem);
            mCenterPt = br.Position;
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        protected override SamplerStatus Sampler([NotNull] JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted |
                                    UserInputControls.NoNegativeResponseAccepted,
                Message = "\nУкажите точку вставки: "
            };
            var dres = prompts.AcquirePoint(jigOpts);
            if (mActualPoint == dres.Value)
            {
                return SamplerStatus.NoChange;
            }

            mActualPoint = dres.Value;
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            mCenterPt = mActualPoint;
            try
            {
                ((BlockReference)Entity).Position = mCenterPt;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }
    }
}