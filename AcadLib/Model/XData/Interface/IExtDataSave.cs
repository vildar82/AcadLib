// ReSharper disable once CheckNamespace
namespace AcadLib.XData
{
    using Autodesk.AutoCAD.ApplicationServices;

    public interface IExtDataSave
    {
        /// <summary>
        /// Словарь для сохранения объекта
        /// </summary>
        DicED GetExtDic(Document doc);

        /// <summary>
        /// установить значения из словаря в объект
        /// </summary>
        void SetExtDic(DicED dicEd, Document doc);
    }
}