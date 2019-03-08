namespace AcadLib.Geometry
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Polyline3d type.
    /// </summary>
    [PublicAPI]
    public static class Polyline3dExtensions
    {
        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline3d along the given plane.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <returns>The projected polyline</returns>
        [CanBeNull]
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d pline, [NotNull] Plane plane)
        {
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline3d parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        [CanBeNull]
        public static Polyline GetProjectedPolyline(this Polyline3d pline, [NotNull] Plane plane, Vector3d direction)
        {
            return plane.Normal.IsPerpendicularTo(direction, new Tolerance(1e-9, 1e-9))
                ? null
                : GeomExt.ProjectPolyline(pline, plane, direction);
        }
    }
}