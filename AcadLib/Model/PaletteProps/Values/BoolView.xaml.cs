namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for BoolView.xaml
    /// </summary>
    public partial class BoolView
    {
        public BoolView(BoolVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}