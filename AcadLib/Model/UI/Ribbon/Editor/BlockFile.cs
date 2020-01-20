using System.Windows.Media;
using AcadLib.Blocks.Visual;

namespace AcadLib.UI.Ribbon.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    public class BlockFile
    {
        public string FileName { get; set; }
        public List<BlockItem> Blocks { get; set; }

        public static List<BlockFile> GetFiles()
        {
            var blockFiles = new List<BlockFile>();
            var blocksDir = IO.Path.GetLocalSettingsFile("Blocks");
            foreach (var file in Directory.EnumerateFiles(blocksDir, "*.dwg", SearchOption.AllDirectories))
            {
                blockFiles.Add(new BlockFile
                {
                    FileName = file.Replace(blocksDir, string.Empty),
                    Blocks = GetBlocks (file)
                });
            }

            return blockFiles;
        }

        private static List<BlockItem> GetBlocks(string file)
        {
            using var db = new Database(false, true);
            db.ReadDwgFile(file, FileShare.ReadWrite, false, String.Empty);
            db.CloseInput(true);
            using var t = db.TransactionManager.StartTransaction();
            return db.BlockTableId.GetObjectT<BlockTable>().GetObjects<BlockTableRecord>()
                .Where(w => !w.IsLayout && !w.IsAnonymous && !w.IsFromExternalReference && !w.IsAProxy)
                .Select(GetBlockItem).ToList();
        }

        private static BlockItem GetBlockItem(BlockTableRecord btr)
        {
            ImageSource image = null;
            try
            {
                image = BlockPreviewHelper.GetPreview(btr);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }

            return new BlockItem
            {
                Name = btr.Name,
                Image = image
            };
        }
    }
}
