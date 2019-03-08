namespace AcadLib.PaletteProps
{
    /// <summary>
    /// Interaction logic for StringValueView.xaml
    /// </summary>
    public partial class StringView
    {
        public StringView(StringVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}