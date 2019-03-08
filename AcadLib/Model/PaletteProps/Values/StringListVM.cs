namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public class StringListVM : BaseValueVM
    {
        public List<string> Values { get; set; }

        public bool AllowCustomValue { get; set; }

        public static StringListView Create([NotNull] IEnumerable<string> values,
            Action<object> update = null,
            Action<StringListVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return CreateS<StringListView, StringListVM>(values, (v, vm) => update?.Invoke(v), config, isReadOnly);
        }

        public static StringListView Create(
            object value,
            Action<object> update = null,
            Action<StringListVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return Create<StringListView, StringListVM>(value, (v, vm) => update?.Invoke(v), config, isReadOnly);
        }
    }
}
