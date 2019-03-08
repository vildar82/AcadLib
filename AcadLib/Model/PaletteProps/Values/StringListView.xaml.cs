namespace AcadLib.PaletteProps
{
    public partial class StringListView
    {
        public StringListView(StringListVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
