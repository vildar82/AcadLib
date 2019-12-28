namespace AcadLib.Styles.StyleManager.UI
{
    using System;
    using Model;
    using Errors;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using ReactiveUI;
    using Unit = System.Reactive.Unit;
    using DynamicData.Binding;

    public class StyleTableVM : BaseModel
    {
        public StyleTableVM(StyleManagerVM baseVM, IStyleTable styleTable)
            : base(baseVM)
        {
            StyleTable = styleTable;
            Name = styleTable.Name;
            Delete = CreateCommand<IStyleItem>(DeleteExec);
        }

        public IStyleTable StyleTable { get; }

        public string Name { get; set; }

        public ReactiveCommand<IStyleItem, Unit> Delete { get; set; }

        [CanBeNull]
        public ObservableCollectionExtended<IStyleItem> Styles { get; set; }

        private void DeleteExec([NotNull] IStyleItem styleItem)
        {
            var doc = AcadHelper.Doc;
            var db = doc.Database;
            if (styleItem.Id.Database != db)
                throw new Exception($"Переключись на чертеж '{db.Filename}'");
            try
            {
                StyleTable.Delete(styleItem);
                Styles?.Remove(styleItem);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }

            Inspector.Show();
        }
    }
}
