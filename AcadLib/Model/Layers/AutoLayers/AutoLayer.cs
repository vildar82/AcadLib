namespace AcadLib.Layers.AutoLayers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    public abstract class AutoLayer
    {
        public LayerInfo Layer { get; set; }

        protected List<string> Commands { get; set; }

        public string GetInfo()
        {
            return $"{Layer.Name} - {string.Join(",", Commands)}";
        }

        public abstract List<ObjectId> GetAutoLayerEnts(List<ObjectId> idAddedEnts);
        public abstract bool IsAutoLayerCommand(string globalCommandName);
    }
}