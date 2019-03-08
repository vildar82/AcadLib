namespace AcadLib.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using JetBrains.Annotations;

    [ValueConversion(typeof(byte), typeof(double))]
    public class TransparenceInvertConverter : ConvertorBase
    {
        [NotNull]
        public override object Convert([NotNull] object value, Type targetType, object parameter, CultureInfo culture)
        {
            var transparence = (byte)value;
            return 255 - transparence;
        }

        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bval = System.Convert.ToByte(value);
            return 255 - bval;
        }
    }
}