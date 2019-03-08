namespace AcadLib.UI.Ribbon.Elements
{
    using System.Windows.Input;
    using System.Windows.Media;

    public interface IRibbonElement
    {
        string Tab { get; set; }

        string Panel { get; set; }

        ICommand Command { get; set; }

        string Name { get; set; }

        ImageSource LargeImage { get; set; }

        ImageSource Image { get; set; }

        string Description { get; set; }
    }
}