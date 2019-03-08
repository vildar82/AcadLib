namespace AcadLib.Utils.Tabs.UI
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ValueConversion(typeof(string), typeof(Brush))]
    public class ErrConverter : MarkupExtension, IValueConverter
    {
        private static readonly Brush err = new SolidColorBrush(Colors.LightCoral);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : err;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
