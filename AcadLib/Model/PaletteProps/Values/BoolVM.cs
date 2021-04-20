namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BoolVM : BaseValueVM
    {
        public static BoolView Create(IEnumerable<bool> values,
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
