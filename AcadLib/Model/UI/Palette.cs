namespace AcadLib.UI
{
    using System;
    using System.Windows;
    using System.Windows.Forms.Integration;
    using AcadLib.Properties;
    using Autodesk.AutoCAD.Windows;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class Palette
    {
        private static PaletteSet palette;
        private static bool stop = true;

        public static void Start()
        {
            palette = GetPalette();
            palette.Visible = !palette.Visible;
            stop = !palette.Visible;
        }

        public static void AddPalette(string name, UIElement view)
        {
            palette = GetPalette();
            var host = new ElementHost { Child = view };
            palette.Add(name, host);
        }

        public static bool IsStop => stop;

        [NotNull]
        private static PaletteSet GetPalette()
        {
            if (palette == null)
            {
                palette = new PaletteSet("ПИК",
                    nameof(Commands.PIK_PaletteProperties),
                    new Guid("F1FFECA8-A9AE-47D6-8682-752D6AF1A15B")) { Icon = Resources.pik };
                palette.StateChanged += Palette_StateChanged;
            }

            return palette;
        }

        private static void Palette_StateChanged(object sender, [NotNull] PaletteSetStateEventArgs e)
        {
            switch (e.NewState)
            {
                case StateEventIndex.Hide:
                    stop = true;
                    break;
                case StateEventIndex.Show:
                    stop = false;
                    break;
            }
        }
    }
}
