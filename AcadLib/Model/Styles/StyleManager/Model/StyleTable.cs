namespace AcadLib.Styles.StyleManager.Model
{
    using System;
    using System.Collections;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Errors;
    using Filer;

    public class StyleTable : StyleTableBase
    {
        private readonly ObjectId styleTableId;

        public StyleTable(ObjectId styleTableId)
        {
            this.styleTableId = styleTableId;
        }

        public override void Delete(IStyleItem styleItem)
        {
            var doc = AcadHelper.Doc;
            var needTwice = false;
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var dbo = styleItem.Id.GetObjectT<DBObject>(OpenMode.ForWrite);
                if (dbo is BlockTableRecord btr)
                {
                    foreach (var dbObject in btr.GetBlockReferenceIds(true, false).GetObjects<DBObject>(OpenMode.ForWrite))
                    {
                        var ownerName = dbObject.OwnerId.GetObject<BlockTableRecord>()?.Name;
                        Inspector.AddError("Удалено", $"Удалено вхождение блока из {ownerName}");
                        dbObject.Erase();
                    }

                    if (btr.IsDynamicBlock)
                    {
                        foreach (var anonymBtr in btr.GetAnonymousBlockIds().GetObjects<BlockTableRecord>())
                        {
                            foreach (var dbObject in anonymBtr.GetBlockReferenceIds(true, false)
                                .GetObjects<DBObject>(OpenMode.ForWrite))
                            {
                                var ownerName = dbObject.OwnerId.GetObject<BlockTableRecord>()?.Name;
                                Inspector.AddError("Удалено", $"Удалено вхождение анонимного блока из {ownerName}");
                                dbObject.Erase();
                            }
                        }
                    }

                    btr.Erase();
                }
                else
                {
                    try
                    {
                        var replaceId = ObjectId.Null;
                        var replaceName = string.Empty;
                        var table = styleTableId.GetObjectT<DBObject>();
                        switch (table)
                        {
                            case SymbolTable st:
                                if (st is LayerTable lt)
                                {
                                    if (styleItem.Name == "0")
                                        throw new Exception("Нельзя удалить 0 слой");
                                    replaceId = lt["0"];
                                    replaceName = "0";
                                }
                                else if (styleItem.Name != "ПИК" && st.Has("ПИК"))
                                {
                                    replaceId = st["ПИК"];
                                    replaceName = "ПИК";
                                }
                                else if (styleItem.Name != "Standart" && st.Has("Standart"))
                                {
                                    replaceId = st["Standart"];
                                    replaceName = "Standart";
                                }
                                else
                                {
                                    replaceId = st.Cast<ObjectId>().FirstOrDefault(s => s != styleItem.Id);
                                    replaceName = replaceId.GetObject<SymbolTableRecord>()?.Name;
                                }

                                break;
                            case DBDictionary dict:
                                if (dict.Contains("ПИК"))
                                {
                                    if (dict["ПИК"] is DictionaryEntry entry)
                                    {
                                        replaceId = (ObjectId)entry.Value;
                                        replaceName = "ПИК";
                                    }
                                }
                                else if (dict.Contains("Standart"))
                                {
                                    if (dict["Standart"] is DictionaryEntry entry)
                                    {
                                        replaceId = (ObjectId)entry.Value;
                                        replaceName = "Standart";
                                    }
                                }

                                break;
                        }

                        if (dbo is LayerTableRecord)
                        {
                            ReplaceLayer(doc.Database, styleItem.Id, replaceId, replaceName);
                        }

                        // Найти все ссылки и зменить
                        var refs = dbo.GetReferences();
                        if (!replaceId.IsNull)
                        {
                            refs.SoftPointerIds.ForEach(p => ReplacePointer(p, styleItem.Id, replaceId, replaceName));
                            refs.HardPointerIds.ForEach(p => ReplacePointer(p, styleItem.Id, replaceId, replaceName));
                            needTwice = true;
                        }
                        else
                        {
                            dbo.Erase();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, $"Ошибка замены референсов в стиле '{styleItem.Name}' в таблице стилей '{Name}'");
                    }
                }

                t.Commit();
            }

            if (needTwice)
            {
                using (doc.LockDocument())
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    var dbo = styleItem.Id.GetObjectT<DBObject>(OpenMode.ForWrite);
                    dbo.Erase();
                    t.Commit();
                }
            }
        }

        private void ReplaceLayer(Database db, ObjectId layerId, ObjectId replaceId, string replaceName)
        {
            var bt = db.BlockTableId.GetObjectT<BlockTable>();
            foreach (var btr in bt.GetObjects<BlockTableRecord>())
            {
                foreach (var entity in btr.GetObjects<Entity>(OpenMode.ForRead).Where(w => w.LayerId == layerId))
                {
                    var entW = entity.Id.GetObjectT<Entity>(OpenMode.ForWrite);
                    entW.LayerId = replaceId;
                    Inspector.AddError($"Заменен слой объекта '{entW.Id.ObjectClass.Name}' на '{replaceName}'");
                }
            }
        }

        private void ReplacePointer(ObjectId pointerId, ObjectId styleId, ObjectId replaceId, string replaceName)
        {
            var dbo = pointerId.GetObjectT<DBObject>(OpenMode.ForWrite);
            if (dbo is BlockTableRecord btr)
            {
                foreach (var objectId in btr)
                {
                    ReplacePointer(objectId, styleId, replaceId, replaceName);
                }
            }

            var props = dbo.GetType().GetProperties();
            var prop = props.FirstOrDefault(p =>
            {
                try
                {
                    var val = p.GetValue(dbo);
                    if (val is ObjectId valId)
                    {
                        return valId == styleId;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Info($"p.GetValue(dbo) prop={p.Name}, {dbo} - {ex.Message}");
                }
                return false;
            });
            if (prop == null)
            {
                Logger.Log.Info($"prop == null {dbo}");
            }
            else
            {
                try
                {
                    prop.SetValue(dbo, replaceId);
                    var msg = $"Заменена ссылка в объекте '{dbo.Id.ObjectClass.Name}' на '{replaceName}'";
                    if (dbo is Entity ent)
                    {
                        Inspector.AddError(msg, ent);
                    }
                    else
                    {
                        Inspector.AddError(msg);
                    }

                    Logger.Log.Info($"prop.SetValue prop={prop.Name} - {dbo.Id.ObjectClass.Name}");
                }
                catch (Exception ex)
                {
                    Logger.Log.Info($"prop.SetValue prop={prop.Name}, {ex.Message} - {dbo.Id.ObjectClass.Name}");
                }
            }
        }
    }
}
