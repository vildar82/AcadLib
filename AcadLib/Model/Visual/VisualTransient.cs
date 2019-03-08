namespace AcadLib.Visual
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.GraphicsInterface;
    using JetBrains.Annotations;

    /// <summary>
    /// Визуализация графики - через TransientManager
    /// </summary>
    [PublicAPI]
    public abstract class VisualTransient : VisualBase
    {
        public static readonly Autodesk.AutoCAD.Geometry.IntegerCollection
            vps = new Autodesk.AutoCAD.Geometry.IntegerCollection();
        protected List<Entity> draws;
        private readonly Document doc = AcadHelper.Doc;

        public VisualTransient([CanBeNull] string layer = null) : base(layer)
        {
        }

        public static void EraseAll()
        {
            try
            {
                TransientManager.CurrentTransientManager.EraseTransients(TransientDrawingMode.Main, 0, vps);
            }
            catch
            {
                // ignored
            }
        }

        public virtual List<Entity> GetDraws()
        {
            return draws;
        }

        /// <summary>
        /// Включение/отключение визуализации (без перестроений)
        /// </summary>
        protected override void DrawVisuals([CanBeNull] List<Entity> ents)
        {
            draws = ents;
            if (ents != null)
            {
                var tm = TransientManager.CurrentTransientManager;
                foreach (var d in ents)
                {
                    tm.AddTransient(d, TransientDrawingMode.Main, 0, vps);
                }
            }
        }

        protected override void EraseDraws()
        {
            if (draws == null || draws.Count == 0)
                return;
            if (doc == AcadHelper.Doc)
            {
                var tm = TransientManager.CurrentTransientManager;
                foreach (var item in draws)
                {
                    tm?.EraseTransient(item, vps);
                    item.Dispose();
                }

                draws = null;
            }
            else
            {
                foreach (var item in draws)
                {
                    item.Dispose();
                }

                draws = null;
            }
        }

        private void DisposeDraws()
        {
            if (draws == null)
                return;
            try
            {
                EraseDraws();
            }
            catch
            {
                //
            }
        }
    }
}