namespace AcadLib.Colors
{
    using System;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class ColorExt
    {
        [Obsolete]
        public static Color AcadColorFromString(this string color)
        {
            return Color.FromColor(color.StringToColor());
        }

        /// <summary>
        /// строку в цвет - из color?.ToString();
        /// </summary>
        [CanBeNull]
        public static Color AcadColorFromString2(this string color)
        {
            if (color.IsNullOrEmpty())
                return null;

            // Index
            if (short.TryParse(color, out var colorIndex))
            {
                return Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
            }

            // RGB
            var rgb = color.Split(',');
            return rgb.Length == 3 ? Color.FromRgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2])) : null;
        }

        [Obsolete]
        [NotNull]
        public static string AcadColorToStrig([CanBeNull] this Color color)
        {
            return color?.ColorValue.ColorToString() ?? string.Empty;
        }

        /// <summary>
        /// Цвет в строку - индекс цвета, или r,g,b.
        /// </summary>
        [NotNull]
        public static string AcadColorToString2([CanBeNull] this Color color)
        {
            if (color == null || color.IsNone)
                return string.Empty;
            if (color.IsByLayer)
                return "256";
            if (color.IsByBlock)
                return "0";
            return color.IsByAci ? color.ColorIndex.ToString() : $"{color.Red},{color.Green},{color.Blue}";
        }

        /// <summary>
        /// Определение цвета объекта - если ПоСлою, то возвращает цвет слоя.
        /// </summary>
        public static Color GetEntityColorAbs([NotNull] this Entity ent)
        {
            var color = ent.Color;
            if (color.IsByLayer)
            {
                var layer = ent.LayerId.GetObject<LayerTableRecord>();
                if (layer == null)
                    return color;
                color = layer.Color;
            }

            return color;
        }
    }
}