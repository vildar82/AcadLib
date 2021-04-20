namespace AcadLib.Geometry.Polylines
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Создание полилинии, добавлением смещений относительно последней вершины
    /// </summary>
    public class PlRel
    {
        private PlRel()
        {
        }

        public List<Point2d> Points { get; set; }
        
        public Polyline Create(bool closed = true)
        {
            return Points.CreatePolyline(closed);
        }

        public static PlRel Start(double x, double y)
        {
            return new PlRel { Points = new List<Point2d> { new Point2d(x, y) } };
        }

        public static PlRel Start(Point2d pt)
        {
            return new PlRel { Points = new List<Point2d> { pt } };
        }

        /// <summary>
        /// Добавление вершины по смещению относительно последней вершины
        /// </summary>
        /// <param name="relX">Смещение по X</param>
        /// <param name="relY">Смещение по Y</param>
        public PlRel Add(double relX, double relY)
        {
            Points.Add(Points.Last().Move(relX, relY));
            return this;
        }

        /// <summary>
        /// Добавление вершины (без относительно)
        /// </summary>
        /// <param name="pt">Вершина</param>
        public PlRel Add(Point2d pt)
        {
            Points.Add(pt);
            return this;
        }
    }
}
