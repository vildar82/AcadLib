namespace AcadLib.Comparers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class Extents3dComparer : IEqualityComparer<Extents3d>
    {
        public Extents3dComparer()
        {
        }

        public Extents3dComparer(Tolerance tolerance)
        {
            Tolerance = tolerance;
        }

        public static Extents3dComparer Default1 { get; } = new Extents3dComparer(new Tolerance(0.1, 1));

        public Tolerance Tolerance { get; set; } = Tolerance.Global;

        public bool Equals(Extents3d x, Extents3d y)
        {
            return x.IsEqualTo(y, Tolerance);
        }

        public int GetHashCode(Extents3d obj)
        {
            return obj.GetHashCode();
        }
    }
}