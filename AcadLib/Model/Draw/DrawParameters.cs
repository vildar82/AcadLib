// ReSharper disable once CheckNamespace
namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Layers;

    [PublicAPI]
    internal class DrawParameters : IDisposable
    {
        private Database db;
        private Color oldColor;
        private ObjectId oldLayer;
        private double oldLineScale;
        private ObjectId oldLineType;
        private LineWeight oldLineWeight;

        public DrawParameters(
            [NotNull] Database db,
            [CanBeNull] LayerInfo layer = null,
            [CanBeNull] Color color = null,
            LineWeight? lineWeight = null,
            [CanBeNull] string lineType = null,
            double? lineTypeScale = null)
        {
            this.db = db;

            // Сохранение текущих свойств чертежа
            oldLayer = db.Clayer;
            oldColor = db.Cecolor;
            oldLineWeight = db.Celweight;
            oldLineType = db.Celtype;
            oldLineScale = db.Celtscale;

            Layer = layer;
            Color = color;
            LineWeight = lineWeight;
            LineType = lineType;
            LineTypeScale = lineTypeScale;

            // установка новых свойств чертежу
            Setup();
        }

        public Color Color { get; set; }

        public LayerInfo Layer { get; set; }

        public string LineType { get; set; }

        public double? LineTypeScale { get; set; }

        public LineWeight? LineWeight { get; set; }

        public void Dispose()
        {
            // Восстановление свойств
            // Слой
            if (Layer != null)
                db.Clayer = oldLayer;

            // Цвет
            if (Color != null)
                db.Cecolor = oldColor;

            // Вес линии
            if (LineWeight != null)
                db.Celweight = oldLineWeight;

            // Тип линии
            if (LineType != null)
                db.Celtype = oldLineType;

            // Вес линии
            if (LineTypeScale != null)
                db.Celtscale = oldLineScale;
        }

        /// <summary>
        /// Установка свойств в базу чертежа
        /// </summary>
        private void Setup()
        {
            if (Layer != null)
            {
                db.Clayer = Layer.CheckLayerState();
            }

            // Цвет
            if (Color != null)
                db.Cecolor = Color;

            // Вес линии
            if (LineWeight != null)
                db.Celweight = LineWeight.Value;

            // Тип линии
            if (LineType != null)
                db.Celtype = db.LoadLineTypePIK(LineType);

            // Вес линии
            if (LineTypeScale != null)
                db.Celtscale = LineTypeScale.Value;
        }
    }
}