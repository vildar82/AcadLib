namespace AcadLib.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using JetBrains.Annotations;

    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    [ValueConversion(typeof(System.Drawing.Color), typeof(SolidColorBrush))]
    [ValueConversion(typeof(Autodesk.AutoCAD.Colors.Color), typeof(SolidColorBrush))]
    public class ColorToBrushConverter : ConvertorBase
    {
        [CanBeNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case System.Drawing.Color dc:
                    return new SolidColorBrush(Color.FromArgb(dc.A, dc.R, dc.G, dc.B));

                case Color mc:
                    return new SolidColorBrush(mc);

                case Autodesk.AutoCAD.Colors.Color ac:
                    var cv = ac.ColorValue;
                    return new SolidColorBrush(Color.FromRgb(cv.R, cv.G, cv.B));
            }

            return null;
        }
    }
}