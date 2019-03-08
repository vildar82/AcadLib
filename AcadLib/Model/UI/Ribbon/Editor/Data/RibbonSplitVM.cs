namespace AcadLib.UI.Ribbon.Editor.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Elements;

    public class RibbonSplitVM : RibbonItemDataVM
    {
        public RibbonSplitVM(RibbonSplit item)
            : base(item)
        {
            Items = new ObservableCollection<RibbonItemDataVM>(
                item?.Items.Select(RibbonVM.ribbonVm.GetItemVM) ?? new List<RibbonItemDataVM>());
        }

        public ObservableCollection<RibbonItemDataVM> Items { get; set; }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonSplit();
            FillItem(item);
            item.Items = Items.Select(s=>s.GetItem()).ToList();
            return item;
        }
    }
}
