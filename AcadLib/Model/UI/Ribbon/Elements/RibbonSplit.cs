namespace AcadLib.UI.Ribbon.Elements
{
    using System.Collections.Generic;
    using System.Windows.Input;

    /// <summary>
    /// Выпадающий список элементов
    /// </summary>
    public class RibbonSplit : RibbonItemData
    {
        public List<RibbonItemData> Items { get; set; } = new List<RibbonItemData>();

        public override ICommand GetCommand()
        {
            return null;
        }
    }
}
