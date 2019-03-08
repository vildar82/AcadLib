using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Layers.Filter;
using AcadLib.Layers.LayerState;
using Autodesk.AutoCAD.DatabaseServices;
using iTextSharp.text;
using JetBrains.Annotations;

namespace AcadLib.Template
{
    [Flags]
    public enum TemplateItemEnum
    {
        Layers = 1,
        LayerStates = 2,
        LayerFilters = 4,
        TextStyles = 8,
        DimStyles = 16,
        MLeaderStyles = 32,
        TableStyles = 64
    }
    
    [PublicAPI]
    public class CopyFromTemplate
    {
        public void Copy(Database dbDest, string sourceFile, TemplateItemEnum copyItems)
        {
            using (var dbSrc = new Database(false, false))
            {
                dbSrc.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                dbSrc.CloseInput(true);
                using (var t = dbSrc.TransactionManager.StartTransaction())
                {
                    // Слои
                    try
                    {
                        if (copyItems.HasFlag(TemplateItemEnum.LayerFilters))
                            ImportLayerFilter.ImportLayerFilterTree(dbSrc, dbDest);
                        else if (copyItems.HasFlag(TemplateItemEnum.Layers))
                            ImportLayerFilter.CopyLayers(dbSrc, dbDest);
                        if (copyItems.HasFlag(TemplateItemEnum.LayerStates))
                            ImportLayerState.ImportLayerStates(dbDest, dbSrc);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, "CopyFromTemplate слои");
                        $"Ошибка копирования слоев - {ex.Message}".WriteToCommandLine();
                    }
                    
                    if (copyItems.HasFlag(TemplateItemEnum.TextStyles))
                        CopySymbolTableItems(dbSrc.TextStyleTableId, dbDest.TextStyleTableId, "Текстовые стили");
                    if (copyItems.HasFlag(TemplateItemEnum.DimStyles))
                        CopySymbolTableItems(dbSrc.DimStyleTableId, dbDest.DimStyleTableId, "Размерные стили");
                    if (copyItems.HasFlag(TemplateItemEnum.TableStyles))
                        CopyDbDictItems(dbSrc.TableStyleDictionaryId, dbDest.TableStyleDictionaryId, "Табличные стили");
                    if (copyItems.HasFlag(TemplateItemEnum.MLeaderStyles))
                        CopyDbDictItems(dbSrc.MLeaderStyleDictionaryId, dbDest.MLeaderStyleDictionaryId, "Стили мультивыноски");
                    t.Commit();
                }
            }
        }

        private void CopyDbDictItems(ObjectId srcDictId, ObjectId destDictId, string name)
        {
            try
            {
                var srcDict = srcDictId.GetObject<DBDictionary>();
                var ids = new List<ObjectId>(srcDict.Count);
                foreach (var entry in srcDict)
                {
                    ids.Add(entry.Value);
                }
                var idsCol = new ObjectIdCollection(ids.ToArray());
                srcDictId.Database.WblockCloneObjects(idsCol, destDictId, new IdMapping(),
                    DuplicateRecordCloning.Replace, false);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CopySymbolTableItems {name}.");
                $"Ошибка копирования {name} - {ex.Message}".WriteToCommandLine();
            }
        }

        private void CopySymbolTableItems(ObjectId srcSymbolTableId, ObjectId destSymbolTableId, string name)
        {
            try
            {
                var srcTable = srcSymbolTableId.GetObject<SymbolTable>();
                var idsCol = new ObjectIdCollection(srcTable.Cast<ObjectId>().ToArray());
                srcSymbolTableId.Database.WblockCloneObjects(idsCol, destSymbolTableId, new IdMapping(),
                    DuplicateRecordCloning.Replace, false);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CopySymbolTableItems {name}.");
                $"Ошибка копирования {name} - {ex.Message}".WriteToCommandLine();
            }
        }
    }
}