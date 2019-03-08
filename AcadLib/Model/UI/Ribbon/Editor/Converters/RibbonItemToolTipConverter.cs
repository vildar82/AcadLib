namespace AcadLib.UI.Ribbon.Editor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using AcadLib.UI.Ribbon.Editor.Data;
    using Elements;
    using NetLib;

    public class RibbonItemToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = (RibbonItemDataVM) value;
                return
                    $"{item.GetType().Name} \n{item.Name} \n{item.Description} \n{(item.IsTest ? "Тест" : "")} \n{item.Access?.JoinToString()}";
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
