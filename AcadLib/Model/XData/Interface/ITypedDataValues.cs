// ReSharper disable once CheckNamespace
namespace AcadLib.XData
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface ITypedDataValues
    {
        /// <summary>
        /// Список сохраняемяхъ значений
        /// </summary>
        /// <returns></returns>
        List<TypedValue> GetDataValues(Document doc);

        /// <summary>
        /// Установка значений
        /// </summary>
        /// <param name="values"></param>
        /// <param name="doc"></param>
        void SetDataValues(List<TypedValue> values, Document doc);
    }
}