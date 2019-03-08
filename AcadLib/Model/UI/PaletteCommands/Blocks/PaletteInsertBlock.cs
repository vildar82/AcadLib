using NetLib.WPF.Data;

namespace AcadLib.PaletteCommands
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using Autodesk.AutoCAD.DatabaseServices;
    using Blocks;
    using JetBrains.Annotations;
    using Layers;

    /// <summary>
    /// Кнопка для вставки одного блока
    /// </summary>
    public class PaletteInsertBlock : PaletteCommand
    {
        internal string blName;
        internal bool explode;
        internal string file;
        internal List<Property> props;

        public PaletteInsertBlock()
        {
        }

        public PaletteInsertBlock(
            string blName,
            string file,
            string name,
            Bitmap image,
            string description,
            string group = "",
            [CanBeNull] List<Property> props = null,
            bool isTest = false,
            bool explode = false)
            : base(name, image, string.Empty, description, group, isTest)
        {
            this.blName = blName;
            this.file = file;
            this.props = props;
            this.explode = explode;
            CreateContexMenu();
            commandStart.CommandName = $"Вставка блока {blName}";
        }

        public LayerInfo Layer { get; set; }

        public override void Execute()
        {
            try
            {
                CopyBlock(explode ? DuplicateRecordCloning.Replace : DuplicateRecordCloning.Ignore);
                var blRefId = BlockInsert.Insert(blName, Layer, props);
                if (explode)
                {
                    var doc = AcadHelper.Doc;
                    using (doc.LockDocument())
                    using (var t = doc.TransactionManager.StartTransaction())
                    {
                        var blRef = blRefId.GetObjectT<BlockReference>(OpenMode.ForWrite);
                        var owner = blRef.BlockId.GetObjectT<BlockTableRecord>(OpenMode.ForWrite);
                        using (var explodes = new DBObjectCollection())
                        {
                            blRef.Explode(explodes);
                            foreach (Entity explode in explodes)
                            {
                                explode.Layer = blRef.Layer;
                                owner.AppendEntity(explode);
                                t.AddNewlyCreatedDBObject(explode, true);
                            }

                            blRef.Erase();
                        }

                        t.Commit();
                    }
                }

                Statistic.PluginStatisticsHelper.PluginStart(commandStart);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка при вставке блока - {e.Message}");
            }
        }

        private bool CanRedefine()
        {
            return Block.HasBlockThisDrawing(blName);
        }

        private void CopyBlock(DuplicateRecordCloning mode)
        {
            var doc = AcadHelper.Doc;
            var db = doc.Database;
            using (doc.LockDocument())
            {
                // Выбор и вставка блока
                if (mode == DuplicateRecordCloning.Replace)
                {
                    Block.Redefine(blName, file, db);
                }
                else
                {
                    Block.CopyBlockFromExternalDrawing(new List<string> { blName }, file, db, mode);
                }
            }
        }

        private void CreateContexMenu()
        {
            ContexMenuItems = new List<MenuItemCommand>();
            var menu = new MenuItemCommand("Переопределить", new RelayCommand(Redefine, CanRedefine));
            ContexMenuItems.Add(menu);
        }

        /// <summary>
        /// Переопределение блока
        /// </summary>
        private void Redefine()
        {
            if (!CanRedefine())
            {
                MessageBox.Show($"В текущем чертеже нет блока {blName}.");
                return;
            }

            CopyBlock(DuplicateRecordCloning.Replace);
        }
    }
}
