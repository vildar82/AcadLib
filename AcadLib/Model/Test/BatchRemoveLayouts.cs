namespace AcadLib.Test
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Autodesk.AutoCAD.DatabaseServices;
    using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;

    public static class BatchRemoveLayouts
    {
        public static void Batch()
        {
            var dwgFile = SelectFile();
            using (var db = new Database(false, true))
            {
                db.ReadDwgFile(dwgFile, System.IO.FileShare.Read, true, string.Empty);
                db.CloseInput(true);
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var dict = db.LayoutDictionaryId.GetObject<DBDictionary>();
                    foreach (var entry in dict)
                    {
                        var l = entry.Value.GetObjectT<Layout>(OpenMode.ForWrite);
                        if (!l.ModelType)
                        {
                            l.Erase();
                        }
                    }

                    t.Commit();
                }

                db.SaveAs(dwgFile, DwgVersion.Current);
            }
        }

        private static string SelectFile()
        {
            var d = new OpenFileDialog("Выбор файла", "", "dwg", "Очистка листов", OpenFileDialog.OpenFileDialogFlags.NoFtpSites);
            if (d.ShowDialog() == DialogResult.OK)
            {
                return d.Filename;
            }

            throw new OperationCanceledException();
        }
    }
}
