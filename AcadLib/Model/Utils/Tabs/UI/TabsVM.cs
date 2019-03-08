using System.Reactive;

namespace AcadLib.Utils.Tabs.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using Data;
    using History;
    using JetBrains.Annotations;
    using Model.Utils.Tabs.History.Db;
    using NetLib;
    using NetLib.Notification;
    using NetLib.WPF;
    using ReactiveUI;
    using ReactiveUI.Legacy;
    using Tabs.History.Db;

    public class TabsVM : BaseViewModel
    {
        private ReactiveList<TabVM> history = new ReactiveList<TabVM>();

        public TabsVM([NotNull] Tabs tabs)
        {
            try
            {
                SessionCount = tabs.SessionCount;
                Sessions = tabs.Sessions.Select(s => new SessionVM(s)).ToList();
                Ok = CreateCommand(OkExec);
                IsOn = RestoreTabs.GetIsOn();
                this.WhenAnyValue(v => v.IsOn).Skip(1)
                    .Delay(TimeSpan.FromMilliseconds(100))
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(dispatcher)
                    .Subscribe(RestoreTabs.SetIsOn);
                HasRestoreTabs = Sessions?.Count > 0;
                if (!HasRestoreTabs)
                {
                    HasHistory = true;
                }

                this.WhenAnyValue(v => v.HistorySearch).Skip(1).Subscribe(s => History.Reset());
                History = history.CreateDerivedCollection(t => t, HistoryFilter, HistoryOrder);
                LoadHistory();
            }
            catch (Exception ex)
            {
                AcadLib.Logger.Log.Error(ex, "RestoreTabs.TabsVM");
            }
        }

        public List<SessionVM> Sessions { get; set; }

        public ReactiveCommand<Unit, Unit> Ok { get; set; }

        public bool HasHistory { get; set; }

        public bool HasRestoreTabs { get; set; }

        public double RestoreTabsColRestoreWidth { get; set; } = 300;

        public double RestoreTabsColNameWidth { get; set; } = 500;

        public string HistorySearch { get; set; }

        public IReactiveDerivedList<TabVM> History { get; set; }

        public bool IsOn { get; set; }

        public int SessionCount { get; set; }

        private TabVM GetTab(string tab, bool restore, DateTime start)
        {
            return new TabVM(tab, restore)
            {
                Start = start
            };
        }

        private void OkExec()
        {
            DialogResult = true;
        }

        public void OpenFileExec(TabVM tab)
        {
            if (File.Exists(tab.File))
            {
                var argument = "/select, \"" + tab.File + "\"";
                Process.Start("explorer.exe", argument);
            }
            else
            {
                var notify = new Notify(new NotifyOptions(TimeSpan.FromSeconds(2),
                    Window,
                    NotifyCorner.BottomCenter,
                    with: 300,
                    offsetX: 0,
                    offsetY: 5));
                notify.Show("Путь скопирован в буфер обмена",
                    NotifyType.Information,
                    new NotifyMessageOptions { ShowCloseButton = false });
                Clipboard.SetText(tab.File);
            }
        }

        private bool HistoryFilter(TabVM tab)
        {
            return HistorySearch.IsNullOrEmpty() || Regex.IsMatch(tab.File, Regex.Escape(HistorySearch), RegexOptions.IgnoreCase);
        }

        private int HistoryOrder(TabVM t1, TabVM t2)
        {
            return t2.Start.CompareTo(t1.Start);
        }

        private void LoadHistory()
        {
            Logger.Info("RestoreTabs TabsVM LoadHistory start");
            var cache = HistoryModel.LoadHistoryCache();
            if (cache.Any())
            {
                Task.Run(() =>
                {
                    var tabs = cache.Select(s => GetTab(s.File, false, s.Start)).ToList();
                    dispatcher.Invoke(() =>
                    {
                        try
                        {
                            tabs.ForEach(t => history.Add(t));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "RestoreTabs TabsVM dispatcher.Invoke History adds from cache.");
                        }
                    });
                });
            }

            Task.Run(() =>
            {
                var dbItems = cache.Any()
                    ? new DbHistory().LoadHistoryFiles(cache.Max(m => m.Start)).ToList()
                    : new DbHistory().LoadHistoryFiles().ToList();
                Logger.Info("RestoreTabs TabsVM Load dbItems.");
                var tabs = dbItems.Select(s => GetTab(s.DocPath, false, s.Start)).ToList();
                Task.Delay(TimeSpan.FromMilliseconds(300)).Wait();
                dispatcher.Invoke(() =>
                {
                    try
                    {
                        tabs.ForEach(t => history.Add(t));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "RestoreTabs TabsVM dispatcher.Invoke History");
                    }
                });
                var removeTabs = history.GroupBy(g => g.File).SelectMany(s => s.OrderByDescending(o => o.Start).Skip(1));
                Task.Delay(TimeSpan.FromMilliseconds(300)).Wait();
                foreach (var tab in removeTabs)
                {
                    dispatcher.Invoke(() =>
                    {
                        try
                        {
                            history.Remove(tab);
                        }
                        catch
                        {
                        }
                    });
                }
            });

            // Загрузка из базы и сохранение в кеш
            Task.Run(() =>
            {
                if (!cache.Any() || HistoryModel.NeedUpdateCashe())
                {
                    var dbItems = new DbHistory().LoadHistoryFiles().ToList();
                    var tabs = dbItems.Select(s => new HistoryTab { File = s.DocPath, Start = s.Start })
                        .GroupBy(g => g.File).Select(s => s.OrderByDescending(o => o.Start).FirstOrDefault()).ToList();
                    HistoryModel.SaveHistoryCache(tabs);
                }
            });
        }

        private HistoryTab GetHistoryTab(StatEvents item)
        {
            return new HistoryTab { File = item.DocPath, Start = item.Start };
        }
    }
}