namespace AcadLib.DB
{
    using Autodesk.AutoCAD.DatabaseServices;
    using NetLib;

    public static class SummaryInfoExt
    {
        public static object? GetDwgCustomPropertyValue(this Database db, string prop)
        {
            var dictProps = db.SummaryInfo.CustomProperties;
            while (dictProps.MoveNext())
            {
                var entry = dictProps.Entry;
                if (prop.EqualsAnyIgnoreCase(entry.Key?.ToString()))
                {
                    return entry.Value;
                }
            }

            return null;
        }
    }
}
