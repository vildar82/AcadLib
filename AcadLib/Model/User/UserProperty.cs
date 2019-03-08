namespace AcadLib.User
{
    using System.Windows.Controls;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Свойство
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// Внутренний идентификатор свойства
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Пользвательское имя свойства
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение свойства
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Описание мвойства
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Контрол значения
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public Control ValueControl { get; set; }
    }
}