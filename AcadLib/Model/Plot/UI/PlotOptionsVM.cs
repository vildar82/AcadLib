using System.Reactive;

namespace AcadLib.Plot.UI
{
    using NetLib.WPF;
    using ReactiveUI;

    public class PlotOptionsVM : BaseViewModel
    {
        private readonly PlotOptions _options;

        public PlotOptionsVM(PlotOptions options)
        {
            _options = options;
            Reset = CreateCommand(ResetExec);
            OK = CreateCommand(OkExec);
            ResetExec();
        }

        public ReactiveCommand<Unit, Unit> Reset { get; set; }
        public ReactiveCommand<Unit, Unit> OK { get; set; }
        
        public bool DefaultPlotCurOrFolder { get; set; }
        public string FilterByNames { get; set; }
        public string FilterByNumbers { get; set; }
        public bool FilterState { get; set; }
        public bool OnePdfOrEachDwg { get; set; }
        public bool IncludeSubdirs { get; set; }
        public bool SortTabOrName { get; set; }
        public bool BlankOn { get; set; }
        public int BlankPageNumber { get; set; }

        private void ResetExec()
        {
            DefaultPlotCurOrFolder = _options.DefaultPlotCurOrFolder;
            FilterState = _options.FilterState;
            FilterByNames = _options.FilterByNames;
            FilterByNumbers = _options.FilterByNumbers;
            OnePdfOrEachDwg = _options.OnePdfOrEachDwg;
            IncludeSubdirs = _options.IncludeSubdirs;
            SortTabOrName = _options.SortTabOrName;
            BlankOn = _options.BlankOn;
            BlankPageNumber = _options.BlankPageNumber;
        }
        
        private void OkExec()
        {
            _options.DefaultPlotCurOrFolder = DefaultPlotCurOrFolder;
            _options.BlankOn = BlankOn;
            _options.FilterByNames = FilterByNames;
            _options.FilterState = FilterState;
            _options.IncludeSubdirs = IncludeSubdirs;
            _options.BlankPageNumber = BlankPageNumber;
            _options.FilterByNumbers = FilterByNumbers;
            _options.SortTabOrName = SortTabOrName;
            _options.OnePdfOrEachDwg = OnePdfOrEachDwg;
            DialogResult = true;
        }
    }
}