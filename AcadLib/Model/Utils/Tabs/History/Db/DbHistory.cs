namespace AcadLib.Utils.Tabs.History.Db
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public class DbHistory
    {
        [NotNull]
        public IEnumerable<StatEvents> LoadHistoryFiles()
        {
            return new List<StatEvents>();
        }

        [NotNull]
        public IEnumerable<StatEvents> LoadHistoryFiles(DateTime start)
        {
            return new List<StatEvents>();
        }

        public class StatEvents
        {
            public string DocPath { get; set; }
            public DateTime Start { get; set; }
        }
    }
}
