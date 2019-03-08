namespace AcadLib.PaletteProps
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Controls;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using ReactiveUI;

    public abstract class BaseValueVM: BaseModel, IValue
    {
        private static object value;
        private static Action<object, BaseValueVM> update;
        private static BaseValueVM vm;

        public bool IsReadOnly { get; set; }

        public object Value { get; set; }
        public object ValueOrig { get; set; }

        public List<ObjectId> Elements { get; set; }

        public static TView CreateS<TView, TVm>(
            [NotNull] IEnumerable<object> values,
            Action<object, BaseValueVM> update = null,
            Action<TVm> configure = null,
            bool isReadOnly = false)
            where TVm : BaseValueVM, new()
            where TView : Control
        {
            object value;
            var uniqValues = values.GroupBy(g => g).Select(s => s.Key);
            value = uniqValues.Skip(1).Any() ? PalettePropsService.Various : uniqValues.FirstOrDefault();
            return Create<TView, TVm>(value, update, configure, isReadOnly);
        }

        public static TView Create<TView, TVm>(
            object value,
            Action<object, BaseValueVM> update = null,
            Action<TVm> configure = null,
            bool isReadOnly = false)
            where TVm : BaseValueVM, new()
            where TView : Control
        {
            if (update == null)
                isReadOnly = true;
            var vm = new TVm { Value = value, ValueOrig = value, IsReadOnly = isReadOnly };
            configure?.Invoke(vm);
            vm.WhenAnyValue(v => v.Value).Skip(1)
                .Where(w => w != vm.ValueOrig)
                .ObserveOnDispatcher()
                .Throttle(TimeSpan.FromMilliseconds(400))
                .Subscribe(c => Update(c, update, vm));
            return (TView)Activator.CreateInstance(typeof(TView), vm);
        }

        /// <inheritdoc />
        public void UpdateValue(object obj)
        {
            Value = obj;
        }

        protected static void Update(object value, Action<object, BaseValueVM> update, BaseValueVM vm)
        {
            if (update == null || value == null || Equals(PalettePropsService.Various, value))
                return;
            BaseValueVM.value = value;
            BaseValueVM.update = update;
            BaseValueVM.vm = vm;
            AcadHelper.Doc.SendStringToExecute($"{nameof(Commands._InternalUse_UpdatePropValue)} ", true, false, true);
        }

        public static void InternalUpdate(Document doc)
        {
            using (var t = doc.TransactionManager.StartTransaction())
            {
                Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fffffff} Palette Props Update Value = {value}");
                update(value, vm);
                t.Commit();
            }
        }

        protected static void UpdateTarget(BaseValueVM vm)
        {
            vm.Value = vm.ValueOrig;
        }
    }
}
