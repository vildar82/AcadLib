// ReSharper disable once CheckNamespace
namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class MergePolyline
    {
        /// <summary>
        /// Объединение полилиний по совпадающим вершинам. Без самопересечений.
        /// </summary>
        /// <param name="pls">Полилинии которые нужно объединить - остаются как есть.</param>
        /// <param name="tolerancePoint">Допуск для определения совпадения вершин полилиний</param>
        /// <returns>Объединенная полилиния</returns>
        /// <exception cref="Exception">Ошибка объединения полининий без самопересечения.</exception>
        [CanBeNull]
        public static Polyline Merge([CanBeNull] this List<Polyline> pls, double tolerancePoint = 2)
        {
            if (pls == null || pls.Count == 0)
                return null;

            Polyline merge = null;
            if (pls.Count == 1)
            {
                return (Polyline)pls[0].Clone();
            }

            var plsList = pls.ToList(); // Копирование списка
            // Сортировка полилиний по расстоянию между центрами
            plsList = SortByNearestCenterExtents(plsList);

            var maxIteration = pls.Count * pls.Count;
            var iterationCount = 0;

            while (plsList.Count > 1)
            {
                var plsRemove = new List<Polyline>();
                var fpl = plsList[0];
                foreach (var item in plsList.Skip(1))
                {
                    var plMerge = MergeTwoPl(fpl, item, tolerancePoint);
                    if (plMerge != null)
                    {
                        plsRemove.Add(item);
                        plsRemove.Add(fpl);
                        fpl = plMerge;
                        merge = plMerge;
                    }
                }

                if (plsRemove.Count > 0)
                {
                    foreach (var item in plsRemove)
                    {
                        plsList.Remove(item);
                    }

                    plsList.Insert(0, fpl);
                }

                // страховочный выход из цикла
                iterationCount++;
                if (iterationCount == maxIteration)
                {
                    merge = null;
                    break;
                }
            }

            return merge;
        }

        [NotNull]
        private static Polyline AddVertex(
            [NotNull] Polyline pl1,
            [NotNull] Polyline pl2,
            int indexInPl1,
            int indexInPl2,
            Point2d ptInPl2,
            int dir = 1)
        {
            var plNew = (Polyline)pl1.Clone();
            for (var i = 0; i < pl2.NumberOfVertices; i++)
            {
                plNew.AddVertexAt(indexInPl1++, ptInPl2, 0, 0, 0);

                // След вершина на второй линии
                indexInPl2 = NextIndex(indexInPl2, pl2, dir);
                ptInPl2 = pl2.GetPoint2dAt(indexInPl2);
            }

            return plNew;
        }

        [NotNull]
        private static Polyline Merge(
            [NotNull] Polyline pl1,
            [NotNull] Polyline pl2,
            [NotNull] PolylineVertex ptInPl1,
            [NotNull] PolylineVertex ptInPl2)
        {
            var indexInPl1 = ptInPl1.Index + 1;
            var indexInPl2 = ptInPl2.Index;
            var pt = ptInPl2.Pt;
            var plMerged = AddVertex(pl1, pl2, indexInPl1, indexInPl2, pt);
            if (!plMerged.CheckCross())
            {
                plMerged.Dispose();
                plMerged = AddVertex(pl1, pl2, indexInPl1, indexInPl2, pt, -1);
            }

            return plMerged;
        }

        [CanBeNull]
        private static Polyline MergeTwoPl([NotNull] Polyline pl1, [NotNull] Polyline pl2, double tolerance)
        {
            Polyline plMerged = null;

            // Точки полилиний
            var plVertexes = PolylineVertex.GetVertexes(pl1, "1");
            plVertexes.AddRange(PolylineVertex.GetVertexes(pl2, "2"));

            // группировка совпадающих точек на обоих полилиниях
            var comparer = new Comparers.Point2dEqualityComparer(tolerance);
            var nearsPts = plVertexes.GroupBy(g => g.Pt, comparer)
                .Where(w => w.Any(p => p.Name == "1") && w.Any(p => p.Name == "2")).ToList();
            if (!nearsPts.Any())
            {
                return null;
            }

            // Попытка создать объединенную полилинию - от первой найденной общей точки двух полинилиний
            var fpt = nearsPts.FirstOrDefault();
            if (fpt != null)
            {
                var ptInPl1 = fpt.First(f => f.Name == "1");
                var ptInPl2 = fpt.First(f => f.Name == "2");
                plMerged = Merge(pl1, pl2, ptInPl1, ptInPl2);
            }

            if (plMerged != null && !plMerged.CheckCross())
            {
                // Вторая попытка - от последней общей точки
                plMerged.Dispose();
                var lpt = nearsPts.Last();
                if (lpt != null)
                {
                    var ptInPl1 = lpt.First(f => f.Name == "1");
                    var ptInPl2 = lpt.First(f => f.Name == "2");
                    plMerged = Merge(pl1, pl2, ptInPl1, ptInPl2);
                }
            }

            if (plMerged != null && !plMerged.CheckCross())
            {
                // Неудалось объединить полилинии без самопересечений
                plMerged.Dispose();
                throw new Exception("Ошибка объединения полининий без самопересечения.");
            }

            return plMerged;
        }

        private static int NextIndex(int index, [NotNull] Polyline pl, int step)
        {
            var next = index + step;
            if (next == pl.NumberOfVertices)
            {
                next = 0;
            }
            else if (next == -1)
            {
                next = pl.NumberOfVertices - 1;
            }

            return next;
        }

        /// <summary>
        /// Сортировка полилиний по расчтоянию между центрами границ
        /// </summary>
        /// <param name="pls"></param>
        [NotNull]
        private static List<Polyline> SortByNearestCenterExtents([NotNull] List<Polyline> pls)
        {
            var res = new List<Polyline>();
            var plsCenters = pls.Select(s => new { pl = s, center = s.GeometricExtents.Center() })
                .OrderBy(o => o.center.X + o.center.Y).ToList();

            // .OrderBy(o => o.center.X).ThenBy(o => o.center.Y).ToList();
            var fPlC = plsCenters.First();
            res.Add(fPlC.pl);
            plsCenters.Remove(fPlC);
            var plCPrew = fPlC;
            for (var i = 1; i < pls.Count; i++)
            {
                var plCMin = plsCenters.OrderBy(m => (m.center - plCPrew.center).Length).First();
                res.Add(plCMin.pl);
                plsCenters.Remove(plCMin);
                plCPrew = plCMin;
            }

            return res;
        }
    }
}