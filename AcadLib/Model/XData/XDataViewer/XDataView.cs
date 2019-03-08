// ReSharper disable once CheckNamespace
namespace AcadLib.XData.Viewer
{
    using System;
    using System.Text;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class XDataView
    {
        public static void View()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var ed = doc.Editor;
            var db = doc.Database;

            var opt = new PromptEntityOptions("\nВыбери приметив:");
            var res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                var sbInfo = new StringBuilder();
                string entName;
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var ent = (Entity)res.ObjectId.GetObject(OpenMode.ForRead, false, true);
                    entName = ent.ToString();
                    if (ent.XData != null)
                    {
                        sbInfo.AppendLine("XData:");
                        foreach (var item in ent.XData)
                        {
                            sbInfo.AppendLine($"    {GetTypeCodeName(item.TypeCode)} = {item.Value}");
                        }
                    }

                    if (!ent.ExtensionDictionary.IsNull)
                    {
                        sbInfo.AppendLine("\nExtensionDictionary:");
                        ExploreDictionary(ent.ExtensionDictionary, ref sbInfo);
                    }

                    if (sbInfo.Length == 0)
                    {
                        ed.WriteMessage("\nНет расширенных данных у {0}", ent);
                        return;
                    }

                    t.Commit();
                }

                var formXdataView = new FormXDataView(sbInfo.ToString(), entName);
                Application.ShowModalDialog(formXdataView);
            }
        }

        private static void ExploreDictionary(ObjectId idDict, ref StringBuilder sbInfo, string tab = "    ")
        {
            var entry = idDict.GetObject(OpenMode.ForRead);
            switch (entry)
            {
                case DBDictionary _:
                    var dict = (DBDictionary)entry;
                    foreach (var item in dict)
                    {
                        sbInfo.AppendLine($"{tab}{item.Key}");
                        ExploreDictionary(item.Value, ref sbInfo, tab + "    ");
                    }

                    break;

                case Xrecord _:
                    var xrec = (Xrecord)entry;
                    foreach (var item in xrec)
                    {
                        sbInfo.AppendLine($"{tab}    {GetTypeCodeName(item.TypeCode)} = {item.Value}");
                    }

                    break;
            }
        }

        [CanBeNull]
        private static string GetTypeCodeName(short typeCode)
        {
            return Enum.GetName(typeof(DxfCode), typeCode);
        }
    }
}