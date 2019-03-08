namespace AcadLib.Layers.LayersSelected
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.WPF;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public class LayersSelectedVM : BaseViewModel
    {
        public LayersSelectedVM()
        {
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
            BindingDoc(AcadHelper.Doc);
        }

        [Reactive]
        public List<LayerInfo> Layers { get; set; }

        private void BindingDoc([NotNull] Document doc)
        {
            Update();
            doc.ImpliedSelectionChanged -= Document_ImpliedSelectionChanged1;
            doc.ImpliedSelectionChanged += Document_ImpliedSelectionChanged1;
        }

        private void DocumentManager_DocumentActivated(object sender, [NotNull] DocumentCollectionEventArgs e)
        {
            BindingDoc(e.Document);
        }

        private void Document_ImpliedSelectionChanged1(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            try
            {
                Layers = LayersSelectedService.GetSelectedLayers().OrderBy(o => o.Name).ToList();
            }
            catch
            {
                Layers = new List<LayerInfo>();
            }
        }
    }
}