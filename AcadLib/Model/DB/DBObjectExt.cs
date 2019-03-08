using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace AcadLib
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class DBObjectExt
    {
        [NotNull]
        public static T UpgradeOpenTr<T>([NotNull] this T dbo)
            where T : DBObject
        {
            return dbo.IsWriteEnabled ? dbo : dbo.Id.GetObjectT<T>(OpenMode.ForWrite);
        }

        /// <summary>
        /// Удаление словаря из объекта.
        /// </summary>
        /// <param name="dbo"></param>
        public static void RemoveAllExtensionDictionary([NotNull] this DBObject dbo)
        {
            if (dbo.ExtensionDictionary.IsNull)
                return;
            var extDic = (DBDictionary)dbo.ExtensionDictionary.GetObject(OpenMode.ForWrite);
            if (extDic == null)
                return;
            dbo = dbo.Id.GetObject<DBObject>(OpenMode.ForWrite);
            if (dbo == null)
                return;
            foreach (var entry in extDic)
            {
                extDic.Remove(entry.Key);
            }

            dbo.ReleaseExtensionDictionary();
        }

        /// <summary>
        /// Удаление расширенных данных из объекта
        /// </summary>
        /// <param name="dbo"></param>
        public static void RemoveAllXData(this DBObject dbo)
        {
            if (dbo.XData == null)
                return;
            var appNames = from TypedValue tv in dbo.XData.AsArray() where tv.TypeCode == 1001 select tv.Value.ToString();
            dbo = dbo.Id.GetObject<DBObject>(OpenMode.ForWrite);
            if (dbo == null)
                return;
            foreach (var appName in appNames)
            {
                dbo.XData = new ResultBuffer(new TypedValue(1001, appName));
            }
        }
    }
}
