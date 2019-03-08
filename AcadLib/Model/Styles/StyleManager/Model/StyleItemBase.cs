namespace AcadLib.Styles.StyleManager.Model
{
    using Autodesk.AutoCAD.DatabaseServices;

    public abstract class StyleItemBase : IStyleItem
    {
        public string Name { get; set; }
        public ObjectId Id { get; set; }
    }
}
