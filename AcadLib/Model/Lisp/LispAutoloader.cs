namespace AcadLib.Lisp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices;
    using JetBrains.Annotations;
    using Path = IO.Path;

    public static class LispAutoloader
    {
        private static List<string> lispFiles;

        public static void Start()
        {
            lispFiles = new List<string>();
            if (PikSettings.PikFileSettings?.AutoLoadLispPathBySettings?.Count > 0)
            {
                lispFiles = PikSettings.PikFileSettings.AutoLoadLispPathBySettings;
            }

            if (PikSettings.GroupFileSettings?.AutoLoadLispPathBySettings?.Count > 0)
            {
                lispFiles.AddRange(PikSettings.GroupFileSettings.AutoLoadLispPathBySettings);
            }

            // Для удаленщиков грузить лисп оптимизацйи
            if (General.IsRemoteUser())
            {
                var lispFile = @"Script\Lisp\OptimiseVarRemote.lsp";
                lispFiles.Add(lispFile);
                    Logger.Log.Info($"Добавлен лисп файл для оптимизации работы удаленщика - {lispFile}");
            }
            else
            {
                Logger.Log.Info("Пропущен лисп файл для оптимизации работы удаленщика. Это не удаленщик!");
            }

            if (lispFiles.Count == 0) lispFiles = null;
        }

        public static void LoadLisp([NotNull] Document doc)
        {
            foreach (var refLisp in PikSettings.PikFileSettings.AutoLoadLispPathBySettings)
            {
                var startupLispFile = Path.GetLocalSettingsFile(refLisp);
                if (File.Exists(startupLispFile))
                {
                    var lspPath = startupLispFile.Replace('\\', '/');
                    doc.SendStringToExecute($"(load \"{lspPath}\") ", true, false, true);
                    Logger.Log.Info($"LispAutoloader Загружен лисп {refLisp}.");
                }
                else
                {
                    Logger.Log.Info($"LispAutoloader Не найден лисп {refLisp}.");
                }
            }
        }
    }
}
