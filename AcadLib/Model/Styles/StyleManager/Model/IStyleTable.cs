namespace AcadLib.Styles.StyleManager.Model
{
    public interface IStyleTable
    {
        string Name { get; set; }
        void Delete(IStyleItem styleItem);
    }
}
