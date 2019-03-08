namespace AcadLib.User
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Настройки пользователя
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Настройки плагинов
        /// </summary>
        [NotNull]
        public List<PluginSettings> PluginSettings { get; set; } = new List<PluginSettings>();
    }
}