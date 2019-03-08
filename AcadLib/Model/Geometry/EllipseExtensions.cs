// ReSharper disable once CheckNamespace
namespace Autodesk.AutoCAD.DatabaseServices
{
    using AcadLib.Geometry;
    using Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides extension methods for the Ellipse type.
    /// </summary>
    [PublicAPI]
    public static class EllipseExtensions
    {
        /// <summary>
        /// Generates a polyline to approximate an ellipse.
        /// </summary>
        /// <param name="ellipse">The ellipse to be approximated</param>
        /// <returns>A new Polyline instance</returns>
        [NotNull]
        public static Polyline ToPolyline([NotNull] this Ellipse ellipse)
        {
            var pline = new PolylineSegmentCollection(ellipse).ToPolyline();
            pline.Closed = ellipse.Closed;
            pline.Normal = ellipse.Normal;
            pline.Elevation = ellipse.Center.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, ellipse.Normal))).Z;
            return pline;
        }
    }
}