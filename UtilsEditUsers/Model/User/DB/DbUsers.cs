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
        private readonly CAD_AutoCADEntities entities;

        public DbUsers()
        {
            entities = new CAD_AutoCADEntities();
        }

        public List<AutocadUsers> GetUsers()
        {
            return entities.AutocadUsers.ToList();
        }

        public void DeleteUser(AutocadUsers user)
        {
            entities.AutocadUsers.Remove(user);
        }

        public void Save()
        {
            entities.SaveChanges();
        }

        public void Dispose()
        {
            entities?.Dispose();
        }
    }
}