namespace AcadLib.UI.Ribbon.Editor.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Elements;

    public class RibbonVisualGroupInsertBlockVM : RibbonInsertBlockVM
    {
        public RibbonVisualGroupInsertBlockVM(RibbonVisualGroupInsertBlock item, List<BlockFile> blockFiles)
            : base(item, blockFiles)
        {
            Groups = new ObservableCollection<FilterGroup>(item.Groups ?? new List<FilterGroup>());
        }

        public ObservableCollection<FilterGroup> Groups { get; set; }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonVisualGroupInsertBlock();
            FillItem(item);
            item.Groups = Groups.ToList();
            return item;
        }
    }
}
