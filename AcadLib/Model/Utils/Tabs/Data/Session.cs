namespace AcadLib.Utils.Tabs.Data
{
    using System;
    using System.Collections.Generic;

    public class Session
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public List<string> Drawings { get; set; } = new List<string>();
    }
}