namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Autodesk.AutoCAD.Colors;
    using JetBrains.Annotations;
    using ReactiveUI;

    public class ColorVM : BaseValueVM
    {
        public static ColorView Create([NotNull] IEnumerable<Color> values,
            Action<object> update = null,
            Action<ColorVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return CreateS<ColorView, ColorVM>(values, (v,vm) => update?.Invoke(v), config, isReadOnly);
        }

        public static ColorView Create(
            object value,
            Action<object> update = null,
            Action<ColorVM> config = null,
            bool isReadOnly = false)
        {
            if (update == null)
                isReadOnly = true;
            return Create<ColorView, ColorVM>(value, (v,vm) => update?.Invoke(v), config, isReadOnly);
        }
    }
}
