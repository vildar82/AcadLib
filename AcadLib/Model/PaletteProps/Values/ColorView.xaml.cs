namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for ColorValueView.xaml
    /// </summary>
    public partial class ColorView
    {
        public ColorView(ColorVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}