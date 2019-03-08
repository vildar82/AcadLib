using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AcadLib.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using NetLib.Monad;

    public static class EditorExt
    {
        public static void AcadLoadInfo([NotNull] this Assembly assm)
        {
            try
            {
                var asmName = assm.GetName();
                var msg = $"PIK. {asmName.Name} загружен, версия {asmName.Version}";
                msg.WriteToCommandLine();
                Logger.Log.Info(msg);
            }
            catch
            {
                //
            }
        }

        public static void AcadLoadError(
            [NotNull] this Assembly assm,
            [CanBeNull] Exception ex = null,
            [CanBeNull] string err = null)
        {
            try
            {
                var asmName = assm.GetName();
                $"PIK. Ошибка загрузки {asmName.Name}, версия:{asmName.Version} - {err} {ex?.Message}.".WriteToCommandLine();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Выделение объектов и зумирование по границе
        /// </summary>
        /// <param name="ids">Элементв</param>
        /// <param name="ed">Редактор</param>
        public static void SetSelectionAndZoom([NotNull] this List<ObjectId> ids, Editor ed = null)
        {
            try
            {
                var doc = AcadHelper.Doc;
                ed = doc.Editor;
                using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    if (!ids.Any())
                    {
                        "Нет объектов для выделения.".WriteToCommandLine();
                        return;
                    }

                    var ext = new Extents3d();
                    ids.Select(s => s.GetObject(OpenMode.ForRead)).Iterate(o =>
                    {
                        if (o.Bounds.HasValue)
                            ext.AddExtents(o.Bounds.Value);
                    });
                    ed.Zoom(ext);
                    Autodesk.AutoCAD.Internal.Utils.SelectObjects(ids.ToArray());
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                $"Ошибка выделения объектов - {ex.Message}.".WriteToCommandLine();
            }
        }

        public static void AddEntToImpliedSelection(this Editor ed, ObjectId id)
        {
            try
            {
                var idsToSel = new List<ObjectId> { id };
                var selRes = ed.SelectImplied();
                if (selRes.Status == PromptStatus.OK)
                {
                    idsToSel.AddRange(selRes.Value.GetObjectIds());
                }

                ed.SetImpliedSelection(idsToSel.ToArray());
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Выбор объектов в заданных границах
        /// В модели
        /// </summary>
        [NotNull]
        public static List<ObjectId> SelectInExtents([NotNull] this Editor ed, Extents3d ext)
        {
            using (ed.Document.LockDocument())
            {
                Debug.WriteLine($"SelectInExtents IsApplicationContext={Application.DocumentManager.IsApplicationContext}.");
                ed.Try(e => e.Document.Database.TileMode = true);
                ed.Try(e => e.Zoom(ext.Offset(10)));
                ext.TransformBy(ed.WCS2UCS());
                var minPt = ext.MinPoint;
                var maxPt = ext.MaxPoint;
                var selRes = ed.SelectCrossingWindow(minPt, maxPt);
                if (selRes.Status == PromptStatus.OK)
                {
                    return selRes.Value.GetObjectIds().ToList();
                }

                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Выбор объектов в заданных границах
        /// В модели
        /// </summary>
        [NotNull]
        public static List<ObjectId> SelectInExtents2([NotNull] this Editor ed, Extents3d ext)
        {
            Debug.WriteLine($"SelectInExtents2 IsApplicationContext={Application.DocumentManager.IsApplicationContext}.");
            List<TypedValue> filterList = new List<TypedValue>
            {
                new TypedValue((int)DxfCode.Start, "*"),
                new TypedValue((int)DxfCode.Operator, "<and"),
                new TypedValue((int)DxfCode.Operator, ">,>,*"),
                new TypedValue((int)DxfCode.XCoordinate, ext.MinPoint),
                new TypedValue((int)DxfCode.Operator, "<,<,*"),
                new TypedValue((int)DxfCode.XCoordinate, ext.MaxPoint),
                new TypedValue((int)DxfCode.Operator, "and>")
            };
            var selRes = ed.SelectAll(new SelectionFilter(filterList.ToArray()));
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds().ToList();
            }

            throw new OperationCanceledException();
        }

        [NotNull]
        public static List<ObjectId> SelectByPolygon([NotNull] this Editor ed, [NotNull] IEnumerable<Point3d> pts)
        {
            using (ed.Document.LockDocument())
            {
                Debug.WriteLine($"SelectByPolygon IsApplicationContext={Application.DocumentManager.IsApplicationContext}.");
                var ext = new Extents3d();
                var ptsCol = new List<Point3d>();
                var wcsToUcs = ed.WCS2UCS();
                foreach (var pt in pts)
                {
                    ext.AddPoint(pt);
                    var ptUCS = pt.TransformBy(wcsToUcs);
                    ptsCol.Add(ptUCS);
                }
                ed.Zoom(ext);
                var selRes = ed.SelectCrossingPolygon(new Point3dCollection(ptsCol.ToArray()));
                if (selRes.Status == PromptStatus.OK)
                {
                    return selRes.Value.GetObjectIds().ToList();
                }

                throw new OperationCanceledException();
            }
        }
    }
}
