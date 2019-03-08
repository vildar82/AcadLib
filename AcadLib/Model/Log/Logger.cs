namespace AcadLib
{
    using System;
    using System.IO;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using JetBrains.Annotations;
    using Yandex.Metrica;

    public static class Logger
    {
        [NotNull] public static readonly LoggAddinExt Log;

        [PublicAPI]
        public static readonly string UserGroup = AutoCAD_PIK_Manager.Settings.PikSettings.UserGroup;

        static Logger()
        {
            Log = new LoggAddinExt();
            try
            {
                YandexMetricaFolder.SetCurrent(Path.GetTempPath());
                YandexMetrica.Activate("4a039a94-ea44-43b7-9025-59a856dd7120");
                YandexMetrica.Config.CustomAppVersion = Commands.AcadLibVersion;
            }
            catch (Exception ex)
            {
                AutoCAD_PIK_Manager.Log.Error(ex, "YandexMetrica Activate error");
            }
        }
    }

    [PublicAPI]
    public class LoggAddinExt : AutoCAD_PIK_Manager.LogAddin
    {
        public override void Debug(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Debug(newMsg);
        }

        [PublicAPI]
        public void Debug(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Debug(ex, newMsg);
        }

        public override void Error(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Error(newMsg);
        }

        public void Error(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Error(ex, newMsg);
            YandexReport(msg, ex);
        }

        public void Error([NotNull] Exception ex)
        {
            var newMsg = GetMessage(ex.Message);
            base.Error(ex, newMsg);
            YandexReport(ex.Message, ex);
        }

        public override void Fatal(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Fatal(newMsg);
        }

        [PublicAPI]
        public void Fatal(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Fatal(ex, newMsg);
            YandexReport(msg, ex);
        }

        public override void Info(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Info(newMsg);
        }

        [PublicAPI]
        public void Info(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Info(ex, newMsg);
        }

        public new void InfoLisp(string msg)
        {
            base.InfoLisp(msg);
        }

        public override void Mail(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Mail(newMsg);
        }

        public override void Mail(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Mail(ex, newMsg);
        }

        /// <summary>
        /// Отзыв
        /// </summary>
        [PublicAPI]
        public void Report(string msg)
        {
            Error("#Report: " + msg);
        }

        private static void YandexReport(string msg, Exception ex)
        {
            try
            {
                YandexMetrica.ReportError(msg, ex);
            }
            catch (Exception e)
            {
                AutoCAD_PIK_Manager.Log.Error(e, $"YandexReport");
            }
        }

        public void StartCommand([CanBeNull] CommandStart command)
        {
            base.Info($"Start command: {command?.CommandName}; Сборка: {command?.Assembly?.FullName}; ");
        }

        public void StartLisp(string command, string file)
        {
            base.Info($"Start Lisp: {command}; Файл: {file}; ");
        }

        public override void Warn(string msg)
        {
            var newMsg = GetMessage(msg);
            base.Warn(newMsg);
        }

        public void Warn(Exception ex, string msg)
        {
            var newMsg = GetMessage(msg);
            base.Warn(ex, newMsg);
        }

        [NotNull]
        private static string GetMessage(string msg)
        {
            return $"{msg};Doc={Application.DocumentManager?.MdiActiveDocument?.Name}";
        }
    }
}
