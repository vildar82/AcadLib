namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.Monad;
    using XData;

    /// <summary>
    /// Расширенные данные объекта
    /// </summary>
    [PublicAPI]
    public static class XDataExt
    {
        private static readonly Dictionary<Type, short> dictXDataTypedValues = new Dictionary<Type, short>
        {
            { typeof(int), (short)DxfCode.ExtendedDataInteger32 },
            { typeof(double), (short)DxfCode.ExtendedDataReal },
            { typeof(string), (short)DxfCode.ExtendedDataAsciiString }
        };

        /// <summary>
        /// Регистрация приложения в RegAppTable
        /// </summary>
        public static void RegApp([NotNull] this Database db, string regAppName)
        {
            RegAppHelper.RegApp(db, regAppName);
        }

        /// <summary>
        /// Регистрация приложения PIK в RegAppTable
        /// </summary>
        /// <param name="db"></param>
        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static void RegAppPIK([NotNull] this Database db)
        {
            RegApp(db, ExtDicHelper.PikApp);
        }

        /// <summary>
        /// Удаление XData связанных с этим приложением
        /// </summary>
        /// <param name="dbo"></param>
        /// <param name="regAppName"></param>
        public static void RemoveXData([NotNull] this DBObject dbo, string regAppName)
        {
            if (dbo.GetXDataForApplication(regAppName) != null)
            {
                var rb = new ResultBuffer(new TypedValue(1001, regAppName));
                var isWriteEnabled = dbo.IsWriteEnabled;
                if (!isWriteEnabled)
                {
                    using (var dboWrite = dbo.Id.Open(OpenMode.ForWrite, false, true))
                    {
                        dboWrite.XData = rb;
                    }
                }
                else
                {
                    dbo.XData = rb;
                }
            }
        }

        public static bool HasXData([NotNull] this DBObject dbo, [NotNull] string regApp)
        {
            return dbo.GetXDataForApplication(regApp) != null;
        }

        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static void RemoveXDataPIK([NotNull] this DBObject dbo)
        {
            RemoveXData(dbo, ExtDicHelper.PikApp);
        }

        /// <summary>
        /// Приложение не регистрируется !!!
        /// </summary>
        public static void SetXData([NotNull] this DBObject dbo, string regAppName, string value)
        {
            using (var rb = new ResultBuffer(
                new TypedValue((short)DxfCode.ExtendedDataRegAppName, regAppName)))
            {
                if (value.Length <= 255)
                {
                    var tv = new TypedValue((short)DxfCode.ExtendedDataAsciiString, value);
                    rb.Add(tv);
                }
                else
                {
                    var index = 0;
                    foreach (var s in value.Split(250))
                    {
                        var tv = new TypedValue((short)DxfCode.ExtendedDataAsciiString, $"{index++}#{s}");
                        rb.Add(tv);
                    }
                }

                dbo.XData = rb;
            }
        }

        /// <summary>
        /// Приложение не регистрируется !!!
        /// </summary>
        public static void SetXData([NotNull] this DBObject dbo, string regAppName, int value)
        {
            using (var rb = new ResultBuffer(
                new TypedValue((short)DxfCode.ExtendedDataRegAppName, regAppName),
                new TypedValue((short)DxfCode.ExtendedDataInteger32, value)))
            {
                dbo.XData = rb;
            }
        }

        /// <summary>
        /// Запись значения в XData
        /// Регистрируется приложение regAppName
        /// </summary>
        /// <param name="dbo">DBObject</param>
        /// <param name="regAppName">Имя приложения</param>
        /// <param name="value">Значение одного из стандартного типа - int, double, string</param>
        public static void SetXData<T>([NotNull] this DBObject dbo, string regAppName, [NotNull] T value)
        {
            RegApp(dbo.Database, regAppName);
            var tvValu = GetTypedValue(value);
            using (var rb = new ResultBuffer(new TypedValue((short)DxfCode.ExtendedDataRegAppName, regAppName), tvValu))
            {
                dbo.XData = rb;
            }
        }

        /// <summary>
        /// Запись значения в XData
        /// Регистрируется приложение regAppName
        /// </summary>
        /// <param name="dbo">DBObject</param>
        /// <param name="value">Значение одного из стандартного типа - int, double, string</param>
        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static void SetXDataPIK<T>([NotNull] this DBObject dbo, [NotNull] T value)
        {
            SetXData(dbo, ExtDicHelper.PikApp, value);
        }

        /// <summary>
        /// Запись int
        /// ПРиложение не регистрируется
        /// </summary>
        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static void SetXDataPIK([NotNull] this DBObject dbo, int value)
        {
            SetXData(dbo, ExtDicHelper.PikApp, value);
        }

        public static int GetXData([NotNull] this DBObject dbo, string regAppName)
        {
            var rb = dbo.GetXDataForApplication(regAppName);
            if (rb != null)
            {
                foreach (var item in rb)
                {
                    if (item.TypeCode == (short)DxfCode.ExtendedDataInteger32)
                    {
                        return (int)item.Value;
                    }
                }
            }

            return 0;
        }

        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static int GetXDatPIK([NotNull] this DBObject dbo)
        {
            return GetXData(dbo, ExtDicHelper.PikApp);
        }

        public static string GetXDataString([NotNull] this DBObject dbo, string regAppName)
        {
            var rb = dbo.GetXDataForApplication(regAppName);
            return rb != null ? GetStringValue(rb.GetEnumerator(), false) : string.Empty;
        }

        private static string GetStringValue(ResultBufferEnumerator tvEnumerator, bool hasCurrent)
        {
            var regex = new Regex(@"^\d{1,2}#");
            string nextVal = null;

            if (!hasCurrent) tvEnumerator.MoveNext();
            do
            {
                var tv = tvEnumerator.Current;
                if (tv.TypeCode == (short)DxfCode.ExtendedDataAsciiString)
                {
                    nextVal = tv.Value?.ToString();
                    break;
                }
            }
            while (tvEnumerator.MoveNext());

            if (nextVal == null)
                return null;

            var match = regex.Match(nextVal);
            if (!match.Success)
                return nextVal;

            var sb = new StringBuilder(nextVal.Substring(match.Length));

            while (tvEnumerator.MoveNext())
            {
                if (tvEnumerator.Current.TypeCode != (short)DxfCode.ExtendedDataAsciiString)
                    break;

                var val = tvEnumerator.Current.Value?.ToString();
                if (val == null)
                    break;
                match = regex.Match(val);
                if (!match.Success)
                    break;
                var valNext = val.Substring(match.Length);
                sb.Append(valNext);
            }

            return sb.ToString();
        }

        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static string GetXDatPIKString([NotNull] this DBObject dbo)
        {
            return GetXDataString(dbo, ExtDicHelper.PikApp);
        }

        /// <summary>
        /// Считывание значение с объекта
        /// </summary>
        /// <typeparam name="T">Тип значения - int, double, string</typeparam>
        /// <returns>Значение или дефолтное значение для этого типа (0,0,null) если не найдено</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        [CanBeNull]
        public static T GetXData<T>([NotNull] this DBObject dbo, string regAppName)
        {
            var rb = dbo.GetXDataForApplication(regAppName);
            if (rb != null)
            {
                var dxfT = dictXDataTypedValues[typeof(T)];
                var rbEnumerator = rb.GetEnumerator();
                while (rbEnumerator.MoveNext())
                {
                    if (rbEnumerator.Current.TypeCode == dxfT)
                    {
                        if (rbEnumerator.Current.TypeCode == (short)DxfCode.ExtendedDataAsciiString)
                        {
                            return (T)(object)GetStringValue(rbEnumerator, true);
                        }

                        return (T)rbEnumerator.Current.Value;
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Считывание значения из XData определенного типа, приложения PIK
        /// </summary>
        /// <typeparam name="T">Тип значения - int, double, string</typeparam>
        /// <param name="dbo">Объект</param>
        /// <returns>Значение или дефолтное значение типа, если не найдено</returns>
        [Obsolete("Лучше использовать свой `regAppName` для каждого плагина (задачи)")]
        public static T GetXDataPIK<T>([NotNull] this DBObject dbo)
        {
            return GetXData<T>(dbo, ExtDicHelper.PikApp);
        }

        private static TypedValue GetTypedValue([NotNull] object value)
        {
            var dxfCode = dictXDataTypedValues[value.GetType()];
            var tv = new TypedValue(dxfCode, value);
            return tv;
        }
    }
}
