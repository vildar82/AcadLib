using AcadLib.Geometry;

namespace AcadLib.Hatches
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public class HatchLoopPl : IDisposable
    {
        public Curve Loop { get; set; }

        public HatchLoopTypes Types { get; set; }

        public void Dispose()
        {
            Loop?.Dispose();
        }

        [CanBeNull]
        public Polyline GetPolyline()
        {
            switch (Loop)
            {
                case Polyline pl: return pl;
                case Circle circle : return circle.ToPolyline();
            }

            return null;
        }
    }
}
