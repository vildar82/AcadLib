using System.Linq;
using Autodesk.AutoCAD.EditorInput;

namespace AcadLib.Geometry
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Region type.
    /// </summary>
    public static class RegionExtensions
    {
        /// <summary>
        /// Gets the centroid of the region.
        /// </summary>
        /// <param name="reg">The instance to which the method applies.</param>
        /// <returns>The centroid of the region (WCS coordinates).</returns>
        public static Point3d Centroid([NotNull] this Region reg)
        {
            using (var sol = new Solid3d())
            {
                sol.Extrude(reg, 2.0, 0.0);
                return sol.MassProperties.Centroid - reg.Normal;
            }
        }

        /// <summary>
        /// Создать штриховку из региона (области).
        /// Используется коммандный метод - ed.Command!!! 
        /// </summary>
        /// <param name="reg">Регион</param>
        /// <param name="ed">Редактор</param>
        /// <returns></returns>
        public static ObjectId ConvertToHatch(this Region reg, Editor ed)
        {
            using (var added = new AddedObjects(reg.Database))
            {
                ed.Command("_-HATCH", "_S", reg.Id, "", "");
                return added.Added.FirstOrDefault(o => o.ObjectClass == General.ClassHatch);
            }
        }
    }
}