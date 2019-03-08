namespace AcadLib.UI.Ribbon.Elements
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;
    using IO;
    using Layers;
    using NetLib.WPF.Data;
    using Application = Autodesk.AutoCAD.ApplicationServices.Application;

    public class RibbonVisualInsertBlock : RibbonInsertBlock
    {
        public string Filter { get; set; }

        public override ICommand GetCommand()
        {
            return new RelayCommand(Execute);
        }

        public void Execute()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                    return;
                using (doc.LockDocument())
                {
                    var filePath = GetFilePath();
                    Blocks.Visual.VisualInsertBlock.InsertBlock(filePath, IsFilter, new LayerInfo(Layer), Explode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Ошибка при вставке блока - {ex.Message}");
            }
        }

        private bool IsFilter(string name)
        {
            return Regex.IsMatch(name, Filter, RegexOptions.IgnoreCase);
        }
    }
}
