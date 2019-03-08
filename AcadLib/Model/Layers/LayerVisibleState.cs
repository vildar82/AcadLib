namespace AcadLib.Layers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Состояние слоев - для проверки видимости объектов на чертеже
    /// </summary>
    [PublicAPI]
    public class LayerVisibleState
    {
        private Dictionary<string, bool> layerVisibleDict;

        /// <summary>
        /// Нужно создавать новый объект LayerVisibleState после возмоного изменения состояния слоев пользователем.
        /// </summary>
        /// <param name="db"></param>
        public LayerVisibleState([NotNull] Database db)
        {
            layerVisibleDict = GetLayerVisibleState(db);
        }

        /// <summary>
        /// Объект на видим - не скрыт, не на выключенном или замороженном слое
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public bool IsVisible([NotNull] Entity ent)
        {
            bool res;
            if (!ent.Visible)
            {
                res = false;
            }
            else
            {
                // Слой выключен или заморожен
                layerVisibleDict.TryGetValue(ent.Layer, out res);
            }

            return res;
        }

        [NotNull]
        private Dictionary<string, bool> GetLayerVisibleState([NotNull] Database db)
        {
            var res = new Dictionary<string, bool>();
            var lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            foreach (var idLayer in lt)
            {
                var layer = (LayerTableRecord)idLayer.GetObject(OpenMode.ForRead);
                res.Add(layer.Name, !layer.IsOff && !layer.IsFrozen);
            }

            return res;
        }
    }
}