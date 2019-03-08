using Autodesk.AutoCAD.ApplicationServices.Core;

namespace AcadLib.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;

    public static class DbUsingExt
    {
        public static DbUsing Using(this Database db, params string[] props)
        {
            return new DbUsing(db, props);
        }
    }

    /// <summary>
    /// Использование свойств чертежа и восстановление после использовния
    /// </summary>
    public class DbUsing : IDisposable
    {
        private readonly Database _db;
        private readonly List<(PropertyInfo prop, object val)> oldValues;

        public DbUsing(Database db, params string[] props)
        {
            _db = db;
            var dbType = typeof(Database);
            oldValues = props.Select(s =>
            {
                var prop = dbType.GetProperty(s);
                var val = prop.GetValue(db);
                return (prop, val);
            }).ToList();
        }

        public void Dispose()
        {
            foreach (var (prop, val) in oldValues)
            {
                prop.SetValue(_db, val);
            }
        }
    }
}
