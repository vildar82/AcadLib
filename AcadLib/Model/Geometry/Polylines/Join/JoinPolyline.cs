namespace AcadLib.Geometry.Polylines.Join
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    class JoinPolyline
    {
        public Point3d Pt { get; set; }

        public Polyline Pl { get; set; }

        public bool IsStartPt { get; set; }

        public JoinPolyline OtherEndJoinPolyline { get; set; }

        public bool IsActualPt { get; set; } = true;
    }
}