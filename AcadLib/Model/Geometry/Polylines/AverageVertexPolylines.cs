// ReSharper disable once CheckNamespace
namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Усреднение вершин соседних полилиний.
    /// Только для полилиний с линейными сегментами.
    /// Усредняются вершины на обоих полилиниях
    /// </summary>
    [PublicAPI]
    public static class AverageVertexPolylines
    {
        /// <summary>
        /// Усреднение вершин
        /// </summary>
        /// <param name="pl">Первая полилиния</param>
        /// <param name="plOther">Вторая полилиния</param>
        /// <param name="tolerance">Определение совпадения вершин для их усреднения</param>
        [Obsolete("Используй вариант с приклеиванием вершин к сегментам.", true)]
        public static void AverageVertexes([NotNull] this Polyline pl, [NotNull] ref Polyline plOther, Tolerance tolerance)
        {
            var ptsOther = plOther.GetPoints();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                var pt = pl.GetPoint2dAt(i);
                var nearestPtOther = ptsOther.Where(p => p.IsEqualTo(pt, tolerance)).ToList();

                // усреднение вершин
                if (nearestPtOther.Any())
                {
                    // Средняя точка
                    var avaragePt = pt.Center(nearestPtOther.First());
                    pl.RemoveVertexAt(i);
                    pl.AddVertexAt(i, avaragePt, 0, 0, 0);

                    foreach (var item in nearestPtOther)
                    {
                        var index = ptsOther.IndexOf(item);
                        plOther.RemoveVertexAt(index);
                        plOther.AddVertexAt(index, avaragePt, 0, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Усреднение совпадающих вершин (по tolerance) у двух полилиний.
        /// "Приклеивание" вершин к соседним сегментам полилиний (по tolerance), для обеих полилинии
        /// </summary>
        /// <param name="pl">Первая полилиния</param>
        /// <param name="plOther">Вторая полилиния</param>
        /// <param name="tolerance">Допуск - поиск совпадающих вершин и ближайших сегментов</param>
        /// <param name="stickToSegment">Делать ли "прилипание" вершин к сегментам соседней полилинии (для обеих полилиний)</param>
        public static void AverageVertexes(
            [NotNull] this Polyline pl,
            [NotNull] ref Polyline plOther,
            Tolerance tolerance,
            bool stickToSegment)
        {
            var ptsOther = plOther.GetPoints();

            // Индексы вершин второй полилинии совпадающие с первой
            var averageVertexesOther = new List<int>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                var pt = pl.GetPoint3dAt(i);
                var pt2d = pt.Convert2d();
                var nearestPtOther = ptsOther.Where(p => p.IsEqualTo(pt2d, tolerance)).ToList();

                // усреднение вершин
                if (nearestPtOther.Any())
                {
                    // Средняя точка
                    var avaragePt = pt2d.Center(nearestPtOther.First());
                    pl.MoveVertex(i, avaragePt);

                    foreach (var item in nearestPtOther)
                    {
                        var index = ptsOther.IndexOf(item);
                        plOther.MoveVertex(index, avaragePt);
                        averageVertexesOther.Add(index);
                    }
                }
                else if (stickToSegment)
                {
                    pl.StickVertexToPl(i, pt, plOther, tolerance);
                }
            }

            // Приклеивание вершин второй полилинии к сегментам первой
            if (stickToSegment)
            {
                for (var i = 0; i < ptsOther.Count; i++)
                {
                    if (!averageVertexesOther.Contains(i))
                    {
                        var pt = plOther.GetPoint3dAt(i);
                        plOther.StickVertexToPl(i, pt, pl, tolerance);
                    }
                }
            }
        }

        /// <summary>
        /// Перенос вершины
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="indexVertex">Номер вершины</param>
        /// <param name="newPlacePt">Новая точка</param>
        public static void MoveVertex([NotNull] this Polyline pl, int indexVertex, Point2d newPlacePt)
        {
            pl.RemoveVertexAt(indexVertex);
            pl.AddVertexAt(indexVertex, newPlacePt, 0, 0, 0);
        }

        private static void StickVertexToPl(
            this Polyline plModify,
            int indexVertex,
            Point3d ptVertex,
            [NotNull] Polyline plOther,
            Tolerance tolerance)
        {
            var ptStick = plOther.GetClosestPointTo(ptVertex, Vector3d.ZAxis, false);
            if ((ptVertex - ptStick).Length <= tolerance.EqualPoint)
            {
                MoveVertex(plModify, indexVertex, ptStick.Convert2d());
            }
        }
    }
}