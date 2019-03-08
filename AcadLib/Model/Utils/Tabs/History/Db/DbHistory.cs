namespace AcadLib.Utils.Tabs.History.Db
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Linq;
    using JetBrains.Annotations;
    using Model.Utils.Tabs.History.Db;

    public class DbHistory
    {
        [NotNull]
        private readonly Entities _db;

        public DbHistory()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "vpp-sql04",
                InitialCatalog = "StatEvents",
                IntegratedSecurity = false,
                UserID = "CAD_AllUsers",
                Password = "qwerty!2345",
            };
            var conBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ToString(),
                Metadata = @"res://*/Model.Utils.Tabs.History.Db.DbEvents.csdl|res://*/Model.Utils.Tabs.History.Db.DbEvents.ssdl|res://*/Model.Utils.Tabs.History.Db.DbEvents.msl"
            };
            var con = conBuilder.ToString();
            _db = new Entities(con);
            _db.Configuration.AutoDetectChangesEnabled = false;
            _db.Configuration.LazyLoadingEnabled = true;
        }

        [NotNull]
        public IEnumerable<StatEvents> LoadHistoryFiles()
        {
            var now = DateTime.Now;
            var login = Environment.UserName.ToLower();
            var items = _db.StatEvents.AsNoTracking().Where(w => (w.App == "AutoCAD" || w.App == "Civil") &&
                                                           w.EventName == "Открытие" &&
                                                           DbFunctions.DiffDays(w.Start, now) < 100 &&
                                                           w.UserName.ToLower() == login).ToList();
            return items.GroupBy(g => g.DocPath).Select(s => s.OrderByDescending(o => o.Start).FirstOrDefault());
        }

        [NotNull]
        public IEnumerable<StatEvents> LoadHistoryFiles(DateTime start)
        {
            var login = Environment.UserName.ToLower();
            var items = _db.StatEvents.AsNoTracking().Where(w => w.Start > start &&
                                                                        (w.App == "AutoCAD" || w.App == "Civil") &&
                                                                        w.EventName == "Открытие" &&
                                                                        w.UserName.ToLower() == login).ToList();
            return items.GroupBy(g => g.DocPath).Select(s => s.OrderByDescending(o => o.Start).FirstOrDefault());
        }
    }
}
