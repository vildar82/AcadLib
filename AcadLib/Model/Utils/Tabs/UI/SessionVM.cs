namespace AcadLib.Utils.Tabs.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Threading;
    using Data;
    using NetLib.WPF;
    using ReactiveUI;

    public class SessionVM : BaseModel
    {
        public SessionVM(Session session)
        {
            Date = session.Date;
            Tabs = session.Drawings.Select(s => new TabVM(s, false)).ToList();
            this.WhenAnyValue(v => v.RestoreAll).Subscribe(s => Tabs.ForEach(t => t.Restore = s));
        }

        public DateTime Date { get; set; }

        public List<TabVM> Tabs { get; set; }

        public bool RestoreAll { get; set; }
    }
}