using Naming.Common;
using PathChecker.Models;

namespace AcadLib.Statistic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;
    using NetLib;
    using PathChecker;
    using Reactive;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Exception = System.Exception;

    public static class EventsStatisticService
    {
        private static bool veto;
        private static string sn;
        private static Eventer eventer;
        private static string overrideName;
        private static Document _currentDoc;
        private static string lastModeChange;
        private static string lastSaveAsFile;

        [NotNull]
        private static readonly List<string> _exceptedUsers = new List<string>
        {
            "PokrovskiyID",
            "valievtr",
            "PrudnikovVS",
            "ParamazovaSK",
            "vrublevskiyba",
            "arslanovti",
            "ishmaevar",
            "karadzhayanra"
        };

        public static void Start()
        {
            try
            {
                if (IsExcludeUser()) return;
                Application.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
                Task.Run(() => { eventer = new Eventer(GetApp(), HostApplicationServices.Current.releaseMarketVersion); });
                Application.DocumentManager.DocumentCreateStarted += DocumentManager_DocumentCreateStarted;
                Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
                Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
                Application.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;

                foreach (Document doc in Application.DocumentManager)
                {
                    eventer?.Start(global::PathChecker.Models.SaveType.Default, null);
                    SubscribeDoc(doc);
                }

                sn = GetRegistrySerialNumber();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "EventsStatisticService.Start");
            }
        }

        private static bool IsExceptedUser()
        {
            return _exceptedUsers.Any(u => u.EqualsIgnoreCase(Environment.UserName));
        }

        private static bool IsExcludeUser()
        {
            if (IsExceptedUser())
            {
                Logger.Log.Info("Пользователь исключен из нейминга.");
                return true;
            }

            // Департамент продукта
            var isProductUser = UserInfo.IsProductUser;
            if (isProductUser)
            {
                Logger.Log.Info("Пользователь из Деп.Продукта - Статистика и нейминг пропущен.");
                return true;
            }

            // Индустрия
            if (Environment.UserDomainName.EqualsIgnoreCase("DSK2"))
            {
                Logger.Log.Info("Пользователь из Индустрии - Статистика и нейминг пропущен.");
                return true;
            }

            if (Environment.UserName.EqualsIgnoreCase("egorov_ps"))
            {
                Logger.Log.Info("Пользователь из исключений - Фаталит автокад при сохранении.");
                return true;
            }

            return false;
        }

        [NotNull]
        private static string GetApp()
        {
            try
            {
                if (CivilTest.IsCivil()) return "Civil";
            }
            catch
            {
                // Это не Civil
            }

            return "AutoCAD";
        }

        private static void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            eventer?.Start(global::PathChecker.Models.SaveType.Default, null);
        }

        private static void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
        {
            Debug.WriteLine($"DocumentManager_DocumentLockModeChanged {e.GlobalCommandName} {e.Document.Name}");
            try
            {
                short dbmod = (short)Application.GetSystemVariable("DBMOD");
                switch (e.GlobalCommandName)
                {
                    case "QSAVE":
                        Logger.Log.Info("Eventer DocumentLockModeChanged=QSAVE");
                        StopSave(e, global::PathChecker.Models.SaveType.Default);
                        lastModeChange = "QSAVE";
                        break;
                    case "SAVEAS":
                        Logger.Log.Info("Eventer DocumentLockModeChanged=SAVEAS");
                        if (lastModeChange != "SAVEAS")
                        {
                            lastModeChange = "SAVEAS";
                            StopSave(e, global::PathChecker.Models.SaveType.SaveAs);
                        }

                        break;
                    case "#SAVEAS":
                        Logger.Log.Info("Eventer DocumentLockModeChanged=#SAVEAS");
                        if (lastModeChange != "SAVEAS" || lastSaveAsFile != e.Document.Name)
                        {
                            StopSave(e, global::PathChecker.Models.SaveType.SaveAs);
                        }

                        lastModeChange = "#SAVEAS";
                        break;
                    case "CLOSE":
                        Logger.Log.Info("Eventer DocumentLockModeChanged=CLOSE");
                        if (dbmod != 0 && lastModeChange != "CLOSE")
                        {
                            switch (MessageBox.Show("Файл изменен. Хотите сохранить изменения?", "Внимание!",
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                            {
                                case MessageBoxResult.Yes:
                                    if (!StopSave(e, global::PathChecker.Models.SaveType.Default))
                                    {
                                        e.Veto();
                                        CloseSave(e.Document);
                                    }
                                    else
                                    {
                                        e.Veto();
                                        CloseDiscard(e.Document);
                                    }

                                    lastModeChange = "CLOSE";
                                    break;
                                case MessageBoxResult.No:
                                    e.Veto();
                                    CloseDiscard(e.Document);
                                    break;
                                case MessageBoxResult.Cancel:
                                    e.Veto();
                                    break;
                            }
                        }

                        lastModeChange = "CLOSE";
                        break;
                    default:
                        lastModeChange = null;
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Отмена
                e.Veto();
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex, $"EventsStatisticService DocumentManager_DocumentLockModeChanged, GlobalCommandName={e?.GlobalCommandName}");
            }
        }

        private static void CloseDiscard(Document doc)
        {
            _currentDoc = doc;
            Application.Idle += CloseDiscardOnIdle;
        }

        private static void CloseDiscardOnIdle(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= CloseDiscardOnIdle;
                if (_currentDoc == null)
                    return;
                Logger.Log.Info($"EventsStatisticService CloseDiscardOnIdle {_currentDoc?.Name}.");
                _currentDoc.CloseAndDiscard();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "EventsStatisticService CloseDiscardOnIdle");
            }
        }

        private static void CloseSave(Document doc)
        {
            _currentDoc = doc;
            Application.Idle += CloseSaveOnIdle;
        }

        private static void CloseSaveOnIdle(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= CloseSaveOnIdle;
                if (_currentDoc == null)
                    return;
                Logger.Log.Info($"EventsStatisticService CloseSaveOnIdle {_currentDoc?.Name}.");
                _currentDoc.CloseAndSave(_currentDoc.Name);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "EventsStatisticService CloseSaveOnIdle");
            }
        }

        private static bool StopSave(DocumentLockModeChangedEventArgs e, global::PathChecker.Models.SaveType @case)
        {
            Logger.Log.Info($"Eventer StopSave case={@case}, doc={e?.Document?.Name}.");
            lastSaveAsFile = e.Document.Name;
            BeginSave(e.Document.Name, @case);
            if (veto)
            {
                Logger.Log.Info($"Eventer Veto case={@case}, doc={e?.Document?.Name}.");
                e.Veto();
                Debug.WriteLine($"StopSave Veto {e.GlobalCommandName}");
                return true;
            }

            Debug.WriteLine($"StopSave no veto {e.GlobalCommandName}");
            return false;
        }

        private static void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            eventer?.Finish(EventType.Close, e?.FileName, sn);
        }

        private static void DocumentManager_DocumentCreateStarted(object sender, DocumentCollectionEventArgs e)
        {
            eventer?.Start(global::PathChecker.Models.SaveType.Default, null);
        }

        private static void DocumentManager_DocumentCreated(object sender, [NotNull] DocumentCollectionEventArgs e)
        {
            SubscribeDoc(e.Document);
        }

        private static void SubscribeDoc([CanBeNull] Document doc)
        {
            if (doc == null)
                return;

            if (sn == null || sn.StartsWith("000"))
            {
                try
                {
                    sn = Application.GetSystemVariable("_pkser") as string;
                    Logger.Log.Info($"EventsStatisticService (_pkser) SerialNumber = {sn}");
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "EventsStatisticService - GetSystemVariable(\"_pkser\")");
                }

                if (sn == null || sn.StartsWith("000"))
                {
                    sn = GetRegistrySerialNumber();
                    Logger.Log.Info($"EventsStatisticService (Registry) SerialNumber = {sn}");
                }
            }

            try
            {
                var db = doc.Database;
                db.Events().SaveComplete.Throttle(TimeSpan.FromSeconds(3))
                    .Do(s => Logger.Log.Info($"Eventer SaveComplete Do - {s?.EventArgs?.FileName}."))
                    .Subscribe(s => Db_SaveComplete(s?.Sender, s?.EventArgs));
                eventer?.Finish(EventType.Open, doc.Name, sn);
                Logger.Log.Info("SubscribeDoc end");
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "EventsStatisticService SubscribeDoc");
            }
        }

        private static string GetRegistrySerialNumber()
        {
            try
            {
                var prod = Registry.LocalMachine.OpenSubKey(HostApplicationServices.Current.MachineRegistryProductRootKey);
                return prod.GetValue("SerialNumber")?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void BeginSave(string file, global::PathChecker.Models.SaveType @case)
        {
            veto = false;
            Debug.WriteLine($"Db_BeginSave {file}");
            if (!IsDwg(file))
                return;
            if (IsCheckError(eventer?.Start(@case, file)))
            {
                // Отменить сохранение файла
                veto = true;
                Debug.WriteLine($"Отменить сохранение {file}");
            }
        }

        private static void Db_SaveComplete(object sender, DatabaseIOEventArgs e)
        {
            Debug.WriteLine($"Db_SaveComplete {e.FileName}");
            if (!IsDwg(e?.FileName)) return;
            eventer?.Finish(EventType.Save, e.FileName, sn);
        }

        private static bool IsCheckError(PathCheckerResult checkRes)
        {
            Debug.WriteLine($"checkRes FilePathOverride={checkRes?.FilePathOverride}");
            if (checkRes != null)
            {
                switch (checkRes.NextAction)
                {
                    case NextAction.Proceed: return false;
                    case NextAction.SaveOverride:
                        SaveOverride(checkRes.FilePathOverride);
                        return true;
                    case NextAction.Cancel: throw new OperationCanceledException();
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            return false;
        }

        private static void SaveIdle(object sender, EventArgs e)
        {
            Application.Idle -= SaveIdle;
            var doc = AcadHelper.Doc;
            using (doc.LockDocument())
            {
                doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
            }
        }

        private static void SaveOverride(string overrideName)
        {
            EventsStatisticService.overrideName = overrideName;
            Application.Idle += SaveOverride_Idle;
        }

        private static void SaveOverride_Idle(object sender, EventArgs e)
        {
            Application.Idle -= SaveOverride_Idle;
            Logger.Log.Info("EventsStatisticService SaveOverride_Idle");
            if (string.IsNullOrEmpty(overrideName))
                return;
            var doc = AcadHelper.Doc;
            var oldFile = doc.Name;
            try
            {
                using (doc.LockDocument())
                {
                    doc.Database.SaveAs(overrideName, DwgVersion.Current);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла как '{overrideName}' - {ex.Message}");
                Logger.Log.Error(ex, $"SaveOverride.SaveAs - overrideName={overrideName}.");
                return;
            }

            try
            {
                Application.DocumentManager.Open(overrideName, false);
                overrideName = null;
                doc.CloseAndDiscard();
                BackupOldFile(oldFile);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"SaveOverride - oldFile={oldFile}, overrideName={overrideName}.");
            }
        }

        private static void BackupOldFile(string oldFile)
        {
            if (!File.Exists(oldFile)) return;
            var newName = $"{oldFile}.renamed";
            try
            {
                File.Move(oldFile, newName);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"BackupOldFile - oldFile={oldFile}, newName={newName}.");
            }
        }

        private static bool IsDwg(string fileName)
        {
            try
            {
                return Path.GetExtension(fileName).EqualsIgnoreCase(".dwg");
            }
            catch
            {
                return false;
            }
        }
    }
}
