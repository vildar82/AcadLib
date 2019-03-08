namespace AcadLib.Scale
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Exceptions;

    public static class ScaleHelper
    {
        /// <summary>
        /// Текущий масштаб аннотаций.
        /// </summary>
        public static double GetCurrentAnnoScale(this Database db)
        {
            try
            {
                return 1 / db.Cannoscale.Scale;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "GetCurrentAnnoScale");
            }

            return 1;
        }

        /// <summary>
        /// Получить масштаб аннотаций чертежа по имени масштаба
        /// </summary>
        /// <param name="db">Чертеж</param>
        /// <param name="nameScale">Имя масштаба - 1:1000</param>
        /// <returns>ObjectContext</returns>
        public static ObjectContext GetAnnotationScale (this Database db, string nameScale)
        {
            var occ = GetAnnotationScales(db);
            return occ.HasContext(nameScale) ? occ.GetContext(nameScale) : null;
        }

        public static ObjectContextCollection GetAnnotationScales(this Database db)
        {
            return db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
        }

        public static ObjectContext GetOrAddAnnotationScale (this Database db, string nameScale, double scale, bool fix)
        {
            var annoScale = GetAnnotationScale(db, nameScale);
            if (annoScale != null)
            {
                if (fix)
                    FixAnnotationScale(db, (AnnotationScale)annoScale, scale);
                return annoScale;
            }
            AddAnnotationScale(db, nameScale, scale);
            return GetAnnotationScale(db, nameScale);
        }

        /// <summary>
        /// Добавление масштаба аннотаций в чертеже
        /// </summary>
        /// <param name="db">Чертеж</param>
        /// <param name="nameScale">Имя масштаба</param>
        /// <param name="scale">Масштаб - 1:scale, PaperUnits=1, DrawingUnits=scale</param>
        /// <returns>True - масштаб создан. False - такое имя масштаба уже есть (без проверки scale)</returns>
        public static bool AddAnnotationScale(this Database db, string nameScale, double scale)
        {
            var occ = db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
            if (!occ.HasContext(nameScale))
            {
                var annoScale = new AnnotationScale
                {
                    Name = nameScale,
                    PaperUnits = 1,
                    DrawingUnits = scale
                };
                occ.AddContext(annoScale);
                return true;
            }

            return false;
        }

        public static bool FixAnnotationScale(this Database db, AnnotationScale annoScale, double scale)
        {
            var annoFactor = annoScale.PaperUnits / annoScale.DrawingUnits;
            var checkFactor = 1/scale;
            if (Math.Abs(annoFactor - checkFactor) > 0.0001)
            {
                annoScale.PaperUnits = 1;
                annoScale.DrawingUnits = scale;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Исправление масштаба аннотации
        /// </summary>
        /// <param name="db">Чертеж</param>
        /// <param name="nameScale">Имя масштаба</param>
        /// <param name="scale">Масштаб - 1:scale, PaperUnits=1, DrawingUnits=scale</param>
        /// <returns>True - если масштаб был исправлен. False - масштаб не изменился.</returns>
        public static bool FixAnnotationScale(this Database db, string nameScale, double scale)
        {
            var annoScale = (AnnotationScale)GetAnnotationScale(db, nameScale);
            if (annoScale == null)
                throw new AnnotationScaleNotFound($"Не найден масштаб аннотаций '{nameScale}' в текущем чертеже.");
            return FixAnnotationScale(db, annoScale, scale);
        }

        public static void SetAnnotationScale(this Database db, string nameScale, double scale)
        {
            var annoScale = GetOrAddAnnotationScale(db, nameScale, scale, true);
            db.Cannoscale = (AnnotationScale)annoScale;
        }
    }
}