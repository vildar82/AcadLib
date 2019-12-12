namespace UtilsEditUsers.Model.User.DB
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Linq;
    using JetBrains.Annotations;
    using Properties;

    [PublicAPI]
    public class DbUsers : IDisposable
    {
        private readonly AcadUsersDbContext _db;

        public DbUsers()
        {
            _db = new AcadUsersDbContext();
        }

        public List<AcadUsersDbContext.AutocadUser> GetUsers()
        {
            return _db.AutocadUsers.ToList();
        }

        public void DeleteUser(AcadUsersDbContext.AutocadUser user)
        {
            _db.AutocadUsers.Remove(user);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}