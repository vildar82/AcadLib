using NetLib;

namespace AcadLib.Colors
{
    using System.Collections.Generic;
    using System.IO;
    using Errors;
    using JetBrains.Annotations;
    using OfficeOpenXml;

    [PublicAPI]
    public class ColorBook
    {
        public ColorBook(string name)
        {
            Name = name;
            Colors = new List<ColorItem>();
        }

        public List<ColorItem> Colors { get; set; }

        public string Name { get; set; }

        [NotNull]
        public static ColorBook ReadFromFile([NotNull] string NcsFile)
        {
            var colorBookNcs = new ColorBook("NCS");

            using (var exlPack = new ExcelPackage(new FileInfo(NcsFile)))
            {
                var wsNcs = exlPack.Workbook.Worksheets["NCS"];
                var row = 1;
                do
                {
                    row++;

                    var nameNcs = wsNcs.Cells[row, 1].Text;
                    if (string.IsNullOrEmpty(nameNcs))
                        break;

                    var r = GetByte(wsNcs.Cells[row, 2].Text);
                    if (r.Failure)
                    {
                        Inspector.AddError($"Ошибка в ячейке [{row},2] - {r.Error}");
                        continue;
                    }

                    var g = GetByte(wsNcs.Cells[row, 3].Text);
                    if (g.Failure)
                    {
                        Inspector.AddError($"Ошибка в ячейке [{row},2] - {g.Error}");
                        continue;
                    }

                    var b = GetByte(wsNcs.Cells[row, 4].Text);
                    if (b.Failure)
                    {
                        Inspector.AddError($"Ошибка в ячейке [{row},2] - {b.Error}");
                        continue;
                    }

                    var colorItem = new ColorItem(nameNcs, r.Value, g.Value, b.Value);
                    colorBookNcs.Colors.Add(colorItem);
                } while (true);
            }

            return colorBookNcs;
        }

        [NotNull]
        private static Result<byte> GetByte(string value)
        {
            return !byte.TryParse(value, out var res)
                ? Result.Fail<byte>($"Не определен бит из значения {value}")
                : Result.Ok(res);
        }
    }
}