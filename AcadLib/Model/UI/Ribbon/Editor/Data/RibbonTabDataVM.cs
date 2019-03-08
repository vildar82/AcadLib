namespace AcadLib.UI.Ribbon.Editor.Data
{
    using System.Collections.ObjectModel;
    using NetLib.WPF;

    public class RibbonTabDataVM : BaseModel
    {
        public string Name { get; set; }
        public ObservableCollection<RibbonPanelDataVM> Panels { get; set; } = new ObservableCollection<RibbonPanelDataVM>();
    }
}
