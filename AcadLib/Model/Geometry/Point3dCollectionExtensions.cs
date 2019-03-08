namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Comparers;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Point3dCollection type.
    /// </summary>
    [PublicAPI]
    public static class Point3dCollectionExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains([NotNull] this Point3dCollection pts, Point3d pt)
        {
            return pts.Contains(pt, Tolerance.Global);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains([NotNull] this Point3dCollection pts, Point3d pt, Tolerance tol)
        {
            for (var i = 0; i < pts.Count; i++)
            {
                if (pt.IsEqualTo(pts[i], tol))
                    return true;
            }

            return false;
        }

        [NotNull]
        public static List<Point3d> RemoveDuplicate([NotNull] this List<Point3d> pts, Tolerance tol)
        {
            return pts.Distinct(new Point3dEqualityComparer(tol.EqualPoint)).ToList();
        }

        /// <summary>
        /// Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        [Obsolete("Use RemoveDuplicate List<Point3d>")]
        public static void RemoveDuplicate([NotNull] this Point3dCollection pts)
        {
            pts.RemoveDuplicate(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        [Obsolete("Use RemoveDuplicate List<Point3d>")]
        public static void RemoveDuplicate([NotNull] this Point3dCollection pts, Tolerance tol)
        {
            var ptsClean = RemoveDuplicate(pts.Cast<Point3d>().ToList(), tol);
            pts.Clear();
            foreach (var pt in ptsClean)
            {
                pts.Add(pt);
            }
        }

        /// <summary>
        /// Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        /// ArgumentException is thrown if the collection is null or empty.</exception>
        public static Extents3d ToExtents3d([NotNull] this Point3dCollection pts)
        {
            return pts.Cast<Point3d>().ToExtents3d();
        }

        /// <summary>
        /// Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        /// ArgumentException is thrown if the sequence is null or empty.</exception>
        public static Extents3d ToExtents3d([NotNull] this IEnumerable<Point3d> pts)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            if (pts.Any() != true)
                throw new ArgumentException("Null or empty sequence");

            // ReSharper disable once PossibleMultipleEnumeration
            var pt = pts.First();

            // ReSharper disable once PossibleMultipleEnumeration
            return pts.Aggregate(new Extents3d(pt, pt), (e, p) =>
            {
                e.AddPoint(p);
                return e;
            });
        }
    }
}