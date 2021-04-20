namespace Autodesk.AutoCAD.DatabaseServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AcadLib;

    public static class DatabaseExtensions
    {
        /// <summary>
        /// Opens a DBObject in ForRead mode (kaefer @ TheSwamp)
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? GetObject<T>(this ObjectId id) where T : DBObject
        {
            return id.GetObject<T>(OpenMode.ForRead);
        }

        public static T? GetObject<T>(this ObjectId id,Transaction t) where T : DBObject
        {
            return id.GetObject<T>(OpenMode.ForRead, t);
        }

        public static T GetObjectT<T>(this ObjectId id) where T : DBObject
        {
            return id.GetObjectT<T>(OpenMode.ForRead);
        }

        public static T GetObjectT<T>(this ObjectId id, Transaction t)
            where T : DBObject
        {
            return id.GetObjectT<T>(OpenMode.ForRead, t);
        }

        // Opens a DBObject in the given mode (kaefer @ TheSwamp)
        public static T? GetObject<T>(this ObjectId id, OpenMode mode) where T : DBObject
        {
            if (!id.IsValidEx())
                return null;
            return id.GetObject(mode, false, true) as T;
        }

        public static T? GetObject<T>(this ObjectId id, OpenMode mode, Transaction t) where T : DBObject
        {
            if (!id.IsValidEx())
                return null;
            return t.GetObject(id, mode, false, true) as T;
        }

        public static T GetObjectT<T>(this ObjectId id, OpenMode mode) where T : DBObject
        {
            if (!id.IsValidEx())
                throw new InvalidOperationException();
            return (T)id.GetObject(mode, false, true);
        }

        public static T GetObjectT<T>(this ObjectId id, OpenMode mode, Transaction t) where T : DBObject
        {
            if (!id.IsValidEx())
                throw new InvalidOperationException();
            return (T)t.GetObject(id, mode, false, true);
        }

        // Opens a collection of DBObject in ForRead mode (kaefer @ TheSwamp)
        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids) where T : DBObject
        {
            return ids.GetObjects<T>(OpenMode.ForRead);
        }

        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids, Transaction t) where T : DBObject
        {
            return ids.GetObjects<T>(OpenMode.ForRead, t);
        }

        // Opens a collection of DBObject in the given mode (kaefer @ TheSwamp)
        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids, OpenMode mode) where T : DBObject
        {
            return ids
                .Cast<ObjectId>()
                .Select(id => id.GetObject<T>(mode))
                .Where(res => res != null)!;
        }

        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids, OpenMode mode, Transaction t)
            where T : DBObject
        {
            return ids
                .Cast<ObjectId>()
                .Select(id => id.GetObject<T>(mode, t))
                .Where(res => res != null)!;
        }

        /// <summary>
        /// Имя блока в том числе динамического.
        /// Без условия открытой транзакции.
        /// br.DynamicBlockTableRecord.Open(OpenMode.ForRead)
        /// </summary>
        public static string GetEffectiveName(this BlockReference br)
        {
            using var btrDyn = (BlockTableRecord)br.DynamicBlockTableRecord.Open(OpenMode.ForRead);
            return btrDyn.Name;
        }
    }
}