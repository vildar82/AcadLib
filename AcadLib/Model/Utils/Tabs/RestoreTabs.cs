namespace AcadLib.Utils.Tabs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AcadLib.UI.StatusBar;
    using AutoCAD_PIK_Manager;
    using Autodesk.AutoCAD.ApplicationServices;
    using Data;
    using Errors;
    using JetBrains.Annotations;
    using NetLib;
    using Properties;
    using Statistic;
    using UI;
    using User;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Commands = AcadLib.Commands;
    using Path = IO.Path;

    /// <summary>
    /// Восстановление ранее отурытых вкладок
    /// </summary>
    public static class RestoreTabs
    {
        internal const string PluginName = "RestoreTabs";
        private const string ParamRestoreIsOn = "RestoreTabsIsOn";
        [NotNull]
        private static readonly List<Document> _docs = new List<Document>();
        private static string cmd;
        private static Tabs _tabs;

        public static void Init()
        {
            try
            {
                Logger.Log.Info("RestoreTabs Init");
                UserSettingsService.RegPlugin(PluginName, CreateUserSettings, CheckUserSettings);
                UserSettingsService.ChangeSettings += UserSettingsService_ChangeSettings;

                // Добавление кнопки в статус бар
                StatusBarEx.AddPane(string.Empty, "Откытие чертежей", (p, e) => Restore(), icon: Resources.restoreFiles16);

                var isOn = UserSettingsService.GetPluginValue<bool>(PluginName, ParamRestoreIsOn);
                if (isOn)
                {
                    Logger.Log.Info("RestoreTabs включен.");
                    Subscribe();
                    var tabsData = LoadData();
                    if (tabsData.Data?.Sessions?.Any(s => s?.Drawings?.Count > 0) == true)
                    {
                        Restore();
                    }
                }
                else
                {
                    Logger.Log.Info("RestoreTabs отключен.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "RestoreTabs.Init");
            }
        }

        private static PluginSettings CreateUserSettings()
        {
            return new PluginSettings
            {
                Name = PluginName,
                Title = "Восстановление вкладок",
                Properties = new List<UserProperty>
                {
                    new UserProperty
                    {
                        ID = ParamRestoreIsOn,
                        Name = "Запускать при старте",
                        Value = true,
                        Description = "Открывать окно открытия чертежей последнего сеанса при старте автокада."
                    }
                }
            };
        }

        private static void CheckUserSettings(PluginSettings pluginSettings)
        {
            pluginSettings.Title = "Восстановление вкладок";
            var propIsOn = pluginSettings.Properties.FirstOrDefault(p => p.ID == ParamRestoreIsOn) ?? new UserProperty
            {
                ID = ParamRestoreIsOn,
                Value = true,
            };
            propIsOn.Name = "Запускать при старте";
            propIsOn.Description = "Открывать окно открытия чертежей последнего сеанса при старте автокада.";
            pluginSettings.Properties = new List<UserProperty> { propIsOn };
        }

        /// <summary>
        /// Воссатановление вкладок
        /// </summary>
        private static void Restore()
        {
            var tabsData = LoadData();
            _tabs = tabsData.Data;
            _tabs.Sessions = tabsData.Data.Sessions.Where(w => w.Drawings?.Any() == true)
                .OrderByDescending(o => o.Date).Take(tabsData.Data.SessionCount).ToList();
            Application.Idle += Application_Idle;
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= Application_Idle;
                Logger.Log.Info("RestoreTabs Application_Idle");
                var tabVM = new TabsVM(_tabs);
                var tabsView = new TabsView(tabVM);
                if (Application.ShowModalWindow(tabsView) == true)
                {
                    try
                    {
                        Application.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;
                        var closeDocs = Application.DocumentManager.Cast<Document>().Where(w => !w.IsNamedDrawing).ToList();
                        var tabsRestore = tabVM.Sessions.SelectMany(s => s.Tabs.Where(w => w.Restore)).Select(s => s.File).ToList();
                        if (tabVM.HasHistory)
                        {
                            tabsRestore = tabsRestore.Union(tabVM.History.Where(w => w.Restore).Select(s => s.File)).Distinct().ToList();
                        }

                        foreach (var item in tabsRestore)
                        {
                            try
                            {
                                Application.DocumentManager.Open(item, false);
                            }
                            catch (Exception ex)
                            {
                                Inspector.AddError($"Ошибка открытия файла '{item}' - {ex.Message}");
                            }
                        }

                        // Закрыть пустые чертежи
                        foreach (var doc in closeDocs)
                        {
                            try
                            {
                                doc.CloseAndDiscard();
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error(ex, "RestoreTabs. Закрыть пустые чертежи.");
                            }
                        }

                        if (tabsRestore?.Any() == true)
                            LogRestoreTabs(tabsRestore);
                    }
                    finally
                    {
                        Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
                        Inspector.Show();
                    }
                }

                if (tabVM.SessionCount != _tabs.SessionCount)
                {
                    _tabs.SessionCount = tabVM.SessionCount;
                    var tabsData = LoadData();
                    tabsData.Data.SessionCount = tabVM.SessionCount;
                    tabsData.TrySave();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "RestoreTabs.Application_Idle");
            }
        }

        private static void LogRestoreTabs(List<string> tabsRestore)
        {
            Task.Run(() =>
            {
                PluginStatisticsHelper.PluginStart("RestoreTabsOpen");
                Logger.Log.Info($"RestoreTabsOpen: {tabsRestore.JoinToString()}");
            });
        }

        private static void UserSettingsService_ChangeSettings(object sender, EventArgs e)
        {
            var isOn = UserSettingsService.GetPluginValue<bool>(PluginName, ParamRestoreIsOn);
            if (isOn)
            {
                Subscribe();
            }
            else
            {
                Unsubscribe();
            }
        }

        private static void Subscribe()
        {
            foreach (Document doc in Application.DocumentManager)
            {
                AddTab(doc);
            }

            // Если автокад закрывается, то не нужно обрабатывать события закрытия чертежей
            Application.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
            Application.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;

            // Подписаться на события открытия/закрытия чертежей
            Application.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;
            Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
            Application.DocumentManager.DocumentDestroyed -= DocumentManager_DocumentDestroyed;
            Application.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
        }

        private static void Unsubscribe()
        {
            try
            {
                Application.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
                Application.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;
                Application.DocumentManager.DocumentDestroyed -= DocumentManager_DocumentDestroyed;

                foreach (var tab in _docs)
                {
                    if (tab?.Database != null)
                        tab.Database.SaveComplete -= Database_SaveComplete;
                }

                _docs.Clear();
            }
            catch
            {
                // Если подписок не было
            }
        }

        [NotNull]
        private static string GetFile()
        {
            return Path.GetUserPluginFile(PluginName, PluginName + ".json");
        }

        private static void DocumentManager_DocumentLockModeChanged(object sender, [NotNull] DocumentLockModeChangedEventArgs e)
        {
            switch (e.GlobalCommandName)
            {
                case "":
                case "#":
                case "#CLOSE":
                case "#QUIT":
                    return;
            }

            cmd = e.GlobalCommandName;
        }

        private static void AddTab(Document doc)
        {
            if (doc?.Database == null || _docs.Contains(doc))
                return;
            _docs.Add(doc);
            doc.Database.SaveComplete -= Database_SaveComplete;
            doc.Database.SaveComplete += Database_SaveComplete;
            if (doc.IsNamedDrawing)
                SaveTabs();
        }

        private static void Database_SaveComplete(object sender, Autodesk.AutoCAD.DatabaseServices.DatabaseIOEventArgs e)
        {
            if (cmd != "QUIT")
                SaveTabs();
        }

        private static void RemoveTabs()
        {
            _docs.RemoveAll(t => t?.Database == null);
            SaveTabs();
        }

        private static void SaveTabs()
        {
            Debug.WriteLine("SaveTabs");
            var drawings = _docs.Where(w => w?.Database != null && w.IsNamedDrawing).Select(s => s.Name).ToList();
            if (drawings.Count == 0)
                return;
            var tabsData = LoadData();
            var session = tabsData.Data.Sessions.FirstOrDefault(s => s.Id == AcadHelper.GetCurrentAcadProcessId());
            if (session == null)
            {
                session = new Session { Drawings = drawings, Id = AcadHelper.GetCurrentAcadProcessId(), Date = DateTime.Now };
                tabsData.Data.Sessions.Add(session);
            }
            else
            {
                session.Drawings = drawings;
                session.Date = DateTime.Now;
            }

            tabsData.Data.Sessions = tabsData.Data.Sessions
                .Where(s => s.Drawings?.Any() == true)
                .Distinct(new SessionComparer())
                .OrderByDescending(o => o.Date).Take(tabsData.Data.SessionCount).ToList();
            tabsData.TrySave();
        }

        private static void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            Debug.WriteLine($"DocumentManager_DocumentDestroyed cmd={cmd}");
            if (cmd == "CLOSE" && System.IO.Path.IsPathRooted(e.FileName))
            {
                RemoveTabs();
            }
        }

        private static void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            AddTab(e?.Document);
        }

        public static void SetIsOn(bool isOn)
        {
            UserSettingsService.SetPluginValue(PluginName, ParamRestoreIsOn, isOn);
        }

        public static bool GetIsOn()
        {
            return UserSettingsService.GetPluginValue<bool>(PluginName, ParamRestoreIsOn);
        }

        private static LocalFileData<Tabs> LoadData()
        {
            var tabsData = new LocalFileData<Tabs>(GetFile(), false);
            tabsData.TryLoad(() => new Tabs());
            return tabsData;
        }
    }

    internal class SessionComparer : IEqualityComparer<Session>
    {
        /// <inheritdoc />
        public bool Equals(Session x, Session y)
        {
            return !x.Drawings.Except(y.Drawings).Any();
        }

        /// <inheritdoc />
        public int GetHashCode(Session session)
        {
            return 0;
        }
    }
}
