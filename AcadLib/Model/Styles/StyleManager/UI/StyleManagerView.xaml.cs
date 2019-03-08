namespace AcadLib.Styles.StyleManager.UI
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using JetBrains.Annotations;

    /// <summary>
    /// Interaction logic for StyleManager.xaml
    /// </summary>
    public partial class StyleManagerView
    {
        public StyleManagerView(StyleManagerVM vm) : base(vm)
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, [NotNull] MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg =
                    new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = MouseWheelEvent,
                        Source = sender
                    };
                if (((Control)sender).Parent is UIElement parent)
                    parent.RaiseEvent(eventArg);
            }
        }
    }
}