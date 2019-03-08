namespace AcadLib.UI.Ribbon.Options
{
    using System.Collections.Generic;

    /// <summary>
    /// Настройки ленты
    /// </summary>
    public class RibbonOptions
    {
        public List<ItemOptions> Tabs { get; set; } = new List<ItemOptions>();

        public string ActiveTab { get; set; }

        public Dictionary<string, bool> DictToggleState { get; set; } = new Dictionary<string, bool>();
    }
}
