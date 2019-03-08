namespace AcadLib.Utils
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;

    public static class ClearObjectsExtData
    {
        public static void Clear(Document doc)
        {
            var ids = doc.Editor.Select("Выбор объектов для очистки словарей");
            using (var t = doc.TransactionManager.StartTransaction())
            {
                foreach (var ent in ids.GetObjects<Entity>())
                {
                    if (!ent.ExtensionDictionary.IsNull)
                    {
                        var extD = ent.ExtensionDictionary.GetObject<DBObject>(OpenMode.ForWrite);
                        extD?.Erase();
                    }
                }

                t.Commit();
            }
        }
    }
}
