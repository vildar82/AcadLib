// ReSharper disable once CheckNamespace
namespace Autodesk.AutoCAD.DatabaseServices
{
    using Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Viewport type.
    /// </summary>
    [PublicAPI]
    public static class ViewportExtensions
    {
        /// <summary>
        /// Gets the transformation matrix from the specified paper space viewport Display Coordinate System (DCS)
        /// to the paper space Display Coordinate System (PSDCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        public static Matrix3d DCS2PSDCS([NotNull] this Viewport vp)
        {
            return
                Matrix3d.Scaling(vp.CustomScale, vp.CenterPoint) *
                Matrix3d.Displacement(vp.CenterPoint.GetAsVector()) *
                Matrix3d.Displacement(vp.ViewCenter.Convert3d().GetAsVector().Negate());
        }

        /// <summary>
        /// Gets the transformation matrix from the specified model space viewport Display Coordinate System (DCS)
        /// to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The DCS to WDCS transformation matrix.</returns>
        public static Matrix3d DCS2WCS([NotNull] this Viewport vp)
        {
            return
                Matrix3d.Rotation(-vp.TwistAngle, vp.ViewDirection, vp.ViewTarget) *
                Matrix3d.Displacement(vp.ViewTarget - Point3d.Origin) *
                Matrix3d.PlaneToWorld(vp.ViewDirection);
        }

        /// <summary>
        /// Gets the transformation matrix from the Paper Space Display Coordinate System (PSDCS)
        /// to the specified paper space viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        public static Matrix3d PSDCS2DCS([NotNull] this Viewport vp)
        {
            return vp.DCS2PSDCS().Inverse();
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS)
        /// to the specified model space viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d WCS2DCS([NotNull] this Viewport vp)
        {
            return vp.DCS2WCS().Inverse();
        }
    }
}