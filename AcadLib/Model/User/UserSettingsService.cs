namespace AcadLib.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoCAD_PIK_Manager.User;
    using IO;
    using JetBrains.Annotations;
    using NetLib;
    using PaletteProps;
    using UI;

    /// <summary>
    /// Настройки пользователя
    /// </summary>
    [PublicAPI]
    public static class UserSettingsService
    {
        internal const string CommonName = Commands.GroupCommon;
        internal const string CommonParamNotify = "NotificationsOn";
        [NotNull]
        private static LocalFileData<UserSettings> _userData;
        private static HashSet<string> _activePlugins = new HashSet<string>();

        static UserSettingsService()
        {
            _userData = new LocalFileData<UserSettings>(Path.GetUserPluginFile(string.Empty, "UserSettings.json"), false);
            _userData.TryLoad(() => new UserSettings());
            RegCommonSettings();
            CommonSettings = GetPluginSettings(CommonName);
        }

        /// <summary>
        /// Событие изменения настроек
        /// </summary>
        public static event EventHandler ChangeSettings;

        /// <summary>
        /// Общие настройки
        /// </summary>
        public static PluginSettings CommonSettings { get; set; }

        /// <summary>
        /// Пользователь согласен на предварительные обновления
        /// </summary>
        public static bool IsPreviewUpdate => AutocadUserService.User?.PreviewUpdate ?? false;

        /// <summary>
        /// Получение значения настройки плагина
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="pluginName">Имя плагина</param>
        /// <param name="parameterId">Имя параметра</param>
        public static T GetPluginValue<T>([NotNull] string pluginName, [NotNull] string parameterId)
        {
            var prop = GetPluginProperty(pluginName, parameterId);
            if (prop == null)
                return default;
            return (T)prop.Value;
        }

        /// <summary>
        /// Получение свойтва плагина
        /// </summary>
        /// <param name="pluginName">Плагин</param>
        /// <param name="parameterId">Свойство</param>
        public static UserProperty GetPluginProperty([NotNull] string pluginName, [NotNull] string parameterId)
        {
            var plugin = GetPluginSettings(pluginName);
            return plugin?.Properties.FirstOrDefault(p => p.ID == parameterId);
        }

        /// <summary>
        /// Установить значение свойства
        /// </summary>
        /// <param name="pluginName">Плагин</param>
        /// <param name="parameterId">Параметр</param>
        /// <param name="value">Значение</param>
        public static void SetPluginValue([NotNull] string pluginName, [NotNull] string parameterId, object value)
        {
            var plugin = GetPluginSettings(pluginName);
            var prop = plugin?.Properties.FirstOrDefault(p => p.ID == parameterId);
            if (prop == null)
                return;
            if (!Equals(value, prop.Value))
            {
                prop.Value = value;
                _userData.TrySave();
            }
        }

        /// <summary>
        /// Получение настроек плагина
        /// </summary>
        /// <param name="name">Имя плагина</param>
        [CanBeNull]
        public static PluginSettings GetPluginSettings([NotNull] string name)
        {
            return _userData.Data.PluginSettings.FirstOrDefault(p => p?.Name == name);
        }

        /// <summary>
        /// Добавление пользовательских настроек плагина
        /// </summary>
        /// <param name="pluginName">Плагин</param>
        public static void RegPlugin([NotNull] string pluginName, Func<PluginSettings> init, Action<PluginSettings> onLoaded)
        {
            _activePlugins.Add(pluginName);
            var plugin = GetPluginSettings(pluginName);
            if (plugin == null)
            {
                plugin = init();
                _userData.Data.PluginSettings.Add(plugin);
            }
            else
            {
                onLoaded(plugin);
            }
        }

        public static void RemovePlugin([NotNull] string pluginName)
        {
            _userData.Data.PluginSettings.RemoveAll(p => p.Name == pluginName);
            _activePlugins.Remove(pluginName);
            _userData.TrySave();
        }

        /// <summary>
        /// Показать настройки пользователя
        /// </summary>
        public static void Show()
        {
            CheckSettings();
            var user = AutocadUserService.LoadUser();
            if (user == null)
            {
                Logger.Log.Warn("Ошибка загрузки пользователя из базы. Загрузка из локального кеша.");
                user = AutocadUserService.LoadBackup();
                if (user == null)
                {
                    Logger.Log.Warn("Ошибка загрузки пользователя из локального кеша.");
                }
            }

            InitControls();

            var userSettingsVm = new UserSettingsVM(user, _userData.Data);
            var userSettingsView = new UserSettingsView(userSettingsVm);
            if (userSettingsView.ShowDialog() != true)
                return;
            AutocadUserService.User = userSettingsVm.User;
            AutocadUserService.Save();
            _userData.TrySave();
            ChangeSettings?.Invoke(null, EventArgs.Empty);
        }

        private static void InitControls()
        {
            foreach (var pluginSetting in _userData.Data.PluginSettings)
            {
                foreach (var property in pluginSetting.Properties)
                {
                    if (property.ValueControl == null)
                    {
                        property.ValueControl = property.Value.CreateControl(v =>
                            property.Value = v);
                    }
                    else if (property.ValueControl.DataContext is IValue valueVM)
                    {
                        valueVM.UpdateValue(property.Value);
                    }
                }
            }
        }

        private static void CheckSettings()
        {
            var incorrectPlugins = new List<PluginSettings>();
            _userData.Data.PluginSettings.Where(p => !_activePlugins.Contains(p.Name)).ToList()
                .ForEach(p => _userData.Data.PluginSettings.Remove(p));
            foreach (var pluginSetting in _userData.Data.PluginSettings)
            {
                if (pluginSetting.Name.IsNullOrEmpty() || !pluginSetting.Properties.Any())
                {
                    incorrectPlugins.Add(pluginSetting);
                    continue;
                }

                var incorrectProps = new List<UserProperty>();
                foreach (var property in pluginSetting.Properties)
                {
                    if (property.ID.IsNullOrEmpty())
                    {
                        incorrectProps.Add(property);
                    }
                }

                incorrectProps.ForEach(r => pluginSetting.Properties.Remove(r));
                if (pluginSetting.Properties.Count == 0)
                {
                    incorrectPlugins.Add(pluginSetting);
                }
            }

            incorrectPlugins.ForEach(p => _userData.Data.PluginSettings.Remove(p));
        }

        private static void RegCommonSettings()
        {
            RegPlugin(CommonName, CreateCommonPlugin, CheckCommonPlugin);
        }

        private static PluginSettings CreateCommonPlugin()
        {
            var p = new PluginSettings { Name = CommonName };
            AddCommonParamNotify(p);
            return p;
        }

        private static bool AddCommonParamNotify(PluginSettings plugin)
        {
            plugin.Add(CommonParamNotify, "Уведомления", "Включение/отключение всплывающих уведомлений об изменении настроек", true);
            return true;
        }

        private static void CheckCommonPlugin(PluginSettings plugin)
        {
            plugin.GetPluginValue(CommonParamNotify, () => AddCommonParamNotify(plugin));
        }
    }
}
