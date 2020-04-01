using System;
using System.Linq;
using AutoCAD_PIK_Manager.Settings;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AcadLib.Utils
{
    public class Optimization
    {
        private Editor _ed;

        public void Optimize(Document doc)
        {
            _ed = doc.Editor;

            // Проверка и очистка чертежа
            Log("Очистка и проверка чертежа...");
            doc.Editor.Command(nameof(Commands.PIK_PurgeAuditRegen));

            // Установка переменных оптимизации
            SetOptimizeVariables();

            // Запуск отдельных команд по оптимизации
            SetOptimizationByCommands();
        }

        private void SetOptimizationByCommands()
        {
            Log("Уменьшение точности аппроксимации объектов на текущем видовом экране:");
            _ed.Command("_viewres", "_y", 10);
        }

        private void SetOptimizeVariables()
        {
            if (PikSettings.PikFileSettings.Optimization?.Any() != true)
            {
                Log("Не заданы переменные оптимизации в настройках. Выход.");
                return;
            }

            Log("Установка переменных для оптимизации скорости работы AutoCAD:");
            foreach (var variable in PikSettings.PikFileSettings.Optimization)
            {
                try
                {
                    var oldVal = variable.Name.GetSystemVariableTry();
                    variable.Value.SetSystemVariable(variable.Name);
                    Log($"'{variable.Name}'='{variable.Value}' (было '{oldVal}'). {variable.Description}");
                }
                catch (Exception ex)
                {
                    Log($"Ошибка установки переменной '{variable.Name}'='{variable.Value}' - {ex.Message}");
                }
            }
        }

        private void Log(string msg)
        {
            _ed.WriteMessage($"\n{msg}\n");
            Logger.Log.Info(msg);
        }
    }
}