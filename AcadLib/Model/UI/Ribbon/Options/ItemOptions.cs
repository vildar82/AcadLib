namespace AcadLib.UI.Ribbon.Options
{
    using System.Collections.Generic;
    using Autodesk.Private.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Настройки элемента ленты
    /// </summary>
    public class ItemOptions
    {
        /// <summary>
        /// Индекс элемента в родительском элементе
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Видимость элемента
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Ссылка на элемент
        /// </summary>
        [JsonIgnore]
        public IRibbonContentUid Item { get; set; }

        /// <summary>
        /// Вложенные элементы
        /// </summary>
        public List<ItemOptions> Items { get; set; } = new List<ItemOptions>();

        /// <summary>
        /// Имя элемента
        /// </summary>
        public string UID { get; set; }
    }
}
