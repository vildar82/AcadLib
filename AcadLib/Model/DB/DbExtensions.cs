namespace Autodesk.AutoCAD.DatabaseServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AcadLib;
    using ApplicationServices.Core;
    using AutoCAD_PIK_Manager.Settings;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class DbExtensions
    {
        public const string PIK = General.Company;

        private static string UserGroup { get; } = PikSettings.UserGroup;

        /// <summary>
        /// Текущее пространство - Model (не лист, и не редактор блоков)
        /// </summary>
        /// <returns></returns>
        public static bool IsModel(this Database db)
        {
            return db.TileMode && "BLOCKEDITOR".GetSystemVariable<int>() != 1;
        }

        public static ObjectId GetMS(this Database db)
        {
            return SymbolUtilityServices.GetBlockModelSpaceId(db);
        }

        [NotNull]
        public static BlockTable BT(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            return db.BlockTableId.GetObjectT<BlockTable>(mode);
        }

        [NotNull]
        public static BlockTableRecord MS(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            return MsId(db).GetObjectT<BlockTableRecord>(mode);
        }
        
        public static ObjectId MsId(this Database db)
        {
            using (var bt = (BlockTable)db.BlockTableId.Open(OpenMode.ForRead))
            {
                return bt[BlockTableRecord.ModelSpace];
            }
        }

        /// <summary>
        /// Получение углового размерного стиля ПИК
        /// </summary>
        public static ObjectId GetDimAngularStylePIK(this Database db)
        {
            // Загрузка простого стиля ПИК
            GetDimStyle(db, PIK, UserGroup);

            // Загрузка углового стиля ПИК
            return GetDimStyle(db, PIK + "$2", UserGroup);
        }

        public static ObjectId GetDimStyle(this Database db, string styleName, string templateName)
        {
            var idStyle = GetDimStyleId(db, styleName);
            if (idStyle.IsNull)
            {
                // Копирование размерного стиля из шаблона
                try
                {
                    idStyle = CopyObjectFromTemplate(db, GetDimStyleId, styleName, db.DimStyleTableId, templateName);
                }
                catch
                {
                    // ignored
                }

                if (idStyle.IsNull)
                {
                    idStyle = db.Dimstyle;
                }
            }

            return idStyle;
        }

        /// <summary>
        /// Получение размерного стиля ПИК
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static ObjectId GetDimStylePIK(this Database db)
        {
            return GetDimStyle(db, PIK, UserGroup);
        }

        public static ObjectId GetLineTypeIdByName([NotNull] this Database db, string name)
        {
            var resVal = ObjectId.Null;
            using (var ltTable = (LinetypeTable)db.LinetypeTableId.Open(OpenMode.ForRead))
            {
                if (ltTable.Has(name))
                {
                    resVal = ltTable[name];
                }
                else if (!string.Equals(name, SymbolUtilityServices.LinetypeContinuousName, StringComparison.OrdinalIgnoreCase))
                {
                    resVal = db.GetLineTypeIdContinuous();
                }
            }

            return resVal;
        }

        public static ObjectId GetLineTypeIdContinuous([NotNull] this Database db)
        {
            return db.GetLineTypeIdByName(SymbolUtilityServices.LinetypeContinuousName);
        }

        public static ObjectId GetMleaderStyle(this Database db, string styleName, string template, bool update)
        {
            var idStyle = ObjectId.Null;
            if (!update)
            {
                idStyle = GetMleaderStyleId(db, styleName);
            }

            if (update || idStyle.IsNull)
            {
                // Копирование стиля из шаблона
                try
                {
                    idStyle = CopyObjectFromTemplate(db, GetMleaderStyleId, styleName, db.MLeaderStyleDictionaryId, template);
                }
                catch
                {
                    // ignored
                }

                if (idStyle.IsNull)
                {
                    idStyle = db.MLeaderstyle;
                }
            }

            return idStyle;
        }

        /// <summary>
        /// Получение табличного стиля ПИК
        /// </summary>
        public static ObjectId GetTableStylePIK(this Database db)
        {
            return GetTableStylePIK(db, PIK, false);
        }

        /// <summary>
        /// Получение табличного стиля ПИК с обновлением (DuplicateRecordCloning.Replace)
        /// Не обновляется существующий стиль ПИК!!!
        /// </summary>
        public static ObjectId GetTableStylePIK(this Database db, bool update)
        {
            return GetTableStylePIK(db, PIK, update);
        }

        /// <summary>
        /// Получение табличного стиля ПИК
        /// </summary>
        public static ObjectId GetTableStylePIK(this Database db, string styleName)
        {
            return GetTableStylePIK(db, styleName, false);
        }

        /// <summary>
        /// Получение табличного стиля ПИК
        /// </summary>
        public static ObjectId GetTableStylePIK(this Database db, string styleName, bool update)
        {
            return GetTableStylePIK(db, styleName, UserGroup, update);
        }

        public static ObjectId GetTableStylePIK(this Database db, string styleName, [CanBeNull] string templateName)
        {
            return GetTableStylePIK(db, styleName, templateName ?? UserGroup, false);
        }

        /// <summary>
        /// Получение табличного стиля ПИК
        /// </summary>
        public static ObjectId GetTableStylePIK(this Database db, string styleName, string templateName, bool update)
        {
            var idStyle = ObjectId.Null;
            if (!update)
            {
                idStyle = GetTableStyleId(db, styleName);
            }

            if (update || idStyle.IsNull)
            {
                // Копирование стиля таблиц из шаблона
                try
                {
                    idStyle = CopyObjectFromTemplate(db, GetTableStyleId, styleName, db.TableStyleDictionaryId,
                        templateName);
                }
                catch
                {
                    //
                }

                if (idStyle.IsNull)
                {
                    idStyle = db.Tablestyle;
                }
            }

            return idStyle;
        }

        /// <summary>
        /// Получение текстового стиля ПИК
        /// </summary>
        public static ObjectId GetTextStylePIK(this Database db)
        {
            return GetTextStylePIK(db, PIK, UserGroup);
        }

        /// <summary>
        /// Получение табличного стиля ПИК
        /// </summary>
        public static ObjectId GetTextStylePIK(this Database db, string styleName)
        {
            return GetTextStylePIK(db, styleName, UserGroup);
        }

        /// <summary>
        /// Получение или копирование из шаблона текстового стиля
        /// </summary>
        /// <param name="db">Чертеж</param>
        /// <param name="styleName">Имя стиля</param>
        /// <param name="templateFile">Имя шаблона, без разрешения</param>
        /// <returns></returns>
        public static ObjectId GetTextStylePIK(this Database db, string styleName, string templateFile)
        {
            var idStyle = GetTextStylePik(db, styleName);
            if (idStyle.IsNull)
            {
                // Копирование стиля таблиц из шаблона
                try
                {
                    idStyle = CopyObjectFromTemplate(db, GetTextStylePik, styleName, db.TableStyleDictionaryId, templateFile);
                }
                catch
                {
                    // ignored
                }

                if (idStyle.IsNull)
                {
                    idStyle = db.Textstyle;
                }
            }

            return idStyle;
        }

        public static IEnumerable<T> IterateDB<T>([NotNull] this Database db) where T : DBObject
        {
            for (var i = db.BlockTableId.Handle.Value; i < db.Handseed.Value; i++)
            {
                if (!db.TryGetObjectId(new Handle(i), out var id))
                    continue;
                var objT = id.GetObject<T>();
                if (objT != null)
                {
                    yield return objT;
                }
            }
        }

        /// <summary>
        /// Текущий аннотативный масштаб чертежа. 100, 10 и т.п.
        /// </summary>
        public static double Scale(this Database db)
        {
            return AcadLib.Scale.ScaleHelper.GetCurrentAnnoScale(db);
        }

        // Копирование стиля таблиц ПИК из файла шаблона
        private static ObjectId CopyObjectFromTemplate(
            Database db,
            Func<Database, string, ObjectId> getObjectId,
            string styleName,
            ObjectId ownerIdTable,
            string templateName)
        {
            var idStyleDest = ObjectId.Null;

            // файл шаблона
            var fileTemplate = Path.Combine(PikSettings.LocalSettingsFolder, "Template", UserGroup,
                templateName + ".dwt");
            if (File.Exists(fileTemplate))
            {
                using (var dbTemplate = new Database(false, true))
                {
                    dbTemplate.ReadDwgFile(fileTemplate, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                    dbTemplate.CloseInput(true);
                    var idStyleInTemplate = getObjectId(dbTemplate, styleName);
                    if (!idStyleInTemplate.IsNull)
                    {
                        using (var map = new IdMapping())
                        {
                            using (var ids = new ObjectIdCollection(new[] { idStyleInTemplate }))
                            {
                                using (Application.DocumentManager.MdiActiveDocument?.LockDocument())
                                {
                                    db.WblockCloneObjects(ids, ownerIdTable, map, DuplicateRecordCloning.Replace, false);
                                }

                                idStyleDest = map[idStyleInTemplate].Value;
                            }
                        }
                    }
                }
            }

            return idStyleDest;
        }

        private static ObjectId GetDictStyleId(Database db, string styleName, [NotNull] Func<Database, ObjectId> idDictTable)
        {
            var idStyle = ObjectId.Null;
            using (var dictTableStyle = (DBDictionary)idDictTable(db).Open(OpenMode.ForRead))
            {
                if (dictTableStyle.Contains(styleName))
                {
                    idStyle = dictTableStyle.GetAt(styleName);
                }
            }

            return idStyle;
        }

        private static ObjectId GetDimStyleId(Database db, string styleName)
        {
            return GetStyleId(db, styleName, d => d.DimStyleTableId);
        }

        private static ObjectId GetMleaderStyleId(Database db, string styleName)
        {
            return GetDictStyleId(db, styleName, d => d.MLeaderStyleDictionaryId);
        }

        private static ObjectId GetStyleId(Database db, string styleName, [NotNull] Func<Database, ObjectId> idSymbolTable)
        {
            var idStyle = ObjectId.Null;
            using (var symbolTable = (SymbolTable)idSymbolTable(db).Open(OpenMode.ForRead))
            {
                if (symbolTable.Has(styleName))
                {
                    idStyle = symbolTable[styleName];
                }
            }

            return idStyle;
        }

        private static ObjectId GetTableStyleId(Database db, string styleName)
        {
            return GetDictStyleId(db, styleName, d => d.TableStyleDictionaryId);
        }

        private static ObjectId GetTextStylePik(Database db, string styleName)
        {
            return GetStyleId(db, styleName, d => d.TextStyleTableId);
        }
    }
}