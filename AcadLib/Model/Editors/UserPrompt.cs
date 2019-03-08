namespace Autodesk.AutoCAD.EditorInput
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AcadLib;
    using AcadLib.Jigs;
    using DatabaseServices;
    using Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class UserPrompt
    {
        /// <summary>
        /// Запрос целого числа
        /// </summary>
        /// <param name="ed">ed</param>
        /// <param name="defaultNumber">Номер по умолчанию</param>
        /// <param name="msg">Строка запроса</param>
        /// <exception cref="Exception">Отменено пользователем.</exception>
        /// <returns>Результат успешного запроса.</returns>
        public static int GetNumber([NotNull] this Editor ed, int defaultNumber, string msg)
        {
            var opt = new PromptIntegerOptions(msg)
            {
                DefaultValue = defaultNumber
            };
            var res = ed.GetInteger(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// Простой запрос точки - с преобразованием в WCS
        /// </summary>
        /// <param name="ed">ed</param>
        /// <param name="msg">Строка запроса</param>
        /// <exception cref="Exception">Отменено пользователем.</exception>
        /// <returns>Point3d in WCS</returns>
        public static Point3d GetPointWCS([NotNull] this Editor ed, string msg)
        {
            var res = ed.GetPoint(msg);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// Запрос точки вставки с висящим прямоугольником на курсоре - габариты всталяемого объекта
        /// Чтобы человек выбрал нашел место на чертежа соответствующих размеров.
        /// Точка - нижний левый угол
        /// </summary>
        public static Point3d GetRectanglePoint([NotNull] this Editor ed, double len, double height)
        {
            var jigRect = new RectangleJig(len, height);
            var res = ed.Drag(jigRect);
            if (res.Status != PromptStatus.OK)
                throw new OperationCanceledException();
            return jigRect.Position;
        }

        public static Extents3d PromptExtents([NotNull] this Editor ed, string msgPromptFirstPoint, string msgPromptsecondPoint)
        {
            var extentsPrompted = new Extents3d();
            var prPtRes = ed.GetPoint(msgPromptFirstPoint);
            if (prPtRes.Status == PromptStatus.OK)
            {
                var prCornerRes = ed.GetCorner(msgPromptsecondPoint, prPtRes.Value);
                if (prCornerRes.Status == PromptStatus.OK)
                {
                    extentsPrompted.AddPoint(prPtRes.Value);
                    extentsPrompted.AddPoint(prCornerRes.Value);
                }
                else
                {
                    throw new OperationCanceledException();
                }
            }
            else
            {
                throw new OperationCanceledException();
            }

            return extentsPrompted;
        }

        /// <summary>
        /// Pапрос выбора объектов
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="msg">Строка запроса</param>
        /// <exception cref="Exception">Отменено пользователем.</exception>
        /// <returns>Список выбранных объектов</returns>
        [NotNull]
        public static List<ObjectId> Select([NotNull] this Editor ed, string msg)
        {
            var selOpt = new PromptSelectionOptions();
            selOpt.Keywords.Add(AcadHelper.IsRussianAcad() ? "Все" : "ALL");
            selOpt.MessageForAdding = msg + selOpt.Keywords.GetDisplayString(true);
            var selRes = ed.GetSelection(selOpt);
            if (selRes.Status == PromptStatus.OK)
                return selRes.Value.GetObjectIds().ToList();
            throw new OperationCanceledException();
        }

        [NotNull]
        public static List<ObjectId> Select([NotNull] this Editor ed, string msg, Func<List<ObjectId>> selectAll)
        {
            var selOpt = new PromptSelectionOptions();
            selOpt.Keywords.Add(AcadHelper.IsRussianAcad() ? "Все" : "ALL");
            selOpt.MessageForAdding = msg + selOpt.Keywords.GetDisplayString(true);
            selOpt.KeywordInput += (s, e) => { selectAll(); };
            var selRes = ed.GetSelection(selOpt);
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds().ToList();
            }

            throw new OperationCanceledException();
        }

        [NotNull]
        public static List<ObjectId> Select([NotNull] this Editor ed, string msg, string start)
        {
            var selOpt = new PromptSelectionOptions { MessageForAdding = msg };
            var filter = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, start) });
            var selRes = ed.GetSelection(selOpt, filter);
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds().ToList();
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// Pапрос выбора блоков
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="msg">Строка запроса</param>
        /// <exception cref="Exception">Отменено пользователем.</exception>
        /// <returns>Список выбранных блоков</returns>
        [NotNull]
        public static List<ObjectId> SelectBlRefs([NotNull] this Editor ed, string msg)
        {
            var filList = new[] { new TypedValue((int)DxfCode.Start, "INSERT") };
            var filter = new SelectionFilter(filList);
            var selOpt = new PromptSelectionOptions()
            {
                MessageForAdding = msg
            };
            var selRes = ed.GetSelection(selOpt, filter);
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds().ToList();
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// Выбор объекта заданного типа на чертеже. В том числе, на заблокированном слое.
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="ed">Editor</param>
        /// <param name="prompt">Запрос выбора объекта пользователю</param>
        /// <param name="exactMatch">Точное соответствие типа объекта</param>
        /// <returns></returns>
        public static ObjectId SelectEntity<T>([NotNull] this Editor ed, string prompt, bool exactMatch = true)
            where T : Entity
        {
            return SelectEntity<T>(ed, prompt, out _, exactMatch);
        }

        /// <summary>
        /// Выбор объекта заданного типа на чертеже. В том числе, на заблокированном слое.
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="ed">Editor</param>
        /// <param name="prompt">Запрос выбора объекта пользователю</param>
        /// <param name="exactMatch">Точное соответствие типа объекта</param>
        /// <returns></returns>
        public static ObjectId SelectEntity<T>([NotNull] this Editor ed, string prompt, out Point3d pickedPt,
            bool exactMatch = true)
            where T : Entity
        {
            var selOpt = new PromptEntityOptions($"\n{prompt}");
            selOpt.SetRejectMessage($"\nМожно выбрать {typeof(T).Name}");
            selOpt.AddAllowedClass(typeof(T), exactMatch);
            selOpt.AllowObjectOnLockedLayer = true;
            var selRes = ed.GetEntity(selOpt);
            if (selRes.Status != PromptStatus.OK)
            {
                throw new OperationCanceledException();
            }

            pickedPt = selRes.PickedPoint;
            return selRes.ObjectId;
        }

        /// <summary>
        /// Выбор объекта на чертеже заданного типа
        /// </summary>
        /// <typeparam name="T">Тип выбираемого объекта</typeparam>
        /// <param name="ed">Editor</param>
        /// <param name="prompt">Строка запроса</param>
        /// <param name="rejectMsg">Сообщение при выбора неправильного типа объекта</param>
        /// <returns>Выбранный объект</returns>
        public static ObjectId SelectEntity<T>([NotNull] this Editor ed, string prompt, string rejectMsg)
            where T : Entity
        {
            var selOpt = new PromptEntityOptions($"\n{prompt}");
            selOpt.SetRejectMessage($"\n{rejectMsg}");
            selOpt.AddAllowedClass(typeof(T), true);
            selOpt.AllowObjectOnLockedLayer = true;
            var selRes = ed.GetEntity(selOpt);
            if (selRes.Status != PromptStatus.OK)
            {
                throw new OperationCanceledException();
            }

            return selRes.ObjectId;
        }
    }
}
