namespace AcadLib.Visual
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    public interface IVisualService : IDisposable
    {
        /// <summary>
        /// Включение/выключение визуализации
        /// </summary>
        bool VisualIsOn { get; set; }

        string LayerForUser { get; set; }

        /// <summary>
        /// Обновление визуализации
        /// </summary>
        void VisualUpdate();

        List<Entity> CreateVisual();

        /// <summary>
        /// Удаление визуализации
        /// </summary>
        void VisualsDelete();
    }
}