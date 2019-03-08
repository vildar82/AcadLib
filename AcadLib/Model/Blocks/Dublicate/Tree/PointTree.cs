namespace AcadLib.Blocks.Dublicate.Tree
{
    using System;
    using JetBrains.Annotations;

    [PublicAPI]
    public struct PointTree : IEquatable<PointTree>
    {
        public static double tolerance = CheckDublicateBlocks.Tolerance.EqualPoint;
        public double X;
        public double Y;
        private int hX;
        private int hY;

        public PointTree(double x, double y)
        {
            X = x;
            Y = y;
            hX = X.GetHashCode();
            hY = Y.GetHashCode();
        }

        public bool Equals(PointTree other)
        {
            return Math.Abs(X - other.X) < tolerance &&
                   Math.Abs(Y - other.Y) < tolerance;
        }

        public override int GetHashCode()
        {
            return hX ^ hY;
        }
    }
}