namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    /// <summary>
    /// Загрузка вспомогательных сборок
    /// </summary>
    [PublicAPI]
    public static class LoadService
    {
        public static void DeleteTry(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // ignored
                }
            }
        }

        [NotNull]
        public static List<DllVer> GetDllsForCurVerAcad([NotNull] List<string> dlls)
        {
            Logger.Log.Info($"GetDllsForCurVerAcad dlls={dlls.JoinToString(Path.GetFileNameWithoutExtension)}");
            var dllsToLoad = new List<DllVer>();
            if (int.TryParse(HostApplicationServices.Current.releaseMarketVersion, out var ver))
            {
                var dllVerGroups = dlls.Select(DllVer.GetDllVer).GroupBy(g => g.FileWoVer).ToList();
                foreach (var groupDllVer in dllVerGroups)
                {
                    if (groupDllVer.Skip(1).Any())
                    {
                        var dllWin = groupDllVer.FirstOrDefault(f => f.Ver == ver) ??
                                     groupDllVer.OrderByDescending(o => o.Ver).FirstOrDefault(d => d.Ver <= ver);
                        if (dllWin == null)
                            continue; // Могут быть только специфичные версии, не для текущей - типа Acad_SheetSet_v2018 (нет для 2015)
                        dllsToLoad.Add(dllWin);
                    }
                    else
                    {
                        dllsToLoad.Add(groupDllVer.First());
                    }
                }
            }

            Logger.Log.Info(
                $"GetDllsForCurVerAcad dllsToLoad={dllsToLoad.JoinToString(s => Path.GetFileNameWithoutExtension(s.Dll))}");
            return dllsToLoad;
        }

        /// <summary>
        /// EntityFramework
        /// </summary>
        public static void LoadEntityFramework()
        {
            LoadFromTry(Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Dll\EntityFramework.dll"));
            LoadFromTry(Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder,
                @"Dll\EntityFramework.SqlServer.dll"));
        }

        public static void LoadFrom([NotNull] string dll)
        {
            if (File.Exists(dll))
            {
                var asm = Assembly.LoadFrom(dll);
                Logger.Log.Info($"LoadFrom {asm.FullName}.");
            }
            else
            {
                throw new Exception($"Не найден файл {dll}.");
            }
        }

        /// <summary>
        /// Загрузка сборок из папки.
        /// </summary>
        public static void LoadFromFolder(string dir, int deepLevel = 0)
        {
            try
            {
                if (!Directory.Exists(dir))
                    return;
                var dlls = GetDllsForCurVerAcad(Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly).ToList());
                foreach (var dll in dlls)
                {
                    LoadFromTry(dll.Dll);
                }

                // Углубление в подпапки
                var subDeepLevel = deepLevel - 1;
                if (subDeepLevel < 0)
                    return;
                foreach (var subDir in Directory.EnumerateDirectories(dir))
                {
                    LoadFromFolder(subDir, subDeepLevel);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"LoadFromFolder {dir}");
            }
        }

        public static void LoadFromTry(string dll)
        {
            try
            {
                Debug.WriteLine($"LoadFromTry {dll}");
                LoadFrom(dll);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "LoadFromTry - " + dll);
            }
        }

        public static void LoadPackages([NotNull] string name)
        {
            var dllLocal = Path.Combine(IO.Path.GetUserPluginFolder("packages"), name);
            LoadFromTry(dllLocal);
        }
    }
}