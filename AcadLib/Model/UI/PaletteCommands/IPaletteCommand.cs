namespace AcadLib.PaletteCommands
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Windows.Media;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IPaletteCommand : INotifyPropertyChanged
    {
        List<string> Access { get; set; }

        ICommand Command { get; set; }

        List<MenuItemCommand> ContexMenuItems { get; set; }

        string Description { get; set; }

        string Group { get; set; }

        string HelpMedia { get; set; }

        ImageSource Image { get; set; }

        bool IsTest { get; set; }

        string Name { get; set; }

        void Execute();
    }
}
