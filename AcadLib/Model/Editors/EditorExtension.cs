namespace Autodesk.AutoCAD.EditorInput
{
    using AcadLib.Editors;
    using ApplicationServices.Core;
    using DatabaseServices;
    using Geometry;
    using JetBrains.Annotations;

    public static class EditorExtension
    {
        public static void Zoom([CanBeNull] this Editor ed, Extents3d ext)
        {
            if (ed == null)
                return;
            using (var view = ed.GetCurrentView())
            {
                ext.TransformBy(view.WorldToEye());
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        public static void ZoomExtents([CanBeNull] this Editor ed)
        {
            if (ed == null)
                return;
            var db = ed.Document.Database;
            var ext = (short)Application.GetSystemVariable("cvport") == 1
                ? new Extents3d(db.Pextmin, db.Pextmax)
                : new Extents3d(db.Extmin, db.Extmax);
            ed.Zoom(ext);
        }
    }
}