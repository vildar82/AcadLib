namespace AcadLib.WPF.Controls
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Windows;
    using JetBrains.Annotations;

    /// <summary>
    /// Кнопка выбора цвета.
    /// <controls:AcadColorPick Color="{Binding Color}"/>
    /// </summary>
    public partial class AcadColorPick : INotifyPropertyChanged
    {
        /// <summary>
        /// Цвет (AutoCAD)
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Autodesk.AutoCAD.Colors.Color), typeof(AcadColorPick),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private bool canClearColor;

        public AcadColorPick()
        {
            InitializeComponent();
            CanClearColor = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanClearColor
        {
            get => canClearColor;
            set
            {
                if (value == canClearColor)
                    return;
                canClearColor = value;
                OnPropertyChanged();
            }
        }

        [CanBeNull]
        public Autodesk.AutoCAD.Colors.Color Color
        {
            get => (Autodesk.AutoCAD.Colors.Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
                CanClearColor = Color != null;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Color = null;
        }

        private void SelectColor(object sender, RoutedEventArgs e)
        {
            var colorDlg = new ColorDialog { Color = Color };
            if (colorDlg.ShowModal() == true)
            {
                Color = colorDlg.Color;
            }
        }
    }
}
