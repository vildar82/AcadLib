namespace AcadLib.IO
{
    using System;
    using System.IO;
    using AutoCAD_PIK_Manager.Settings;

    public static class Path
    {
        /// <summary>
        /// Получение файла в общей папке настроек на сервере \\dsk2.picompany.ru\project\CAD_Settings\AutoCAD_server\ShareSettings\[UserGroup]\pluginName\fileName
        /// </summary>
        /// <param name="pluginName">Имя плагина (команды)</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Полный путь к файлу. Наличие файла не проверяется. Папка создается</returns>
        public static string GetSharedFile(string pluginName, string fileName)
        {
            return System.IO.Path.Combine(GetSharedPluginFolder(pluginName), fileName);
        }

        public static string GetSharedCommonFile(string pluginName, string fileName)
        {
            return System.IO.Path.Combine(GetSharedCommonFolder(pluginName), fileName);
        }

        /// <summary>
        /// Получение файла в локальных настройках
        /// </summary>
        /// <param name="relativeFromSettings">Путь от папки Settings</param>
        /// <returns>Полный путь</returns>
        public static string GetLocalSettingsFile(string relativeFromSettings)
        {
            return System.IO.Path.Combine(PikSettings.LocalSettingsFolder, relativeFromSettings);
        }

        public static string GetSharedPluginFolder(string pluginName)
        {
            var pluginFolder = System.IO.Path.Combine(PikSettings.ServerShareSettingsFolder,
                PikSettings.UserGroup, pluginName);
            if (!Directory.Exists(pluginFolder))
            {
                try
                {
                    Directory.CreateDirectory(pluginFolder);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"GetSharedpluginFolder - pluginName={pluginName}");
                }
            }

            return pluginFolder;
        }

        public static string GetSharedCommonFolder(string pluginName)
        {
            var pluginFolder = System.IO.Path.Combine(PikSettings.ServerShareSettingsFolder, pluginName);
            if (!Directory.Exists(pluginFolder))
            {
                try
                {
                    Directory.CreateDirectory(pluginFolder);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"GetSharedCommonFolder - pluginName={pluginName}");
                }
            }

            return pluginFolder;
        }

        /// <summary>
        /// Пользовательская папка настроек
        /// </summary>
        public static string GetUserPikFolder()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create);
            var pikFolder = AutoCAD_PIK_Manager.CompanyInfo.NameEngShort;
            var pikAppDataFolder = System.IO.Path.Combine(appData, pikFolder, "AutoCAD");
            if (!Directory.Exists(pikAppDataFolder))
            {
                Directory.CreateDirectory(pikAppDataFolder);
            }

            return pikAppDataFolder;
        }

        /// <summary>
        /// Путь к пользовательскому файлу настроек плагина
        /// </summary>
        /// <param name="plugin">Имя плагина</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Полный путь к файлу</returns>
        public static string GetUserPluginFile(string plugin, string fileName)
        {
            var pluginFolder = GetUserPluginFolder(plugin);
            return System.IO.Path.Combine(pluginFolder, fileName);
        }

        /// <summary>
        /// Путь к папке плагина
        /// </summary>
        /// <param name="plugin">Имя плагина - имя папки</param>
        /// <returns>Полный путь</returns>
        public static string GetUserPluginFolder(string plugin)
        {
            var pikFolder = GetUserPikFolder();
            var pluginFolder = System.IO.Path.Combine(pikFolder, plugin);
            if (!Directory.Exists(pluginFolder))
                Directory.CreateDirectory(pluginFolder);
            return pluginFolder;
        }
    }
}