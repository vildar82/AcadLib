using System.ComponentModel.DataAnnotations;

namespace AcadLib.Statistic.Db
{
    using System.Data.Entity;

    /// <summary>
    /// База пользователей acad
    /// </summary>
    public class PluginStatisticDbContext : DbContext
    {
        public PluginStatisticDbContext()
            : base (@"data source=vpp-sql04.main.picompany.ru;initial catalog=CAD_REVIT_STATISTICS;persist security info=True;user id=CAD_AllUsers;password=qwerty!2345;MultipleActiveResultSets=True;App=EntityFramework")
        {
        }

        public DbSet<C_PluginStatistic> C_PluginStatistics { get; set; }
    }

    public class C_PluginStatistic
    {
        [Key]
        public int Id_Statistic_Mark { get; set; }
        public string Application { get; set; }
        public string Plugin { get; set; }
        public string Command { get; set; }
        public string Build { get; set; }
        public string Doc { get; set; }
        public string UserName { get; set; }
        public System.DateTime DateStart { get; set; }
        public string DocName { get; set; }
    }
}