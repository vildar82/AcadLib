namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Собирает добавленные в базу чертежа объекты
    /// </summary>
    public class AddedObjects : IDisposable
    {
        public Database Db { get; }

        public AddedObjects(Database db)
        {
            Db = db;
            db.ObjectAppended += Db_ObjectAppended;
            db.ObjectModified += Db_ObjectModified;
        }

        public event ObjectEventHandler ObjectAppended;
        public event ObjectEventHandler ObjectModified;

        public List<ObjectId> Added { get; } = new List<ObjectId>();
        public List<ObjectId> Modified { get; } = new List<ObjectId>();
        
        private void Db_ObjectAppended(object sender, ObjectEventArgs e)
        {
            Added.Add(e.DBObject.Id);
            ObjectAppended?.Invoke(sender, e);
        }
        
        private void Db_ObjectModified(object sender, ObjectEventArgs e)
        {
            Modified.Add(e.DBObject.Id);
            ObjectModified?.Invoke(sender, e);
        }

        public void Dispose()
        {
            Db.ObjectAppended -= Db_ObjectAppended;
            Db.ObjectModified -= Db_ObjectAppended;
        }
    }
}