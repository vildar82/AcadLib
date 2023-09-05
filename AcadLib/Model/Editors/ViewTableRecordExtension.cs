namespace AcadLib.Editors
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public static class ViewTableRecordExtension
    {
        public static Matrix3d EyeToWorld(this ViewTableRecord view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            return
                Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                Matrix3d.Displacement(view.Target - Point3d.Origin) *
                Matrix3d.PlaneToWorld(view.ViewDirection);
        }

        public static Matrix3d WorldToEye(this ViewTableRecord view)
        {
            return view.EyeToWorld().Inverse();
        }
    }
}