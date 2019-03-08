namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public class StringVM : BaseValueVM
    {
        public static StringView Create([NotNull] IEnumerable<string> values,
            Action<object> update = null,
            Action<StringVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return CreateS<StringView, StringVM>(values, (v, vm) => update?.Invoke(v), config, isReadOnly);
        }

        public static StringView Create(
            object value,
            Action<object> update = null,
            Action<StringVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return Create<StringView, StringVM>(value, (v, vm) => update?.Invoke(v), config, isReadOnly);
        }
    }
}
