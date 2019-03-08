namespace AcadLib.PaletteProps
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;

    public static class InputBindingsManager
    {
        public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty =
            DependencyProperty.RegisterAttached(
                "UpdatePropertySourceWhenEnterPressed", typeof(DependencyProperty), typeof(InputBindingsManager),
                new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

        static InputBindingsManager()
        {
        }

        public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
        }

        public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
        {
            return (DependencyProperty)dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
        }

        private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(
            DependencyObject dp,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is UIElement element))
            {
                return;
            }

            if (e.OldValue != null)
            {
                element.LostKeyboardFocus -= Element_LostKeyboardFocus;
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }

            if (e.NewValue != null)
            {
                element.LostKeyboardFocus -= Element_LostKeyboardFocus;
                element.LostKeyboardFocus += Element_LostKeyboardFocus;
                element.PreviewKeyDown -= HandlePreviewKeyDown;
                element.PreviewKeyDown += HandlePreviewKeyDown;
            }
        }

        private static void Element_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DoUpdateTarget(e.Source);
        }

        static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Debug.WriteLine("InputBindingsManager HandlePreviewKeyDown=Enter - DoUpdateSource().");
                DoUpdateSource(e.Source);
            }
        }

        static void DoUpdateSource(object source)
        {
            var property = GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);
            if (property == null)
            {
                return;
            }

            if (!(source is UIElement elt))
            {
                return;
            }

            var binding = BindingOperations.GetBindingExpression(elt, property);
            binding?.UpdateSource();
            Keyboard.ClearFocus();
        }

        static void DoUpdateTarget(object source)
        {
            var property = GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);
            if (property == null)
            {
                return;
            }

            if (!(source is UIElement elt))
            {
                return;
            }

            var binding = BindingOperations.GetBindingExpression(elt, property);
            binding?.UpdateTarget();
        }
    }
}
