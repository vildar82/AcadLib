namespace AcadLib.UI.Ribbon.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using Autodesk.AutoCAD.DatabaseServices;
    using Blocks;
    using IO;
    using Layers;
    using NetLib.WPF.Data;

    public class RibbonInsertBlock : RibbonItemData
    {
        public string File { get; set; }
        public string BlockName { get; set; }
        public bool Explode { get; set; }
        public string Layer { get; set; }
        public List<Property> Properties { get; set; }

        public override ICommand GetCommand()
        {
            return new RelayCommand(Execute);
        }

        public void Execute()
        {
            try
            {
                CopyBlock(Explode ? DuplicateRecordCloning.Replace : DuplicateRecordCloning.Ignore);
                var blRefId = BlockInsert.Insert(BlockName, new LayerInfo(Layer), Properties);
                if (Explode)
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

                Statistic.PluginStatisticsHelper.PluginStart($"Вставка блока {BlockName}");
            }
            catch (Exception e)
            {
                AcadLib.Logger.Log.Error(e);
                MessageBox.Show($"Ошибка при вставке блока - {e.Message}");
            }
        }

        private bool CanRedefine()
        {
            return Block.HasBlockThisDrawing(BlockName);
        }

        private void CopyBlock(DuplicateRecordCloning mode)
        {
            var doc = AcadHelper.Doc;
            var db = doc.Database;
            var filePath = GetFilePath();
            using (doc.LockDocument())
            {
                // Выбор и вставка блока
                if (mode == DuplicateRecordCloning.Replace)
                {
                    Block.Redefine(BlockName, filePath, db);
                }
                else
                {
                    Block.CopyBlockFromExternalDrawing(new List<string> { BlockName }, filePath, db, mode);
                }
            }
        }

        public string GetFilePath()
        {
            if (!File.StartsWith(@"\"))
                File = @"\" + File;
            return Path.GetLocalSettingsFile($@"Blocks{File}");
        }

        /// <summary>
        /// Переопределение блока
        /// </summary>
        private void Redefine()
        {
            if (!CanRedefine())
            {
                MessageBox.Show($"В текущем чертеже нет блока {BlockName}.");
                return;
            }

            CopyBlock(DuplicateRecordCloning.Replace);
        }
    }
}
