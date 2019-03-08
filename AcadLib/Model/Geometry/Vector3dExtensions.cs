namespace AcadLib.Geometry
{
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Provides extension methods for the Vector3d type.
    /// </summary>
    public static class Vector3dExtensions
    {
        /// <summary>
        /// Projects the vector on the WCS
        /// </summary>
        /// <param name="vec">The vector to project.</param>
        /// <returns>The projected vector.</returns>
        public static Vector3d Flatten(this Vector3d vec)
        {
            return new Vector3d(vec.X, vec.Y, 0.0);
        }

        public static Vector2d Convert2d(this Vector3d vec)
        {
            using (var plane = new Plane())
            {
                return vec.Convert2d(plane);
            }
        }

        public static Vector3d Convert3d(this Vector2d vec)
        {
            return new Vector3d(vec.X, vec.Y, 0);
        }
    }
}