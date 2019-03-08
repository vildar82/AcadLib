namespace AcadLib.UI.PaletteCommands
{
    using System;
    using System.Drawing;
    using AcadLib.PaletteCommands;
    using NetLib.WPF.Data;

    public class ToggleButton : PaletteCommand
    {
        public ToggleButton()
        {
        }

        public ToggleButton(string name, Bitmap icon, bool isChecked, Action change, string desc, string group)
        {
            Name = name;
            Image = GetSource(icon, false);
            Group = group;
            Description = desc;
            IsChecked = isChecked;
            Command = new RelayCommand(()=> change());
        }

        public bool IsChecked { get; set; }
    }
}
