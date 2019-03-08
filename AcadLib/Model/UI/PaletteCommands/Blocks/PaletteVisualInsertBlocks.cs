// ReSharper disable once CheckNamespace
namespace AcadLib.PaletteCommands
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using JetBrains.Annotations;
    using Layers;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    /// <summary>
    /// Команда вставки блока из списка
    /// </summary>
    [PublicAPI]
    public class PaletteVisualInsertBlocks : PaletteCommand
    {
        internal bool explode;
        internal string file;
        internal Predicate<string> filter;

        public PaletteVisualInsertBlocks()
        {
        }

        public PaletteVisualInsertBlocks(
            Predicate<string> filter,
            string file,
            string name,
            Bitmap image,
            string description,
            string group = "",
            bool isTest = false,
            bool explode = false)
            : base(name, image, string.Empty, description, group, isTest)
        {
            this.file = file;
            this.explode = explode;
            this.filter = filter;
        }

        public LayerInfo Layer { get; set; }

        public override void Execute()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                    return;
                using (doc.LockDocument())
                {
                    Blocks.Visual.VisualInsertBlock.InsertBlock(file, filter, Layer, explode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Ошибка при вставке блока - {ex.Message}");
            }
        }
    }
}
