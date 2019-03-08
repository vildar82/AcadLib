// ReSharper disable once CheckNamespace
namespace AcadLib.Colors
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    [PublicAPI]
    public static class ColorBookHelper
    {
        private static Database db;
        private static Document doc;
        private static Editor ed;

        public static double CellHeight { get; set; }

        public static double CellWidth { get; set; }

        public static ObjectId IdTextStylePik { get; set; }

        public static double Margin { get; set; }

        public static double TextHeight { get; set; }

        public static void GenerateNCS()
        {
            doc = Application.DocumentManager.Add(string.Empty);
            using (doc.LockDocument())
            {
                ed = doc.Editor;
                db = doc.Database;
                IdTextStylePik = db.GetTextStylePIK();

                using (var t = db.TransactionManager.StartTransaction())
                {
                    // Форма стартовых настроек
                    Options.Show();

                    // Чтение палитры NCS
                    var colorBookNcs = ColorBook.ReadFromFile(Options.Instance.NCSFile);

                    // Запрос точки начала генерации палитр цветов
                    var ptStart = ed.GetPointWCS("Точка вставки");

                    // Расположение цветов в модели
                    var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    PlacementColors(cs, colorBookNcs, ptStart);

                    t.Commit();
                }
            }
        }

        private static void AddLayout(
            Point3d pt,
            int layout,
            double width,
            double height,
            [NotNull] BlockTableRecord cs,
            [NotNull] Transaction t)
        {
            // Полилиния контура листа
            var pl = new Polyline(4);
            pl.AddVertexAt(0, new Point2d(pt.X, pt.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(pt.X + width, pt.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(pt.X + width, pt.Y - height), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(pt.X, pt.Y - height), 0, 0, 0);
            pl.Closed = true;
            pl.SetDatabaseDefaults();

            cs.AppendEntity(pl);
            t.AddNewlyCreatedDBObject(pl, true);

            // Подпись номера листа
            var textHeight = height * 0.008;
            var ptText = new Point3d(pt.X + textHeight * 0.5, pt.Y - textHeight * 1.5, 0);

            var text = new DBText();
            text.SetDatabaseDefaults();
            text.Height = textHeight;
            text.TextStyleId = IdTextStylePik;
            text.TextString = layout.ToString();
            text.Position = ptText;

            cs.AppendEntity(text);
            t.AddNewlyCreatedDBObject(text, true);
        }

        private static void CreateLayout(
            [NotNull] Polyline pl,
            int layout,
            double widthLay,
            double heightLay,
            [NotNull] Transaction t)
        {
            var lm = LayoutManager.Current;
            var idLay = lm.CreateLayout(layout.ToString());

            var extPl = pl.GeometricExtents;

            using (var lay = (Layout)idLay.GetObject(OpenMode.ForWrite))
            {
                var btrLay = (BlockTableRecord)lay.BlockTableRecordId.GetObject(OpenMode.ForWrite);

                var view = new Viewport();
                view.SetDatabaseDefaults();

                btrLay.AppendEntity(view);
                t.AddNewlyCreatedDBObject(view, true);

                view.Width = widthLay;
                view.Height = heightLay;
                view.CenterPoint = new Point3d(widthLay * 0.5, heightLay * 0.5, 0);
                view.On = true;

                view.ViewCenter = extPl.Center().Convert2d();

                var hvp = extPl.MaxPoint.Y - extPl.MinPoint.Y;
                var wvp = extPl.MaxPoint.X - extPl.MinPoint.X;

                var aspect = view.Width / view.Height;

                if (wvp / hvp > aspect)
                {
                    hvp = wvp / aspect;
                }

                view.ViewHeight = hvp;
                view.CustomScale = 1;
            }
        }

        private static void PlacementColors(BlockTableRecord cs, [NotNull] ColorBook colorBookNcs, Point3d ptStart)
        {
            var t = db.TransactionManager.TopTransaction;

            double widthLayout = Options.Instance.Width;
            double heightLayout = Options.Instance.Height;

            var widthCells = widthLayout - widthLayout * 0.1;
            var heightCells = heightLayout - heightLayout * 0.1;

            var columns = Options.Instance.Columns;
            var rows = Options.Instance.Rows;

            // Определение длины и высоты для каждой ячейки цвета
            CellWidth = widthCells / columns;
            CellHeight = heightCells / rows;

            Margin = CellWidth * 0.1;
            TextHeight = Convert.ToInt32(CellWidth * 0.09);

            var ptLayout = ptStart;

            var progress = new ProgressMeter();
            progress.SetLimit(colorBookNcs.Colors.Count);
            progress.Start("Расстановка цветов...");

            // Кол листов
            double cellsCount = columns * rows;
            var layCount = Convert.ToInt32(Math.Ceiling(colorBookNcs.Colors.Count / cellsCount));

            var index = 0;
            for (var l = 1; l < layCount + 1; l++)
            {
                // создание рамки листа
                AddLayout(ptLayout, l, widthLayout, heightLayout, cs, t);
                var ptCellFirst = new Point2d(ptLayout.X + (widthLayout - widthCells) * 0.5,
                    ptLayout.Y - (heightLayout - heightCells) * 0.5);

                // Заполнение ячейками цветов
                for (var r = 0; r < rows; r++)
                {
                    var isBreak = false;
                    for (var c = 0; c < columns; c++)
                    {
                        index++;
                        if (index == colorBookNcs.Colors.Count)
                        {
                            isBreak = true;
                            break;
                        }

                        var colorItem = colorBookNcs.Colors[index];
                        var ptCell = new Point2d(ptCellFirst.X + c * CellWidth, ptCellFirst.Y - r * CellHeight);
                        colorItem.Create(ptCell, cs, t);
                        progress.MeterProgress();
                    }

                    if (isBreak)
                    {
                        break;
                    }
                }

                ptLayout = new Point3d(ptLayout.X, ptLayout.Y + heightLayout, 0);
            }

            progress.Stop();
        }
    }
}