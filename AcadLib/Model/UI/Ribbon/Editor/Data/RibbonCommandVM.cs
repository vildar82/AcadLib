namespace AcadLib.UI.Ribbon.Editor.Data
{
    using Elements;

    public class RibbonCommandVM : RibbonItemDataVM
    {
        public RibbonCommandVM(RibbonCommand item)
            : base(item)
        {
            Command = item.Command;
        }

        public string Command { get; set; }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonCommand();
            FillItem(item);
            return item;
        }

        protected override void FillItem(RibbonItemData item)
        {
            base.FillItem(item);
            var cItem = (RibbonCommand) item;
            cItem.Command = Command;
        }
    }
}
