namespace AcadLib.Layers.AutoLayers
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Авто-слои для размеров
    /// </summary>
    public class AutoLayerDim : AutoLayer
    {
        public AutoLayerDim()
        {
            Layer = new LayerInfo($"{LayerExt.GroupLayerPrefix}_Размеры");
            Commands = new List<string> { "DIM" };
        }

        public override bool IsAutoLayerCommand(string globalCommandName)
        {
            return globalCommandName.StartsWith("DIM");
        }

        public override List<ObjectId> GetAutoLayerEnts(List<ObjectId>? idAddedEnts)
        {
            return idAddedEnts?.Where(IsDimEnt).ToList();
        }

        private static bool IsDimEnt(ObjectId idEnt)
        {
            return idEnt.ObjectClass.IsDerivedFrom(General.ClassDimension);
        }
    }
}