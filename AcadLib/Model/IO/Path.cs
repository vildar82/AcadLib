namespace AcadLib.IO
{
    using System;
    using System.IO;
    using AutoCAD_PIK_Manager.Settings;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class Path
    {
        /// <summary>
        /// Получение файла в общей папке настроек на сервере \\dsk2.picompany.ru\project\CAD_Settings\AutoCAD_server\ShareSettings\[UserGroup]\pluginName\fileName
        /// </summary>
        /// <param name="pluginName">Имя плагина (команды)</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Полный путь к файлу. Наличие файла не проверяется. Папка создается</returns>
        [NotNull]
        public static string GetSharedFile([NotNull] string pluginName, [NotNull] string fileName)
        {
            return System.IO.Path.Combine(GetSharedPluginFolder(pluginName), fileName);
        }

        [NotNull]
        public static string GetSharedCommonFile([NotNull] string pluginName, [NotNull] string fileName)
        {
            return System.IO.Path.Combine(GetSharedCommonFolder(pluginName), fileName);
        }

        /// <summary>
        /// Получение файла в локальных настройках
        /// </summary>
        /// <param name="relativeFromSettings">Путь от папки Settings</param>
        /// <returns>Полный путь</returns>
        [NotNull]
        public static string GetLocalSettingsFile([NotNull] string relativeFromSettings)
        {
            return System.IO.Path.Combine(PikSettings.LocalSettingsFolder, relativeFromSettings);
        }

        [NotNull]
        public static string GetSharedPluginFolder([NotNull] string pluginName)
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

        [NotNull]
        public static string GetSharedCommonFolder([NotNull] string pluginName)
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
        /// <returns></returns>
        [NotNull]
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
        [NotNull]
        public static string GetUserPluginFile([NotNull] string plugin, [NotNull] string fileName)
        {
            var pluginFolder = GetUserPluginFolder(plugin);
            return System.IO.Path.Combine(pluginFolder, fileName);
        }

        /// <summary>
        /// Путь к папке плагина
        /// </summary>
        /// <param name="plugin">Имя плагина - имя папки</param>
        /// <returns>Полный путь</returns>
        [NotNull]
        public static string GetUserPluginFolder([NotNull] string plugin)
        {
            var pikFolder = GetUserPikFolder();
            var pluginFolder = System.IO.Path.Combine(pikFolder, plugin);
            if (!Directory.Exists(pluginFolder))
                Directory.CreateDirectory(pluginFolder);
            return pluginFolder;
        }
    }
}