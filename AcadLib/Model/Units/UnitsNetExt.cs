namespace AcadLib.Units
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UnitsNet;

    [Obsolete]
    [PublicAPI]
    public static class UnitsNetExt
    {
        public static Area Sum<TSource>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, Area> selector)
        {
            return source.Select(selector).Sum();
        }

        public static Area Sum([CanBeNull] this IEnumerable<Area> source)
        {
            var area = Area.Zero;
            if (source != null)
            {
                foreach (var current in source)
                {
                    area += current;
                }
            }

            return area;
        }

        public static Length Sum<TSource>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, Length> selector)
        {
            return source.Select(selector).Sum();
        }

        public static Length Sum([CanBeNull] this IEnumerable<Length> source)
        {
            var length = Length.Zero;
            if (source != null)
            {
                foreach (var current in source)
                {
                    length += current;
                }
            }

            return length;
        }
    }
}