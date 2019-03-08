namespace AcadLib.Utils.Tabs.UI.History
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using NetLib;
    using Path = IO.Path;

    public static class HistoryModel
    {
        [ItemNotNull]
        [NotNull]
        public static List<HistoryTab> LoadHistoryCache()
        {
            var data = new LocalFileData<List<HistoryTab>>(GetHistoryFile(), false);
            data.TryLoad(() => new List<HistoryTab>());
            return data.Data ?? new List<HistoryTab>();
        }

        public static void SaveHistoryCache(List<HistoryTab> historyTabs)
        {
            if (historyTabs?.Any() == true)
            {
                var data = new LocalFileData<List<HistoryTab>>(GetHistoryFile(), false) { Data = historyTabs };
                data.TrySave();
            }
        }

        public static bool NeedUpdateCashe()
        {
            var casheData = File.GetLastWriteTime(GetHistoryFile());
            return (DateTime.Now - casheData).TotalDays > 10;
        }

        [NotNull]
        private static string GetHistoryFile()
        {
            return Path.GetUserPluginFile(RestoreTabs.PluginName, "HistoryTabs.json");
        }
    }
}