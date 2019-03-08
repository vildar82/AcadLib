namespace AcadLib.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Копирование объектов из внешней базы
    /// </summary>
    public static class ExternalCopyObjectsExt
    {
        /// <summary>
        /// Копирование объектов из внешней базы
        /// </summary>
        /// <param name="dbDest">База целевая</param>
        /// <param name="externalFile">Внешний файл</param>
        /// <param name="getOwnerId">Получение таблицы содержащей копируемые элементы</param>
        /// <param name="getCopyIds">Получение списка копируемых объектов из таблицы</param>
        /// <param name="mode">Режим копирования</param>
        public static List<ObjectId> Copy(this Database dbDest, string externalFile, DuplicateRecordCloning mode,
            Func<Database, ObjectId> getOwnerId, Func<ObjectId, List<ObjectId>> getCopyIds)
        {
            using (var dbSource = new Database(false, true))
            {
                dbSource.ReadDwgFile(externalFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                dbSource.CloseInput(true);
                return Copy(dbDest, dbSource, mode, getOwnerId, getCopyIds);
            }
        }

        /// <summary>
        /// Копирует объекты из другого чертежа
        /// </summary>
        /// <param name="dbDest">Чертеж назначения</param>
        /// <param name="dbSrc">Чертеж источник</param>
        /// <param name="mode">Режим копирования</param>
        /// <param name="getOwnerId">Получение контейнера</param>
        /// <param name="getCopyIds">Объекты копирования</param>
        /// <returns>Скопированные объекты</returns>
        public static List<ObjectId> Copy(this Database dbDest, Database dbSrc, DuplicateRecordCloning mode,
            Func<Database, ObjectId> getOwnerId, Func<ObjectId, List<ObjectId>> getCopyIds)
        {
            List<ObjectId> idsSource;
            ObjectId ownerIdDest;
            using (var t = dbSrc.TransactionManager.StartTransaction())
            {
                var ownerIdSourse = getOwnerId(dbSrc);
                ownerIdDest = getOwnerId(dbDest);
                idsSource = getCopyIds(ownerIdSourse);
                t.Commit();
            }

            if (idsSource?.Any() != true)
                return new List<ObjectId>();

            using (var map = new IdMapping())
            using (var ids = new ObjectIdCollection(idsSource.ToArray()))
            {
                dbDest.WblockCloneObjects(ids, ownerIdDest, map, mode, false);
                return idsSource.Select(s => map[s].Value).ToList();
            }
        }

        public static List<ObjectId> WblockCloneObjects(this Database dbDest, ObjectId owner, List<ObjectId> idsCopy,
            DuplicateRecordCloning mode)
        {
            using (var map = new IdMapping())
            using (var ids = new ObjectIdCollection(idsCopy.ToArray()))
            {
                dbDest.WblockCloneObjects(ids, owner, map, mode, false);
                return idsCopy.Select(s => map[s].Value).ToList();
            }
        }
    }
}
