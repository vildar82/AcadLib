namespace AcadLib.Reactive
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;

    public static class EventsMixin
    {
        public static DboEvents Events(this DBObject dbo)
        {
            return new DboEvents(dbo);
        }

        public static DbEvents Events(this Database db)
        {
            return new DbEvents(db);
        }

        public static DocumentsEvents Events(this DocumentCollection docMan)
        {
            return new DocumentsEvents(docMan);
        }
    }
}
