namespace AcadLib.VisualStyle
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.GraphicsInterface;
    using JetBrains.Annotations;

    public class VisualStyleUsing : IDisposable
    {
        private readonly Document doc;
        private readonly VisualStyleType previousStyle;

        public VisualStyleUsing([NotNull] Document doc, VisualStyleType style)
        {
            previousStyle = doc.Database.GetActiveVisualStyle();
            doc.SetActiveVisualStyle(style);
            this.doc = doc;
        }

        public void Dispose()
        {
            doc.SetActiveVisualStyle(previousStyle);
        }
    }
}