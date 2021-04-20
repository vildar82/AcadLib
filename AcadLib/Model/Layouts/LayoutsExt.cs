namespace AcadLib.Layouts
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    public static class LayoutExt
    {
        public static List<Layout> GetLayouts(this Database db)
        {
            var layouts = new List<Layout>();
            var dictLayout = db.LayoutDictionaryId.GetObject<DBDictionary>();
            if (dictLayout != null)
            {
                foreach (var entry in dictLayout)
                {
                    if (entry.Key != "Model")
                    {
                        var layout = entry.Value.GetObject<Layout>();
                        if (layout != null)
                        {
                            layouts.Add(layout);
                        }
                    }
                }
            }

            return layouts;
        }
    }
}