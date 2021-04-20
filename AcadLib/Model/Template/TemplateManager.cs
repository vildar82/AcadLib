namespace AcadLib.Template
{
    using System;
    using System.IO;
    using System.Linq;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.DatabaseServices;
    using Layers;
    using NetLib;

    /// <summary>
    ///     Управление шаблонами
    /// </summary>
    public static class TemplateManager
    {
        public static void ExportToJson(this TemplateData tData, string file)
        {
            tData.Serialize(file);
        }

        public static TemplateData LoadFromDb(Database db)
        {
            return new TemplateData { Layers = db.Layers().ToDictionary(k => k.Name) };
        }

        public static TemplateData LoadFromJson(string file)
        {
            return LoadFromJson(file, false);
        }

        public static TemplateData LoadFromJson(string file, bool logErr)
        {
            if (!File.Exists(file))
            {
                if (logErr)
                    Logger.Log.Warn($"Не найден файл шаблона json - {file}");
                return new TemplateData();
            }

            try
            {
                var templData = file.Deserialize<TemplateData>();
                templData.Name = Path.GetFileName(file);
                return templData;
            }
            catch (Exception ex)
            {
                Logger.Log.Warn(ex, $"Ошибка загрузки файла шаблона json '{file}'");
                return new TemplateData();
            }
        }

        /// <summary>
        ///     Полный путь к шаблону (из папки Template настроек)
        /// </summary>
        /// <param name="templateFileName">Имя файла шаблона с расширением</param>
        public static string GetTemplateFile(string templateFileName)
        {
            return Path.Combine(PikSettings.LocalSettingsFolder, $@"Template\{PikSettings.UserGroup}\{templateFileName}");
        }
    }
}