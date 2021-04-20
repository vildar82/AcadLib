namespace AcadLib.Template
{
    using System.Collections.Generic;
    using Layers;

    public class TemplateData
    {
        private LayerInfo zero = new LayerInfo("0");

        public Dictionary<string, LayerInfo> Layers { get; set; } = new Dictionary<string, LayerInfo>();

        public string Name { get; set; }

        public LayerInfo GetLayer(string layer)
        {
            return GetLayer(layer, false);
        }

        public LayerInfo GetLayer(string layer, bool logErr)
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