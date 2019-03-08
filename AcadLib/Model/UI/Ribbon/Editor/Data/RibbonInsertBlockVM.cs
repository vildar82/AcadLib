namespace AcadLib.UI.Ribbon.Editor.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Blocks;
    using Elements;
    using NetLib;

    public class RibbonInsertBlockVM : RibbonItemDataVM
    {
        public RibbonInsertBlockVM(RibbonInsertBlock item, List<BlockFile> blockFiles)
            : base(item)
        {
            File = blockFiles.FirstOrDefault(f => f.FileName.EqualsIgnoreCase(item.File));
            Layer = item.Layer;
            Explode = item.Explode;
            BlockName = item.BlockName;
            Properties = new ObservableCollection<Property>(item.Properties ?? new List<Property>());
        }

        public ObservableCollection<Property> Properties { get; set; }

        public string BlockName { get; set; }

        public bool Explode { get; set; }

        public BlockFile File { get; set; }
        public string Layer { get; set; }

        public override RibbonItemData GetItem()
        {
            var item = new RibbonInsertBlock();
            FillItem(item);
            return item;
        }

        protected override void FillItem(RibbonItemData item)
        {
            base.FillItem(item);
            var blItem = (RibbonInsertBlock) item;
            blItem.File = File.FileName;
            blItem.Layer = Layer;
            blItem.Explode = Explode;
            blItem.BlockName = BlockName;
            blItem.Properties = Properties.ToList();
        }
    }
}
