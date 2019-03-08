namespace AcadLib.Visual
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public class VisualTransientSimple : VisualTransient
    {
        private readonly List<Entity> ents;

        public VisualTransientSimple([NotNull] List<Entity> ents)
        {
            this.ents = ents;
        }

        [NotNull]
        public override List<Entity> CreateVisual()
        {
            return ents.Select(s => (Entity)s.Clone()).ToList();
        }

        public override void Dispose()
        {
            ents.ForEach(e => e.Dispose());
            base.Dispose();
        }
    }
}