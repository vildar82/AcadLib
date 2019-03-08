namespace AcadLib.Doc
{
    using System;
    using System.Collections.Generic;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices;
    using JetBrains.Annotations;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    /// <summary>
    /// Установки системных переменных для чертежа
    /// </summary>
    [PublicAPI]
    public static class DocSysVarAuto
    {
        /// <summary>
        /// Системные переменные для установки в чертеж
        /// </summary>
        public static Dictionary<string, object> SysVars = new Dictionary<string, object>();

        internal static void Start()
        {
            // Загрузка системных переменных
            LoadSysVars();
        }

        private static void LoadSysVars()
        {
            SysVars = PikSettings.PikFileSettings?.DocSystemVariables ?? new Dictionary<string, object>();
            if (PikSettings.GroupFileSettings?.DocSystemVariables != null)
            {
                foreach (var sv in PikSettings.GroupFileSettings.DocSystemVariables)
                {
                    SysVars[sv.Key] = sv.Value;
                }
            }
        }

        public static void SetSysVars([NotNull] Document doc)
        {
            Logger.Log.Info($"SetSysVars start doc={doc.Name}, ActiveDoc={Application.DocumentManager.MdiActiveDocument?.Name}.");
            foreach (var item in SysVars)
            {
                try
                {
                    Logger.Log.Info($"SetSysVars {item.Key}={item.Value}");
                    var val = item.Value;
                    var cVal = item.Key.GetSystemVariable();
                    if (cVal.Equals(val))
                        continue;
                    var itemType = item.Value.GetType();
                    var reqType = cVal.GetType();
                    if (itemType != reqType)
                    {
                        val = Convert.ChangeType(item.Value, reqType);
                    }

                    item.Key.SetSystemVariableTry(val);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"SetSysVars {item.Key}={item.Value}.");
                }
            }

            Logger.Log.Info("SetSysVars end.");
        }
    }
}
