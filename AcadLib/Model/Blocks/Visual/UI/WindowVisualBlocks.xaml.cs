namespace AcadLib.Blocks.Visual.UI
{
    /// <summary>
    /// Логика взаимодействия для WindowVisualBlocks.xaml
    /// </summary>
    public partial class WindowVisualBlocks
    {
        public WindowVisualBlocks(VisualBlocksViewModel vm) : base(vm)
        {
            InitializeComponent();
            IsUnclosing = true;
        }
    }
}