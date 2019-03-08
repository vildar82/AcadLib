namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using JetBrains.Annotations;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    [PublicAPI]
    public static class ObjectIdExt
    {
        /// <summary>
        /// Выделение объекта на чертеже
        /// </summary>
        /// <param name="entId">Примитив</param>
        /// <param name="ed">Редактор</param>
        public static void Select(this ObjectId entId, [NotNull] Editor ed)
        {
            ed.SetImpliedSelection(new[] { entId });
        }

        public static void HighlightEntity(this ObjectId entId)
        {
            using (var ent = (Entity)entId.Open(OpenMode.ForRead, false, true))
            {
                ent.Highlight();
            }
        }

        /// <summary>
        /// Копирование объекта в одной базе
        /// </summary>
        /// <param name="idEnt">Копируемый объект</param>
        /// <param name="idBtrOwner">Куда копировать (контейнер - BlockTableRecord)</param>
        public static ObjectId CopyEnt(this ObjectId idEnt, ObjectId idBtrOwner)
        {
            var db = idEnt.Database;
            var map = new IdMapping();
            var ids = new ObjectIdCollection(new[] { idEnt });
            db.DeepCloneObjects(ids, idBtrOwner, map, false);
            return map[idEnt].Value;
        }

        /// <summary>
        /// Копирование объектов в одной базе
        /// </summary>
        /// <param name="idsEnt">Копируемый объект</param>
        /// <param name="idBtrOwner">Куда копировать (контейнер - BlockTableRecord)</param>
        public static List<ObjectId> CopyEnts(this List<ObjectId> idsEnt, ObjectId idBtrOwner)
        {
            var db = idBtrOwner.Database;
            var map = new IdMapping();
            var ids = new ObjectIdCollection(idsEnt.ToArray());
            db.DeepCloneObjects(ids, idBtrOwner, map, false);
            return idsEnt.Select(s => map[s].Value).ToList();
        }

        public static void FlickObjectHighlight([NotNull] this Entity ent, int num = 2, int delay1 = 50, int delay2 = 50)
        {
            FlickObjectHighlight(new[] { ent }, num, delay1, delay2);
        }

        public static void FlickObjectHighlight(
            [NotNull] this IEnumerable<Entity> ents,
            int num = 2,
            int delay1 = 50,
            int delay2 = 50)
        {
            var list = ents.ToList();
            if (list.Any() != true)
                return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            for (var i = 0; i < num; i++)
            {
                // Highlight entity
                foreach (var entity in list)
                {
                    entity.Highlight();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay1 msecs
                Thread.Sleep(delay1);

                // Unhighlight entity
                foreach (var entity in list)
                {
                    entity.Unhighlight();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay2 msecs
                Thread.Sleep(delay2);
            }
        }

        public static void FlickObjectHighlight(
            [CanBeNull] this List<ObjectId> ids,
            int num = 2,
            int delay1 = 50,
            int delay2 = 50)
        {
            if (ids?.Any() != true)
                return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var ents = new DisposableSet<Entity>(ids.Select(s => (Entity)s.Open(OpenMode.ForRead))))
            {
                for (var i = 0; i < num; i++)
                {
                    // Highlight entity
                    foreach (var entity in ents)
                    {
                        entity.Highlight();
                    }

                    doc.Editor.UpdateScreen();

                    // Wait for delay1 msecs
                    Thread.Sleep(delay1);

                    // Unhighlight entity
                    foreach (var entity in ents)
                    {
                        entity.Unhighlight();
                    }

                    doc.Editor.UpdateScreen();

                    // Wait for delay2 msecs
                    Thread.Sleep(delay2);
                }
            }
        }

        /// <summary>
        /// Функция производит "мигание" объектом при помощи Highlight/Unhighlight
        /// </summary>
        /// <param name="id">ObjectId для примитива</param>
        /// <param name="num">Количество "миганий"</param>
        /// <param name="delay1">Длительность "подсвеченного" состояния</param>
        /// <param name="delay2">Длительность "неподсвеченного" состояния</param>
        public static void FlickObjectHighlight(this ObjectId id, int num, int delay1, int delay2)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            for (var i = 0; i < num; i++)
            {
                // Highlight entity
                using (doc.LockDocument())
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    var en = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                    var ids = new ObjectId[1];
                    ids[0] = id;
                    var index = new SubentityId(SubentityType.Null, IntPtr.Zero);
                    var path = new FullSubentityPath(ids, index);
                    en.Highlight(path, true);
                    tr.Commit();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay1 msecs
                Thread.Sleep(delay1);

                // Unhighlight entity
                using (doc.LockDocument())
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    var en = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                    var ids = new ObjectId[1];
                    ids[0] = id;
                    var index = new SubentityId(SubentityType.Null, IntPtr.Zero);
                    var path = new FullSubentityPath(ids, index);
                    en.Unhighlight(path, true);
                    tr.Commit();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay2 msecs
                Thread.Sleep(delay2);
            }
        }

        /// <summary>
        /// Функция производит "мигание" подобъектом при помощи Highlight/Unhighlight
        /// </summary>
        /// <param name="idsPath">Цепочка вложенности объектов. BlockReference->Subentity</param>
        /// <param name="num">Количество "миганий"</param>
        /// <param name="delay1">Длительность "подсвеченного" состояния</param>
        /// <param name="delay2">Длительность "неподсвеченного" состояния</param>
        public static void FlickSubentityHighlight(ObjectId[] idsPath, int num, int delay1, int delay2)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            for (var i = 0; i < num; i++)
            {
                // Highlight entity
                using (doc.LockDocument())
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    var subId = new SubentityId(SubentityType.Null, IntPtr.Zero);
                    var path = new FullSubentityPath(idsPath, subId);
                    var ent = (Entity)idsPath[0].GetObject(OpenMode.ForRead);
                    ent.Highlight(path, true);
                    tr.Commit();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay1 msecs
                Thread.Sleep(delay1);

                // Unhighlight entity
                using (doc.LockDocument())
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    var subId = new SubentityId(SubentityType.Null, IntPtr.Zero);
                    var path = new FullSubentityPath(idsPath, subId);
                    var ent = (Entity)idsPath[0].GetObject(OpenMode.ForRead);
                    ent.Unhighlight(path, true);
                    tr.Commit();
                }

                doc.Editor.UpdateScreen();

                // Wait for delay2 msecs
                Thread.Sleep(delay2);
            }
        }

        public static bool IsValidEx(this ObjectId id)
        {
            return id.IsValid && !id.IsNull && !id.IsErased;
        }

        public static void ShowEnt(this ObjectId id, int num, int delay1, int delay2)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null || !id.IsValidEx())
                return;

            using (doc.LockDocument())
            using (var t = id.Database.TransactionManager.StartTransaction())
            {
                if (id.GetObject(OpenMode.ForRead) is Entity ent)
                {
                    try
                    {
                        doc.Editor.Zoom(ent.GeometricExtents.Offset());
                        id.FlickObjectHighlight(num, delay1, delay2);
                        doc.Editor.SetImpliedSelection(new[] { id });
                    }
                    catch
                    {
                        //
                    }
                }

                t.Commit();
            }
        }

        public static void ShowEnt(this ObjectId id)
        {
            ShowEnt(id, 2, 100, 100);
        }

        public static void ShowEnt(this ObjectId id, Extents3d ext, Document docOrig)
        {
            var curDoc = Application.DocumentManager.MdiActiveDocument;
            if (docOrig != curDoc)
            {
                Application.ShowAlertDialog($"Должен быть активен документ {docOrig.Name}");
            }
            else
            {
                if (ext.Diagonal() > 1)
                {
                    docOrig.Editor.Zoom(ext);
                    id.FlickObjectHighlight(2, 100, 100);
                    docOrig.Editor.SetImpliedSelection(new[] { id });

                    // docOrig.Editor.AddEntToImpliedSelection(id);
                }
                else
                {
                    Application.ShowAlertDialog("Границы элемента не определены");
                }
            }
        }
    }
}
