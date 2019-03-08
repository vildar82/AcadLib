namespace AcadLib.XData
{
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using JetBrains.Annotations;

    public static class DboDataSaveExt
    {
        public static void SaveDboDict([NotNull] this IDboDataSave dboSave)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (var dbo = dboSave.GetDBObject())
            {
                using (var entDic = new EntDictExt(dbo, dboSave.PluginName))
                {
                    entDic.Save(dboSave.GetExtDic(doc));
                }
            }
        }

        public static void LoadDboDict([NotNull] this IDboDataSave dboSave)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (var dbo = dboSave.GetDBObject())
            {
                using (var entDic = new EntDictExt(dbo, dboSave.PluginName))
                {
                    var dicED = entDic.Load();
                    dboSave.SetExtDic(dicED, doc);
                }
            }
        }

        /// <summary>
        /// Удаление словаря из объекта
        /// </summary>
        /// <param name="dboSave">Объект чертежа</param>
        /// <param name="dicName">Имя удаляемого словаря или пусто для удаления всего словаря плагина</param>
        public static void DeleteDboDict([NotNull] this IDboDataSave dboSave, [CanBeNull] string dicName = null)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (var dbo = dboSave.GetDBObject())
            {
                using (var entDic = new EntDictExt(dbo, dboSave.PluginName))
                {
                    entDic.Delete(dicName);
                }
            }
        }
    }
}