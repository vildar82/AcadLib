namespace AcadLib.UI.Ribbon.Elements
{
    using System.Windows.Input;

    /// <summary>
    /// Разделитель выпадающей панели
    /// </summary>
    public class RibbonBreakPanel : RibbonItemData
    {
        public override ICommand GetCommand()
        {
            return null;
        }
    }
}
