namespace AcadLib.Layers.LayersSelected
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.EditorInput;
    using JetBrains.Annotations;

    public static class LayersSelectedService
    {
        public static void Show(Document doc)
        {
            var vm = new LayersSelectedVM();
            var view = new LayersSelectedView(vm);
            view.Show();
        }

        [NotNull]
        public static List<LayerInfo> GetSelectedLayers()
        {
            var doc = AcadHelper.Doc;
            var ed = doc.Editor;
            var sel = ed.SelectImplied();
            if (sel.Status != PromptStatus.OK)
                return new List<LayerInfo>();
            List<LayerInfo> layers;
            using (var t = doc.TransactionManager.StartTransaction())
            {
                layers = sel.Value.GetObjectIds().Layers();
                t.Commit();
            }

            return layers;
        }
    }
}
