// ReSharper disable once CheckNamespace
namespace AcadLib.Colors
{
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class ColorItem
    {
        public ColorItem(string name, byte r, byte g, byte b)
        {
            Name = name;
            Color = Color.FromRgb(r, g, b);
        }

        public Color Color { get; set; }

        public string Name { get; set; }

        public void Create(Point2d ptCell, [NotNull] BlockTableRecord cs, [NotNull] Transaction t)
        {
            var cellWidth = ColorBookHelper.CellWidth;
            var cellHeight = ColorBookHelper.CellHeight;
            var margin = ColorBookHelper.Margin;
            var marginHalf = margin * 0.5;

            var ptText = new Point3d(ptCell.X + cellWidth * 0.5, ptCell.Y - ColorBookHelper.TextHeight - margin, 0);

            var pl = new Polyline(4);
            pl.AddVertexAt(0, new Point2d(ptCell.X + margin, ptText.Y - marginHalf), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(ptCell.X + cellWidth - margin, ptText.Y - marginHalf), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(ptCell.X + cellWidth - margin, ptCell.Y - cellHeight + margin), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(ptCell.X + margin, ptCell.Y - cellHeight + margin), 0, 0, 0);
            pl.Closed = true;
            pl.SetDatabaseDefaults();
            pl.Color = Color;

            cs.AppendEntity(pl);
            t.AddNewlyCreatedDBObject(pl, true);

            var h = new Hatch();
            h.SetDatabaseDefaults();
            h.SetHatchPattern(HatchPatternType.PreDefined, "Solid");
            h.Annotative = AnnotativeStates.False;

            cs.AppendEntity(h);
            t.AddNewlyCreatedDBObject(h, true);

            h.Associative = true;
            h.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection(new[] { pl.Id }));
            h.Color = Color;
            h.EvaluateHatch(true);

            var text = new DBText();
            text.SetDatabaseDefaults();
            text.HorizontalMode = TextHorizontalMode.TextCenter;
            text.Annotative = AnnotativeStates.False;
            text.Height = ColorBookHelper.TextHeight;
            text.AlignmentPoint = ptText;
            text.AdjustAlignment(cs.Database);
            text.TextStyleId = ColorBookHelper.IdTextStylePik;
            text.TextString = Name;

            cs.AppendEntity(text);
            t.AddNewlyCreatedDBObject(text, true);
        }
    }
}