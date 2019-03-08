namespace AcadLib.Layers.AutoLayers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public abstract class AutoLayer
    {
        public LayerInfo Layer { get; set; }

        protected List<string> Commands { get; set; }

        [NotNull]
        public string GetInfo()
        {
            return $"{Layer.Name} - {string.Join(",", Commands)}";
        }

        public abstract List<ObjectId> GetAutoLayerEnts(List<ObjectId> idAddedEnts);
        public abstract bool IsAutoLayerCommand(string globalCommandName);
    }
}