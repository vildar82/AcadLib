namespace AcadLib.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using JetBrains.Annotations;

    [ValueConversion(typeof(byte), typeof(double))]
    public class TransparenceToOpacityConverter : ConvertorBase
    {
        [NotNull]
        public override object Convert([NotNull] object value, Type targetType, object parameter, CultureInfo culture)
        {
            var transparence = (byte)value;
            var opacity = transparence / (double)byte.MaxValue;
            if (opacity < 0.1)
            {
                opacity = 0.1;
            }

            return opacity;
        }
    }
}