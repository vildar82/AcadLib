namespace AcadLib.XData
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Запись XRecord
    /// </summary>
    public class RecXD
    {
        public RecXD()
        {
        }

        public RecXD(string name, List<TypedValue> values)
        {
            Name = name;
            Values = values;
        }

        public string Name { get; set; }

        public List<TypedValue> Values { get; set; }

        public bool IsEmpty()
        {
            return Values == null || Values.Count == 0;
        }
    }
}