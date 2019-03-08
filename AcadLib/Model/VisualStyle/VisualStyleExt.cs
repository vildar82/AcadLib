namespace AcadLib.VisualStyle
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.GraphicsInterface;
    using JetBrains.Annotations;

    public static class VisualStyleExt
    {
        /// <summary>
        /// Тип визуального стиля текущего вида
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static VisualStyleType GetActiveVisualStyle([NotNull] this Database db)
        {
            using (var vt = (ViewportTable)db.ViewportTableId.Open(OpenMode.ForRead))
            using (var vtr = (ViewportTableRecord)vt["*Active"].Open(OpenMode.ForWrite))
            using (var vs = (DBVisualStyle)vtr.VisualStyleId.Open(OpenMode.ForRead))
            {
                return vs.Type;
            }
        }

        /// <summary>
        /// Установка визуального стиля текущему виду
        /// </summary>
        public static void SetActiveVisualStyle([NotNull] this Document doc, VisualStyleType style)
        {
            var db = doc.Database;
            var ed = doc.Editor;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var vt = (ViewportTable)db.ViewportTableId.GetObject(OpenMode.ForRead);
                var vtr = (ViewportTableRecord)vt["*Active"].GetObject(OpenMode.ForWrite);
                var dict = (DBDictionary)db.VisualStyleDictionaryId.GetObject(OpenMode.ForRead);
                vtr.VisualStyleId = dict.GetAt(GetStyleName(style));
                t.Commit();
                ed.UpdateTiledViewportsFromDatabase();
            }
        }

        [NotNull]
        private static string GetStyleName(VisualStyleType style)
        {
            switch (style)
            {
                case VisualStyleType.Flat: return "Flat";
                case VisualStyleType.FlatWithEdges: return "FlatWithEdges";
                case VisualStyleType.Gouraud: return "Gouraud";
                case VisualStyleType.GouraudWithEdges: return "GouraudWithEdges";
                case VisualStyleType.Wireframe2D: return "2dWireframe";
                case VisualStyleType.Wireframe3D: return "3dWireframe";
                case VisualStyleType.Hidden: return "Hidden";
                case VisualStyleType.Basic: return "Basic";
                case VisualStyleType.Realistic: return "Realistic";
                case VisualStyleType.Conceptual: return "Conceptual";
                case VisualStyleType.Custom: return "Custom";
                case VisualStyleType.Dim: return "Dim";
                case VisualStyleType.Brighten: return "Brighten";
                case VisualStyleType.Thicken: return "Thicken";
                case VisualStyleType.LinePattern: return "Linepattern";
                case VisualStyleType.FacePattern: return "Facepattern";
                case VisualStyleType.ColorChange: return "ColorChange";
                case VisualStyleType.FaceOnly: return "FaceOnly";
                case VisualStyleType.EdgeOnly: return "EdgeOnly";
                case VisualStyleType.DisplayOnly: return "DisplayOnly";
                case VisualStyleType.JitterOff: return "JitterOff";
                case VisualStyleType.OverhangOff: return "OverhangOff";
                case VisualStyleType.EdgeColorOff: return "EdgeColorOff";
                case VisualStyleType.ShadesOfGray: return "ShadesOfGray";
                case VisualStyleType.Sketchy: return "Sketchy";
                case VisualStyleType.XRay: return "XRay";
                case VisualStyleType.ShadedWithEdges: return "ShadedWithEdges";
                case VisualStyleType.Shaded: return "Shaded";
                case VisualStyleType.EmptyStyle: return "EmptyStyle";
                default: throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }
    }
}