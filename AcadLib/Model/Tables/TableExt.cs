namespace AcadLib
{
    using System;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Geometry;
    using Hatches;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class TableExt
    {
        public static LineWeight LwDataRow = LineWeight.LineWeight018;

        public static void SetTextString([NotNull] this Cell cell, string text, double widthFactor = 1)
        {
            cell.TextString = Math.Abs(widthFactor - 1) < 0.0001 ? text : $@"{{\W{widthFactor}{text}}}";
        }

        [NotNull]
        public static Cell SetValue([NotNull] this Cell cell, [CanBeNull] object value)
        {
            if (value == null)
                return cell;
            cell.SetValue(value, ParseOption.ParseOptionNone);
            return cell;
        }

        public static Cell MoveDown([NotNull] this Cell cell)
        {
            return cell.ParentTable.Cells[cell.Row + 1, cell.Column];
        }

        public static Cell MoveRight([NotNull] this Cell cell)
        {
            return cell.ParentTable.Cells[cell.Row, cell.Column + 1];
        }

        public static void SetBorders([NotNull] this Table table, LineWeight lw)
        {
            if (table.Rows.Count < 2)
                return;

            var rowTitle = table.Rows[0];
            SetRowTitle(rowTitle);

            var rowHead = table.Rows[1];
            SetRowHeader(rowHead, lw);

            foreach (var row in table.Rows.Skip(2))
            {
                SetRowData(row, lw);
            }
        }

        /// <summary>
        /// Вставка штриховки в ячеку таблицы.
        /// Должна быть запущена транзакция.
        /// Таблица должна быть в базе чертежа.
        /// Штриховка добавляется в базу.
        /// </summary>
        [NotNull]
        public static Hatch SetCellHatch(
            [NotNull] this Cell cell,
            int colorIndex = 0,
            LineWeight lineWeight = LineWeight.LineWeight015,
            double patternScale = 1,
            string standartPattern = "LINE",
            double patternAngleRad = 0)
        {
            var table = cell.ParentTable;
            table.RecomputeTableBlock(true);
            var btr = (BlockTableRecord)table.OwnerId.GetObject(OpenMode.ForWrite);
            var cellExt = OffsetExtToMarginCell(cell.GetExtents().ToExtents3d(), cell);
            using (var cellPl = cellExt.GetPolyline())
            {
                var h = cellPl.GetPoints().CreateHatch();
                h.PatternAngle = patternAngleRad;
                h.PatternScale = patternScale;
                h.SetHatchPattern(HatchPatternType.PreDefined, standartPattern);
                h.ColorIndex = colorIndex;
                h.LineWeight = lineWeight;
                h.Linetype = SymbolUtilityServices.LinetypeContinuousName;
                var t = btr.Database.TransactionManager.TopTransaction;
                btr.AppendEntity(h);
                t.AddNewlyCreatedDBObject(h, true);
                h.EvaluateHatch(true);
                return h;
            }
        }

        private static Extents3d OffsetExtToMarginCell(Extents3d ext, [NotNull] Cell cell)
        {
            return new Extents3d(
                new Point3d(ext.MinPoint.X - cell.Borders.Horizontal.Margin ?? 0, ext.MinPoint.Y - cell.Borders.Top.Margin ?? 0, 0),
                new Point3d(ext.MaxPoint.X + cell.Borders.Horizontal.Margin ?? 0, ext.MaxPoint.Y + cell.Borders.Top.Margin ?? 0, 0)
                );
        }

        private static void SetCell([NotNull] CellBorder cell, LineWeight lw, bool visible)
        {
            // cell.Overrides = GridProperties.Visibility | GridProperties.LineWeight;
            cell.LineWeight = lw;
            cell.IsVisible = visible;
        }

        private static void SetRowData([NotNull] Row row, LineWeight lw)
        {
            SetCell(row.Borders.Bottom, LwDataRow, true);
            SetCell(row.Borders.Horizontal, LwDataRow, true);
            SetCell(row.Borders.Left, lw, true);
            SetCell(row.Borders.Right, lw, true);
            SetCell(row.Borders.Top, LwDataRow, true);
            SetCell(row.Borders.Vertical, lw, true);
        }

        private static void SetRowHeader([NotNull] Row row, LineWeight lw)
        {
            SetCell(row.Borders.Bottom, lw, true);
            SetCell(row.Borders.Horizontal, lw, true);
            SetCell(row.Borders.Left, lw, true);
            SetCell(row.Borders.Right, lw, true);
            SetCell(row.Borders.Top, lw, true);
            SetCell(row.Borders.Vertical, lw, true);
        }

        private static void SetRowTitle([NotNull] Row row)
        {
            SetCell(row.Borders.Bottom, LineWeight.LineWeight000, false);
            SetCell(row.Borders.Horizontal, LineWeight.LineWeight000, false);
            SetCell(row.Borders.Left, LineWeight.LineWeight000, false);
            SetCell(row.Borders.Right, LineWeight.LineWeight000, false);
            SetCell(row.Borders.Top, LineWeight.LineWeight000, false);
            SetCell(row.Borders.Vertical, LineWeight.LineWeight000, false);
        }

        /// <summary>
        /// Объединение одинаковых строк в колонке
        /// </summary>
        /// <param name="t">Таблица</param>
        /// <param name="col">Колонка</param>
        /// <param name="startRow">Стартовая строка сравнения</param>
        public static void MergeCol(this Table t, int col, int startRow)
        {
            Cell prevCell = null;
            var prewRow = startRow;
            for (var r = startRow; r < t.Rows.Count; r++)
            {
                var cel = t.Cells[r, col];
                if (!cel.TextString.EqualsIgnoreCase(prevCell?.TextString))
                {
                    Merge(t, col, prewRow, col, r - 1);
                    prevCell = cel;
                    prewRow = r;
                }
            }

            var lastRow = t.Rows.Count - 1;
            var lastCel = t.Cells[lastRow, col];
            if (lastCel.TextString.EqualsIgnoreCase(prevCell?.TextString))
            {
                Merge(t, col, prewRow, col, lastRow);
            }
        }

        private static void Merge(Table table, int colStart, int rowStart, int colEnd, int rowEnd)
        {
            if (colEnd - colStart <= 0 && rowEnd - rowStart <= 0)
                return;
            var rangew = CellRange.Create(table, rowStart, colStart, rowEnd, colEnd);
            table.MergeCells(rangew);
        }
    }
}