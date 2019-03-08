namespace AcadLib.CommandLock.UI
{
    using System.Windows.Input;

    public class Button
    {
        public ICommand Command { get; set; }

        public bool IsCancel { get; set; }

        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public string ToolTip { get; set; }
    }
}