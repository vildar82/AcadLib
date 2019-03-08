namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for IntListValueView.xaml
    /// </summary>
    public partial class IntListView
    {
        public IntListView(IntListVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}