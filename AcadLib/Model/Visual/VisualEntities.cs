namespace AcadLib.Visual
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Отрисовка графики в чертеже (добавлением в базу чертежа)
    /// </summary>
    public class VisualEntities : VisualBase
    {
        private readonly List<Entity> _ents;
        private List<ObjectId> _drawIds;

        public VisualEntities([NotNull] List<Entity> ents, string layerName)
            : base(layerName)
        {
            _ents = ents;
        }

        public override List<Entity> CreateVisual()
        {
            return _ents?.Select(s => (Entity) s.Clone()).ToList();
        }

        protected override void DrawVisuals(List<Entity> draws)
        {
            EraseDraws();
            var doc = AcadHelper.Doc;
            using var _ = doc.LockDocument();
            using var t = doc.TransactionManager.StartTransaction();
            var ms = doc.Database.MS(OpenMode.ForWrite);
            _drawIds = new List<ObjectId>();
            foreach (var entity in draws)
            {
                ms.AppendEntity(entity);
                t.AddNewlyCreatedDBObject(entity, true);
                _drawIds.Add(entity.Id);
            }

            t.Commit();
        }

        protected override void EraseDraws()
        {
            if (_drawIds == null)
                return;
            var doc = AcadHelper.Doc;
            using var _ = doc.LockDocument();
            using var t = doc.TransactionManager.StartTransaction();
            foreach (var entity in _drawIds.GetObjects<Entity>(OpenMode.ForWrite))
            {
                entity.Erase();
            }

            t.Commit();
            _drawIds = null;
        }
    }
}