namespace AcadLib.Styles.StyleManager.Model
{
    public abstract class StyleTableBase : IStyleTable
    {
        public string Name { get; set; }
        public abstract void Delete(IStyleItem styleItem);
    }
}
