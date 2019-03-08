namespace AcadLib.Reactive
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    public class DbEvents
    {
        private readonly Database db;

        public DbEvents(Database db)
        {
            this.db = db;
        }

        public IObservable<EventPattern<DatabaseIOEventArgs>> SaveComplete =>
            Observable.FromEventPattern<DatabaseIOEventHandler, DatabaseIOEventArgs>
                (x => db.SaveComplete += x, x => db.SaveComplete -= x);

        public IObservable<EventPattern<ObjectEventArgs>> ObjectModified =>
            Observable.FromEventPattern<ObjectEventHandler, ObjectEventArgs>
                (x => db.ObjectModified += x, x => db.ObjectModified -= x);
    }
}
