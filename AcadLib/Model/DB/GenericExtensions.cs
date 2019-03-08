// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    using AcadLib.Blocks;
    using JetBrains.Annotations;
    using Linq;

    [PublicAPI]
    public static class GenericExtensions
    {
        // Applies the given Action to each element of the collection (mimics the F# Seq.iter function).
        public static void Iterate<T>([NotNull] this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        // Applies the given Action to each element of the collection (mimics the F# Seq.iteri function).
        // The integer passed to the Action indicates the index of element.
        public static void Iterate<T>([NotNull] this IEnumerable<T> collection, Action<T, int> action)
        {
            var i = 0;
            foreach (var item in collection)
                action(item, i++);
        }

        // Creates a System.Data.DataTable from a BlockAttribute collection.
        [NotNull]
        public static Data.DataTable ToDataTable([NotNull] this IEnumerable<BlockAttribute> blockAtts, string name)
        {
            var dTable = new Data.DataTable(name);
            dTable.Columns.Add("Name", typeof(string));
            dTable.Columns.Add("Quantity", typeof(int));
            blockAtts
                .GroupBy(blk => blk, (blk, blks) => new { Block = blk, Count = blks.Count() },
                    new BlockAttributeEqualityComparer())
                .Iterate(row =>
                {
                    var dRow = dTable.Rows.Add(row.Block.Name, row.Count);
                    row.Block.Attributes.Iterate(att =>
                    {
                        if (!dTable.Columns.Contains(att.Key))
                            dTable.Columns.Add(att.Key);
                        dRow[att.Key] = att.Value;
                    });
                });
            return dTable;
        }
    }
}