namespace AcadLib.Blocks.Visual
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Layers;
    using Statistic;
    using UI;

    [PublicAPI]
    public static class VisualInsertBlock
    {
        private static readonly Dictionary<Predicate<string>, List<IVisualBlock>> dictFiles =
            new Dictionary<Predicate<string>, List<IVisualBlock>>();

        private static readonly Dictionary<Func<string, string>, List<IVisualBlock>> dictGroup =
            new Dictionary<Func<string, string>, List<IVisualBlock>>();

        private static LayerInfo _layer;
        private static WindowVisualBlocks winVisual;

        public static void InsertBlockGroups(
            string fileBlocks,
            [NotNull] Func<string, string> filterGroup,
            [CanBeNull] LayerInfo layer = null)
        {
            _layer = layer;
            if (!dictGroup.TryGetValue(filterGroup, out var visuals))
            {
                visuals = LoadVisuals(fileBlocks, filterGroup);
                dictGroup.Add(filterGroup, visuals);
            }

            ShowVisuals(visuals);
        }

        public static void InsertBlock(
            string fileBlocks,
            [NotNull] Predicate<string> filter,
            [CanBeNull] LayerInfo layer = null,
            bool explode = false)
        {
            _layer = layer;
            if (!dictFiles.TryGetValue(filter, out var visuals))
            {
                visuals = LoadVisuals(fileBlocks, n => filter(n) ? string.Empty : null);
                dictFiles.Add(filter, visuals);
            }

            ShowVisuals(visuals, explode);
        }

        [NotNull]
        public static List<IVisualBlock> LoadVisuals(string file, Func<string, string> filter)
        {
            var visualBlocks = new List<IVisualBlock>();
            using (var dbTemp = new Database(false, true))
            {
                dbTemp.ReadDwgFile(file, FileOpenMode.OpenForReadAndReadShare, true, string.Empty);
                using (var t = dbTemp.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)dbTemp.BlockTableId.GetObject(OpenMode.ForRead);
                    foreach (var idBtr in bt)
                    {
                        var btr = (BlockTableRecord)idBtr.GetObject(OpenMode.ForRead);
                        var group = filter(btr.Name);
                        if (group != null)
                        {
                            var visualBl = new VisualBlock(btr) { File = file, Group = group };
                            visualBlocks.Add(visualBl);
                        }
                    }

                    t.Commit();
                }

                var alpha = NetLib.Comparers.AlphanumComparator.New;
                visualBlocks.Sort((v1, v2) => alpha.Compare(v1.Name, v2.Name));
            }

            return visualBlocks;
        }

        /// <summary>
        /// Переопределенеи блока
        /// </summary>
        public static void Redefine([CanBeNull] IVisualBlock block)
        {
            if (block == null)
                return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            using (doc.LockDocument())
            {
                Block.Redefine(block.Name, block.File, doc.Database);
            }
        }

        public static void Insert([CanBeNull] IVisualBlock block, bool explode = false)
        {
            if (block == null)
                return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            GetInsertBtr(block.Name, block.File, db);
            BlockInsert.Insert(block.Name, _layer, explode);
            PluginStatisticsHelper.PluginStart(new CommandStart($"Вставка блока {block.Name}", Assembly.GetCallingAssembly()));
        }

        private static void ShowVisuals([NotNull] List<IVisualBlock> blocks, bool explode = false)
        {
            var vm = new VisualBlocksViewModel(blocks) { Explode = explode };
            if (winVisual == null)
            {
                winVisual = new WindowVisualBlocks(vm);
            }
            else
            {
                vm.IsHideWindow = true;
                winVisual.Model = vm;
            }

            winVisual.Show();
        }

        private static void GetInsertBtr(string name, string fileBlocks, [NotNull] Database dbdest)
        {
            // Есть ли уже блок в текущем файле
            using (var bt = (BlockTable)dbdest.BlockTableId.Open(OpenMode.ForRead))
            {
                if (bt.Has(name))
                {
                    return;
                }
            }

            // Копирование блока из файла шаблона
            Block.CopyBlockFromExternalDrawing(name, fileBlocks, dbdest);
        }
    }
}