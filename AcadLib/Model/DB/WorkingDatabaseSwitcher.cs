namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// This class switches the WorkingDatabase. It was created for using in the
    /// tests.
    /// </summary>
    public sealed class WorkingDatabaseSwitcher : IDisposable
    {
        private readonly Database oldDb;

        /// <summary>
        /// Переключение рабочей базы
        /// </summary>
        /// <param name="db">База временно устанавливаемя как рабочая WorkingDatabase</param>
        public WorkingDatabaseSwitcher(Database db)
        {
            oldDb = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = db;
        }

        public void Dispose()
        {
            HostApplicationServices.WorkingDatabase = oldDb;
        }
    }
}