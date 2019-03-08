namespace AcadLib.Template
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Layers;

    [PublicAPI]
    public class TemplateData
    {
        private LayerInfo zero = new LayerInfo("0");

        public Dictionary<string, LayerInfo> Layers { get; set; } = new Dictionary<string, LayerInfo>();

        public string Name { get; set; }

        [CanBeNull]
        public LayerInfo GetLayer([NotNull] string layer)
        {
            return GetLayer(layer, false);
        }

        [CanBeNull]
        public LayerInfo GetLayer([NotNull] string layer, bool logErr)
        {
            if (!Layers.TryGetValue(layer, out var li))
            {
                // Нет слоя в шалоне - лог и создать слой
                if (logErr)
                    Logger.Log.Error($"Нет слоя '{layer}' в шаблоне '{Name}'");
                li = new LayerInfo(layer);
                Layers.Add(layer, li);
            }

            return li;
        }
    }
}