namespace AcadLib.Styles.StyleManager.UI
{
    using System;
    using System.Collections;
    using System.Linq;
    using AcadLib.Styles.StyleManager.Model;
    using Autodesk.AutoCAD.DatabaseServices;
    using Errors;
    using Filer;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using NLog;
    using ReactiveUI;
    using ReactiveUI.Legacy;
    using Unit = System.Reactive.Unit;

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
        public ReactiveList<IStyleItem> Styles { get; set; }

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
