namespace AcadLib.Utils.Tabs.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Вкладки для восстановления
    /// </summary>
    public class Tabs
    {
        public int SessionCount { get; set; } = 1;

        public List<Session> Sessions { get; set; } = new List<Session>();
    }
}
