namespace AcadLib.Comparers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Сравнение точек с заданным допуском
    /// </summary>
    [PublicAPI]
    public class Point2dEqualityComparer : IEqualityComparer<Point2d>
    {
        /// <summary>
        /// С допуском поумолчанию - Global
        /// </summary>
        public Point2dEqualityComparer()
        {
        }

        /// <summary>
        /// Задается допуск для точек
        /// </summary>
        public Point2dEqualityComparer(double equalPoint)
        {
            Tolerance = new Tolerance(Tolerance.Global.EqualVector, equalPoint);
        }

        /// <summary>
        /// Допуск 1.
        /// </summary>
        [NotNull]
        public static Point2dEqualityComparer Comparer1 => new Point2dEqualityComparer(1);

        public Tolerance Tolerance { get; set; } = Tolerance.Global;

        public bool Equals(Point2d p1, Point2d p2)
        {
            return p1.IsEqualTo(p2, Tolerance);
        }

        public int GetHashCode(Point2d p)
        {
            return 0;
        }
    }
}