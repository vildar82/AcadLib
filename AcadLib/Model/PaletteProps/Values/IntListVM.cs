using System.Linq;
using JetBrains.Annotations;

namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;

    public class IntListVM : IntVM
    {
        public bool AllowCustomValue { get; set; }

        public List<object> Values { get; set; }

        public static IntListView Create([NotNull] IEnumerable<int> values,
            Action<object> update = null,
            Action<IntListVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return CreateS<IntListView, IntListVM>(values.Cast<object>(), (v,vm) => update?.Invoke(v), config, isReadOnly);
        }

        public static IntListView Create(
            object value,
            Action<object> update = null,
            Action<IntListVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return Create<IntListView, IntListVM>(value, (v,vm) => update?.Invoke(v), config, isReadOnly);
        }
    }
}
