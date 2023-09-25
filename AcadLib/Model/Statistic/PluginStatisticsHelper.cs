namespace AcadLib.Statistic
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Db;
    using JetBrains.Annotations;
    using NetLib;
    using General = General;

    [PublicAPI]
    public static class PluginStatisticsHelper
    {
        private static string _app;
        private static string _acadLibVer;
        private static bool? _isCivil = GetIsCivil();
        private static bool _isInsertStatisticError;
        private static string? _settingsVersions;

        static PluginStatisticsHelper()
        {
            AcadYear = HostApplicationServices.Current.releaseMarketVersion;
        }

        public static string AcadYear { get; }

        public static bool IsCivil => _isCivil ?? false;

        public static string App => _app ??= IsCivil ? "Civil" : "AutoCAD";

        public static void AddStatistic()
        {
            try
            {
                var caller = new StackTrace().GetFrame(1).GetMethod();
                PluginStart(CommandStart.GetCallerCommand(caller));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PluginStatisticsHelper.AddStatistic");
            }
        }

        public static void PluginStart(string command)
        {
            var com = new CommandStart(command, Assembly.GetCallingAssembly());
            PluginStart(com);
        }

        public static void PluginStart(CommandStart command)
        {
            if (!IsUserStatistic())
                return;
            try
            {
                var version = command.Assembly != null
                    ? FileVersionInfo.GetVersionInfo(command.Assembly.Location).ProductVersion
                    : string.Empty;
                if (command.Plugin.IsNullOrEmpty())
                    command.Plugin = command.Assembly?.GetName().Name;
                if (command.Doc.IsNullOrEmpty())
                    command.Doc = Application.DocumentManager.MdiActiveDocument?.Name;
                InsertStatistic(App, command.Plugin, command.CommandName, version, command.Doc);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PluginStart.");
            }
        }

        public static void StartAutoCAD()
        {
            try
            {
                InsertStatistic($"{App} {AcadYear} Run", "AcadLib", $"{App} Run", Commands.AcadLibVersion.ToString(), string.Empty);

                // Статистика обновления настроек
                UpdateSettings();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "StartAutoCAD.");
            }
        }

        /// <summary>
        /// Запись статистики обновления настроек
        /// </summary>
        private static void UpdateSettings()
        {
            try
            {
                if (PikSettings.IsUpdatedSettings)
                {
                    InsertStatistic($"{App} Update", "AcadLib",
                        PikSettings.IsDisabledSettings ? "Настройки отключены" : "Настройки последние",
                        Commands.AcadLibVersion.ToString(), string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PluginStatisticsHelper.UpdateSettings");
            }
        }

        private static bool IsUserStatistic()
        {
            return !General.IsCadManager() && !General.IsBimUser;
        }

        public static void InsertStatistic(string appName, string? plugin, string? command, string? version, string? doc)
        {
            Task.Run(() =>
            {
                SendLogToRobot(appName, plugin, command, version, doc);
                SendLogToPikTools(plugin, command, version);

                if (_isInsertStatisticError || Environment.UserName.EqualsIgnoreCase("chuchkalovaav"))
                {
                    return;
                }

                try
                {
                    var db = new PluginStatisticDbContext();
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.C_PluginStatistics.Add(new C_PluginStatistic
                    {
                        Application = appName,
                        Plugin = plugin ?? string.Empty,
                        Command = command ?? string.Empty,
                        Build = version?.Truncate(40) ?? string.Empty,
                        Doc = doc?.Truncate(500) ?? string.Empty,
                        UserName = Environment.UserName,
                        DateStart = DateTime.Now,
                        DocName = Path.GetFileName(doc),
                    });

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _isInsertStatisticError = true;
                    Logger.Log.Error(
                        ex,
                        $"PluginStatisticsHelper Insert. appName={appName}, plugin={plugin}, command={command}, version={version}, doc={doc}, docName={Path.GetFileName(doc)}");
                }
            }).ConfigureAwait(false);
        }

        private static async void SendLogToRobot(
            string appName,
            string? plugin,
            string? command,
            string? version,
            string? doc)
        {
            try
            {
                using var client = new HttpClient();
                var json = "{" +
                           "\"source\": \"cad\"," +
                           $"\"UserName\": \"{Environment.UserName}\"," +
                           $"\"MachineName\": \"{Environment.MachineName}\"," +
                           $"\"Message\": \"{command}\"," +
                           $"\"Group\": \"{General.UserGroup}\"," +
                           $"\"Application\": \"{appName}\"," +
                           $"\"Plugin\": \"{plugin}\"," +
                           $"\"Build\": \"{version}\"," +
                           $"\"Doc\": \"{GetPath(doc)}\"" +
                           "}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("https://bim.pik.ru/robotlogs/cad", content);
            }
            catch
            {
                // ignore
            }
        }

        private static async void SendLogToPikTools(
            string? plugin,
            string? command,
            string? version)
        {
            try
            {
                using var client = new HttpClient();
                _settingsVersions ??= Enumerable.Reverse(PikSettings.Versions)
                    .JoinToString(g => $"{g.GroupName} - {g.VersionLocal}");

                var json =
                    "[{" +
                    "\"Level\": \"Information\"," +
                    $"\"MessageTemplate\": \"{plugin}, {command}\"," +
                    "\"Properties\": {" +
                    "\"SourceContext\": \"Program\"," +
                    $"\"MachineName\": \"{Environment.MachineName}\"," +
                    $"\"EnvironmentUserName\": \"{Environment.UserName}\"," +
                    $"\"Application\": \"OldTools_Civil_{command ?? plugin}\"," +
                    $"\"AutoCAD_Version\": \"{AcadYear}\"," +
                    "\"Mode\": \"Production\"," +
                    $"\"Plugin_Version\": \"{_settingsVersions}, {version}\"" +
                    "}}]";

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("http://10.177.202.52:5000", content);
            }
            catch
            {
                // ignore
            }
        }

        private static string GetPath(string? doc)
        {
            return doc?.Replace(@"\", @"\\");
        }

        private static bool GetIsCivil()
        {
            try
            {
                return CivilTest.IsCivil();
            }
            catch
            {
                return false;
            }
        }
    }
}