using AcadLib.Blocks;
using AcadLib.Doc;
using DynamicData.Kernel;
using OfficeOpenXml;

namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using AutoCAD_PIK_Manager.Settings;
    using AutoCAD_PIK_Manager.User;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Runtime;
    using Blocks.Visual;
    using Colors;
    using Editors;
    using Errors;
    using Field;
    using JetBrains.Annotations;
    using Layers;
    using Layers.AutoLayers;
    using Layers.LayersSelected;
    using NetLib;
    using NetLib.IO;
    using NetLib.Notification;
    using PaletteCommands;
    using PaletteProps;
    using Plot;
    using Properties;
    using Statistic;
    using Template;
    using UI.Ribbon;
    using UI.Ribbon.Editor;
    using UI.StatusBar;
    using User;
    using Utils;
    using Exception = System.Exception;
    using Path = System.IO.Path;

    [PublicAPI]
    public class Commands : IExtensionApplication
    {
        public const string CommandBlockList = "PIK_BlockList";
        public const string CommandCleanZombieBlocks = "PIK_CleanZombieBlocks";
        public const string CommandColorBookNCS = "PIK_ColorBookNCS";
        public const string CommandXDataView = "PIK_XDataView";
        public const string GroupCommon = "Общие";
        public const string Group = AutoCAD_PIK_Manager.Commands.Group;

        public static readonly Assembly AcadLibAssembly = Assembly.GetExecutingAssembly();

        public static readonly System.Version AcadLibVersion = AcadLibAssembly.GetName().Version;
        public static readonly string CurDllDir = Path.GetDirectoryName(AcadLibAssembly.Location);

        internal static readonly string FileCommonBlocks =
            Path.Combine(PikSettings.LocalSettingsFolder, @"Blocks\Блоки-оформления.dwg");

        private readonly Timer timer = new Timer();

        private List<DllResolve> dllsResolve;
        [NotNull]
        internal static Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        /// <summary>
        ///     Общие команды для всех отделов определенные в этой сборке
        /// </summary>
        public static List<IPaletteCommand> CommandsPalette { get; set; }

        public void Initialize()
        {
#if DEBUG
            // Отключение отладочных сообщений биндинга (тормозит сильно)
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Off;
#endif
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                Logger.Log.Info("start Initialize AcadLib");
                StatusBarEx.AddPaneUserGroup();
                SendStartAutocadStatistic();
                if (PikSettings.IsDisabledSettings)
                {
                    Logger.Log.Info("Настройки отключены (PikSettings.IsDisabledSettings) - загрузка прервана.");
                    return;
                }

                Notify.SetScreenSettings(new NotifyOptions(with: 400));

                CheckUpdates.Start();
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                }

                PaletteSetCommands.Init();
                AllCommandsCommon();

                // Автослоиtest
                AutoLayersService.Init();

                // Загрузка сборок из папки ../Script/Net - без вложенных папок
                LoadService.LoadFromFolder(Path.Combine(PikSettings.LocalSettingsFolder, @"Script\NET"), 1);

                // Обработка чертежей
                DocAuto.Start();

                // Лента
                RibbonBuilder.InitRibbon();
                Logger.Log.Info("end Initialize AcadLib");
                AcadLibAssembly.AcadLoadInfo();
                CheckUser();

                // Восстановление вкладок чCheckUpdatesNotifyертежей
                //Utils.Tabs.RestoreTabs.Init(); // Фаталит у Черновой
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                Logger.Log.Info("AcadLib Initialize end success.");
            }
            catch (Exception ex)
            {
                $"PIK. Ошибка загрузки AcadLib, версия:{AcadLibVersion} - {ex.Message}.".WriteToCommandLine();
                Logger.Log.Error(ex, "AcadLib Initialize.");
            }
        }

        private void CheckUser()
        {
            try
            {
                if (AutocadUserService.User == null)
                {
                    Logger.Log.Warn("Настройки группы пользователя не заданы - открытие окна настроек пользователя.");
                    UserSettingsService.Show();
                }
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }

        private void SendStartAutocadStatistic()
        {
            try
            {
                PluginStatisticsHelper.StartAutoCAD();
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }

        private void CheckOtherAcadVersionProcess()
        {
            try
            {
                if (General.IsBimUser)
                    return;
                var curVer = Application.ProductVersion.GetMajorAcadVersion();
                var acads = Process.GetProcessesByName("acad");
                if (acads.Length == 1)
                    return;
                var otherVer = acads
                    .Select(GetMajorAcadVersion)
                    .FirstOrDefault(o => !curVer.EqualsIgnoreCase(o));

                if (!otherVer.IsNullOrEmpty())
                {
                    var msg = $"Нельзя запускать две разные версии acad! Занимаются две лицензии. Текущая версия {curVer}, другая запущенная версия {otherVer}.";
                    Logger.Log.Info(msg);
                    HostApplicationServices.Current.FatalError(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CheckOtherAcadVersionProcess");
            }
        }

        private string GetMajorAcadVersion(Process p)
        {
            var ver = FileVersionInfo.GetVersionInfo(p.MainModule.FileName).ProductVersion;
            return ver.GetMajorAcadVersion();
        }

        public void Terminate()
        {
            RibbonBuilder.SaveActiveTab();
            Logger.Log.Info("Terminate AcadLib");
        }

        [CommandMethod(Group, "PIK_Acadlib_About", CommandFlags.Modal)]
        public void About()
        {
            CommandStart.Start(doc =>
            {
                var ed = doc.Editor;
                ed.WriteMessage($"\nБиблиотека AcadLib версии {AcadLibVersion}");
            });
        }

        [CommandMethod(Group, "PIK_UserSettings", CommandFlags.Modal)]
        public void PIK_UserSettings()
        {
            CommandStart.Start(doc =>
            {
                UserSettingsService.Show();
                UserInfo.ShowUserProfileRegister();
            });
        }

        [CommandMethod(Group, CommandBlockList, CommandFlags.Modal)]
        public void BlockListCommand()
        {
            CommandStart.Start(doc => doc.Database.List());
        }

        [CommandMethod(Group, CommandCleanZombieBlocks, CommandFlags.Modal)]
        public void CleanZombieBlocks()
        {
            CommandStart.Start(doc =>
            {
                var db = doc.Database;
                var countZombie = db.CleanZombieBlock();
                doc.Editor.WriteMessage($"\nУдалено {countZombie} зомби!☻");
            });
        }

        [CommandMethod(Group, CommandColorBookNCS, CommandFlags.Modal | CommandFlags.Session)]
        public void ColorBookNCS()
        {
            CommandStart.Start(doc => ColorBookHelper.GenerateNCS());
        }

        [CommandMethod(Group, nameof(PIK_AutoLayersAll), CommandFlags.Modal)]
        public void PIK_AutoLayersAll()
        {
            CommandStart.Start(doc => AutoLayersService.AutoLayersAll());
        }

        [CommandMethod(Group, nameof(PIK_AutoLayersStart), CommandFlags.Modal)]
        public void PIK_AutoLayersStart()
        {
            CommandStart.Start(doc =>
            {
                AutoLayersService.Start();
                doc.Editor.WriteMessage($"\n{AutoLayersService.GetInfo()}");
            });
        }

        [CommandMethod(Group, nameof(PIK_AutoLayersStatus), CommandFlags.Modal)]
        public void PIK_AutoLayersStatus()
        {
            CommandStart.Start(doc => doc.Editor.WriteMessage($"\n{AutoLayersService.GetInfo()}"));
        }

        [CommandMethod(Group, nameof(PIK_AutoLayersStop), CommandFlags.Modal)]
        public void PIK_AutoLayersStop()
        {
            CommandStart.Start(doc =>
            {
                AutoLayersService.Stop();
                doc.Editor.WriteMessage($"\n{AutoLayersService.GetInfo()}");
            });
        }

        [CommandMethod(Group, nameof(PIK_BlocksUnitsless), CommandFlags.Modal)]
        public void PIK_BlocksUnitsless()
        {
            CommandStart.Start(doc =>
            {
                var db = doc.Database;
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                    foreach (var id in bt)
                    {
                        var btr = (BlockTableRecord)id.GetObject(OpenMode.ForRead);
                        if (btr.IsLayout || btr.IsAnonymous || btr.IsDependent)
                            continue;
                        if (btr.Units != UnitsValue.Undefined)
                        {
                            btr = (BlockTableRecord)id.GetObject(OpenMode.ForWrite);
                            btr.Units = UnitsValue.Undefined;
                        }
                    }
                    t.Commit();
                }
            });
        }

        [CommandMethod(Group, nameof(PIK_DbObjectsCountInfo), CommandFlags.Modal)]
        public void PIK_DbObjectsCountInfo()
        {
            CommandStart.Start(doc =>
            {
                var db = doc.Database;
                var ed = doc.Editor;
                var allTypes = new Dictionary<string, int>();
                for (var i = db.BlockTableId.Handle.Value; i < db.Handseed.Value; i++)
                {
                    if (!db.TryGetObjectId(new Handle(i), out var id))
                        continue;
                    if (allTypes.ContainsKey(id.ObjectClass.Name))
                        allTypes[id.ObjectClass.Name]++;
                    else
                        allTypes.Add(id.ObjectClass.Name, 1);
                }
                var sortedByCount = allTypes.OrderBy(i => i.Value);
                foreach (var item in sortedByCount)
                    ed.WriteMessage($"\n{item.Key} - {item.Value}");
            });
        }

        [CommandMethod(Group, nameof(PIK_ExportTemplateToJson), CommandFlags.Modal)]
        public void PIK_ExportTemplateToJson()
        {
            CommandStart.Start(doc =>
            {
                if (!doc.IsNamedDrawing)
                    throw new Exception("Чертеж не сохранен на диске");
                var tData = TemplateManager.LoadFromDb(doc.Database);
                var file = Path.ChangeExtension(doc.Name, "json");
                tData.ExportToJson(file ?? throw new InvalidOperationException());
                Process.Start(file);
            });
        }

        [CommandMethod(Group, nameof(PIK_LayersSelectedObjects), CommandFlags.UsePickSet)]
        public void PIK_LayersSelectedObjects()
        {
            CommandStart.Start(LayersSelectedService.Show);
        }

        /// <summary>
        ///     Визуальное окно для вставки блока из файла
        /// </summary>
        /// <param name="rb">Парметры: Имя файла, имя слоя, соответствия имен блоков</param>
        [LispFunction(nameof(PIK_LispInsertBlockFromFbDwg))]
        public void PIK_LispInsertBlockFromFbDwg([CanBeNull] ResultBuffer rb)
        {
            try
            {
                if (rb == null)
                    return;
                var tvs = rb.AsArray();
                if (!tvs.Any())
                    return;
                var fileName = tvs[0].Value.ToString();
                var layerName = tvs[1].Value.ToString();
                var layer = new LayerInfo(layerName);
                var matchs = tvs.Skip(2).ToList();
                var file = Path.Combine(PikSettings.LocalSettingsFolder, @"flexBrics\dwg\", fileName);
                VisualInsertBlock.InsertBlock(file,
                    n => matchs.Any(r => Regex.IsMatch(n, r.Value.ToString(), RegexOptions.IgnoreCase)),
                    layer);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PIK_LispInsertBlockFromFbDwg");
            }
        }

        [LispFunction(nameof(PIK_LispLog))]
        public void PIK_LispLog([NotNull] ResultBuffer rb)
        {
            var tvs = rb.AsArray();
            if (tvs.Any())
                Logger.Log.InfoLisp(tvs[0].Value.ToString());
        }

        [CommandMethod(Group, nameof(PIK_ModelObjectsCountInfo), CommandFlags.Modal)]
        public void PIK_ModelObjectsCountInfo()
        {
            CommandStart.Start(doc =>
            {
                var db = doc.Database;
                var ed = doc.Editor;
                using var t = db.TransactionManager.StartTransaction();
                var allTypes = new Dictionary<string, int>();
                var ms = db.CurrentSpaceId.GetObjectT<BlockTableRecord>(OpenMode.ForRead);
                foreach (var id in ms)
                {
                    if (allTypes.ContainsKey(id.ObjectClass.Name))
                        allTypes[id.ObjectClass.Name]++;
                    else
                        allTypes.Add(id.ObjectClass.Name, 1);
                }

                var sortedByCount = allTypes.OrderBy(i => i.Value);
                foreach (var item in sortedByCount)
                    ed.WriteMessage($"\n{item.Key} - {item.Value}");
                t.Commit();
            });
        }

        [CommandMethod(Group, nameof(PIK_PlotToPdf), CommandFlags.Session)]
        public void PIK_PlotToPdf()
        {
            CommandStart.Start(doc =>
            {
                using (doc.LockDocument())
                    PlotDirToPdf.PromptAndPlot(doc);
            });
        }

        [CommandMethod(Group, nameof(PIK_PurgeAuditRegen), CommandFlags.Modal)]
        public void PIK_PurgeAuditRegen()
        {
            CommandStart.Start(doc =>
            {
                var ed = doc.Editor;
                ed.Command("_-purge", "_All", "*", "_No");
                ed.Command("_-purge", "_R", "*", "_No");
                ed.Command("_audit", "_Yes");
                ed.Command("_-scalelistedit", "_R", "_Y", "_E");
                DbDictionaryCleaner.Start();
                DbDictionaryCleaner.Clean(doc.Database);
                ed.Regen();
            });
        }

        [CommandMethod(Group, nameof(PIK_Optimization), CommandFlags.Modal)]
        public void PIK_Optimization()
        {
            CommandStart.Start(doc =>
            {
                new Optimization().Optimize(doc);
            });
        }

        [CommandMethod(Group, nameof(PIK_Ribbon), CommandFlags.Modal)]
        public void PIK_Ribbon()
        {
            CommandStart.Start(d => RibbonBuilder.CreateRibbon());
        }

        [CommandMethod(Group, nameof(PIK_SearchById), CommandFlags.Modal)]
        public void PIK_SearchById()
        {
            CommandStart.Start(doc =>
            {
                var ed = doc.Editor;
                var res = ed.GetString("\nВведи ObjectID, например: 8796086050096");
                if (res.Status != PromptStatus.OK) return;
                var idLong = long.Parse(res.StringResult);
                var db = doc.Database;
                using var t = db.TransactionManager.StartTransaction();
                var objectId = new ObjectId(new IntPtr(idLong));
                var dbo = objectId.GetObject<DBObject>();
                $"{dbo.Id.ObjectClass.Name}, Owner={dbo.OwnerId.ObjectClass.Name}, IsErased={dbo.IsErased}, IsPersistent={dbo.IsPersistent}, IsAProxy={dbo.IsAProxy}".WriteToCommandLine();
                dbo.Id.ShowEnt();
                t.Commit();
            });
        }

        [CommandMethod(Group, nameof(PIK_UpdateFieldsInObjects), CommandFlags.Modal)]
        public void PIK_UpdateFieldsInObjects()
        {
            CommandStart.Start(doc => UpdateField.UpdateInSelected());
        }

        [CommandMethod(Group, CommandXDataView, CommandFlags.Modal)]
        public void XDataView()
        {
            CommandStart.Start(doc => XData.Viewer.XDataView.View());
        }

        [CommandMethod(Group, nameof(PIK_ListModules), CommandFlags.Modal)]
        public void PIK_ListModules()
        {
            CommandStart.Start(doc => ListModules.List());
        }

        [CommandMethod(Group, nameof(PIK_Errors), CommandFlags.Modal)]
        public void PIK_Errors()
        {
            CommandStart.StartWoStat(d => Inspector.ShowLast());
        }

        [CommandMethod(Group, nameof(PIK_StyleManager), CommandFlags.Modal)]
        public void PIK_StyleManager()
        {
            CommandStart.Start(d => Styles.StyleManager.StyleManagerService.ManageStyles());
        }

        [CommandMethod(Group, nameof(PIK_PaletteProperties), CommandFlags.Modal | CommandFlags.Redraw)]
        public void PIK_PaletteProperties()
        {
            CommandStart.Start(d => UI.Palette.Start());
        }

        [CommandMethod(Group, nameof(PIK_CheckUpdates), CommandFlags.Transparent)]
        public void PIK_CheckUpdates()
        {
            CommandStart.Start(d => CheckUpdates.CheckUpdatesNotify(false));
        }

        [CommandMethod(Group, nameof(PIK_RegAppsList), CommandFlags.Transparent)]
        public void PIK_RegAppsList()
        {
            CommandStart.Start(d =>
            {
                var dlg = new SaveFileDialog
                {
                    Title = "Сохранение файла со списком зарегистрированных приложенией",
                    FileName = $"regApps_{Path.GetFileNameWithoutExtension(AcadHelper.Doc.Name)}.txt",
                    DefaultExt = ".txt",
                    AddExtension = true
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllLines(dlg.FileName, d.Database.GetRegApps());
                    Process.Start(dlg.FileName);
                }
            });
        }

        [CommandMethod(Group, nameof(PIK_CleanRegApps), CommandFlags.Modal)]
        public void PIK_CleanRegApps()
        {
            CommandStart.Start(d => d.Database.CleanRegApps());
        }

        [CommandMethod(Group, nameof(PIK_ClearObjectsExtData), CommandFlags.Modal)]
        public void PIK_ClearObjectsExtData()
        {
            CommandStart.Start(ClearObjectsExtData.Clear);
        }

        [CommandMethod(Group, nameof(PIK_BatchRemoveLayoutsTest), CommandFlags.Modal)]
        public void PIK_BatchRemoveLayoutsTest()
        {
            CommandStart.Start(d => Test.BatchRemoveLayouts.Batch());
        }

        [CommandMethod(Group, nameof(_InternalUse_UpdatePropValue), CommandFlags.Modal | CommandFlags.Redraw)]
        public void _InternalUse_UpdatePropValue()
        {
            CommandStart.StartWoStat(BaseValueVM.InternalUpdate);
        }

        [CommandMethod(Group, nameof(PIK_TestState), CommandFlags.Modal)]
        public void PIK_TestState()
        {
            CommandStart.StartWoStat(d => TestState.Start());
        }

        [CommandMethod(Group, nameof(PIK_RibbonEditor), CommandFlags.Session)]
        public void PIK_RibbonEditor()
        {
            CommandStart.StartWoStat(d => new RibbonEditor().Edit());
        }

        [CommandMethod(Group, nameof(PIK_StartPalette), CommandFlags.Session)]
        public void PIK_StartPalette()
        {
            CommandStart.StartWoStat(d => PaletteSetCommands.Start());
        }

        /// <summary>
        ///     Список общих команд
        /// </summary>
        internal static void AllCommandsCommon()
        {
            try
            {
                CommandsPalette = new List<IPaletteCommand>
                {
                    new PaletteInsertBlock("PIK_Project-Logo", FileCommonBlocks, "Блок логотипа", Resources.logo,
                        "Вставка блока логотипа ПИК.", GroupCommon),
                    new PaletteCommand("Просмотр расширенных данных примитива", Resources.PIK_XDataView,
                        CommandXDataView, "Просмотр расширенных данных (XData) примитива.", GroupCommon),
                    new PaletteCommand("Проверка и очистка", Resources.purge, nameof(PIK_PurgeAuditRegen),
                        "Очистка (_purge), проверка (_audit), сброс списка масштабов аннотации (_scalelistedit) и регенерация чертежа.",
                        GroupCommon),
                    new PaletteCommand("Последние ошибки", Resources.error, nameof(PIK_Errors),
                        "Показать окно последних ошибок", GroupCommon),
                    new PaletteCommand("Настройки", Resources.userSettings, nameof(PIK_UserSettings),
                        "Настройки пользователя", GroupCommon)
                };
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "AcadLib.AllCommandsCommon()");
                CommandsPalette = new List<IPaletteCommand>();
            }
        }

        private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (dllsResolve == null)
                {
                    // Сборки в основной папке dll
                    dllsResolve = DllResolve.GetDllResolve(CurDllDir, SearchOption.AllDirectories);

                    // Все сборки из папки Script\NET
                    dllsResolve.AddRange(DllResolve.GetDllResolve(
                        Path.Combine(PikSettings.LocalSettingsFolder, @"Script\NET"),
                        SearchOption.AllDirectories));

                    // Оставить только сборки под текущую версию автокада
                    dllsResolve = FilterDllResolveVersions(dllsResolve);
                }

                Debug.WriteLine($"AcadLib AssemblyResolve {args.Name}");
                var dllResolver = dllsResolve.FirstOrDefault(f => f.IsResolve(args.Name));
                if (dllResolver == null)
                {
                    var fmAssembly = ResolveFmAssembly(args.Name);
                    if (fmAssembly != null)
                    {
                        Logger.Log.Info($"resolve fm assembly - {fmAssembly.FullName}");
                        return fmAssembly;
                    }

                    Debug.WriteLine("AcadLib dllResolver == null");
                    return null;
                }

                var asm = dllResolver.LoadAssembly();
                Logger.Log.Info($"resolve assembly - {asm.FullName}");
                return asm;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"Ошибка AssemblyResolve - {args.Name}.");
            }

            return null;
        }

        private Assembly? ResolveFmAssembly(string argsName)
        {
            var fmResolvers = new List<string>
            {
                "FamilyManager.CAD.Data",
                "FamilyManager.CAD.Shared",
                "FamilyManager.Client.Shared",
                "FamilyManager.Controls",
                "BimLab.FamilyManager.Shared",
                "BimLab.FamilyManager.V2.Dto",
                "FamilyManager.V2.SDK.Client",
            };

            var fmResolver = fmResolvers.FirstOrDefault(r => argsName.StartsWith(r + ","));
            if (fmResolver == null)
                return null;

            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName.StartsWith(fmResolver));
        }

        [NotNull]
        private List<DllResolve> FilterDllResolveVersions(List<DllResolve> dllResolves)
        {
            return LoadService.GetDllsForCurVerAcad(dllsResolve.Select(s => s.DllFile).ToList())
                .Select(s => new DllResolve(s.Dll) { DllName = s.FileWoVer }).ToList();
        }
    }
}
