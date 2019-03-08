namespace AcadLib.UI.Ribbon.Editor
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using Data;

    public partial class RibbonView
    {
        public RibbonView(RibbonVM vm)
            : base(vm)
        {
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            var ribbonVm = (RibbonVM) DataContext;
            var itemVm = lb.SelectedItem as RibbonItemDataVM;
            ribbonVm.SelectedItem = itemVm;
        }
    }
}
