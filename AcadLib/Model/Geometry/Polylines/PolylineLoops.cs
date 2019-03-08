// ReSharper disable once CheckNamespace
namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class PolylineLoops
    {
        /// <summary>
        /// Точки "петли" полилинии между точками пересечения.
        /// </summary>
        /// <param name="contour">Исходная Полилиния</param>
        /// <param name="ptIntersect1">Первая точка петли (пересечения)</param>
        /// <param name="ptIntersect2">Вторая точка петли (пересечения)</param>
        /// <param name="above">Петля выше или ниже точек пересечения</param>
        /// <param name="includePtIntersects">Включать ли сами точки пересечения в результат</param>
        /// <returns>Список точек петли пересечения</returns>
        [NotNull]
        public static List<Point2d> GetLoopSideBetweenHorizontalIntersectPoints(
            [NotNull] this Polyline contour,
            Point3d ptIntersect1,
            Point3d ptIntersect2,
            bool above = true,
            bool includePtIntersects = true)
        {
            var res = above
                ? GetLoopSide(contour, ptIntersect1, ptIntersect2, (seg) => seg.StartPoint.Y > seg.EndPoint.Y,
                    includePtIntersects)
                : GetLoopSide(contour, ptIntersect1, ptIntersect2, (seg) => seg.StartPoint.Y < seg.EndPoint.Y,
                    includePtIntersects);
            return res;
        }

        /// <summary>
        /// Точки петли полилинии слева/справа от точек пересечения
        /// </summary>
        [NotNull]
        public static List<Point2d> GetLoopSideBetweenVerticalIntersectPoints(
            [NotNull] this Polyline contour,
            Point3d ptIntersect1,
            Point3d ptIntersect2,
            bool isRightSide = true,
            bool includePtIntersects = true)
        {
            var res = isRightSide
                ? GetLoopSide(contour, ptIntersect1, ptIntersect2, (seg) => seg.StartPoint.X > seg.EndPoint.X,
                    includePtIntersects)
                : GetLoopSide(contour, ptIntersect1, ptIntersect2, (seg) => seg.StartPoint.X < seg.EndPoint.X,
                    includePtIntersects);
            return res;
        }

        private static void AddPoint([NotNull] List<Point2d> pointsLoop, int dir, ref int indexCur, [NotNull] Polyline contour)
        {
            var pt = contour.GetPoint2dAt(indexCur);
            pointsLoop.Add(pt);
            indexCur += dir;
            if (indexCur == -1)
            {
                indexCur = contour.NumberOfVertices - 1;
            }
            else if (indexCur == contour.NumberOfVertices)
            {
                indexCur = 0;
            }
        }

        [NotNull]
        private static List<Point2d> GetLoopSide(
            [NotNull] this Polyline contour,
            Point3d ptIntersect1,
            Point3d ptIntersect2,
            Func<LineSegment3d, bool> checkSeg,
            bool includePtIntersects = true)
        {
            var pointsLoopSide = new List<Point2d>();

            var ptIntersectStart = ptIntersect1;
            var ptIntersectEnd = ptIntersect2;

            // Индекс стартовой точки петли (вершины) с нужной стороны от точки пересечения
            var indexStart = GetStartIndex(contour, ptIntersect1, checkSeg, out var dir);
            var indexCur = indexStart;

            var indexEnd = GetStartIndex(contour, ptIntersect2, checkSeg, out var dirEnd);
            if (dir == 0)
            {
                dir = dirEnd;
                indexCur = indexEnd;
                indexEnd = indexStart;
                ptIntersectStart = ptIntersect2;
                ptIntersectEnd = ptIntersect1;
            }

            if (includePtIntersects)
                pointsLoopSide.Add(ptIntersectStart.Convert2d());

            if (dir != 0)
            {
                if (indexCur == indexEnd)
                {
                    AddPoint(pointsLoopSide, dir, ref indexCur, contour);
                }
                else
                {
                    while (indexCur != indexEnd)
                    {
                        AddPoint(pointsLoopSide, dir, ref indexCur, contour);
                    }

                    // Добавление последней вершины
                    AddPoint(pointsLoopSide, dir, ref indexCur, contour);
                }
            }

            if (includePtIntersects)
                pointsLoopSide.Add(ptIntersectEnd.Convert2d());

            return pointsLoopSide;
        }

        private static int GetStartIndex(
            [NotNull] Polyline contour,
            Point3d ptIntersect1,
            Func<LineSegment3d, bool> checkSeg,
            out int dir)
        {
            var param = contour.GetParameterAtPointTry(ptIntersect1);

            var indexMin = (int)param;
            if (indexMin == contour.NumberOfVertices)
                indexMin = 0;

            var indexMax = (int)Math.Ceiling(param);
            if (indexMax == contour.NumberOfVertices)
                indexMax = 0;
            var seg = contour.GetLineSegmentAt(indexMin);
            var indexStart = indexMax;
            if (indexMin == indexMax)
            {
                dir = 0;
            }
            else
            {
                dir = 1;
                if (checkSeg(seg))
                {
                    indexStart = indexMin;
                    dir = -1;
                }
            }

            return indexStart;
        }
    }
}