using System.ServiceModel.Configuration;
using AcadLib.Strings;

namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class Block
    {
        public static Tolerance Tolerance01 = new Tolerance(0.01, 0.01);

        /// <summary>
        /// Создание блока из примитивов в памяти
        /// в текущем чертеже, должна быть запущена транзакция
        /// </summary>
        /// <param name="ents">Примитивы в памяти</param>
        /// <param name="name">Имя блока</param>
        /// <param name="location">Точка вставки (сдвинутся все примитивы)</param>
        /// <param name="overrideBlock">Заменить объекты в блоки если он уже есть</param>
        /// <exception cref="Exception">Такое имя блока уже есть.</exception>
        public static ObjectId CreateBlock([NotNull] this List<Entity> ents, string name, Point3d location, bool overrideBlock)
        {
            var db = HostApplicationServices.WorkingDatabase;
            var bt = db.BlockTableId.GetObjectT<BlockTable>(OpenMode.ForWrite);
            var t = db.TransactionManager.TopTransaction;
            BlockTableRecord btr;
            if (bt.Has(name))
            {
                if (!overrideBlock)
                    return bt[name];
                btr = bt[name].GetObjectT<BlockTableRecord>(OpenMode.ForWrite);
                foreach (var entity in btr.GetObjects<Entity>(OpenMode.ForWrite))
                    entity.Erase();
            }
            else
            {
                btr = new BlockTableRecord { Name = name };
                bt.Add(btr);
                t.AddNewlyCreatedDBObject(btr, true);
            }
            
            var vec = Point3d.Origin - location;
            var matrix = Matrix3d.Identity;
            var hasMatrix = false;
            if (vec.Length > 0.001)
            {
                matrix = Matrix3d.Displacement(vec);
                hasMatrix = true;
            }

            foreach (var ent in ents)
            {
                if (hasMatrix)
                    ent.TransformBy(matrix);
                btr.AppendEntity(ent);
                t.AddNewlyCreatedDBObject(ent, true);
            }

            return btr.Id;
        }

        /// <summary>
        /// Создание блока из объектов чертежа.
        /// Должна быть запусщена транзакция. блок добавляется в таблицу блоков.
        /// </summary>
        /// <param name="entIds">Объекты чертежа</param>
        /// <param name="name">Имя блока</param>
        /// <param name="location">Точка вставки блока</param>
        /// <param name="erase">Удалять исходные объекты</param>
        public static ObjectId CreateBlock([NotNull] this List<ObjectId> entIds, string name, Point3d location, bool erase)
        {
            var db = entIds[0].Database;
            var t = db.TransactionManager.TopTransaction;
            ObjectId idBtr;

            // создание определения блока
            BlockTableRecord btr;
            using (var bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForWrite))
            using (btr = new BlockTableRecord())
            {
                btr.Name = name;
                idBtr = bt.Add(btr);
                t.AddNewlyCreatedDBObject(btr, true);
            }

            // копирование выбранных объектов в блок
            var idsCol = new ObjectIdCollection(entIds.ToArray());
            using (var mapping = new IdMapping())
            {
                db.DeepCloneObjects(idsCol, idBtr, mapping, false);
            }

            // перемещение объектов в блоке
            btr = (BlockTableRecord)idBtr.GetObject(OpenMode.ForRead);
            var moveMatrix = Matrix3d.Displacement(Point3d.Origin - location);
            foreach (var idEnt in btr)
            {
                var ent = idEnt.GetObject<Entity>(OpenMode.ForWrite) ?? throw new InvalidOperationException();
                ent.TransformBy(moveMatrix);
            }

            // удаление выбранных объектов
            if (erase)
            {
                foreach (ObjectId idEnt in idsCol)
                {
                    if (!idEnt.IsValidEx())
                        continue;
                    var ent = idEnt.GetObject<Entity>(OpenMode.ForWrite) ?? throw new InvalidOperationException();
                    ent.Erase();
                }
            }

            return idBtr;
        }

        /// <summary>
        /// Это пользовательский блок, а не лист, ссылка, анонимный или спец.блок(*).
        /// </summary>
        public static bool IsUserBlock([NotNull] this BlockTableRecord btr)
        {
            return !btr.IsLayout && !btr.IsAnonymous && !btr.IsFromExternalReference && !btr.Name.StartsWith("*");
        }

        /// <summary>
        /// Копирование определения блока из файла с общими блоками оформления
        /// </summary>
        public static ObjectId CopyCommonBlockFromTemplate(string blName, Database db)
        {
            var res = CopyBlockFromExternalDrawing(blName, BlockInsert.fileCommonBlocks, db);
            return res;
        }

        /// <summary>
        /// Определен ли данный блок в активном чертеже
        /// </summary>
        public static bool HasBlockThisDrawing(string name)
        {
            var doc = AcadHelper.Doc;
            using (var bt = (BlockTable)doc.Database.BlockTableId.Open(OpenMode.ForRead))
            {
                return bt.Has(name);
            }
        }

        /// <summary>
        /// Копирование определенич блока из внешнего чертежа
        /// </summary>
        /// <param name="blName">Имя блока</param>
        /// <param name="fileDrawing">Полный путь к чертежу из которого копируется блок</param>
        /// <param name="destDb">База чертежа в который копируетсяя блок</param>
        /// <param name="mode">Режим для уже существующих элементов - пропускать или заменять.</param>
        /// <exception cref="Exception">Если нет блока в файле fileDrawing.</exception>
        public static ObjectId CopyBlockFromExternalDrawing(
            string blName,
            string fileDrawing,
            Database destDb,
            DuplicateRecordCloning mode = DuplicateRecordCloning.Ignore)
        {
            if (mode == DuplicateRecordCloning.Ignore)
            {
                using (var bt = (BlockTable)destDb.BlockTableId.Open(OpenMode.ForRead))
                {
                    if (bt.Has(blName))
                    {
                        return bt[blName];
                    }
                }
            }

            var blNames = new List<string> { blName };
            var resCopy = CopyBlockFromExternalDrawing(blNames, fileDrawing, destDb, mode);
            if (!resCopy.TryGetValue(blName, out var idRes))
            {
                throw new Autodesk.AutoCAD.Runtime.Exception(Autodesk.AutoCAD.Runtime.ErrorStatus.MissingBlockName,
                    $"Не найден блок {blName}");
            }

            return idRes;
        }

        /// <summary>
        /// Перелопределение блока
        /// </summary>
        public static void Redefine(string name, string file, Database destDb)
        {
            var idBtr = CopyBlockFromExternalDrawing(name, file, destDb, DuplicateRecordCloning.Replace);

            // Синхронизация атрибутов
            idBtr.SynchronizeAttributes();
        }

        /// <summary>
        /// Копирование определенич блока из внешнего чертежа
        /// </summary>
        /// <param name="filter">Фильтр блоков, которые нужно копировать</param>
        /// <param name="fileDrawing">Полный путь к чертежу из которого копируется блок</param>
        /// <param name="destDb">База чертежа в который копируетсяя блок</param>
        /// <param name="mode">Режим для существующих элементов - пропускать или заменять</param>
        /// <exception cref="Exception">Если нет блока в файле fileDrawing.</exception>
        /// <returns>Список пар значений имени блока и idBtr</returns>
        [NotNull]
        public static Dictionary<string, ObjectId> CopyBlockFromExternalDrawing(
            Predicate<string> filter,
            string fileDrawing,
            Database destDb,
            DuplicateRecordCloning mode = DuplicateRecordCloning.Ignore)
        {
            var resVal = new Dictionary<string, ObjectId>();
            using (var extDb = new Database(false, true))
            {
                extDb.ReadDwgFile(fileDrawing, System.IO.FileShare.ReadWrite, true, string.Empty);
                extDb.CloseInput(true);
                var valToCopy = new Dictionary<ObjectId, string>();
                using (var bt = (BlockTable)extDb.BlockTableId.Open(OpenMode.ForRead))
                {
                    foreach (var idBtr in bt)
                    {
                        using (var btr = (BlockTableRecord)idBtr.Open(OpenMode.ForRead))
                        {
                            if (!btr.IsLayout && !btr.IsDependent && !btr.IsAnonymous && filter(btr.Name))
                            {
                                valToCopy.Add(btr.Id, btr.Name);
                            }
                        }
                    }
                }

                // Копир
                if (valToCopy.Count > 0)
                {
                    // Получаем текущую базу чертежа
                    using (var map = new IdMapping())
                    {
                        using (var ids = new ObjectIdCollection(valToCopy.Keys.ToArray()))
                        {
                            destDb.WblockCloneObjects(ids, destDb.BlockTableId, map, mode, false);
                            foreach (var item in valToCopy)
                            {
                                resVal.Add(item.Value, map[item.Key].Value);
                            }
                        }
                    }
                }
            }

            return resVal;
        }

        /// <summary>
        /// Копирование определенич блока из внешнего чертежа
        /// </summary>
        /// <param name="blNames">Имена блоков</param>
        /// <param name="fileDrawing">Полный путь к чертежу из которого копируется блок</param>
        /// <param name="destDb">База чертежа в который копируетсяя блок</param>
        /// <param name="mode">Режим для существующих элементов - пропускать или заменять</param>
        /// <exception cref="Exception">Если нет блока в файле fileDrawing.</exception>
        /// <returns>Список пар значений имени блока и idBtr</returns>
        [NotNull]
        public static Dictionary<string, ObjectId> CopyBlockFromExternalDrawing(
            [NotNull] IList<string> blNames,
            string fileDrawing,
            Database destDb,
            DuplicateRecordCloning mode = DuplicateRecordCloning.Ignore)
        {
            var resVal = new Dictionary<string, ObjectId>();
            var uniqBlNames = blNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (mode == DuplicateRecordCloning.Ignore)
            {
                // Если уже есть эти блоки
                using (var btDest = (BlockTable)destDb.BlockTableId.Open(OpenMode.ForRead))
                {
                    var existBls = new List<string>();
                    foreach (var uniqBlName in uniqBlNames)
                    {
                        if (btDest.Has(uniqBlName))
                        {
                            existBls.Add(uniqBlName);
                            resVal.Add(uniqBlName, btDest[uniqBlName]);
                        }
                    }

                    if (existBls.Any())
                    {
                        uniqBlNames = uniqBlNames.Except(existBls).ToList();
                    }
                }
            }

            if (!uniqBlNames.Any())
            {
                return resVal;
            }

            using (var extDb = new Database(false, true))
            {
                extDb.ReadDwgFile(fileDrawing, System.IO.FileShare.ReadWrite, true, string.Empty);
                extDb.CloseInput(true);
                var valToCopy = new Dictionary<ObjectId, string>();
                using (var bt = (BlockTable)extDb.BlockTableId.Open(OpenMode.ForRead))
                {
                    foreach (var blName in uniqBlNames)
                    {
                        if (bt.Has(blName))
                        {
                            var id = bt[blName];
                            valToCopy.Add(id, blName);
                        }
                    }
                }

                // Копир
                if (valToCopy.Count > 0)
                {
                    // Получаем текущую базу чертежа
                    using (var map = new IdMapping())
                    {
                        using (var ids = new ObjectIdCollection(valToCopy.Keys.ToArray()))
                        {
                            destDb.WblockCloneObjects(ids, destDb.BlockTableId, map, mode, false);
                            foreach (var item in valToCopy)
                            {
                                resVal.Add(item.Value, map[item.Key].Value);
                            }
                        }
                    }
                }
            }

            // Если задан режим переопределения - то перерисовка геометрии динамических блоков
            if (mode == DuplicateRecordCloning.Replace)
            {
                using (var t = destDb.TransactionManager.StartTransaction())
                {
                    foreach (var item in resVal)
                    {
                        if (item.Value.IsValidEx())
                        {
                            var btr = (BlockTableRecord)item.Value.GetObject(OpenMode.ForRead);
                            if (btr.IsDynamicBlock)
                            {
                                try
                                {
                                    btr = (BlockTableRecord)item.Value.GetObject(OpenMode.ForWrite);
                                    btr.UpdateAnonymousBlocks();
                                }
                                catch
                                {
                                    //
                                }
                            }
                        }
                    }

                    t.Commit();
                }
            }

            return resVal;
        }

        /// <summary>
        /// Копирование определения блока и добавление его в таблицу блоков.
        /// </summary>
        /// <param name="idBtrSource">Копируемый блок</param>
        /// <param name="name">Имя для нового блока</param>
        /// <returns>ID скопированного блока, или существующего в чертеже с таким именем.</returns>
        public static ObjectId CopyBtr(ObjectId idBtrSource, string name)
        {
            ObjectId idBtrCopy;
            var db = idBtrSource.Database;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var btrSource = (BlockTableRecord)t.GetObject(idBtrSource, OpenMode.ForRead);
                var bt = (BlockTable)t.GetObject(db.BlockTableId, OpenMode.ForRead);

                // проверка имени блока
                if (bt.Has(name))
                {
                    idBtrCopy = bt[name];
                }
                else
                {
                    var btrCopy = (BlockTableRecord)btrSource.Clone();
                    btrCopy.Name = name;
                    bt = (BlockTable)bt.Id.GetObject(OpenMode.ForWrite);
                    idBtrCopy = bt.Add(btrCopy);
                    t.AddNewlyCreatedDBObject(btrCopy, true);

                    // Копирование объектов блока
                    var ids = new ObjectIdCollection();
                    foreach (var idEnt in btrSource)
                    {
                        ids.Add(idEnt);
                    }

                    var map = new IdMapping();
                    db.DeepCloneObjects(ids, idBtrCopy, map, false);
                }

                t.Commit();
            }

            return idBtrCopy;
        }

        /// <summary>
        /// Копирование листа
        /// </summary>
        /// <returns>ID Layout</returns>
        public static ObjectId CopyLayout(Database db, string layerSource, string layerCopy)
        {
            var dbOrig = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = db;
            var lm = LayoutManager.Current;

            // Нужно проверить имена. Вдруг нет листа источника, или уже есть копируемый лист.
            lm.CopyLayout(layerSource, layerCopy);
            var idLayoutCopy = lm.GetLayoutId(layerCopy);
            HostApplicationServices.WorkingDatabase = dbOrig;
            return idLayoutCopy;
        }

        /// <summary>
        /// Клонирование листа.
        /// Должна быть открыта транзакция!!!
        /// </summary>
        /// <param name="db">База в которой это производится. Должна быть WorkingDatabase</param>
        /// <param name="existLayoutName">Имя существующего листа, с которого будет клонироваться новый лист.
        /// Должен существовать в чертеже.</param>
        /// <param name="newLayoutName">Имя для нового листа.</param>
        /// <returns>ObjectId нового листа</returns>
        public static ObjectId CloneLayout([NotNull] Database db, string existLayoutName, string newLayoutName)
        {
            ObjectId newLayoutId;
            ObjectId existLayoutId;
            using (new WorkingDatabaseSwitcher(db))
            {
                var lm = LayoutManager.Current;
                newLayoutId = lm.CreateLayout(newLayoutName);
                existLayoutId = lm.GetLayoutId(existLayoutName);
            }

            var objIdCol = new ObjectIdCollection();
            ObjectId idBtrNewLayout;
            using (var newLayout = (Layout)newLayoutId.GetObject(OpenMode.ForWrite))
            {
                var curLayout = (Layout)existLayoutId.GetObject(OpenMode.ForRead);
                newLayout.CopyFrom(curLayout);
                idBtrNewLayout = newLayout.BlockTableRecordId;
                using (var btrCurLayout = (BlockTableRecord)curLayout.BlockTableRecordId.Open(OpenMode.ForRead))
                {
                    foreach (var objId in btrCurLayout)
                    {
                        objIdCol.Add(objId);
                    }
                }
            }

            var idMap = new IdMapping();
            db.DeepCloneObjects(objIdCol, idBtrNewLayout, idMap, false);
            return newLayoutId;
        }

        /// <summary>
        /// Получение валидной строки для имени блока. С замоной всех ненужных символов на .
        /// </summary>
        /// <param name="name">Имя для блока</param>
        /// <returns>Валидная строка имени</returns>
        [NotNull]
        public static string GetValidNameForBlock([NotNull] string name)
        {
            return name.GetValidDbSymbolName();
        }

        /// <summary>
        /// Проверка дублирования вхождений блоков
        /// </summary>
        /// <param name="blk1"></param>
        /// <param name="blk2"></param>
        /// <returns></returns>
        public static bool IsDuplicate([NotNull] this BlockReference blk1, [NotNull] BlockReference blk2)
        {
            var tol = new Tolerance(1, 1);
            return
                blk1.OwnerId == blk2.OwnerId &&
                blk1.Name == blk2.Name &&
                Math.Abs(NetLib.DoubleExt.Round(blk1.Rotation, 1) - NetLib.DoubleExt.Round(blk2.Rotation, 1)) < 0.001 &&
                blk1.Position.IsEqualTo(blk2.Position, tol) &&
                blk1.ScaleFactors.IsEqualTo(blk2.ScaleFactors, tol);
        }

        /// <summary>
        /// Удаление всех объектов из блока.
        /// Блок должен быть открыт для записи
        /// </summary>
        /// <param name="btr"></param>
        public static void ClearEntity([NotNull] this BlockTableRecord btr)
        {
            foreach (var idEnt in btr)
            {
                using (var ent = (Entity)idEnt.Open(OpenMode.ForWrite, false, true))
                {
                    ent.Erase();
                }
            }
        }

        /// <summary>
        /// Проверка натуральной трансформации блока - без масштабирования
        /// blRef.ScaleFactors.IsEqualTo(new Scale3d(1), Tolerance01)
        /// </summary>
        public static bool CheckNaturalBlockTransform([NotNull] this BlockReference blRef)
        {
            return blRef.ScaleFactors.IsEqualTo(new Scale3d(1), Tolerance01);
        }

        /// <summary>
        /// Нормализация блока с сохранением границ (опираясь на нижнюю точку границы до и после нормализации)
        /// </summary>
        /// <param name="blRef"></param>
        [Obsolete("Не работает!!!", true)]
        public static void Normalize([NotNull] this BlockReference blRef)
        {
            // Корректировка масштабирования и зеркальности
            var scale1 = new Scale3d(1);
            if (blRef.ScaleFactors != scale1)
            {
                blRef.ScaleFactors = scale1;
            }

            if (Math.Abs(blRef.Rotation) > 0.0001)
            {
                var matRotate = Matrix3d.Rotation(-blRef.Rotation, Vector3d.ZAxis, blRef.Position);
                blRef.TransformBy(matRotate);
            }
        }
    }
}