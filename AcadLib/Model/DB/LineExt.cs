namespace AcadLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class LineExt
    {
        [CanBeNull]
        public static Line GetUnionLine([NotNull] this List<Line> lines)
        {
            using (var wasteLines = new DisposableSet<Line>())
            {
                Line prew = null;
                Line unionLine = null;
                foreach (var l in lines)
                {
                    if (prew == null)
                    {
                        prew = l;
                        continue;
                    }

                    var line = prew.GetUnionLine(l);
                    wasteLines.Add(line);
                    unionLine = prew;
                    prew = line;
                }

                if (prew == null)
                    return null;
                if (unionLine == null)
                {
                    return (Line)prew.Clone();
                }

                wasteLines.Remove(prew);
                return prew;
            }
        }

        /// <summary>
        /// Создание общей линии из двух
        /// </summary>
        /// <param name="l1">Первая линия</param>
        /// <param name="l2">Вторая линия</param>
        /// <returns></returns>
        [NotNull]
        public static Line GetUnionLine([NotNull] this Line l1, [NotNull] Line l2)
        {
            var pt1 = l1.GetClosestPointTo(l2.StartPoint, Vector3d.ZAxis, true).Center(l2.StartPoint);
            var pt2 = l1.GetClosestPointTo(l2.EndPoint, Vector3d.ZAxis, true).Center(l2.EndPoint);
            var pt3 = l2.GetClosestPointTo(l1.StartPoint, Vector3d.ZAxis, true).Center(l1.StartPoint);
            var pt4 = l2.GetClosestPointTo(l1.EndPoint, Vector3d.ZAxis, true).Center(l1.EndPoint);
            var len = new[]
            {
                ((pt1 - pt2).Length, pt1, pt2),
                ((pt1 - pt3).Length, pt1, pt3),
                ((pt1 - pt4).Length, pt1, pt4),
                ((pt2 - pt3).Length, pt2, pt3),
                ((pt2 - pt4).Length, pt2, pt4),
                ((pt3 - pt4).Length, pt3, pt4)
            }.OrderByDescending(l => l.Length).First();
            return new Line(len.Item2, len.Item3);
        }
    }
}