namespace AcadLib.Visual
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Layers;

    [PublicAPI]
    public abstract class VisualBase : IVisualService
    {
        protected bool isOn;

        public VisualBase([CanBeNull] string layer = null)
        {
            LayerForUser = layer ?? SymbolUtilityServices.LayerZeroName;
        }

        public string LayerForUser { get; set; }

        public bool VisualIsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                VisualUpdate();
            }
        }

        public abstract List<Entity> CreateVisual();

        public virtual void Dispose()
        {
            VisualsDelete();
        }

        public virtual void VisualsDelete()
        {
            try
            {
                EraseDraws();
            }
            catch
            {
                // ignored
            }
        }

        public virtual void DrawInDb(Database db)
        {
            using var t = db.TransactionManager.StartTransaction();
            var ms = db.MS(OpenMode.ForWrite);
            var visuals = CreateVisual();
            var layer = new LayerInfo(LayerForUser ?? "999_visuals").CheckLayerState();
            foreach (var visual in visuals)
            {
                visual.LayerId = layer;
                ms.AppendEntity(visual);
                t.AddNewlyCreatedDBObject(visual, true);
            }

            t.Commit();
        }

        public virtual void VisualUpdate()
        {
            VisualsDelete();

            // Включение визуализации на чертеже
            if (isOn)
            {
                try
                {
                    DrawVisuals(CreateVisual());
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex);
                }
            }
        }

        protected abstract void DrawVisuals(List<Entity> draws);

        protected abstract void EraseDraws();

        protected ObjectId GetLayerForVisual([CanBeNull] string layer)
        {
            var lay = new LayerInfo(layer ?? SymbolUtilityServices.LayerZeroName);
            return lay.CheckLayerState();
        }
    }
}