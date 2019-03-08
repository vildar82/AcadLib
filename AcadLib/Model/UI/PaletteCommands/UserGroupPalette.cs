namespace AcadLib.PaletteCommands
{
    using System;
    using System.Collections.Generic;

    public class UserGroupPalette
    {
        public List<IPaletteCommand> Commands { get; set; }

        public string CommandStartPalette { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public PaletteSetCommands Palette { get; set; }
    }
}
