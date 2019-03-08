namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public class BoolVM : BaseValueVM
    {
        public static BoolView Create([NotNull] IEnumerable<bool> values,
            Action<object> update = null,
            Action<BoolVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return CreateS<BoolView, BoolVM>(values.Cast<object>(), (v, vm) => update?.Invoke(v), config, isReadOnly);
        }

        public static BoolView Create(
            object value,
            Action<object> update = null,
            Action<BoolVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return Create<BoolView, BoolVM>(value, (v, vm) => update?.Invoke(v), config, isReadOnly);
        }
    }
}
