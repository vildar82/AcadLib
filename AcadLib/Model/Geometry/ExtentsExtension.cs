// ReSharper disable once CheckNamespace

namespace Autodesk.AutoCAD.DatabaseServices
{
    using System.Collections.Generic;
    using Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class ExtentsExtension
    {
        /// <summary>
        /// Пересечение границ
        /// </summary>
        /// <param name="ext">Граница</param>
        /// <param name="otherExt">Другая граница</param>
        /// <returns>Граница пересечения</returns>
        public static Extents2d Intersect(this Extents2d ext, Extents2d otherExt)
        {
            if (ext.MaxPoint.X <= otherExt.MinPoint.X ||
                ext.MaxPoint.Y <= otherExt.MinPoint.Y ||
                ext.MinPoint.X >= otherExt.MaxPoint.X ||
                ext.MinPoint.Y >= otherExt.MaxPoint.Y)
                return new Extents2d();

            var d = ext.MinPoint.X - otherExt.MinPoint.X;
            var ixMin = d < 0 ? ext.MinPoint.X - d : ext.MinPoint.X;

            d = ext.MaxPoint.X - otherExt.MaxPoint.X;
            var ixMax = d > 0 ? ext.MaxPoint.X - d : ext.MaxPoint.X;

            d = ext.MinPoint.Y - otherExt.MinPoint.Y;
            var iyMin = d < 0 ? ext.MinPoint.Y - d : ext.MinPoint.Y;

            d = ext.MaxPoint.Y - otherExt.MaxPoint.Y;
            var iyMax = d > 0 ? ext.MaxPoint.Y - d : ext.MaxPoint.Y;

            return new Extents2d(ixMin, iyMin, ixMax, iyMax);
        }

        /// <summary>
        /// Общая граница
        /// </summary>
        /// <param name="exts"></param>
        /// <returns></returns>
        public static Extents3d Union([NotNull] this IEnumerable<Extents3d> exts)
        {
            var ext = new Extents3d();
            foreach (var extents3D in exts)
            {
                ext.AddExtents(extents3D);
            }

            return ext;
        }

        /// <summary>
        /// Определение точки центра границы Extents3d
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static Point3d Center(this Extents3d ext)
        {
            return new Point3d((ext.MaxPoint.X + ext.MinPoint.X) * 0.5,
                (ext.MaxPoint.Y + ext.MinPoint.Y) * 0.5, 0);
        }

        public static Extents2d Convert2d(this Extents3d ext)
        {
            return new Extents2d(ext.MinPoint.Convert2d(), ext.MaxPoint.Convert2d());
        }

        public static Extents3d Convert3d(this Extents2d ext)
        {
            return new Extents3d(ext.MinPoint.Convert3d(), ext.MaxPoint.Convert3d());
        }

        /// <summary>
        /// Длина диагонали границ (расстояние между точками MaxPoint и MinPoint)
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static double Diagonal(this Extents3d ext)
        {
            return (ext.MaxPoint - ext.MinPoint).Length;
        }

        public static double Diagonal(this Extents2d ext)
        {
            return (ext.MaxPoint - ext.MinPoint).Length;
        }

        public static double GetArea(this Extents3d ext)
        {
            return (ext.MaxPoint.X - ext.MinPoint.X) * (ext.MaxPoint.Y - ext.MinPoint.Y);
        }

        public static double GetArea(this Extents2d ext)
        {
            return (ext.MaxPoint.X - ext.MinPoint.X) * (ext.MaxPoint.Y - ext.MinPoint.Y);
        }

        /// <summary>
        /// Расстояние по Y
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static double GetHeight(this Extents3d ext)
        {
            return ext.MaxPoint.Y - ext.MinPoint.Y;
        }

        /// <summary>
        /// Расстояние по X
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static double GetLength(this Extents3d ext)
        {
            return ext.MaxPoint.X - ext.MinPoint.X;
        }

        [NotNull]
        public static Polyline GetPolyline(this Extents2d ext)
        {
            return ext.Convert3d().GetPolyline();
        }

        [NotNull]
        public static Polyline GetPolyline(this Extents3d ext)
        {
            var pl = new Polyline();
            pl.AddVertexAt(0, ext.MinPoint.Convert2d(), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(ext.MinPoint.X, ext.MaxPoint.Y), 0, 0, 0);
            pl.AddVertexAt(2, ext.MaxPoint.Convert2d(), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(ext.MaxPoint.X, ext.MinPoint.Y), 0, 0, 0);
            pl.Closed = true;
            return pl;
        }

        [NotNull]
        public static List<Point3d> GetRegularGridPoints(this Extents3d ext, double len)
        {
            var ptsGrid = new List<Point3d>();
            var extL = ext.GetLength();
            var extH = ext.GetHeight();
            var iX = (int)(extL / len) + 1;
            var iY = (int)(extH / len) + 1;
            var dX = extL / iX;
            var dY = extH / iY;
            for (var x = 0; x < iX; x++)
            {
                for (var y = 0; y < iY; y++)
                {
                    ptsGrid.Add(new Point3d(ext.MinPoint.X + len * 0.5 + x * dX, ext.MinPoint.Y + len * 0.5 + y * dY, 0));
                }
            }

            return ptsGrid;
        }

        /// <summary>
        /// Попадает ли точка внутрь границы
        /// </summary>
        /// <returns></returns>
        public static bool IsPointInBounds(this Extents3d ext, Point3d pt)
        {
            return pt.X > ext.MinPoint.X && pt.Y > ext.MinPoint.Y &&
                   pt.X < ext.MaxPoint.X && pt.Y < ext.MaxPoint.Y;
        }

        /// <summary>
        /// Попадает ли точка внутрь границы
        /// </summary>
        /// <returns></returns>
        public static bool IsPointInBounds(this Extents3d ext, Point3d pt, double tolerance)
        {
            return pt.X > ext.MinPoint.X - tolerance && pt.Y > ext.MinPoint.Y - tolerance &&
                   pt.X < ext.MaxPoint.X + tolerance && pt.Y < ext.MaxPoint.Y + tolerance;
        }

        /// <summary>
        /// Попадает ли точка внутрь границы
        /// </summary>
        public static bool IsPointInBounds(this Extents2d ext, Point2d pt, double tolerance = double.Epsilon)
        {
            return pt.X > ext.MinPoint.X - tolerance && pt.Y > ext.MinPoint.Y - tolerance &&
                   pt.X < ext.MaxPoint.X + tolerance && pt.Y < ext.MaxPoint.Y + tolerance;
        }

        public static Extents3d Offset(this Extents3d ext, double percent = 50)
        {
            var dX = ext.GetLength() * (percent / 100) * 0.5;
            var dY = ext.GetHeight() * (percent / 100) * 0.5;
            return new Extents3d(
                new Point3d(ext.MinPoint.X - dX, ext.MinPoint.Y - dY, 0),
                new Point3d(ext.MaxPoint.X + dX, ext.MaxPoint.Y + dY, 0)
            );
        }

        public static Extents3d OffsetAbs(this Extents3d ext, double offset = 10)
        {
            return new Extents3d(
                new Point3d(ext.MinPoint.X - offset, ext.MinPoint.Y - offset, 0),
                new Point3d(ext.MaxPoint.X + offset, ext.MaxPoint.Y + offset, 0)
            );
        }
    }
}
