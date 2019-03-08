namespace AcadLib.Geometry
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public static class CircleExt
    {
        public static Polyline ToPolyline(this Circle circle, int aproximateCount = 10)
        {
            var pts = new List<Point2d> { circle.StartPoint.Convert2d() };
            var delta = circle.EndParam / aproximateCount;
            var param = delta;
            for (var i = 0; i < aproximateCount; i++)
            {
                pts.Add(circle.GetPointAtParameter(param).Convert2d());
                param += delta;
            }

            return pts.CreatePolyline(true);
        }
    }
}
