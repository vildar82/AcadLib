// ReSharper disable once CheckNamespace
namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class TypedValueExt
    {
        public static T GetTypedValue<T>([CanBeNull] this Dictionary<string, object> dictValues, string name, T defaultValue = default)
        {
            if (dictValues == null)
                return defaultValue;
            if (dictValues.TryGetValue(name, out var value))
            {
                try
                {
                    return value.GetValue<T>();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex,
                        $"TypedValueExt, GetValue из словаря значений - по имени параметра '{name}'. object = {value} тип {value.GetType()}");
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        [Obsolete("Use GetTypedValue")]
        public static T GetValue<T>([CanBeNull] this Dictionary<string, object> dictValues, string name, T defaultValue)
        {
            return GetTypedValue(dictValues, name, defaultValue);
        }

        public static IEnumerable<KeyValuePair<string, object>> ToListPairs([CanBeNull] this IEnumerable<TypedValue> values)
        {
            if (values != null)
            {
                string key = null;
                foreach (var value in values)
                {
                    if (key == null)
                    {
                        key = value.Value.ToString();
                    }
                    else
                    {
                        yield return new KeyValuePair<string, object>(key, value.Value);
                        key = null;
                    }
                }
            }
        }

        [NotNull]
        public static Dictionary<string, object> ToDictionary([CanBeNull] this IEnumerable<TypedValue> values)
        {
            var dictValues = new Dictionary<string, object>();
            if (values == null)
                return dictValues;
            var name = string.Empty;
            foreach (var item in values)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    try
                    {
                        dictValues.Add(name, item.Value);
                        name = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, $"ToDictionary() - name={name}, value={item.Value}");
                    }
                }
                else
                {
                    name = item.GetTvValue<string>();
                }
            }

            return dictValues;
        }

        /// <summary>
        /// Возвращает значение TypedValue
        /// Типы - int, bool, byte, double, string - те которые возможны в TypedValue DxfCode
        /// Не проверяется соответствие типа значения и номера кода DxfCode !!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tv"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        [CanBeNull]
        public static T GetTvValue<T>(this TypedValue tv)
        {
            try
            {
                return tv.Value.GetValue<T>();
            }
            catch
            {
                // ignored
            }

            return default;
        }

        /// <summary>
        /// Создание TypedValue для сохранение в расширенные данные DxfCode.ExtendedData
        /// bool, byte, int, double, string
        /// </summary>
        public static TypedValue GetTvExtData([CanBeNull] object value)
        {
            if (value == null)
                return new TypedValue();
            var code = 0;
            var tvValue = value;
            switch (value)
            {
                case bool b:
                    code = (int)DxfCode.ExtendedDataInteger32;
                    tvValue = b ? 1 : 0;
                    break;
                case Enum _:
                    code = (int)DxfCode.ExtendedDataInteger32;
                    tvValue = (int)value;
                    break;
                case int _:
                    code = (int)DxfCode.ExtendedDataInteger32;
                    break;
                case byte _:
                    code = (int)DxfCode.ExtendedDataInteger32;
                    break;
                case double _:
                    code = (int)DxfCode.ExtendedDataReal;
                    break;
                case string _:
                    code = (int)DxfCode.ExtendedDataAsciiString;
                    break;
            }

            if (code == 0)
                throw new Exception("Invalid TypedValue");

            return new TypedValue(code, tvValue);
        }
    }
}