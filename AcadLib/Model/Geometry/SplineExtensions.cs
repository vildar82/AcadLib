namespace AcadLib.Geometry
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using AcRx = Autodesk.AutoCAD.Runtime;

    /// <summary>
    /// Provides extension methods for the Spline type.
    /// </summary>
    public static class SplineExtensions
    {
        /// <summary>
        /// Gets the centroid of the closed planar spline.
        /// </summary>
        /// <param name="spl">The instance to which the method applies.</param>
        /// <returns>The centroid of the spline (WCS coordinates).</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNonPlanarEntity is thrown if the Spline is not planar.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNotApplicable is thrown if the Spline is not closed.</exception>
        public static Point3d Centroid([NotNull] this Spline spl)
        {
            if (!spl.IsPlanar)
                throw new AcRx.Exception(AcRx.ErrorStatus.NonPlanarEntity);
            if (spl.Closed != true)
                throw new AcRx.Exception(AcRx.ErrorStatus.NotApplicable);
            using (var curves = new DBObjectCollection())
            {
                curves.Add(spl);
                using (var dboc = Region.CreateFromCurves(curves))
                {
                    return ((Region)dboc[0]).Centroid();
                }
            }
        }
    }
}