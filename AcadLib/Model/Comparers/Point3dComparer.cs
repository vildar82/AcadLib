namespace AcadLib.Comparers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Сравнение точек с заданным допуском
    /// </summary>
    [PublicAPI]
    public class Point3dEqualityComparer : IEqualityComparer<Point3d>
    {
        /// <summary>
        /// С допуском поумолчанию - Global
        /// </summary>
        public Point3dEqualityComparer()
        {
        }

        /// <summary>
        /// Задается допуск для точек
        /// </summary>
        public Point3dEqualityComparer(double equalPoint)
        {
            Tolerance = new Tolerance(Tolerance.Global.EqualVector, equalPoint);
        }

        /// <summary>
        /// Допуск 1 мм.
        /// </summary>
        [NotNull]
        public static Point3dEqualityComparer Comparer1 => new Point3dEqualityComparer(1);

        public Tolerance Tolerance { get; set; } = Tolerance.Global;

        public bool Equals(Point3d p1, Point3d p2)
        {
            return p1.IsEqualTo(p2, Tolerance);
        }

        public int GetHashCode(Point3d p)
        {
            return 0;
        }
    }
}