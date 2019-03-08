namespace AcadLib.Styles.StyleManager.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AcadLib.Styles.StyleManager.Model;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using ReactiveUI;
    using ReactiveUI.Legacy;

    public class StyleManagerVM : BaseViewModel
    {
        public StyleManagerVM()
        {
            LoadStyleTables();
        }

        public List<BaseModel> StyleTables { get; set; } = new List<BaseModel>();

        private void LoadStyleTables()
        {
            var doc = AcadHelper.Doc;
            var db = doc.Database;
            using (var t = doc.TransactionManager.StartTransaction())
            {
                StyleTables.Add(GetStyleTable(db.TextStyleTableId, "Текстовые стили"));
                StyleTables.Add(GetStyleTable(db.DimStyleTableId, "Размерные стили"));
                StyleTables.Add(GetStyleTableFromDict(db.MLeaderStyleDictionaryId, "Мультивыноски"));
                StyleTables.Add(GetStyleTableFromDict(db.TableStyleDictionaryId, "Стили таблиц"));
                StyleTables.Add(GetStyleTable(db.LinetypeTableId, "Типы линий"));
                StyleTables.Add(GetStyleTable(db.LayerTableId, "Слои"));
                StyleTables.Add(GetStyleTable(db.BlockTableId, "Блоки"));
                StyleTables.Add(GetStyleTableFromDict(db.NamedObjectsDictionaryId, "Словари"));
                t.Commit();
            }
        }

        [NotNull]
        private StyleTableVM GetStyleTableFromDict(ObjectId styleDictId, string name)
        {
            var dict = styleDictId.GetObjectT<DBDictionary>();
            var styles = dict.Cast<DictionaryEntry>().Select(s => new StyleItem
            {
                Name = s.Key.ToString(),
                Id = (ObjectId)s.Value
            }).OrderBy(o => o.Name);
            return new StyleTableVM(this, new StyleTable(styleDictId))
            {
                Name = name,
                Styles = new ReactiveList<IStyleItem>(styles),
            };
        }

        [NotNull]
        private StyleTableVM GetStyleTable(ObjectId symbolTableId, string name)
        {
            var table = symbolTableId.GetObjectT<SymbolTable>();
            var styles = table.GetObjects<SymbolTableRecord>().Select(s => new StyleItem
            {
                Name = s.Name,
                Id = s.Id
            }).OrderBy(o => o.Name);
            return new StyleTableVM(this, new StyleTable(symbolTableId))
            {
                Name = name,
                Styles = new ReactiveList<IStyleItem>(styles),
            };
        }
    }
}
