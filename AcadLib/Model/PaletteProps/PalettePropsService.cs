namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using AcadLib.UI;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Errors;
    using JetBrains.Annotations;
    using Reactive;
    using UI;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    /// <summary>
    /// Палитра свойств
    /// </summary>
    [PublicAPI]
    public static class PalettePropsService
    {
        public static string Various { get; } = "*Различные*";
        [NotNull]
        public static readonly PalettePropsVM propsVM = new PalettePropsVM();
        [NotNull]
        private static readonly List<PalettePropsProvider> providers = new List<PalettePropsProvider>();
        private static IDisposable entModifiedObs;
        private static HashSet<ObjectId> idsHash;

        /// <summary>
        /// Добавление провайдера
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="getTypes">Для выделенных объектов вернуть группы типов свойств. Транзакция уже запущена.</param>
        public static void Registry(string name, Func<ObjectId[], Document, List<PalettePropsType>> getTypes)
        {
            if (providers.Any(p => p.Name == name))
                throw new Exception($"Такой провайдер свойств палитры уже есть - '{name}'");
            if (providers.Count == 0)
            {
                Init();
            }

            providers.Add(new PalettePropsProvider(name, getTypes));
        }

        private static void Init()
        {
            Palette.AddPalette("Свойства", new PalettePropsView(propsVM));
            Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
            foreach (var doc in Application.DocumentManager)
                DocumentSelectionChangeSubscribe(doc as Document);
        }

        private static void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            DocumentSelectionChangeSubscribe(e.Document);
        }

        private static void DocumentSelectionChangeSubscribe([CanBeNull] Document doc)
        {
            if (doc == null) return;
            doc.ImpliedSelectionChanged += Document_ImpliedSelectionChanged;
        }

        private static void Document_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ShowSelection();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private static void ShowSelection()
        {
            if (Palette.IsStop || !providers.Any())
            {
                Clear();
                return;
            }

            var doc = AcadHelper.Doc;
            var sel = doc.Editor.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0)
            {
                // Очистить палитру свойств
                Clear();
                return;
            }

            entModifiedObs?.Dispose();
            var ids = sel.Value.GetObjectIds();
            idsHash = new HashSet<ObjectId>(ids);

            // группы по типу объектов
            var groups = new List<PalettePropsType>();
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                foreach (var provider in providers)
                {
                    try
                    {
                        var types = provider.GetTypes(ids, doc)
                            .Where(w => w?.Groups?.Any(g => g?.Properties?.Any() == true) == true);
                        groups.AddRange(types);
                    }
                    catch (Exception ex)
                    {
                        Inspector.AddError($"Ошибка обработки группы свойств '{provider.Name}' - {ex}");
                    }
                }

                t.Commit();
            }

            if (groups.Count == 0)
            {
                propsVM.Clear();
            }
            else
            {
                propsVM.Types = groups.OrderByDescending(o => o.Count).ToList();
                propsVM.SelectedType = propsVM.Types[0];
            }

            SubscibeEntityModified(doc.Database);
            if (Inspector.HasErrors)
            {
                "ПИК Свойства. Выделение обработано с ошибками! См. PIK_Errors".WriteToCommandLine();
            }
        }

        private static void Clear()
        {
            idsHash = null;
            entModifiedObs?.Dispose();
            propsVM.Clear();
        }

        private static void SubscibeEntityModified(Database db)
        {
            if (idsHash == null)
                return;
            entModifiedObs = db.Events().ObjectModified
                .Where(w => w?.EventArgs?.DBObject?.Id.IsNull != true &&
                    idsHash?.Contains(w.EventArgs.DBObject.Id) == true)
                .ObserveOnDispatcher()
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe(s => { Application.Idle += ModifiedIdle; });
        }

        private static void ModifiedIdle(object sender, EventArgs e)
        {
            Application.Idle -= ModifiedIdle;
            ShowSelection();
        }
    }
}
