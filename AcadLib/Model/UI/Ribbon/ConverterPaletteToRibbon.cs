namespace AcadLib.UI.Ribbon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using AcadLib.PaletteCommands;
    using Data;
    using Elements;
    using NetLib;
    using PaletteCommands;

    public class ConverterPaletteToRibbon
    {
        private string imagesDir;
        private string dirBlocks;

        public void Convert(string tabName, List<IPaletteCommand> commands)
        {
            dirBlocks = IO.Path.GetLocalSettingsFile("Blocks");
            var fileRibbon = RibbonGroupData.GetRibbonFile(tabName);
            var dir = Path.GetDirectoryName(fileRibbon);
            Directory.CreateDirectory(dir);
            imagesDir = Path.Combine(dir, "Images");
            Directory.CreateDirectory(imagesDir);

            var tab = new RibbonTabData
            {
                Name = tabName,
                Panels = commands.GroupBy(g => g.Group).Select(s => new RibbonPanelData
                {
                Name = s.Key,
                Items = s.Select(GetItem).ToList()
                }).ToList()
            };
            var ribbonGroup = new RibbonGroupData { Tabs = new List<RibbonTabData> { tab } };
            ribbonGroup.Save(fileRibbon);
        }

        private RibbonItemData GetItem(IPaletteCommand com)
        {
            RibbonItemData item;
            switch (com)
            {
                case PaletteInsertBlock paletteInsertBlock:
                    item = new RibbonInsertBlock
                    {
                        Name = paletteInsertBlock.blName,
                        File = GetBlockFile(paletteInsertBlock.file),
                        Explode = paletteInsertBlock.explode,
                        Layer = paletteInsertBlock.Layer?.Name,
                        BlockName = paletteInsertBlock.blName,
                        Properties = paletteInsertBlock.props
                    };
                    break;
                case PaletteVisualInsertBlocks paletteVisualInsertBlocks:
                    item = new RibbonVisualInsertBlock
                    {
                        Name = paletteVisualInsertBlocks.Name ?? paletteVisualInsertBlocks.CommandName,
                        File = GetBlockFile(paletteVisualInsertBlocks.file),
                        Explode = paletteVisualInsertBlocks.explode,
                        Layer = paletteVisualInsertBlocks.Layer?.Name,
                    };
                    break;
                case SplitCommand splitCommand:
                    item = new RibbonSplit
                    {
                        Items = splitCommand.Commands?.Select(GetItem).ToList()
                    };
                    break;
                case ToggleButton toggleButton:
                    item = new RibbonToggle
                    {
                        Name = toggleButton.Name ?? toggleButton.CommandName,
                        IsChecked = toggleButton.IsChecked,
                        Command = toggleButton.CommandName
                    };
                    break;
                case PaletteCommand paletteCommand:
                    item = new RibbonCommand
                    {
                        Name = paletteCommand.Name ?? paletteCommand.CommandName,
                        Command = paletteCommand.CommandName,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(com));
            }

            item.Name = com.Name;
            item.Access = com.Access;
            item.Description = com.Description;
            item.IsTest = com.IsTest;
            SaveImage(com);
            return item;
        }

        private void SaveImage(IPaletteCommand com)
        {
            if (com.Name.IsNullOrEmpty())
                return;
            var imageName = RibbonGroupData.GetImageName(com.Name);
            var file = Path.Combine(imagesDir, imageName);
            var fi = new FileInfo(file);
            using (var fileStream = fi.Create())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)com.Image));
                encoder.Save(fileStream);
            }
        }

        private string GetBlockFile(string file)
        {
            if (file.Contains(dirBlocks, StringComparison.OrdinalIgnoreCase))
            {
                return file.Replace(dirBlocks, "");
            }

            Debugger.Launch();
            Debugger.Break();
            throw new Exception();
        }
     }
}
