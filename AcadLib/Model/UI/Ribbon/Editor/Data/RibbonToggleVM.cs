namespace AcadLib.UI.Ribbon.Editor.Data
{
    using Elements;

    public class RibbonToggleVM : RibbonCommandVM
    {
        public RibbonToggleVM(RibbonToggle item)
            : base(item)
        {
            IsChecked = item.IsChecked;
        }

        public bool IsChecked { get; set; }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonToggle();
            FillItem(item);
            item.IsChecked = IsChecked;
            return item;
        }
    }
}
