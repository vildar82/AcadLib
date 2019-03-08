namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using NetLib;

    public class IntVM : BaseValueVM
    {
        public static IntView Create([NotNull] IEnumerable<int> values,
            Action<object> update = null,
            Action<IntVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            var updateA = GetUpdateAction(update);
            return CreateS<IntView, IntVM>(values.Cast<object>(), updateA, config, isReadOnly);
        }

        public static IntView Create(
            object value,
            Action<object> update = null,
            Action<IntVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            var updateA = GetUpdateAction(update);
            return Create<IntView, IntVM>(value, updateA, config, isReadOnly);
        }

        private static Action<object, BaseValueVM> GetUpdateAction(Action<object> update)
        {
            if (update == null)
                return null;
            return (v, vm) =>
            {
                int iVal;
                try
                {
                    iVal = v.GetValue<int>();
                }
                catch
                {
                    // Не число
                    UpdateTarget(vm);
                    return;
                }

                update(iVal);
            };
        }
    }
}
