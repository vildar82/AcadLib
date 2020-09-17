namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for IntValueView.xaml
    /// </summary>
    public partial class IntView
    {
        public IntView(IntVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}