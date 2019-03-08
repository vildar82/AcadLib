namespace AcadLib.Layers.LayerState
{
    using System;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Errors;

    public static class ImportLayerState
    {
        /// <summary>
        ///     Импорт всех конфигураций слоев из файла источника
        /// </summary>
        /// <param name="dbDest"></param>
        /// <param name="sourceFile"></param>
        public static void ImportLayerStates(this Database dbDest, string sourceFile)
        {
            try
            {
                using (var dbSrc = new Database(false, false))
                {
                    dbSrc.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                    dbSrc.CloseInput(true);
                    ImportLayerStates(dbSrc, dbDest);
                }
            }
            catch (Exception ex)
            {
                Inspector.AddError($"Ошибка импорта концигурации слоев из файла '{sourceFile}' - {ex.Message}");
            }
        }

        internal static void ImportLayerStates(Database dbSrc, Database dbDest)
        {
            var names = dbSrc.LayerStateManager.GetLayerStateNames(false, false).Cast<string>().ToList();
            foreach (var name in names)
            {
                try
                {
                    dbDest.LayerStateManager.ImportLayerStateFromDb(name, dbSrc);
                }
                catch
                {
                    // Если конфигурация уже есть
                }
            }
        }
    }
}