namespace AcadLib.Utils.Tabs.UI.History
{
    using System;

    [Equals]
    public class HistoryTab
    {
        public string File { get; set; }
        [IgnoreDuringEquals]
        public DateTime Start { get; set; }

        public static bool operator ==(HistoryTab left, HistoryTab right) => Operator.Weave(left, right);

        public static bool operator != (HistoryTab left, HistoryTab right) => Operator.Weave(left, right);
    }
}