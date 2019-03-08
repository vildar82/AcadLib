namespace AcadLib.Geometry.Polylines.Join
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class JoinPolylines
    {
        /// <summary>
        /// Объединение полилиний
        /// </summary>
        /// <param name="lines">Полилинии</param>
        /// <param name="joined">Объекдиненные полилинии</param>
        /// <param name="disposePls">Очищать объекдиненные полилинии?</param>
        /// <param name="wedding">Прополка</param>
        /// <param name="tolerance"></param>
        public static void Join(
            [NotNull] this List<Polyline> lines,
            ref List<Polyline> joined,
            bool disposePls = true,
            bool wedding = true,
            Tolerance tolerance = default)
        {
            if (lines.Count == 0)
            {
                return;
            }

            var fl = lines.First();
            lines.Remove(fl);
            joined.Add(fl);
            List<int> icol;
            try
            {
                icol = fl.JoinEntities(lines.ToArray<Entity>()).Cast<int>().ToList();
                if (wedding)
                    fl.Wedding(tolerance, false, true);
            }
            catch
            {
                Join(lines, ref joined, disposePls, wedding, tolerance);
                return;
            }

            if (icol.Count == 0)
            {
                joined.AddRange(lines);
                return;
            }

            var removePls = icol.Select(s => lines[s]).ToList();
            foreach (var pl in removePls)
            {
                lines.Remove(pl);
                if (disposePls)
                {
                    pl.Dispose();
                }
            }

            Join(lines, ref joined, disposePls, wedding, tolerance);
        }
    }
}