namespace AcadLib.Tables
{
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using JetBrains.Annotations;
    using Jigs;
    using NetLib;

    /// <summary>
    /// Построение таблицы
    /// 1 CalcRows
    /// 2 Create
    /// 3 Insert
    /// </summary>
    [PublicAPI]
    public abstract class CreateTable : ICreateTable
    {
        protected readonly Database db;
        protected readonly double scale;

        public CreateTable(Database db)
        {
            this.db = db;
            scale = Scale.ScaleHelper.GetCurrentAnnoScale(db);
        }

        public string Layer { get; set; }

        public LineWeight LwBold { get; set; } = LineWeight.LineWeight050;

        public int RowHeight { get; set; } = 8;

        public int NumColumns { get; set; }

        public int NumRows { get; set; }

        /// <summary>
        /// Имя стиля из шаблона. Если пусто, то ПИК
        /// </summary>
        public string StyleName { get; set; }

        /// <summary>
        /// Имя шаблона
        /// </summary>
        public string TemplateName { get; set; }

        public string Title { get; set; }

        public double TitleHeight { get; set; } = 5;

        public abstract void CalcRows();

        [NotNull]
        public Table Create()
        {
            var table = GetTable();
            return table;
        }

        public void Insert([NotNull] Table table, [NotNull] Document doc)
        {
            insertTable(table, doc);
        }

        protected abstract void FillCells(Table table);

        /// <summary>
        /// перед вызовом необходимо заполнить свойства - Title, NumRows, NumColumns
        /// </summary>
        [NotNull]
        protected Table GetTable()
        {
            var table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = StyleName.IsNullOrEmpty()
                ? db.GetTableStylePIK()
                : db.GetTableStylePIK(StyleName, TemplateName);

            if (!string.IsNullOrEmpty(Layer))
            {
                var layerId = Layers.LayerExt.CheckLayerState(Layer);
                table.LayerId = layerId;
            }

            table.SetSize(NumRows, NumColumns);
            table.SetBorders(LwBold);
            table.SetRowHeight(RowHeight);

            // Название таблицы
            var rowTitle = table.Cells[0, 0];
            rowTitle.Alignment = CellAlignment.MiddleCenter;
            rowTitle.TextHeight = TitleHeight;
            if (Title != null)
                rowTitle.TextString = Title;

            // Строка заголовков столбцов
            var rowHeaders = table.Rows[1];
            rowHeaders.Height = 15;
            rowHeaders.Borders.Bottom.LineWeight = LwBold;

            // Заполнение шапки столбцов и их ширин
            SetColumnsAndCap(table.Columns);

            // Заполнение строк
            FillCells(table);

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = LwBold;

            table.GenerateLayout();
            return table;
        }

        protected abstract void SetColumnsAndCap(ColumnsCollection columns);

        private void insertTable([NotNull] Table table, [NotNull] Document doc)
        {
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var jigTable = new TableJig(table, scale, "Вставка таблицы");
                if (doc.Editor.Drag(jigTable).Status == PromptStatus.OK)
                {
                    var cs = (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                    cs.AppendEntity(table);
                    t.AddNewlyCreatedDBObject(table, true);
                }

                t.Commit();
            }
        }
    }
}
