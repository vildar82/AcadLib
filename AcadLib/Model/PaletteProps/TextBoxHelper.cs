namespace AcadLib.PaletteProps
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class TextBoxHelper
    {
        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, OnIsMonitoringChanged));

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }

        private static void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as TextBox, textBox => textBox.SelectAll());
        }

        private static void ControlGotFocus<TDependencyObject>(TDependencyObject sender, Action<TDependencyObject> action) where TDependencyObject : DependencyObject
        {
            if (sender != null)
            {
                if (GetSelectAllOnFocus(sender))
                {
                    sender.Dispatcher.BeginInvoke(action, sender);
                }
            }
        }

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox txtBox)) return;
            if ((bool)e.NewValue)
            {
                txtBox.GotFocus += TextBoxGotFocus;
            }
            else
            {
                txtBox.GotFocus -= TextBoxGotFocus;
            }
        }
    }
}
