namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using NetLib;

    public class DoubleVM : BaseValueVM
    {
        public static DoubleView Create([NotNull] IEnumerable<double> values,
            Action<object> update = null,
            Action<DoubleVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            var updateA = GetUpdateAction(update);
            return CreateS<DoubleView, DoubleVM>(values.Cast<object>(), updateA, config, isReadOnly);
        }

        public static DoubleView Create(
            object value,
            Action<object> update = null,
            Action<DoubleVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            var updateA = GetUpdateAction(update);
            return Create<DoubleView, DoubleVM>(value, updateA, config, isReadOnly);
        }

        private static Action<object, BaseValueVM> GetUpdateAction(Action<object> update)
        {
            if (update == null)
                return null;
            return (v, vm) =>
            {
                double dVal;
                try
                {
                    dVal = v.GetValue<double>();
                }
                catch
                {
                    // Не число
                    UpdateTarget(vm);
                    return;
                }

                update(dVal);
            };
        }
    }
}
