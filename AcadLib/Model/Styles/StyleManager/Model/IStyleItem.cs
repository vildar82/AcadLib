namespace AcadLib.Styles.StyleManager.Model
{
    using Autodesk.AutoCAD.DatabaseServices;

    public interface IStyleItem
    {
        string Name { get; set; }
        ObjectId Id { get; set; }
    }
}
