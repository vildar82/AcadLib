namespace System.Data
{
    using AcadLib;
    using Autodesk.AutoCAD.DatabaseServices;
    using Collections.Generic;
    using IO;
    using JetBrains.Annotations;
    using Linq;

    [PublicAPI]
    public static class DataExtensions
    {
        // Gets the column names collection of the datatable
        [NotNull]
        public static IEnumerable<string> GetColumnNames([NotNull] this DataTable dataTbl)
        {
            return dataTbl.Columns.Cast<DataColumn>().Select(col => col.ColumnName);
        }

        // Creates an AutoCAD Table from the datatable.
        [NotNull]
        public static Table ToAcadTable([NotNull] this DataTable dataTbl, double rowHeight, double columnWidth)
        {
            // return dataTbl.Rows.Cast<DataRow>().ToAcadTable(dataTbl.TableName, dataTbl.GetColumnNames(), rowHeight, columnWidth);
            var tbl = new Table();
            tbl.Rows[0].Height = rowHeight;
            tbl.Columns[0].Width = columnWidth;
            tbl.InsertColumns(0, columnWidth, dataTbl.Columns.Count - 1);
            tbl.InsertRows(0, rowHeight, dataTbl.Rows.Count + 1);
            tbl.Cells[0, 0].Value = dataTbl.TableName;
            dataTbl.GetColumnNames()
                .Iterate((name, i) => tbl.Cells[1, i].Value = name);
            dataTbl.Rows
                .Cast<DataRow>()
                .Iterate((row, i) =>
                    row.ItemArray.Iterate((item, j) =>
                        tbl.Cells[i + 2, j].Value = item));
            return tbl;
        }

        // Writes a csv file from the datatable.
        public static void WriteCsv([NotNull] this DataTable dataTbl, [NotNull] string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine(dataTbl.GetColumnNames().Aggregate((s1, s2) => $"{s1},{s2}"));
                dataTbl.Rows
                    .Cast<DataRow>()
                    .Select(row => row.ItemArray.Aggregate((s1, s2) => $"{s1},{s2}")).ToList()
                    .ForEach(line => writer.WriteLine(line));
            }
        }

        // Writes an Excel file from the datatable (using late binding)
        public static void WriteXls([NotNull] this DataTable dataTbl, string filename, [CanBeNull] string sheetName, bool visible)
        {
            var mis = Type.Missing;
            var xlApp = LateBinding.GetOrCreateInstance("Excel.Application");
            xlApp.Set("DisplayAlerts", false);
            var workbooks = xlApp.Get("Workbooks");
            object worksheet;
            var workbook = File.Exists(filename) ? workbooks.Invoke("Open", filename) : workbooks.Invoke("Add", mis);
            if (string.IsNullOrEmpty(sheetName))
                worksheet = workbook.Get("Activesheet");
            else
            {
                var worksheets = workbook.Get("Worksheets");
                try
                {
                    worksheet = worksheets.Get("Item", sheetName);
                    worksheet.Get("Cells").Invoke("Clear");
                }
                catch
                {
                    worksheet = worksheets.Invoke("Add", mis);
                    worksheet.Set("Name", sheetName);
                }
            }

            var range = worksheet.Get("Cells");
            dataTbl.GetColumnNames()
                .Iterate((name, i) => range.Get("Item", 1, i + 1).Set("Value2", name));
            dataTbl.Rows
                .Cast<DataRow>()
                .Iterate((row, i) => row.ItemArray
                    .Iterate((item, j) => range.Get("Item", i + 2, j + 1).Set("Value2", item)));
            xlApp.Set("DisplayAlerts", true);
            if (visible)
            {
                xlApp.Set("Visible", true);
            }
            else
            {
                if (File.Exists(filename))
                    workbook.Invoke("Save");
                else
                {
                    var fileFormat =
                        string.CompareOrdinal("11.0", (string)xlApp.Get("Version")) < 0 &&
                        filename.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase)
                            ? 51
                            : -4143;
                    workbook.Invoke("Saveas", filename, fileFormat, string.Empty, string.Empty, false, false, 1, 1);
                }

                workbook.Invoke("Close");
                xlApp.ReleaseInstance();
            }
        }
    }
}