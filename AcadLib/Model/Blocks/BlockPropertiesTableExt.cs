namespace AcadLib.Blocks
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Internal.DatabaseServices;

    /// <summary>
    /// Таблица свойств блока
    /// </summary>
    public static class BlockPropertiesTableExt
    {
        /// <summary>
        /// Таблица свойств блока. Должна быть запущена транзакция!
        /// </summary>
        public static System.Data.DataTable? GetBlockPropertiesTable(this BlockTableRecord dynBtr)
        {
            var t = dynBtr.Database.TransactionManager.TopTransaction;
            using var bpt = GetBpt(dynBtr, t);
            if (bpt == null)
                return null;
            var dTable = new System.Data.DataTable($"Таблица свойств блока {dynBtr.Name}");
            var columns = GetColumns(bpt);
            dTable.Columns.AddRange(columns.ToArray());
            foreach (BlockPropertiesTableRow bptRow in bpt.Rows)
            {
                var row = dTable.NewRow();
                for (var i = 0; i < columns.Count; i++)
                {
                    var col = columns[i];
                    if (string.IsNullOrEmpty(col.ColumnName))
                        continue;
                    var bptCol = bpt.Columns[i];
                    var tv = bptRow[bptCol];
                    var val = tv.AsArray()[0].Value;
                    row[col] = val;
                }

                dTable.Rows.Add(row);
            }

            return dTable;
        }

        private static BlockPropertiesTable? GetBpt(BlockTableRecord dynBtr, Transaction t)
        {
            var extDic = dynBtr.ExtensionDictionary.GetObject<DBDictionary>();
            if (extDic == null)
                return null;
            var graph = extDic.GetAt("ACAD_ENHANCEDBLOCK").GetObject<EvalGraph>();
            if (graph == null)
                return null;

            // graph.GetNode - в 2017 не работает! Метод не найден! через dynamic работает.
            var nodes = graph.GetAllNodes().Select(f =>
                     ((dynamic)graph).GetNode((uint)f, OpenMode.ForRead, t) as DBObject);
            foreach (var node in nodes)
            {
                if (node is BlockPropertiesTable bpt)
                    return bpt;
                node?.Dispose();
            }

            return null;
        }

        private static List<System.Data.DataColumn> GetColumns(BlockPropertiesTable bpt)
        {
            var columns = new List<System.Data.DataColumn>();
            foreach (BlockPropertiesTableColumn bptColumn in bpt.Columns)
            {
                var type = bptColumn.DefaultValue.AsArray()[0].Value.GetType();
                var col = new System.Data.DataColumn(bptColumn.Parameter?.Name, type);
                columns.Add(col);
            }

            return columns;
        }
    }
}