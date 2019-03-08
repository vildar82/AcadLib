namespace AcadLib.Geometry.Polylines
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Создание полилинии, добавлением смещений относительно последней вершины
    /// </summary>
    public class PlRel
    {
        private PlRel()
        {
        }

        [NotNull]
        public List<Point2d> Points { get; set; }
        
        [NotNull]
        public Polyline Create(bool closed = true)
        {
            return Points.CreatePolyline(closed);
        }

        [NotNull]
        public static PlRel Start(double x, double y)
        {
            return new PlRel { Points = new List<Point2d> { new Point2d(x, y) } };
        }

        [NotNull]
        public static PlRel Start(Point2d pt)
        {
            return new PlRel { Points = new List<Point2d> { pt } };
        }

        /// <summary>
        /// Добавление вершины по смещению относительно последней вершины
        /// </summary>
        /// <param name="relX">Смещение по X</param>
        /// <param name="relY">Смещение по Y</param>
        /// <returns></returns>
        [NotNull]
        public PlRel Add(double relX, double relY)
        {
            Points.Add(Points.Last().Move(relX, relY));
            return this;
        }

        /// <summary>
        /// Добавление вершины (без относительно)
        /// </summary>
        /// <param name="pt">Вершина</param>
        [NotNull]
        public PlRel Add(Point2d pt)
        {
            Points.Add(pt);
            return this;
        }
    }
}
