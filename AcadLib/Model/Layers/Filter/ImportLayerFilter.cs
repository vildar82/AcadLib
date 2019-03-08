namespace AcadLib.Layers.Filter
{
    using System;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.LayerManager;
    using Errors;
    using JetBrains.Annotations;
    using LayerState;

    /// <summary>
    ///     Импорт фильтрами слоев
    /// </summary>
    [PublicAPI]
    public static class ImportLayerFilter
    {
        public static void ImportLayerFilterAndState([NotNull] this Database dbDest, string sourceFile)
        {
            try
            {
                using (var dbSrc = new Database(false, false))
                {
                    dbSrc.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                    dbSrc.CloseInput(true);
                    using (var t = dbSrc.TransactionManager.StartTransaction())
                    {
                        ImportLayerFilterTree(dbSrc, dbDest);
                        ImportLayerState.ImportLayerStates(dbDest, dbSrc);
                        t.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Inspector.AddError($"Ошибка имполра фильтра слоев из из файла '{sourceFile}' - {ex.Message}");
            }
        }

        /// <summary>
        ///     Импорт фильтра слоев из файла в корневой фильтр слоев чертежа назначения.
        ///     При этом копируются все слои из чертежа источника
        /// </summary>
        /// <param name="sourceFile">Файл источник из которого копируются фильтры слоев</param>
        /// <param name="dbDest">Чертеж назначения - в который копируются фильтры слоев</param>
        public static void ImportLayerFilterTree([NotNull] this Database dbDest, string sourceFile)
        {
            try
            {
                using (var dbSrc = new Database(false, false))
                {
                    dbSrc.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                    dbSrc.CloseInput(true);
                    using (var t = dbSrc.TransactionManager.StartTransaction())
                    {
                        ImportLayerFilterTree(dbSrc, dbDest);
                        t.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Inspector.AddError($"Ошибка имполра фильтра слоев из из файла '{sourceFile}' - {ex.Message}");
            }
        }

        [NotNull]
        public static IdMapping CopyLayers([NotNull] Database dbSrc, Database dbDest)
        {
            var lt = dbSrc.LayerTableId.GetObjectT<LayerTable>();
            var layerIds = new ObjectIdCollection(lt.GetObjects<LayerTableRecord>().Select(s => s.Id).ToArray());
            var idmap = new IdMapping();
            if (layerIds.Count > 0)
                dbSrc.WblockCloneObjects(layerIds, dbDest.LayerTableId, idmap, DuplicateRecordCloning.Replace, false);
            return idmap;
        }

        public static void ImportLayerFilterTree(Database dbSrc, Database dbDest)
        {
            // Копирование слоев
            var idmap = CopyLayers(dbSrc, dbDest);

            // Импорт фильтров
            var lft = dbDest.LayerFilters;
            ImportNestedFilters(dbSrc.LayerFilters.Root, lft.Root, idmap);
            dbDest.LayerFilters = lft;
        }

        private static void ImportNestedFilters([NotNull] LayerFilter srcFilter, LayerFilter destFilter, IdMapping idmap)
        {
            foreach (LayerFilter sf in srcFilter.NestedFilters)
            {
                // Опеределяем не было ли фильтра слоев
                // с таким же именем во внешней db
                var df = destFilter.NestedFilters.Cast<LayerFilter>().FirstOrDefault(f => f.Name.Equals(sf.Name));
                if (df == null)
                {
                    if (sf is LayerGroup)
                    {
                        // Создаем новую группу слоев если ничего не найдено
                        var sfgroup = sf as LayerGroup;
                        var dfgroup = new LayerGroup { Name = sf.Name };
                        df = dfgroup;
                        var lyrs = sfgroup.LayerIds;
                        foreach (ObjectId lid in lyrs)
                        {
                            if (idmap.Contains(lid))
                            {
                                var idp = idmap[lid];
                                dfgroup.LayerIds.Add(idp.Value);
                            }
                        }

                        destFilter.NestedFilters.Add(df);
                    }
                    else
                    {
                        // Создаем фильтр слоев если ничего не найдено
                        df = new LayerFilter
                        {
                            Name = sf.Name,
                            FilterExpression = sf.FilterExpression
                        };
                        destFilter.NestedFilters.Add(df);
                    }
                }

                // Импортируем другие фильтры
                ImportNestedFilters(sf, df, idmap);
            }
        }
    }
}