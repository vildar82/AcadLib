using AcadLib.PaletteProps.UI;

namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using JetBrains.Annotations;

    /// <summary>
    /// Методы расширения для свойтв на палитре
    /// </summary>
    [PublicAPI]
    public static class ValuesExt
    {
        /// <summary>
        /// Создать дефолтный контрол по типу свойства - bool, int, double, string
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="update">Обновление значения</param>
        /// <returns>Контрол для палитры</returns>
        public static Control CreateControl(this object value, Action<object> update = null, bool isReadOnly = false)
        {
            return GetControl(value, value, update, isReadOnly);
        }

        /// <summary>
        /// Создать дефолтный контрол по типу свойства - bool, int, double, string
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="update">Обновление значения</param>
        /// <returns>Контрол для палитры</returns>
        public static Control CreateControl([NotNull] this IEnumerable<object> values, Action<object> update = null, bool isReadOnly = false)
        {
            var value = GetValue(values);
            return GetControl(values.FirstOrDefault(), value, update, isReadOnly);
        }

        private static object GetValue(IEnumerable<object> values)
        {
            var uniqValues = values.GroupBy(g => g).Select(s => s.Key);
            object value;
            if (uniqValues.Skip(1).Any())
            {
                return PalettePropsService.Various;
            }

            return uniqValues.FirstOrDefault();
        }

        private static Control GetControl(object targetType, object value, Action<object> update, bool isReadOnly = false)
        {
            switch (targetType)
            {
                case bool _: return BoolVM.Create(value, update, isReadOnly: isReadOnly);
                case int _: return IntVM.Create(value, update, isReadOnly: isReadOnly);
                case double _: return DoubleVM.Create(value, update, isReadOnly: isReadOnly);
            }

            return StringVM.Create(value, update, isReadOnly: isReadOnly);
        }
    }
}
