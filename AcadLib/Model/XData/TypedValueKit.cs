namespace AcadLib.XData
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Набирает параметры TypedValue
    /// </summary>
    [PublicAPI]
    public class TypedValueExtKit
    {
        public TypedValueExtKit()
        {
            Values = new List<TypedValue>();
        }

        public List<TypedValue> Values { get; private set; }

        public void Add(string name, object value)
        {
            var tvName = TypedValueExt.GetTvExtData(name);
            var tvValue = TypedValueExt.GetTvExtData(value);
            Values.Add(tvName);
            Values.Add(tvValue);
        }
    }
}