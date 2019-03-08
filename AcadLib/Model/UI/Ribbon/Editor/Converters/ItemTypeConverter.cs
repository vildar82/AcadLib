namespace AcadLib.UI.Ribbon.Editor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
