// ReSharper disable once CheckNamespace
namespace AcadLib.PaletteCommands
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using JetBrains.Annotations;

    /// <summary>
    /// Контекстное меню команды
    /// </summary>
    [PublicAPI]
    public class MenuItemCommand
    {
        public MenuItemCommand(string name, ICommand command)
        {
            Name = name;
            Command = command;
        }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public object Icon { get; set; }

        public string Name { get; set; }

        public ObservableCollection<MenuItemCommand> SubItems { get; set; }
    }
}