namespace AcadLib.Visual
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Элемент который может визуализироваться
    /// </summary>
    public interface IVisualElement
    {
        /// <summary>
        /// Создание элементов визуализации
        /// </summary>
        List<Entity> CreateVisual();
    }
}