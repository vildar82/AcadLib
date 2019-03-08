namespace AcadLib.Visual
{
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public class VisualOption
    {
        public VisualOption(System.Drawing.Color c, Point3d pos, byte alpha = 0)
        {
            SetColor(c);
            Position = pos;
            Transparency = new Transparency(alpha);
        }

        public VisualOption(System.Drawing.Color c, byte alpha = 0)
        {
            SetColor(c);
            Transparency = new Transparency(alpha);
        }

        public Color Color { get; set; }

        public Transparency Transparency { get; set; }

        public Point3d Position { get; set; }

        public LineWeight? LineWeight { get; set; }

        public double? TextHeight { get; set; }

        public void SetColor(System.Drawing.Color color)
        {
            Color = Color.FromColor(color);
        }
    }
}