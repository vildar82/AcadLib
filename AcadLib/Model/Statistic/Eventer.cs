namespace AcadLib.Statistic
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using FileLog.Entities;
    using JetBrains.Annotations;
    using Microsoft.Win32;
    using Naming.Dto;
    using Naming.Sdk;
    using NetLib;
    using NetLib.AD;
    using PathChecker;
    using UserData = Naming.Dto.UserData;

    /// <summary>
    /// Класс для отправки событий
    /// </summary>
    public class Eventer
    {
        private readonly ApiClient _client;
        [NotNull]
        private readonly PathChecker _pathChecker;
        private UserData _userData;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="app">Имя приложения</param>
        /// <param name="version">Версия приложения</param>
        public Eventer(string app, string version)
        {
            App = app;
            AppType = GetAppType(app);
            Version = version;
            _client = new ApiClient(GetUrl("baseUrl"));
            _pathChecker = new PathChecker(_client);
            _userData = GetUserDataAd();
        }

        private string App { get; }

        private AppType AppType { get; }

        private DateTime StartEvent { get; set; }

        private string Version { get; }

        /// <summary>
        /// Конец события
        /// </summary>
        /// <param name="eventName">Имя события</param>
        public void Finish(EventType eventType, string docPath, string serialNumber)
        {
            if (StartEvent == DateTime.MinValue)
            {
                Logger.Log.Info($"Event Finish StartEvent = 0!");
                StartEvent = DateTime.Now;
            }

            var eventEnd = DateTime.Now;
            Task.Run(
                () =>
                {
                    try
                    {
                        Logger.Log.Info($"Eventer Finish {eventType}, {docPath}, {serialNumber}");
                        if (string.IsNullOrEmpty(docPath) || !Path.IsPathRooted(docPath) || !File.Exists(docPath))
                            return;
                        var fileName = Path.GetFileNameWithoutExtension(docPath);
                        var userName = Environment.UserName;
                        var compName = Environment.MachineName;
                        var fi = new FileInfo(docPath);
                        var fileSize = fi.Length / 1024000;
                        var eventTimeSec = (int)(eventEnd - StartEvent).TotalSeconds;
                        _client.Log.AddEvent(
                            new StatEvent(
                                App,
                                userName,
                                compName,
                                fileName,
                                docPath,
                                eventType,
                                StartEvent,
                                eventEnd,
                                Version,
                                fileSize,
                                eventTimeSec,
                                serialNumber,
                                _userData?.Fio,
                                _userData?.Department,
                                _userData?.Position));
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Error(e, $"Finish docPath={docPath}");
                    }
                });
        }

        /// <summary>
        /// Начало события
        /// </summary>
        /// <param name="case">Кейс</param>
        /// <param name="docPath">Документ</param>
        public PathCheckerResult Start(Case @case, [CanBeNull] string docPath)
        {
            StartEvent = DateTime.Now;
            PathCheckerResult pathCheckerResult = null;
            if (NeedCheck(docPath))
            {
                try
                {
                    Logger.Log.Info($"Eventer Start Check case={@case}, doc={docPath}");
                    pathCheckerResult = _pathChecker.Check(AppType, @case, docPath, _userData);
                    Logger.Log.Info($"Eventer pathCheckerResult={pathCheckerResult?.CheckResultDto?.Status}");
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"Eventer Start ERROR. case={@case}, doc={docPath}");
                }
            }

            return pathCheckerResult;
        }

        /// <summary>
        /// Получает значение ключа по имени.
        /// </summary>
        /// <param name="name">Имя ключа.</param>
        /// <returns>Значение ключа.</returns>
        /// <exception cref="Exception">Если ключ не найдей бросает исключение.</exception>
        private static string GetUrl(string name)
        {
            var registryPath = @"Software\PIK\BIM\Api";
            using (var key = Registry.CurrentUser.OpenSubKey(registryPath))
            {
                var url = key?.GetValue(name);
                if (key == null || url == null)
                {
                    throw new Exception("Неудалось загрузить настройки");
                }

                return url as string;
            }
        }

        private static UserData GetUserDataAd()
        {
            try
            {
                var userDataNL = ADUtils.GetUserData(Environment.UserName, Environment.UserDomainName);
                return new UserData
                {
                    Position = userDataNL.Position,
                    Department = userDataNL.Department,
                    Fio = userDataNL.Fio
                };
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"GetUserDataAD {Environment.UserName}_{Environment.UserDomainName}");
                return null;
            }
        }

        private static AppType GetAppType(string app)
        {
            switch (app.ToLower())
            {
                case "autocad": return AppType.Autocad;
                case "civil": return AppType.Civil;
            }

            return AppType.Autocad;
        }

        private bool NeedCheck(string docPath)
        {
            // Если путь пустой - то не нужно проверять нейминг (новый чертеж)
            // Если пользователь из списка исключений (бимам типа не нужно проверять)
            return docPath != null;
        }
    }
}
