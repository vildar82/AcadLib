namespace AcadLib.Comparers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class LineEqualityComparer : IEqualityComparer<Line>
    {
        private readonly double maxInterval;
        private readonly double maxWidth;
        private readonly Tolerance tolerance;

        /// <summary>
        /// Проверяет линии на совпадение
        /// </summary>
        /// <param name="vecTolerance">Допус паралельности векторов линий</param>
        /// <param name="maxWidth">Максимальное расстояние между линиями по ширине (расстояние между стартовыми точками двух линий в проекции к перпендикуляру направления линий)</param>
        /// <param name="maxInterval">Максимальное расстояние между линиями по длине</param>
        public LineEqualityComparer(Tolerance vecTolerance, double maxWidth, double maxInterval)
        {
            tolerance = vecTolerance;
            this.maxWidth = maxWidth;
            this.maxInterval = maxInterval;
        }

        /// <inheritdoc />
        public bool Equals(Line l1, Line l2)
        {
            Debug.Assert(l1 != null, nameof(l1) + " != null");
            Debug.Assert(l2 != null, nameof(l2) + " != null");
            var dir1 = l1.EndPoint - l1.StartPoint;
            var dir2 = l2.EndPoint - l2.StartPoint;
            var res = dir1.IsParallelTo(dir2, tolerance);
            if (!res)
            {
                return false;
            }

            if (WidthBetweenLines(l1, l2, dir1) > maxWidth)
            {
                return false;
            }

            return MaxLens(l1, l2) - (l1.Length + l2.Length) <= maxInterval;
        }

        /// <inheritdoc />
        public int GetHashCode(Line line)
        {
            return 0;
        }

        private static double MaxLens([NotNull] Line l1, [NotNull] Line l2)
        {
            return new[]
            {
                (l1.StartPoint - l2.StartPoint).Length,
                (l1.StartPoint - l2.EndPoint).Length,
                (l1.EndPoint - l2.StartPoint).Length,
                (l1.EndPoint - l2.EndPoint).Length,
            }.Max();
        }

        private static double WidthBetweenLines([NotNull] Line l1, [NotNull] Line l2, Vector3d vec)
        {
            return (l1.StartPoint - l2.StartPoint).OrthoProjectTo(vec).Length;
        }
    }
}