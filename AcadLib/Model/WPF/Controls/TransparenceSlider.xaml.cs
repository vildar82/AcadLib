namespace AcadLib.WPF.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using JetBrains.Annotations;

    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for TransparenceSlider.xaml
    /// </summary>
    [UsedImplicitly]
    public partial class TransparenceSlider
    {
        public static readonly DependencyProperty TransparenceProperty =
            DependencyProperty.Register("Transparence", typeof(byte), typeof(TransparenceSlider),
                new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public TransparenceSlider()
        {
            InitializeComponent();
        }

        public byte Transparence
        {
            get => (byte)GetValue(TransparenceProperty);
            set => SetValue(TransparenceProperty, value);
        }
    }
}