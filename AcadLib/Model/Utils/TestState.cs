namespace AcadLib.Utils
{
    using System;
    using AutoCAD_PIK_Manager;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Timer = System.Timers.Timer;

    public static class TestState
    {
        private static DateTime lastState;
        private static Timer timer;

        public static void Start()
        {
            lastState = DateTime.Now;
            timer?.Dispose();
            timer = new Timer(60000) {Enabled = true, AutoReset = true};
            timer.Elapsed += (o, e) => Application.Idle += LogState;
            timer.Elapsed += (o, e) => Check();
        }

        private static void Check()
        {
            var delta = DateTime.Now - lastState;
            if (delta > TimeSpan.FromMinutes(5))
            {
                Logger.Log.Error($"State Error - {delta}");
                Log.SendMail("kozlovma@pik.ru", $"Завис автокад {Environment.UserName},{Environment.MachineName}",
                    $"Последний отзыв - {lastState}");
            }
        }

        private static void LogState(object sender, EventArgs e)
        {
            Application.Idle -= LogState;
            lastState = DateTime.Now;
            Logger.Log.Info("State OK");
        }
    }
}
