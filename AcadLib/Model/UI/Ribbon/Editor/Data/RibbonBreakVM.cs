namespace AcadLib.UI.Ribbon.Editor.Data
{
    using Elements;

    public class RibbonBreakVM : RibbonItemDataVM
    {
        public RibbonBreakVM(RibbonBreakPanel item)
            : base(item)
        {
        }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonBreakPanel();
            return item;
        }
    }
}
