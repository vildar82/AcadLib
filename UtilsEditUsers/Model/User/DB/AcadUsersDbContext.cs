namespace UtilsEditUsers.Model.User.DB
{
    using System;
    using System.Data.Entity;

    public class AcadUsersDbContext : DbContext
    {
        public AcadUsersDbContext()
            : base (@"data source=vpp-sql04;initial catalog=CAD_AutoCAD;persist security info=True;user id=CAD_AllUsers;password=qwerty!2345;MultipleActiveResultSets=True;App=EntityFramework")            
        {
        }

        public DbSet<AutocadUser> AutocadUsers { get; set; }

        public class AutocadUser
        {
            public int ID { get; set; }
            public string Login { get; set; }
            public string FIO { get; set; }
            public string Group { get; set; }
            public bool Disabled { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public bool? PreviewUpdate { get; set; }
            public DateTime? DateRun { get; set; }
            public string AdditionalGroup { get; set; }
        }
    }
}