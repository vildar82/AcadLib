namespace AcadLib.Layers.AutoLayers
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;
    using General = AcadLib.General;

    /// <summary>
    /// Авто-слои для размеров
    /// </summary>
    public class AutoLayerVport : AutoLayer
    {
        public AutoLayerVport()
        {
            Layer = new LayerInfo($"{LayerExt.GroupLayerPrefix}_Видовой экран");
            Commands = new List<string> { "-VPORTS", "+VPORTS", "VIEWPORTS" };
        }

        public override bool IsAutoLayerCommand(string globalCommandName)
        {
            return Commands.Any(a => a.EqualsIgnoreCase(globalCommandName));
        }

        [CanBeNull]
        public override List<ObjectId> GetAutoLayerEnts([CanBeNull] List<ObjectId> idAddedEnts)
        {
            return idAddedEnts?.Where(IsVportEnt).ToList();
        }

        private static bool IsVportEnt(ObjectId idEnt)
        {
            return idEnt.ObjectClass == General.ClassVport;
        }
    }
}