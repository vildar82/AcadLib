namespace AcadLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class EntityHelper
    {
        public static void AddEntityToCurrentSpace(
            [CanBeNull] this IEnumerable<Entity> ents,
            [CanBeNull] EntityOptions entityOptions = null)
        {
            if (ents?.Any() != true)
                return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                foreach (var ent in ents)
                {
                    if (ent.Id != ObjectId.Null || ent.IsDisposed)
                        continue;
                    if (!ent.IsWriteEnabled)
                    {
                        ent.Id.GetObject<Entity>(OpenMode.ForWrite);
                    }

                    ent.SetOptions(entityOptions);
                    cs.AppendEntity(ent);
                    t.AddNewlyCreatedDBObject(ent, true);
                }

                t.Commit();
            }
        }

        public static void AddEntityToCurrentSpace([CanBeNull] this Entity ent)
        {
            AddEntityToCurrentSpace(ent?.Yield());
        }

        public static void AddEntityToCurrentSpace([CanBeNull] this Entity ent, EntityOptions entityOptions)
        {
            AddEntityToCurrentSpace(ent?.Yield(), entityOptions);
        }

        public static void AddPointToCurrentSpace(this Point3d pt, [CanBeNull] EntityOptions opt = null)
        {
            var ptDb = new DBPoint(pt);
            AddEntityToCurrentSpace(ptDb, opt);
        }

        public static void AddPointToCurrentSpace(this Point2d pt, [CanBeNull] EntityOptions opt = null)
        {
            AddPointToCurrentSpace(pt.Convert3d(), opt);
        }

        [NotNull]
        public static DBText CreateText(string text, Point3d pos, [CanBeNull] EntityOptions entityOptions = null)
        {
            var dbText = new DBText
            {
                TextString = text,
                Position = pos
            };
            dbText.SetOptions(entityOptions);
            return dbText;
        }
    }
}