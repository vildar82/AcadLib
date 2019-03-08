namespace AcadLib.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    /// <summary>
    /// Настройки плагина
    /// </summary>
    public class PluginSettings
    {
        public string Name { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Свойства
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public List<UserProperty> Properties { get; set; } = new List<UserProperty>();

        /// <summary>
        /// Добавить свойство
        /// </summary>
        /// <param name="id">Внутреннее имя</param>
        /// <param name="name">Пользовательское имя</param>
        /// <param name="descr">Описание</param>
        /// /// <param name="value">Значение</param>
        /// <returns>Настройки плагина</returns>
        public PluginSettings Add(string id, string name, string descr, object value)
        {
            Properties.Add(new UserProperty
            {
                ID = id,
                Name = name,
                Value = value,
                Description = descr
            });
            return this;
        }

        /// <summary>
        /// Получение значения настройки плагина
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="parameterId">Имя параметра</param>
        /// <param name="onNotFound">Если не найден параметр</param>
        public T GetPluginValue<T>([NotNull] string parameterId, Func<T> onNotFound = null)
        {
            var prop = Properties.FirstOrDefault(p => p.ID == parameterId);
            if (prop == null)
                return onNotFound == null ? default : onNotFound();
            return (T)prop.Value;
        }
    }
}