namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for IntValueView.xaml
    /// </summary>
    public partial class DoubleView
    {
        public DoubleView(DoubleVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}