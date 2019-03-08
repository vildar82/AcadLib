namespace AcadLib.RTree.SpatialIndex
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class RectangleExt
    {
        public static double GetArea([NotNull] this Rectangle rec)
        {
            return rec.Area();
        }

        public static Extents3d GetExtents([NotNull] this Rectangle rec)
        {
            return new Extents3d(new Point3d(rec._min[0], rec._min[1], 0),
                new Point3d(rec._max[0], rec._max[1], 0));
        }

        [NotNull]
        public static Rectangle Offset([NotNull] this Rectangle rec, int offset)
        {
            return new Rectangle(rec._min[0] - offset, rec._min[1] - offset, rec._max[0] + offset, rec._max[1] + offset, 0, 0);
        }

        [NotNull]
        public static Rectangle Union([NotNull] this List<Rectangle> recs)
        {
            var fr = recs[0];
            var min = fr._min;
            var max = fr._max;
            foreach (var rec in recs.Skip(1))
            {
                if (rec._min[0] < min[0])
                    min[0] = rec._min[0];
                if (rec._min[1] < min[1])
                    min[1] = rec._min[1];
                if (rec._max[0] > max[0])
                    max[0] = rec._max[0];
                if (rec._max[1] > max[1])
                    max[1] = rec._max[1];
            }

            return new Rectangle(min, max);
        }
    }
}